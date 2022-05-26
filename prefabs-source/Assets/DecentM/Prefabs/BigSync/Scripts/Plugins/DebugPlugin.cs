using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

namespace DecentM.BigSync.Plugins
{
    public class DebugPlugin : BigSyncPlugin
    {
        public TextMeshProUGUI gui;

        string[] logs = new string[30];

        public void Log(params string[] messages)
        {
            string[] tmp = new string[logs.Length];
            Array.Copy(logs, 1, tmp, 0, this.logs.Length - 1);
            tmp[tmp.Length - 1] = String.Join(" ", messages);
            this.logs = tmp;

            if (this.gui != null)
                this.gui.text = String.Join("\n", this.logs);
        }

        protected override void OnDebugLog(string message)
        {
            this.Log(nameof(OnDebugLog), message);
        }

        protected override void OnSyncBegin(
            BigSyncDirection direction,
            int playerId,
            int arrayLength
        )
        {
            this.Log(
                nameof(OnSyncBegin),
                direction.ToString(),
                playerId.ToString(),
                arrayLength.ToString()
            );
        }

        protected override void OnSyncEnd(
            BigSyncDirection direction,
            int playerId,
            BigSyncStatus status
        )
        {
            this.Log(
                nameof(OnSyncEnd),
                direction.ToString(),
                playerId.ToString(),
                status.ToString()
            );
        }

        protected override void OnSyncProgress(
            BigSyncDirection direction,
            int playerId,
            float progress
        )
        {
            this.Log(
                nameof(OnSyncProgress),
                direction.ToString(),
                playerId.ToString(),
                progress.ToString()
            );
        }
    }
}
