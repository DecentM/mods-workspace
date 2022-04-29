using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.SDKBase;
using DecentM.EditorTools;

namespace DecentM.Metrics
{
    public static class MetricsUrlGenerator
    {
        private static VRCUrl MakeUrl(MetricsUI ui, string metricName, Dictionary<string, string> metricData)
        {
            string query = $"?builtAt={ui.builtAt}&unity={ui.unity}&sdk={ui.sdk}";

            foreach (KeyValuePair<string, string> kvp in metricData)
            {
                query += $"&{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}";
            }

            return new VRCUrl($"{ui.metricsServerBaseUrl}/api/v1/metrics/ingest/{metricName}{query}");
        }

        public static void ClearUrls(URLStore urlStore)
        {
            if (urlStore == null) return;

            urlStore.urls = new object[][] { };
        }

        public static void SaveUrls(MetricsUI ui, URLStore urlStore)
        {
            List<string> instanceIds = RandomStringGenerator.GenerateRandomStrings(ui.instanceCapacity, 4);
            UIManager.SetVersionData();
            InstancePluginManager.SetInstanceIds(instanceIds);

            MatrixInput matrixInput = new MatrixInput();

            matrixInput.instanceIds = instanceIds;
            matrixInput.worldCapacity = ui.worldCapacity;
            matrixInput.minFps = ui.minFps;
            matrixInput.maxFps = ui.maxFps;

            Dictionary<Metric, List<MetricValue>> matrix = MetricsMatrix.GenerateMatrix(matrixInput);

            Dictionary<Metric, List<ResolvedMetricValue[]>> namedCombinations = new Dictionary<Metric, List<ResolvedMetricValue[]>>();

            foreach (KeyValuePair<Metric, List<MetricValue>> pair in matrix)
            {
                int total = 1;

                List<List<ResolvedMetricValue>> input = new List<List<ResolvedMetricValue>>();

                foreach (MetricValue value in pair.Value)
                {
                    List<ResolvedMetricValue> resolvedValues = ResolvedMetricValue.FromMetricValue(value);

                    total *= resolvedValues.Count;
                    input.Add(resolvedValues);
                }

                CombinationsGenerator<ResolvedMetricValue> generator = new CombinationsGenerator<ResolvedMetricValue>();
                List<ResolvedMetricValue[]> result = generator.GetCombinations(input.ToArray(), total);
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

                    object[] data = new object[] { (int)kvp.Key, vrcParams.ToArray() };
                    object[] item = new object[] { data, MakeUrl(ui, kvp.Key.ToString(), urlParams) };

                    urls.Add(item);
                }
            }

            urlStore.urls = urls.ToArray();
        }
    }
}
