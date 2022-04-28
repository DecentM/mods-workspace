using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public struct MatrixInput
    {
        public MatrixInput(List<string> instanceIds, int worldCapacity)
        {
            this.instanceIds = instanceIds;
            this.worldCapacity = worldCapacity;
        }

        public List<string> instanceIds;
        public int worldCapacity;
    }

    public static class MetricsMatrix
    {
        public static Dictionary<Metric, List<MetricValue>> GenerateMatrix(MatrixInput input)
        {
            Dictionary<Metric, List<MetricValue>> matrix = new Dictionary<Metric, List<MetricValue>>();

            List<MetricValue> heartbeatValues = new List<MetricValue>();
            heartbeatValues.Add(new BoolMetricValue("isMaster"));
            heartbeatValues.Add(new BoolMetricValue("isVr"));
            heartbeatValues.Add(new IntRangeMetricValue("timezone", -11, 12));
            matrix.Add(Metric.Heartbeat, heartbeatValues);

            List<MetricValue> respawnValues = new List<MetricValue>();
            respawnValues.Add(new NullMetricValue());
            matrix.Add(Metric.Respawn, respawnValues);

            List<MetricValue> instanceValues = new List<MetricValue>();
            instanceValues.Add(new StringMetricValue("instanceId", input.instanceIds.ToArray()));
            instanceValues.Add(new IntRangeMetricValue("playerCount", 1, input.worldCapacity));
            matrix.Add(Metric.Instance, instanceValues);

            /*
            List<MetricValue> Values = new List<MetricValue>();
            matrix.Add(Metric., Values);
            */

            return matrix;
        }
    }
}
