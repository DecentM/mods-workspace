using UdonSharp;
using DecentM.Pubsub;

namespace DecentM.Performance.Plugins
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class PerformanceGovernorPlugin : PubsubSubscriber
    {
        protected virtual void OnPerformanceModeChange(PerformanceGovernorMode mode, float fps) { }

        protected sealed override void OnPubsubEvent(object name, object[] data)
        {
            switch (name)
            {
                #region Core

                case PerformanceGovernorEvent.OnPerformanceModeChange:
                    {
                        PerformanceGovernorMode mode = (PerformanceGovernorMode)data[0];
                        float fps = (float)data[1];
                        this.OnPerformanceModeChange(mode, fps);
                        return;
                    }

                    #endregion
            }
        }
    }
}
