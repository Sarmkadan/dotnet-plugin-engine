using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PluginEngine.Results
{
    /// <summary>
    /// Provides extension methods for working with <see cref="PluginOperationResult"/> and <see cref="PluginOperationResult{T}"/> types.
    /// </summary>
    public static class PluginOperationResultExtensions
    {
        /// <summary>
        /// Creates a new <see cref="PluginBatchOperationResult"/> from a collection of plugin operation results.
        /// </summary>
        /// <param name="results">The collection of plugin operation results to convert to a batch.</param>
        /// <returns>A new <see cref="PluginBatchOperationResult"/> containing all operations.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="results"/> is <see langword="null"/>.</exception>
        public static PluginBatchOperationResult ToBatchResult(this IEnumerable<PluginOperationResult> results)
        {
            ArgumentNullException.ThrowIfNull(results);

            var batch = new PluginBatchOperationResult();
            var resultList = results.ToList();

            foreach (var result in resultList)
            {
                batch.AddResult(Guid.Empty, "Operation", result);
            }

            batch.TotalDurationMs = resultList.Sum(r => r.DurationMs);
            return batch;
        }

        /// <summary>
        /// Creates a new <see cref="PluginBatchOperationResult"/> from a collection of plugin operation results with plugin identifiers.
        /// </summary>
        /// <param name="results">The collection of plugin operation results with identifiers.</param>
        /// <returns>A new <see cref="PluginBatchOperationResult"/> containing all operations.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="results"/> is <see langword="null"/>.</exception>
        public static PluginBatchOperationResult ToBatchResult(this IEnumerable<(Guid PluginId, string PluginName, PluginOperationResult Result)> results)
        {
            ArgumentNullException.ThrowIfNull(results);

            var batch = new PluginBatchOperationResult();
            var resultList = results.ToList();

            foreach (var (pluginId, pluginName, result) in resultList)
            {
                batch.AddResult(pluginId, pluginName, result);
            }

            batch.TotalDurationMs = resultList.Sum(r => r.Result.DurationMs);
            return batch;
        }

        /// <summary>
        /// Determines whether the operation result contains any failures.
        /// </summary>
        /// <param name="result">The plugin operation result to check.</param>
        /// <returns>True if the operation failed or contains failures; otherwise false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
        public static bool HasFailures(this PluginOperationResult result)
        {
            ArgumentNullException.ThrowIfNull(result);

            return !result.Success || result.ErrorCode.HasValue;
        }

        /// <summary>
        /// Gets a detailed summary of the plugin operation including success/failure statistics.
        /// </summary>
        /// <param name="result">The plugin operation result to summarize.</param>
        /// <returns>A formatted string containing the summary information.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
        public static string GetDetailedSummary(this PluginOperationResult result)
        {
            ArgumentNullException.ThrowIfNull(result);

            var builder = new StringBuilder();
            builder.AppendLine($"Operation Summary:");
            builder.AppendLine($"  Status: {(result.Success ? "SUCCESS" : "FAILURE")}");
            builder.AppendLine($"  Duration: {result.DurationMs}ms");
            builder.AppendLine($"  Timestamp: {result.TimestampUtc:yyyy-MM-dd HH:mm:ss UTC}");

            if (!string.IsNullOrEmpty(result.Message))
            {
                builder.AppendLine($"  Message: {result.Message}");
            }

            if (result.HasFailures())
            {
                builder.AppendLine($"  Error Code: {result.ErrorCode ?? 0}");
                if (!string.IsNullOrEmpty(result.ErrorDetails))
                {
                    builder.AppendLine($"  Error Details: {result.ErrorDetails}");
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Converts a generic plugin operation result to a non-generic result.
        /// </summary>
        /// <typeparam name="T">The data type contained in the result.</typeparam>
        /// <param name="result">The generic plugin operation result to convert.</param>
        /// <returns>A new non-generic <see cref="PluginOperationResult"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
        public static PluginOperationResult ToNonGeneric(this PluginOperationResult result)
        {
            ArgumentNullException.ThrowIfNull(result);

            return new PluginOperationResult
            {
                Success = result.Success,
                Message = result.Message,
                ErrorCode = result.ErrorCode,
                ErrorDetails = result.ErrorDetails,
                DurationMs = result.DurationMs
            };
        }
    }
}
