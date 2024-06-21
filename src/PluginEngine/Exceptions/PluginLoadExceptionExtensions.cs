using System;

namespace PluginEngine.Exceptions
{
	/// <summary>
	/// Provides extension methods for <see cref="PluginLoadException"/> to facilitate common operations
	/// and improve exception handling workflows.
	/// </summary>
	/// <remarks>
	/// These helpers are intended to make it easier to work with <see cref="PluginLoadException"/>
	/// instances without having to repeat guard‑clause checks or manual string formatting throughout
	/// the code base.
	/// </remarks>
	public static class PluginLoadExceptionExtensions
	{
		/// <summary>
		/// Determines whether the exception occurred during the specified load stage.
		/// </summary>
		/// <param name="exception">The <see cref="PluginLoadException"/> to examine. Must not be <c>null</c>.</param>
		/// <param name="stage">The <see cref="PluginLoadStage"/> to compare against.</param>
		/// <returns>
		/// <see langword="true"/> if <c>exception.LoadStage</c> equals the supplied <paramref name="stage"/>; otherwise, <see langword="false"/>.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/>.</exception>
		public static bool IsLoadStage(this PluginLoadException exception, PluginLoadStage stage)
		{
			ArgumentNullException.ThrowIfNull(exception);
			return exception.LoadStage == stage;
		}

		/// <summary>
		/// Generates a human‑readable summary of the plugin load failure.
		/// </summary>
		/// <param name="exception">The <see cref="PluginLoadException"/> containing load failure details. Must not be <c>null</c>.</param>
		/// <returns>
		/// A formatted string describing the failure, including the plugin name, the assembly path, and the load stage at which the error occurred.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/>.</exception>
		public static string GetLoadFailureSummary(this PluginLoadException exception)
		{
			ArgumentNullException.ThrowIfNull(exception);

			return $"Failed to load plugin '{exception.PluginName}' from '{exception.AssemblyPath}' at stage {exception.LoadStage}.";
		}

		/// <summary>
		/// Creates a new <see cref="PluginLoadException"/> with the specified load stage while preserving
		/// the original exception's message, plugin name, assembly path, and inner exception.
		/// </summary>
		/// <param name="exception">The original exception to copy. Must not be <c>null</c>.</param>
		/// <param name="stage">The <see cref="PluginLoadStage"/> to assign to the new exception.</param>
		/// <returns>
		/// A new <see cref="PluginLoadException"/> instance that has the same message, plugin name,
		/// assembly path, and inner exception as <paramref name="exception"/>, but with <paramref name="stage"/> as its <c>LoadStage</c>.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/>.</exception>
		public static PluginLoadException WithLoadStage(this PluginLoadException exception, PluginLoadStage stage)
		{
			ArgumentNullException.ThrowIfNull(exception);

			// Creating a new exception to avoid changing the original exception's state
			return new PluginLoadException(exception.Message, exception.PluginName, exception.AssemblyPath, stage, exception.InnerException);
		}
	}
}
