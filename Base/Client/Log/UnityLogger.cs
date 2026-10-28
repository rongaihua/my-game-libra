//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-28
// Desc: 基于 UnityEngine.Debug 的日志实现
//---------------------------------------------------------------------------------------

using System;

namespace MyGame
{
    /// <summary>
    /// 客户端日志实现：转给 <see cref="UnityEngine.Debug"/>，保留 Unity 控制台的堆栈定位。
    /// </summary>
    public sealed class UnityLogger : ILogger
    {
        /// <summary>
        /// 日志前缀，方便在控制台按工程过滤。
        /// </summary>
        public string Prefix { get; set; } = "[MyGame]";

        /// <summary>
        /// 是否输出调试日志。正式包可关掉。
        /// </summary>
        public bool EnableDebug { get; set; } = true;

        /// <summary>
        /// 调试日志。
        /// </summary>
        /// <param name="message">内容。</param>
        public void Debug(string message)
        {
            if (!EnableDebug)
            {
                return;
            }

            UnityEngine.Debug.Log($"{Prefix} {message}");
        }

        /// <summary>
        /// 普通信息。
        /// </summary>
        /// <param name="message">内容。</param>
        public void Info(string message)
        {
            UnityEngine.Debug.Log($"{Prefix} {message}");
        }

        /// <summary>
        /// 警告。
        /// </summary>
        /// <param name="message">内容。</param>
        public void Warn(string message)
        {
            UnityEngine.Debug.LogWarning($"{Prefix} {message}");
        }

        /// <summary>
        /// 错误。
        /// </summary>
        /// <param name="message">内容。</param>
        public void Error(string message)
        {
            UnityEngine.Debug.LogError($"{Prefix} {message}");
        }

        /// <summary>
        /// 带异常堆栈的错误。
        /// </summary>
        /// <param name="exception">异常。</param>
        /// <param name="message">内容。</param>
        public void Exception(Exception exception, string message)
        {
            UnityEngine.Debug.LogError($"{Prefix} {message}\n{exception}");
        }
    }
}
