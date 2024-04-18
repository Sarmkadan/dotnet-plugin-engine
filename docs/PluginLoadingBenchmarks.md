# PluginLoadingBenchmarks

`PluginLoadingBenchmarks` is a benchmarking suite designed to measure the performance characteristics of the `dotnet-plugin-engine` plugin loading infrastructure. It evaluates various scenarios such as loading individual plugins, concurrent loading, dependency resolution, and unloading/reloading operations. The benchmarks are intended for use with performance testing frameworks like BenchmarkDotNet to assess throughput, latency, and resource utilization under different plugin loading conditions.

## API

### `GlobalSetup`
