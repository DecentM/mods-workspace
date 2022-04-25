using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Video.Components;

namespace DecentM.VideoPlayer
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public sealed class UnityPlayerHandler : BasePlayerHandler
    {
        public override VideoPlayerHandlerType type
        {
            get { return VideoPlayerHandlerType.Unity; }
        }
    }
}
