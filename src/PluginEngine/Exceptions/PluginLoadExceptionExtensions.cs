using System;

namespace PluginEngine.Exceptions
{
	/// <summary>
	/// Provides extension methods for <see cref="PluginLoadException"/> to facilitate common operations
	/// and improve exception handling workflows.
	/// </summary>
	public static class PluginLoadExceptionExtensions
	{
		/// <summary>
		/// Determines whether the exception occurred during the specified load stage.
		/// </summary>
		/// <param name="exception">The exception to check. Cannot be null.</param>
		/// <param name="stage">The load stage to compare against.</param>
		/// <returns><see langword="true"/> if the exception's load stage matches the specified stage; otherwise, <see langword="false"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/>.</exception>
		public static bool IsLoadStage(this PluginLoadException exception, PluginLoadStage stage)
		{
			ArgumentNullException.ThrowIfNull(exception);
			return exception.LoadStage == stage;
		}

		/// <summary>
		/// Generates a human-readable summary of the plugin load failure.
		/// </summary>
		/// <param name="exception">The exception containing load failure details. Cannot be null.</param>
		/// <returns>A formatted string describing the load failure.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/>.</exception>
		public static string GetLoadFailureSummary(this PluginLoadException exception)
		{
			ArgumentNullException.ThrowIfNull(exception);

			return $"Failed to load plugin '{exception.PluginName}' from '{exception.AssemblyPath}' at stage {exception.LoadStage}.";
		}

		/// <summary>
		/// Creates a new <see cref="PluginLoadException"/> with the specified load stage while preserving
		/// the original exception's message, plugin name, and assembly path.
		/// </summary>
		/// <param name="exception">The original exception. Cannot be null.</param>
		/// <param name="stage">The new load stage to assign.</param>
		/// <returns>A new exception instance with the updated load stage.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/>.</exception>
		public static PluginLoadException WithLoadStage(this PluginLoadException exception, PluginLoadStage stage)
		{
			ArgumentNullException.ThrowIfNull(exception);

			// Creating a new exception to avoid changing the original exception's state
			return new PluginLoadException(exception.Message, exception.PluginName, exception.AssemblyPath, stage, exception.InnerException);
		}
	}
}