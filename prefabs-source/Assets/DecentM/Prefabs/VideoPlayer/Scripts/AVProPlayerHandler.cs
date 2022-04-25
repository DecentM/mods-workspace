using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Video.Components.AVPro;

namespace DecentM.VideoPlayer
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public sealed class AVProPlayerHandler : BasePlayerHandler
    {
        public override VideoPlayerHandlerType type
        {
            get { return VideoPlayerHandlerType.AVPro; }
        }
    }
}
