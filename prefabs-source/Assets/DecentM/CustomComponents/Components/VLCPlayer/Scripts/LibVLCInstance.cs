using UnityEngine;
using LibVLCSharp;

namespace DecentM.Mods.CustomComponents.VideoPlayer
{
    // LibVLC can only exist once, so we give others access here in case they need it
    public static class LibVLCSingleton
    {
        // Instantiate libvlc if we're getting it for the first time, otherwise just return the existing one
        private static LibVLC _libvlc;
        private static LibVLC libvlc
        {
            get
            {
                if (_libvlc == null)
                {
                    Core.Initialize(Application.dataPath); //Load VLC dlls
                    _libvlc = new LibVLC(enableDebugLogs: false, "--no-osd"); //--no-osd prevents play and pause icons being overlaid on our video

                    //Setup Error Logging
                    Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
                }

                return _libvlc;
            }
        }

        public static LibVLC GetInstance()
        {
            return libvlc;
        }
    }
}
