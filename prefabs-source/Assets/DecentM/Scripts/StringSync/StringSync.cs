
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UNet;
using UnityEngine.UI;
using TMPro;

public class StringSync : UdonSharpBehaviour
{
    public NetworkInterface network;
    public InputField input;
    public TextMeshProUGUI debug;

    public ByteBufferWriter writer;
    public ByteBufferReader reader;

    void Start()
    {
        if (this.network == null)
        {
            Debug.LogError("[StringSync] The network variable has not been set. This StringSync will not function.");
            this.enabled = false;
            return;
        }

        this.debug.text = "";

        this.network.AddEventsListener(this);
    }

    private void DebugLog(string msg)
    {
        if (this.debug.text.Split('\n').Length > 5)
        {
            this.debug.text = "";
        }

        this.debug.text += $"{msg}\n";
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
    }

    private int OnUNetSendComplete_messageId;
    private int OnUNetSendComplete_succeed;
    public void OnUNetSendComplete()
    {
        this.DebugLog($"OnUNetSendComplete({OnUNetSendComplete_messageId}, {OnUNetSendComplete_succeed})");
    }

    private int OnUNetReceived_sender;
    private byte[] OnUNetReceived_dataBuffer;
    private int OnUNetReceived_dataIndex;
    private int OnUNetReceived_dataLength;
    private int OnUNetReceived_id;
    public void OnUNetReceived()
    {
        this.DebugLog($"OnUNetReceived({OnUNetReceived_sender}, {OnUNetReceived_dataBuffer.ToString()}, {OnUNetReceived_dataIndex}, {OnUNetReceived_dataLength}, {OnUNetReceived_id})");

        this.DebugLog($"Received buffer with length {OnUNetReceived_dataLength}");
        string value = this.reader.ReadUTF8String(OnUNetReceived_dataLength, OnUNetReceived_dataBuffer, OnUNetReceived_dataIndex);
        this.input.text = value;
    }

    public void OnInteract()
    {
        this.DebugLog("OnInteract()");
        string value = this.input.text;
        int length = this.writer.GetUTF8StringSize(value);
        this.DebugLog($"Creating buffer with length {length}");
        byte[] buffer = new byte[length + 1];
        int messageLength = this.writer.WriteVarUTF8String(value, buffer, 0);

        this.DebugLog("Syncing UTF-8 string");
        this.network.SendAll(false, buffer, messageLength);
    }
}
