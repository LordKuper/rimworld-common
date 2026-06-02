using System;
using JetBrains.Annotations;
using Verse;

namespace LordKuper.Common;

/// <summary>
///     Provides logging utilities for error, warning, and informational messages,
///     with support for mod identification and exception details.
/// </summary>
[PublicAPI]
public static class Logger
{
    /// <summary>
    ///     Appends the exception message to the provided message, if the exception is not null and has a message.
    /// </summary>
    /// <param name="message">The base message.</param>
    /// <param name="exception">The exception to append.</param>
    /// <returns>The combined message.</returns>
    private static string AppendExceptionMessage(string message, [CanBeNull] Exception exception)
    {
        return exception != null ? $"{message}{Environment.NewLine}{exception}" : message;
    }

    /// <summary>
    ///     Logs an error message with the specified mod ID and optional exception.
    /// </summary>
    /// <param name="modId">The mod identifier.</param>
    /// <param name="message">The error message.</param>
    /// <param name="exception">The exception to log (optional).</param>
    public static void LogError([NotNull] string modId, [NotNull] string message,
        [CanBeNull] Exception exception = null)
    {
        Log.Error($"{modId}: {AppendExceptionMessage(message, exception)}");
    }

    /// <summary>
    ///     Logs an error message using the default mod ID and optional exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="exception">The exception to log (optional).</param>
    internal static void LogError([NotNull] string message, [CanBeNull] Exception exception = null)
    {
        LogError(CommonMod.ModId, message, exception);
    }

    /// <summary>
    ///     Logs a message with the specified mod ID.
    /// </summary>
    /// <param name="modId">The mod identifier.</param>
    /// <param name="message">The message to log.</param>
    public static void LogMessage([NotNull] string modId, [NotNull] string message)
    {
        Log.Message($"{modId}: {message}");
    }

    /// <summary>
    ///     Logs a message using the default mod ID.
    /// </summary>
    /// <param name="message">The message to log.</param>
    internal static void LogMessage([NotNull] string message)
    {
        LogMessage(CommonMod.ModId, message);
    }

    /// <summary>
    ///     Logs a warning message with the specified mod ID and optional exception.
    /// </summary>
    /// <param name="modId">The mod identifier.</param>
    /// <param name="message">The warning message.</param>
    /// <param name="exception">The exception to log (optional).</param>
    public static void LogWarning([NotNull] string modId, [NotNull] string message,
        [CanBeNull] Exception exception = null)
    {
        Log.Warning($"{modId}: {AppendExceptionMessage(message, exception)}");
    }

    /// <summary>
    ///     Logs a warning message using the default mod ID and optional exception.
    /// </summary>
    /// <param name="message">The warning message.</param>
    /// <param name="exception">The exception to log (optional).</param>
    internal static void LogWarning([NotNull] string message, [CanBeNull] Exception exception = null)
    {
        LogWarning(CommonMod.ModId, message, exception);
    }
}