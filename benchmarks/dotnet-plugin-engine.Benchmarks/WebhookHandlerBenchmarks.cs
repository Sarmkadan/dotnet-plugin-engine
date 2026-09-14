using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using PluginEngine.Webhooks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace dotnet_plugin_engine.Benchmarks
{
    /// <summary>
    /// Benchmarks for measuring the performance of WebhookHandler operations.
    /// </summary>
    [MemoryDiagnoser]
    public class WebhookHandlerBenchmarks
    {
        private WebhookHandler _webhookHandler;
        private Dictionary<string, object> _payload;

        /// <summary>
        /// Sets up the benchmark by creating a WebhookHandler instance and initializing a test payload.
        /// </summary>
        [GlobalSetup]
        public void Setup()
        {
            _webhookHandler = new WebhookHandler();
            _payload = new Dictionary<string, object>
            {
                ["event"] = "test_event",
                ["timestamp"] = DateTime.UtcNow,
                ["data"] = new Dictionary<string, object>
                {
                    ["key1"] = "value1",
                    ["key2"] = 42
                }
            };
        }

        /// <summary>
        /// Measures the performance of processing a webhook payload.
        /// </summary>
        [Benchmark]
        public async Task ProcessWebhookAsync_Benchmark()
        {
            await _webhookHandler.ProcessWebhookAsync(_payload);
        }

        /// <summary>
        /// Measures the performance of validating webhook signatures.
        /// </summary>
        [Benchmark]
        public bool ValidateSignature_Benchmark()
        {
            return _webhookHandler.ValidateSignature(_payload, "test-secret", "test-signature");
        }

        /// <summary>
        /// Cleans up resources after each benchmark iteration.
        /// </summary>
        [GlobalCleanup]
        public void Cleanup()
        {
            _webhookHandler?.Dispose();
            _payload = null;
        }
    }
}