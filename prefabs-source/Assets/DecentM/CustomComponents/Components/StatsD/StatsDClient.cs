using System;
using System.Text;
using System.Linq;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using UnityEngine;

using DecentM.Shared;

namespace DecentM.Mods.CustomComponents.StatsD
{
    enum StatsDMetric
    {
        Gauge,
        Counter,
        Timer,
        Histogram,
        Meter,
    }

    public class StatsDClient : MonoBehaviour
    {
        private Queue<string> sendQueue = new Queue<string>();
        private UdpClient udp;
        public Dictionary<string, string> defaultTags;

        public string host = string.Empty;
        public int port = 8125;

        public float batchIntervalSeconds = 1;
        public float defaultSampleRate = 1;

        public int maxPayloadSize = 512;

        private float elapsed = 0;

        #region Internals

        private void FixedUpdate()
        {
            this.elapsed += Time.fixedDeltaTime;
            if (this.elapsed < this.batchIntervalSeconds)
                return;
            this.elapsed = 0;

            this.TickQueue();
        }

        private void Awake()
        {
            if (this.defaultTags == null)
                this.defaultTags = new Dictionary<string, string>
                {
                    { "app_unity_version", Application.unityVersion },
                    { "app_version", Application.version },
                    { "app_product_name", Application.productName },
                    { "app_platform", Application.platform.ToString() },
                };

            this.udp = new UdpClient(this.port);
            this.udp.Connect(this.host, this.port);
        }

        private void OnDestroy()
        {
            this.udp.Dispose();
            this.udp = null;
        }

        private void QueueMetric(StatsDMetric metric, string name, float value, Dictionary<string, string> tags, float sampleRate)
        {
            Dictionary<string, string> allTags = tags.Union(this.defaultTags).ToDictionary(pair => pair.Key, pair => pair.Value);

            StringBuilder sb = new StringBuilder();

            sb.Append(name);
            sb.Append(':');
            sb.Append(value);
            sb.Append('|');

            switch (metric)
            {
                case StatsDMetric.Gauge:
                    sb.Append('g');
                    break;

                case StatsDMetric.Counter:
                    sb.Append('c');
                    break;

                case StatsDMetric.Timer:
                    sb.Append("ms");
                    break;

                case StatsDMetric.Histogram:
                    sb.Append('h');
                    break;

                case StatsDMetric.Meter:
                    sb.Append('m');
                    break;

                default:
                    break;
            }

            sb.Append('|');
            sb.Append('@');
            sb.Append(sampleRate);

            if (tags.Count > 0)
            {
                sb.Append("|#");
                
                foreach (KeyValuePair<string, string> tag in allTags)
                {
                    if (string.IsNullOrEmpty(tag.Value))
                        sb.Append($"{tag.Key}");
                    else
                        sb.Append($"{tag.Key}:{tag.Value}");

                    sb.Append('|');
                }
            }

            this.sendQueue.Enqueue(sb.ToString());
        }

        private void TickQueue()
        {
            if (this.sendQueue.Count < 1)
                return;

            StringBuilder sb = new StringBuilder();

            while (sb.Length < this.maxPayloadSize && this.sendQueue.Count > 0)
            {
                sb.Append(this.sendQueue.Dequeue());
                sb.Append('\n');
            }

            string packet = sb.ToString();
            byte[] sendBytes = Encoding.ASCII.GetBytes(packet);

            if (this.udp != null)
                this.udp.Send(sendBytes, sendBytes.Length);

            UnityEngine.Debug.Log($"[StatsDClient] Sent {sendBytes.Length} bytes to {host}:{port}. Queue size: {this.sendQueue.Count}\nMessage: {packet}");
        }

        #endregion

        #region Instrumentation

        public void MeasureSync(string name, Action action)
        {
            Stopwatch sw = Stopwatch.StartNew();

            action();
            sw.Stop();

            this.Timer(name, sw.ElapsedMilliseconds, new Dictionary<string, string>
            {
                { "measurement", "ms" },
            });
        }

        public void MeasureTask(string name, Task task)
        {
            Stopwatch sw = Stopwatch.StartNew();

            Parallelism.WaitForTask(task, (success) =>
            {
                sw.Stop();

                this.Timer(name, sw.ElapsedMilliseconds, new Dictionary<string, string>
                {
                    { "measurement", "ms" },
                });
            });
        }

        public void MeasureCallback(string name, Action<Action> callbackReceiver)
        {
            Stopwatch sw = Stopwatch.StartNew();

            callbackReceiver(() =>
            {
                sw.Stop();

                this.Timer(name, sw.ElapsedMilliseconds, new Dictionary<string, string>
                {
                    { "measurement", "ms" },
                });
            });
        }

#endregion

#region Dumb metrics

        public void Gauge(string name, float value, Dictionary<string, string> tags, float sampleRate = 1f)
        {
            this.QueueMetric(StatsDMetric.Gauge, name, value, tags, sampleRate);
        }

        public void Gauge(string name, float value, float sampleRate = 1f)
        {
            this.QueueMetric(StatsDMetric.Gauge, name, value, new Dictionary<string, string> { }, sampleRate);
        }

        public void Counter(string name, float value, Dictionary<string, string> tags, float sampleRate = 1f)
        {
            this.QueueMetric(StatsDMetric.Counter, name, value, tags, sampleRate);
        }

        public void Counter(string name, float value, float sampleRate = 1f)
        {
            this.QueueMetric(StatsDMetric.Counter, name, value, new Dictionary<string, string> { }, sampleRate);
        }

        public void Timer(string name, float value, Dictionary<string, string> tags, float sampleRate = 1f)
        {
            this.QueueMetric(StatsDMetric.Timer, name, value, tags, sampleRate);
        }

        public void Timer(string name, float value, float sampleRate = 1f)
        {
            this.QueueMetric(StatsDMetric.Timer, name, value, new Dictionary<string, string> { }, sampleRate);
        }

        public void Histogram(string name, float value, Dictionary<string, string> tags, float sampleRate = 1f)
        {
            this.QueueMetric(StatsDMetric.Histogram, name, value, tags, sampleRate);
        }

        public void Histogram(string name, float value, float sampleRate = 1f)
        {
            this.QueueMetric(StatsDMetric.Histogram, name, value, new Dictionary<string, string> { }, sampleRate);
        }

        public void Meter(string name, float value, Dictionary<string, string> tags, float sampleRate = 1f)
        {
            this.QueueMetric(StatsDMetric.Meter, name, value, tags, sampleRate);
        }

        public void Meter(string name, float value, float sampleRate = 1f)
        {
            this.QueueMetric(StatsDMetric.Meter, name, value, new Dictionary<string, string> { }, sampleRate);
        }

#endregion
    }
}
