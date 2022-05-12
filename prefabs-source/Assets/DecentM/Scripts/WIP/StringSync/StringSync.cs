using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UNet;
using UnityEngine.UI;
using TMPro;
using System;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class StringSync : UdonSharpBehaviour
{
    // This equals to hard limit + 2
    public int worldCapacity = 34;

    public NetworkInterface network;
    public InputField input;
    public TextMeshProUGUI debug;

    public ByteBufferWriter writer;
    public ByteBufferReader reader;

    private const int MaxMessageSize = 500;

    public string[] _syncedText;

    // The synced text is split up into chunks of 500 long strings, but we mask that with
    // this getter and setter
    public string syncedText
    {
        get { return string.Join("", this._syncedText); }
        set
        {
            int i = 0;

            while (i < value.Length)
            {
                string[] newSendSource = new string[this._syncedText.Length + 1];
                Array.Copy(this._syncedText, newSendSource, this._syncedText.Length);
                newSendSource[newSendSource.Length - 1] = value.Substring(
                    i,
                    Math.Min(MaxMessageSize, value.Length - i)
                );
                this._syncedText = newSendSource;

                i = i + MaxMessageSize;
            }
        }
    }

    // Commands that start with Client are client->master
    private const string ClientRequestResync = "Resync";
    private const string ClientRequestIndex = "Index";

    // Commands that start with Master are master->client
    private const string MasterSendsIndex = "IndexR";

    private int messageLock = -1;
    private bool isClientSyncing = false;

    void Start()
    {
        if (this.network == null)
        {
            Debug.LogError(
                "[StringSync] The network variable has not been set. This StringSync will not function."
            );
            this.enabled = false;
            return;
        }

        this.debug.text = "";

        this.network.AddEventsListener(this);

        // this.syncProgresses = new int[this.worldCapacity];
    }

    private void DebugLog(string msg)
    {
        if (this.debug.text.Split('\n').Length > 5)
        {
            this.debug.text = "";
        }

        this.debug.text += $"{msg}\n";
    }

    private void ResetSyncedText()
    {
        this._syncedText = new string[0];
    }

    public void OnUNetInit()
    {
        this.DebugLog("OnUNetInit()");
    }

    // VRChat doesn't support passing variables with events, so the network interface will set
    // our private variables before sending events. Just like assembly!

    private int OnUNetConnected_playerId;

    public void OnUNetConnected()
    {
        this.DebugLog($"OnUNetConnected({OnUNetConnected_playerId})");
    }

    private int OnUNetDisconnected_playerId;

    public void OnUNetDisconnected()
    {
        this.DebugLog($"OnUNetDisconnected({OnUNetDisconnected_playerId})");
    }

    public void OnUNetPrepareSend()
    {
        // this.DebugLog("OnUNetPrepareSend()");
        if (!this.isClientSyncing || messageLock != -1)
        {
            return;
        }

        this.DebugLog($"Requesting index {this._syncedText.Length} from master");
        this.messageLock = this.RequestIndexSync(this._syncedText.Length);
        this.DebugLog($"messageLock is now {this.messageLock}");
    }

    private int OnUNetSendComplete_messageId;
    private bool OnUNetSendComplete_succeed;

    public void OnUNetSendComplete()
    {
        this.DebugLog(
            $"OnUNetSendComplete({OnUNetSendComplete_messageId}, {OnUNetSendComplete_succeed})"
        );

        if (OnUNetSendComplete_messageId == this.messageLock)
        {
            this.messageLock = -1;
        }
    }

    private int OnUNetReceived_sender;
    private byte[] OnUNetReceived_dataBuffer;
    private int OnUNetReceived_dataIndex;
    private int OnUNetReceived_dataLength;
    private int OnUNetReceived_id;

    public void OnUNetReceived()
    {
        this.DebugLog(
            $"OnUNetReceived({OnUNetReceived_sender}, {OnUNetReceived_dataBuffer.ToString()}, {OnUNetReceived_dataIndex}, {OnUNetReceived_dataLength}, {OnUNetReceived_id})"
        );

        string value = this.reader.ReadUTF8String(
            OnUNetReceived_dataLength,
            OnUNetReceived_dataBuffer,
            OnUNetReceived_dataIndex
        );
        string command = value.Split(null, 2)[0];
        string arguments = value.Split(null, 2)[1];

        // Network messages are strings, where the command and its arguments are separated by a space
        switch (command)
        {
            // This player requested that we (re)start syncing the string to them from the beginning
            // master receives this
            //case ClientRequestResync:
            //    this.syncProgresses[OnUNetReceived_sender] = 0;
            //    break;
            // master receives this
            case ClientRequestIndex:
                int requestedIndex = 0;
                int.TryParse(arguments, out requestedIndex);
                this.SendIndexToPlayer(requestedIndex, OnUNetReceived_sender);
                break;
            case MasterSendsIndex:
                string argIndex = arguments.Split(null, 2)[0];
                string argContent = arguments.Split(null, 2)[1];

                int receivedIndex = 0;
                int.TryParse(argIndex, out receivedIndex);

                // If the local version of the array hasn't grown to this size yet, we need to expand it
                if (_syncedText.Length <= receivedIndex)
                {
                    string[] newSendSource = new string[receivedIndex + 1];
                    Array.Copy(this._syncedText, newSendSource, this._syncedText.Length);
                }

                this._syncedText[receivedIndex] = argContent;
                this.UpdateOutput();
                break;
            default:
                // Ignore messages we don't recognise
                break;
        }
    }

    private void UpdateOutput()
    {
        this.input.text = this.syncedText;
    }

    public void OnInteract()
    {
        this.DebugLog("OnInteract()");
        if (!Networking.LocalPlayer.isMaster)
        {
            this.DebugLog("Not master, not sending sync command to everyone");
            return;
        }

        this.syncedText = this.input.text;

        this.DebugLog($"About to sync {this._syncedText.Length} segment(s)");

        // OnInteract is triggered when the master presses the Sync button. That means we need to discard all sync state and
        // start over.
        this.SendCustomNetworkEvent(
            VRC.Udon.Common.Interfaces.NetworkEventTarget.All,
            nameof(this.RequestResync)
        );
    }

    public void RequestResync()
    {
        if (Networking.LocalPlayer.isMaster)
        {
            this.DebugLog("Master, not sending sync request to self");
            return;
        }

        this.DebugLog($"Client requesting resync");
        this.ResetSyncedText();
        this.isClientSyncing = true;
    }

    private int RequestIndexSync(int index)
    {
        this.DebugLog($"RequestIndexSync({index})");
        return this.SendCommandMaster(ClientRequestIndex, index.ToString());
    }

    private int SendIndexToPlayer(int index, int player)
    {
        this.DebugLog($"SendIndexToPlayer({index}, {player})");
        return this.SendCommandTarget(
            MasterSendsIndex,
            $"{index.ToString()} {this._syncedText[index]}",
            player
        );
    }

    private int SendCommandAll(string command, string arguments)
    {
        string message = $"{command} {arguments}";
        int length = this.writer.GetUTF8StringSize(message);
        byte[] buffer = new byte[length + 1];
        this.writer.WriteVarUTF8String(message, buffer, 0);
        return this.network.SendAll(false, buffer, length);
    }

    private int SendCommandTarget(string command, string arguments, int player)
    {
        string message = $"{command} {arguments}";
        int length = this.writer.GetUTF8StringSize(message);
        byte[] buffer = new byte[length + 1];
        this.writer.WriteVarUTF8String(message, buffer, 0);
        return this.network.SendTarget(false, buffer, length, player);
    }

    private int SendCommandMaster(string command, string arguments)
    {
        string message = $"{command} {arguments}";
        int length = this.writer.GetUTF8StringSize(message);
        byte[] buffer = new byte[length + 1];
        this.writer.WriteVarUTF8String(message, buffer, 0);
        return this.network.SendMaster(false, buffer, length);
    }
}
