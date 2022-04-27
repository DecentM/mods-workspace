using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UdonSharp;
using DecentM.EditorTools;
using VRC.SDKBase;

namespace DecentM.Metrics
{
    public class MetricValues
    {
        public MetricValues(string name, string[] values)
        {
            this.Name = name;
            this.Values = values;
        }

        public MetricValues(string name)
        {
            this.Name = name;
            this.Values = new string[] { "true", "false" };
        }

        public MetricValues(string name, int min, int max)
        {
            this.Name = name;
            this.Values = new string[max - min];

            for (int i = 0; i < this.Values.Length; i++)
            {
                this.Values[i] = $"{min + i}";
            }
        }

        public string Name;
        public string[] Values;
    }

    public struct MetricValue
    {
        public MetricValue(Metric metric, string name, string value)
        {
            this.metric = metric;
            this.name = name;
            this.value = value;
        }

        public Metric metric;
        public string name;
        public string value;
    }

    [CustomEditor(typeof(MetricsUI))]
    public class MetricsUIInspector : DEditor
    {
        MetricsUI ui;
        URLStore urlStore;

        private string metricsServerBaseUrl = "http://localhost:3000";
        private int worldCapacity = 64;

        private Dictionary<Metric, List<MetricValues>> GenerateMatrix()
        {
            Dictionary<Metric, List<MetricValues>> matrix = new Dictionary<Metric, List<MetricValues>>();

            matrix.Clear();

            List<MetricValues> metricPossibleValues = new List<MetricValues>();
            metricPossibleValues.Add(new MetricValues("isMaster"));
            metricPossibleValues.Add(new MetricValues("isVr"));
            metricPossibleValues.Add(new MetricValues("isFbt"));
            // metricPossibleValues.Add(new MetricValues("timezone", -11, 12));
            metricPossibleValues.Add(new MetricValues("vrPlatform", new string[] { "index", "vive", "oculus", "quest-standalone" }));

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

        private List<List<string>> CreateCombinations(int startIndex, List<string> pair, List<string> initialArray)
        {
            List<List<string>> combinations = new List<List<string>>();
            for (int i = startIndex; i < initialArray.Count; i++)
            {
                List<string> value = pair.GetRange(0, pair.Count);
                value.Add(initialArray[i]);
                combinations.Add(value);
                combinations.AddRange(this.CreateCombinations(i + 1, value, initialArray));
            }

            return combinations;
        }

        private int progress = 0;
        private int total = 1;

        private List<List<MetricValue>> CreateCombinations(int startIndex, List<MetricValue> pair, List<MetricValue> initialArray, int depth)
        {
            List<List<MetricValue>> combinations = new List<List<MetricValue>>();
            for (int i = startIndex; i < initialArray.Count; i++)
            {
                List<MetricValue> value = pair.GetRange(0, pair.Count);
                value.Add(initialArray[i]);
                combinations.Add(value);
                combinations.AddRange(this.CreateCombinations(i + 1, value, initialArray, depth + 1));

                this.progress++;
                EditorUtility.DisplayProgressBar($"Creating combinations... ({this.progress}/{this.total})", "", 1f * this.progress / this.total);
            }

            if (depth == 0) EditorUtility.ClearProgressBar();

            return combinations;
        }

        private void SaveUrls()
        {
            Dictionary<Metric, List<MetricValues>> matrix = this.GenerateMatrix();
            Dictionary<Metric, List<List<MetricValue>>> namedCombinations = new Dictionary<Metric, List<List<MetricValue>>>();

            foreach (Metric metric in matrix.Keys)
            {
                List<MetricValue> expandedValues = new List<MetricValue>();

                List<MetricValues> values;
                bool success = matrix.TryGetValue(metric, out values);
                if (!success) continue;

                foreach (MetricValues possibleValue in values)
                {
                    foreach (string metricValue in possibleValue.Values)
                    {
                        MetricValue value = new MetricValue(metric, possibleValue.Name, metricValue);
                        expandedValues.Add(value);
                    }
                }

                List<List<MetricValue>> combinations = this.CreateCombinations(0, new List<MetricValue>(), expandedValues, 0);
                namedCombinations.Add(metric, combinations);
            }

            foreach (KeyValuePair<Metric, List<List<MetricValue>>> kvp in namedCombinations)
            {
                Debug.Log($"{kvp.Key}, {kvp.Value.Count}");
                foreach (List<MetricValue> combination in kvp.Value)
                {
                    Debug.Log($"{string.Join(", ", combination.ToArray())}");
                    this.total += combination.Count * kvp.Value.Count;
                }
            }

            /* foreach (Metric metric in this.matrix.Keys)
            {
                List<MetricValues> values;
                bool success = this.matrix.TryGetValue(metric, out values);
                if (!success) continue;

                foreach (MetricValues possibleValue in values)
                {
                    foreach (string metricValue in possibleValue.Values)
                    {
                        VRCUrl url = this.MakeUrl(possibleValue.Name, metricValue);
                        object[] item = new object[] { new object[] { metric, metricValue }, url };
                        urls.Add(item);
                        Debug.Log(url.ToString());
                    }
                }
            } */

            // List<object[]> urls = new List<object[]>();
            List<VRCUrl> urls = new List<VRCUrl>();

            foreach (KeyValuePair<Metric, List<List<MetricValue>>> kvp in namedCombinations)
            {
                // Generate a URL for each combination
                foreach (List<MetricValue> combination in kvp.Value)
                {
                    Dictionary<string, string> urlValues = new Dictionary<string, string>();

                    foreach (MetricValue metricValue in combination)
                    {
                        Debug.Log($"{metricValue.name}, {metricValue.value}");
                        // urlValues.Add(metricValue.name, metricValue.value);
                    }

                    object[] item = new object[]
                    {
                        // TODO: Add the metric data in here so it can be searched for later
                        new object[] { kvp.Key },
                        this.MakeUrl(kvp.Key.ToString(), urlValues)
                    };

                    urls.Add(this.MakeUrl(kvp.Key.ToString(), urlValues));
                }
            }

            Debug.Log($"Generated {urls.Count} urls");

            this.urlStore.debugUrls = urls.ToArray();

            /* foreach (object[] item in urls)
            {
                Debug.Log(item[1].ToString());
            } */

            // this.urlStore.respawnUrl = this.MakeUrl("respawn");
            // this.urlStore.heartbeatUrl = this.MakeUrl("heartbeat");

            /* VRCUrl[] playerCountUrls = new VRCUrl[this.worldCapacity + 1];

            for (int i = 0; i < playerCountUrls.Length; i++)
            {
                playerCountUrls[i] = this.MakeUrl("player-count", i.ToString());
            } */

            // this.urlStore.playerCountUrls = playerCountUrls;
        }
    }
}
