using System;
using System.Linq;
using System.Collections.Generic;
using DecentM.Metrics.Plugins;
using DecentM.EditorTools;

namespace DecentM.Metrics
{
    public abstract class MetricValue
    {
        public abstract string[] GetPossibleValues();
        public string name;
    }

    public class ResolvedMetricValue
    {
        public ResolvedMetricValue(string name, string value)
        {
            this.name = name;
            this.value = value;
        }

        public static List<ResolvedMetricValue> FromMetricValue(MetricValue metricValue)
        {
            List<ResolvedMetricValue> result = new List<ResolvedMetricValue>();

            foreach (string value in metricValue.GetPossibleValues())
            {
                result.Add(new ResolvedMetricValue(metricValue.name, value));
            }

            return result;
        }

        public string name;
        public string value;
    }

    public class NullMetricValue : MetricValue
    {
        public NullMetricValue()
        {
            this.name = "";
        }

        public override string[] GetPossibleValues()
        {
            return new string[] { "" };
        }
    }

    public class BoolMetricValue : MetricValue
    {
        public BoolMetricValue(string name)
        {
            this.name = name;
        }

        public override string[] GetPossibleValues()
        {
            return new string[] { true.ToString(), false.ToString() };
        }
    }

    public class IntRangeMetricValue : MetricValue
    {
        public IntRangeMetricValue(string name, int min, int max)
        {
            this.name = name;
            this.min = min;
            this.max = max;
        }

        private int min;
        private int max;

        public override string[] GetPossibleValues()
        {
            string[] result = new string[this.max - this.min + 1];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = $"{min + i}";
            }

            return result;
        }
    }

    public class StringMetricValue : MetricValue
    {
        public StringMetricValue(string name, string[] values)
        {
            this.name = name;
            this.values = values;
        }

        public StringMetricValue(string name, string value)
        {
            this.name = name;
            this.values = new string[] { value };
        }

        private string[] values;

        public override string[] GetPossibleValues()
        {
            return this.values;
        }
    }

    public class IntMetricValue : MetricValue
    {
        public IntMetricValue(string name, int[] values)
        {
            this.name = name;
            this.values = values;
        }

        public IntMetricValue(string name, int value)
        {
            this.name = name;
            this.values = new int[] { value };
        }

        private int[] values;

        public override string[] GetPossibleValues()
        {
            List<string> result = new List<string>();

            foreach (int value in this.values)
            {
                result.Add(value.ToString());
            }

            return result.ToArray();
        }
    }

    public class EnumMetricValue : MetricValue
    {
        public EnumMetricValue(string name, dynamic type)
        {
            this.name = name;
            int[] values = Enum.GetValues(type);

            this.values = values.Select(v => (string)Enum.GetName(type, v)).ToArray();
        }

        private string[] values;

        public override string[] GetPossibleValues()
        {
            return this.values;
        }
    }

    public struct MatrixInput
    {
        public List<string> instanceIds;
        public int worldCapacity;
        public int minFps;
        public int maxFps;
    }

    public static class MetricsMatrix
    {
        public static Dictionary<Metric, List<MetricValue>> GenerateMatrix(MatrixInput input)
        {
            Dictionary<Metric, List<MetricValue>> matrix =
                new Dictionary<Metric, List<MetricValue>>();

            List<MetricValue> heartbeatValues = new List<MetricValue>();
            heartbeatValues.Add(new BoolMetricValue("isMaster"));
            heartbeatValues.Add(new BoolMetricValue("isVr"));
            heartbeatValues.Add(new IntRangeMetricValue("fps", input.minFps, input.maxFps));
            matrix.Add(Metric.Heartbeat, heartbeatValues);

            List<MetricValue> respawnValues = new List<MetricValue>();
            respawnValues.Add(new NullMetricValue());
            matrix.Add(Metric.Respawn, respawnValues);

            List<MetricValue> instanceValues = new List<MetricValue>();
            instanceValues.Add(new StringMetricValue("instanceId", input.instanceIds.ToArray()));
            instanceValues.Add(new IntRangeMetricValue("playerCount", 1, input.worldCapacity));
            matrix.Add(Metric.Instance, instanceValues);

            List<MetricValue> interactionValues = new List<MetricValue>();
            interactionValues.Add(
                new StringMetricValue(
                    "objectName",
                    IndividualTrackingPluginManager<InteractionsPlugin>
                        .CollectInteractionNames()
                        .ToArray()
                )
            );
            matrix.Add(Metric.Interaction, interactionValues);

            List<MetricValue> triggerValues = new List<MetricValue>();
            triggerValues.Add(
                new StringMetricValue(
                    "objectName",
                    IndividualTrackingPluginManager<TriggerVolumePlugin>
                        .CollectInteractionNames()
                        .ToArray()
                )
            );
            triggerValues.Add(new BoolMetricValue("state"));
            matrix.Add(Metric.Trigger, triggerValues);

            List<MetricValue> stationValues = new List<MetricValue>();
            stationValues.Add(
                new StringMetricValue(
                    "objectName",
                    IndividualTrackingPluginManager<StationPlugin>
                        .CollectInteractionNames()
                        .ToArray()
                )
            );
            stationValues.Add(new BoolMetricValue("state"));
            matrix.Add(Metric.Station, stationValues);

            List<MetricValue> pickupValues = new List<MetricValue>();
            pickupValues.Add(
                new StringMetricValue(
                    "objectName",
                    IndividualTrackingPluginManager<PickupPlugin>
                        .CollectInteractionNames()
                        .ToArray()
                )
            );
            pickupValues.Add(new BoolMetricValue("state"));
            matrix.Add(Metric.Pickup, pickupValues);

            List<MetricValue> customValues = new List<MetricValue>();
            customValues.Add(
                new StringMetricValue(
                    "metricName",
                    IndividualTrackingPluginManager<CustomEventTrackingPlugin>
                        .CollectInteractionNames()
                        .ToArray()
                )
            );
            matrix.Add(Metric.Custom, customValues);

            List<MetricValue> videoPlayerValues = new List<MetricValue>();
            videoPlayerValues.Add(
                new StringMetricValue(
                    "playerName",
                    IndividualTrackingPluginManager<VideoPlayerTracker>
                        .CollectInteractionNames()
                        .ToArray()
                )
            );
            videoPlayerValues.Add(
                new EnumMetricValue("eventName", typeof(TrackedVideoPlayerEvent))
            );
            matrix.Add(Metric.VideoPlayer, videoPlayerValues);

            List<MetricValue> playerListValues = new List<MetricValue>();
            playerListValues.Add(
                new StringMetricValue(
                    "listName",
                    IndividualTrackingPluginManager<PlayerlistPlugin>
                        .CollectInteractionNames()
                        .ToArray()
                )
            );
            playerListValues.Add(new EnumMetricValue("eventName", typeof(PlayerlistMetric)));

            List<string> players = new List<string>();

            List<PlayerList> playerLists = ComponentCollector<PlayerList>.CollectFromActiveScene();

            foreach (PlayerList list in playerLists)
            {
                players.AddRange(list.players);
            }

            // Add an item for when a player's name cannot be determined
            // (for example when the list is running in blacklist mode)
            players.Add("");
            playerListValues.Add(new StringMetricValue("player", players.ToArray()));
            matrix.Add(Metric.PlayerList, playerListValues);

            /*
             * Copy me to add another metric:
                List<MetricValue> Values = new List<MetricValue>();
                Values.Add(new NullMetricValue());
                matrix.Add(Metric., Values);
            */

            return matrix;
        }
    }
}
