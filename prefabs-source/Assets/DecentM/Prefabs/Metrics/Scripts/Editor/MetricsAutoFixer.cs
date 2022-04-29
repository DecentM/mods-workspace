using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DecentM.EditorTools;

namespace DecentM.Metrics
{
    public class MetricsAutoFixer : AutoSceneFixer
    {
        protected override bool OnPerformFixes()
        {
            IndividualTrackingPluginTroubleshooter.RelinkRequirements();
            return true;
        }
    }
}
