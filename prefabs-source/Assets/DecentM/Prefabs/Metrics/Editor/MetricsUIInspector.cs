using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UdonSharp;
using DecentM.EditorTools;
using VRC.SDKBase;
using DecentM.Metrics.Plugins;

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

    [CustomEditor(typeof(MetricsUI))]
    public class MetricsUIInspector : DEditor
    {
        MetricsUI ui;
        URLStore urlStore;
        InstancePlugin instancePlugin;

        private string metricsServerBaseUrl = "http://localhost:3000";
        private int worldCapacity = 64;
        private int instanceCapacity = 64;

        private Dictionary<Metric, List<MetricValue>> GenerateMatrix()
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
            instanceValues.Add(new StringMetricValue("instanceId", instancePlugin.instanceIds));
            instanceValues.Add(new IntRangeMetricValue("playerCount", 1, this.worldCapacity));
            matrix.Add(Metric.Instance, instanceValues);

            /*
            List<MetricValue> Values = new List<MetricValue>();
            matrix.Add(Metric., Values);
            */

            return matrix;
        }

        private void FillInstanceIds()
        {
            List<string> instanceIds = InstanceIdGenerator.GenerateInstanceIds(this.instanceCapacity, 4);
            this.instancePlugin.instanceIds = instanceIds.ToArray();
        }

        public override void OnInspectorGUI()
        {
            this.ui = (MetricsUI)target;
            this.urlStore = this.ui.GetComponentInChildren<URLStore>();
            this.instancePlugin = this.ui.GetComponentInChildren<InstancePlugin>();

            this.metricsServerBaseUrl = EditorGUILayout.TextField("Metrics server base URL:", this.metricsServerBaseUrl);
            this.worldCapacity = EditorGUILayout.IntField("World capacity", this.worldCapacity);
            this.instanceCapacity = EditorGUILayout.IntField("Instance capacity", this.instanceCapacity);

            if (this.urlStore != null && this.Button("Save"))
            {
                this.SaveUrls();
            }
        }

        private VRCUrl MakeUrl(string metricName, Dictionary<string, string> metricData)
        {
            string query = "?";

            foreach (KeyValuePair<string, string> kvp in metricData)
            {
                query += $"{kvp.Key}={kvp.Value}&";
            }

            if (query == "?") return new VRCUrl($"{this.metricsServerBaseUrl}/api/v1/metrics/ingest/{metricName}");

            return new VRCUrl($"{this.metricsServerBaseUrl}/api/v1/metrics/ingest/{metricName}{query}");
        }

        private int progress = 0;
        private int total = 0;
        private int metricProgress = 0;
        private int metricTotal = 0;

        private void GetCombinationsRec<T>(IList<IEnumerable<T>> sources, T[] chain, int index, ICollection<T[]> combinations)
        {
            foreach (var element in sources[index])
            {
                chain[index] = element;
                if (index == sources.Count - 1)
                {
                    this.progress++;
                    EditorUtility.DisplayProgressBar($"Generating combinations ({this.metricProgress} of {this.metricTotal})...", $"{progress}/{this.total}", (float)progress / this.total);

                    var finalChain = new T[chain.Length];
                    chain.CopyTo(finalChain, 0);
                    combinations.Add(finalChain);
                }
                else
                {
                    this.GetCombinationsRec(sources: sources, chain: chain, index: index + 1, combinations: combinations);
                }
            }
        }

        public List<T[]> GetCombinations<T>(IEnumerable<T>[] enumerables)
        {
            var combinations = new List<T[]>(enumerables.Length);
            if (enumerables.Length > 0)
            {
                var chain = new T[enumerables.Length];
                this.GetCombinationsRec(sources: enumerables, chain: chain, index: 0, combinations: combinations);
            }

            EditorUtility.ClearProgressBar();
            return combinations;
        }

        private void SaveUrls()
        {
            this.FillInstanceIds();

            Dictionary<Metric, List<MetricValue>> matrix = this.GenerateMatrix();
            Dictionary<Metric, List<ResolvedMetricValue[]>> namedCombinations = new Dictionary<Metric, List<ResolvedMetricValue[]>>();

            this.metricProgress = 0;
            this.metricTotal = matrix.Count;

            foreach (KeyValuePair<Metric, List<MetricValue>> pair in matrix)
            {
                this.progress = 0;
                this.total = 1;
                this.metricProgress++;

                List<List<ResolvedMetricValue>> input = new List<List<ResolvedMetricValue>>();

                foreach (MetricValue value in pair.Value)
                {
                    List<ResolvedMetricValue> resolvedValues = ResolvedMetricValue.FromMetricValue(value);

                    this.total *= resolvedValues.Count;
                    input.Add(resolvedValues);
                }

                List<ResolvedMetricValue[]> result = this.GetCombinations(input.ToArray());
                namedCombinations.Add(pair.Key, result);
            }

            // Done generating combinations, now we need to transform it to the format vrc needs

            List<object[]> urls = new List<object[]>();

            foreach (KeyValuePair<Metric, List<ResolvedMetricValue[]>> kvp in namedCombinations)
            {
                foreach (ResolvedMetricValue[] resolvedValues in kvp.Value)
                {
                    Dictionary<string, string> urlParams = new Dictionary<string, string>();

                    foreach (ResolvedMetricValue value in resolvedValues)
                    {
                        if (value.name != "" && value.value != "") urlParams.Add(value.name, value.value);
                    }

                    List<object[]> vrcParams = new List<object[]>();

                    foreach (ResolvedMetricValue value in resolvedValues)
                    {
                        vrcParams.Add(new object[] { value.name, value.value });
                    }

                    object[] data = new object[] { kvp.Key, vrcParams.ToArray() };
                    object[] item = new object[] { data, this.MakeUrl(kvp.Key.ToString(), urlParams) };

                    urls.Add(item);
                }
            }

            this.urlStore.urls = urls.ToArray();
        }
    }
}
