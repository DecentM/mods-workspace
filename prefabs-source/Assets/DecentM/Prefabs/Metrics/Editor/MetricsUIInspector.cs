using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UdonSharp;
using DecentM.EditorTools;
using VRC.SDKBase;

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

        public string name;
        public string value;
    }

    public class BoolMetricValue : MetricValue
    {
        public BoolMetricValue(string name)
        {
            this.name = name;
        }

        public override string[] GetPossibleValues()
        {
            return new string[] { "true", "false" };
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

        private string metricsServerBaseUrl = "http://localhost:3000";
        private int worldCapacity = 64;

        private Dictionary<Metric, List<MetricValue>> GenerateMatrix()
        {
            Dictionary<Metric, List<MetricValue>> matrix = new Dictionary<Metric, List<MetricValue>>();

            matrix.Clear();

            List<MetricValue> metricPossibleValues = new List<MetricValue>();
            metricPossibleValues.Add(new BoolMetricValue("isMaster"));
            metricPossibleValues.Add(new BoolMetricValue("isVr"));
            metricPossibleValues.Add(new BoolMetricValue("isFbt"));
            metricPossibleValues.Add(new IntRangeMetricValue("timezone", -11, 12));
            metricPossibleValues.Add(new StringMetricValue("vrPlatform", new string[] { "index", "vive", "oculus", "quest-standalone" }));

            matrix.Add(Metric.Heartbeat, metricPossibleValues);

            return matrix;
        }

        public override void OnInspectorGUI()
        {
            this.ui = (MetricsUI)target;
            this.urlStore = this.ui.GetComponentInChildren<URLStore>();

            this.metricsServerBaseUrl = EditorGUILayout.TextField("Metrics server base URL:", this.metricsServerBaseUrl);
            this.worldCapacity = EditorGUILayout.IntField("World capacity", this.worldCapacity);

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

            return new VRCUrl($"{this.metricsServerBaseUrl}/api/v1/metrics/ingest/{metricName}{query}");
        }

        private int progress = 0;
        private int total = 0;

        private void GetCombinationsRec<T>(IList<IEnumerable<T>> sources, T[] chain, int index, ICollection<T[]> combinations)
        {
            foreach (var element in sources[index])
            {
                chain[index] = element;
                if (index == sources.Count - 1)
                {
                    this.progress++;
                    EditorUtility.DisplayProgressBar($"Generating combinations... ({progress}/{this.total})", "Skipping...", (float)progress / this.total);

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
            Dictionary<Metric, List<MetricValue>> matrix = this.GenerateMatrix();

            this.total = 1;

            foreach (KeyValuePair<Metric, List<MetricValue>> pair in matrix)
            {
                List<List<string>> input = new List<List<string>>();

                foreach (MetricValue value in pair.Value)
                {
                    string[] possibleValues = value.GetPossibleValues();
                    this.total *= possibleValues.Length;
                    input.Add(possibleValues.ToList());
                }

                List<string[]> result = this.GetCombinations(input.ToArray());

                foreach (string[] strings in result)
                {
                    Debug.Log(string.Join(", ", strings));
                }
            }
        }
    }
}
