using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using PluginEngine.Exceptions;
using System;
using System.Collections.Generic;

namespace dotnet_plugin_engine.Benchmarks
{
    [MemoryDiagnoser]
    /// <summary>
    /// Benchmarks for measuring the performance of PluginException operations.
    /// </summary>
    public class PluginExceptionBenchmarks
    {
        private PluginException _exception;
        private Dictionary<string, object> _context;
        private Guid _entityId;

        /// <summary>
        /// Sets up the benchmark by creating a PluginException instance with a test message,
        /// initializing a context dictionary with 100 entries, and generating a new Guid for entity ID.
        /// </summary>
        [GlobalSetup]
        public void Setup()
        {
            _exception = new PluginException("Test exception");
            _context = new Dictionary<string, object>();
            for (int i = 0; i < 100; i++)
            {
                _context.Add($"Key{i}", $"Value{i}");
            }
            _entityId = Guid.NewGuid();
        }

        /// <summary>
        /// Measures the performance of adding context to a PluginException with varying sizes (10, 100, 1000).
        /// </summary>
        [Benchmark]
        [Params(10, 100, 1000)]
        public void WithContext_Benchmark(int size)
        {
            for (int i = 0; i < size; i++)
            {
                _exception.WithContext($"Key{i}", $"Value{i}");
            }
        }

        /// <summary>
        /// Measures the performance of setting the EntityId on a PluginException 1000 times.
        /// </summary>
        [Benchmark]
        public void WithEntityId_Benchmark()
        {
            for (int i = 0; i < 1000; i++)
            {
                _exception.WithEntityId(_entityId);
            }
        }

        /// <summary>
        /// Measures the performance of calling ToString() on a PluginException 1000 times.
        /// </summary>
        [Benchmark]
        public void ToString_Benchmark()
        {
            for (int i = 0; i < 1000; i++)
            {
                _exception.ToString();
            }
        }
    }
}