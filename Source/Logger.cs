﻿using System;
using System.Diagnostics.CodeAnalysis;
using Verse;

namespace LordKuper.Common
{
    /// <summary>
    ///     Provides logging utilities for error, warning, and informational messages,
    ///     with support for mod identification and exception details.
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class Logger
    {
        /// <summary>
        ///     Appends the exception message to the provided message, if the exception is not null and has a message.
        /// </summary>
        /// <param name="message">The base message.</param>
        /// <param name="exception">The exception to append.</param>
        /// <returns>The combined message.</returns>
        private static string AppendExceptionMessage(string message, Exception exception)
        {
            if (exception != null && !exception.Message.NullOrEmpty())
                return $"{message}{Environment.NewLine}{exception.Message}";
            return message;
        }

        /// <summary>
        ///     Logs an error message with the specified mod ID and optional exception.
        /// </summary>
        /// <param name="modId">The mod identifier.</param>
        /// <param name="message">The error message.</param>
        /// <param name="exception">The exception to log (optional).</param>
        public static void LogError(string modId, string message, Exception exception = null)
        {
            Log.Error($"{modId}: {AppendExceptionMessage(message, exception)}");
        }

        /// <summary>
        ///     Logs an error message using the default mod ID and optional exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="exception">The exception to log (optional).</param>
        internal static void LogError(string message, Exception exception = null)
        {
            LogError(CommonMod.ModId, message, exception);
        }

        /// <summary>
        ///     Logs a message with the specified mod ID.
        /// </summary>
        /// <param name="modId">The mod identifier.</param>
        /// <param name="message">The message to log.</param>
        public static void LogMessage(string modId, string message)
        {
            Log.Message($"{modId}: {message}");
        }

        /// <summary>
        ///     Logs a message using the default mod ID.
        /// </summary>
        /// <param name="message">The message to log.</param>
        internal static void LogMessage(string message)
        {
            LogMessage(CommonMod.ModId, message);
        }

        /// <summary>
        ///     Logs a warning message with the specified mod ID and optional exception.
        /// </summary>
        /// <param name="modId">The mod identifier.</param>
        /// <param name="message">The warning message.</param>
        /// <param name="exception">The exception to log (optional).</param>
        public static void LogWarning(string modId, string message, Exception exception = null)
        {
            Log.Warning($"{modId} : {AppendExceptionMessage(message, exception)}");
        }

        /// <summary>
        ///     Logs a warning message using the default mod ID and optional exception.
        /// </summary>
        /// <param name="message">The warning message.</param>
        /// <param name="exception">The exception to log (optional).</param>
        internal static void LogWarning(string message, Exception exception = null)
        {
            LogWarning(CommonMod.ModId, message, exception);
        }
    }
}