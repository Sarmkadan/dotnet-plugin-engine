using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using PluginEngine.Exceptions;
using System;
using System.Collections.Generic;

namespace dotnet_plugin_engine.Benchmarks
{
    [MemoryDiagnoser]
    public class PluginExceptionBenchmarks
    {
        private PluginException _exception;
        private Dictionary<string, object> _context;
        private Guid _entityId;

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

        [Benchmark]
        [Params(10, 100, 1000)]
        public void WithContext_Benchmark(int size)
        {
            for (int i = 0; i < size; i++)
            {
                _exception.WithContext($"Key{i}", $"Value{i}");
            }
        }

        [Benchmark]
        public void WithEntityId_Benchmark()
        {
            for (int i = 0; i < 1000; i++)
            {
                _exception.WithEntityId(_entityId);
            }
        }

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
