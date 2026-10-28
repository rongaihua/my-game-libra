//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-28
// Desc: 日志门面
//---------------------------------------------------------------------------------------

using System;

namespace MyGame
{
    /// <summary>
    /// 日志门面。未初始化时全部丢弃，不会抛空引用。
    /// 消息一律由调用方用字符串插值拼好，不提供 format 参数。
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// 实际输出实现。
        /// </summary>
        private static ILogger sLogger;

        /// <summary>
        /// 是否已经初始化。
        /// </summary>
        public static bool IsInited => sLogger != null;

        /// <summary>
        /// 设置输出实现。
        /// </summary>
        /// <param name="logger">输出实现。</param>
        public static void Init(ILogger logger)
        {
            sLogger = logger;
        }

        /// <summary>
        /// 清空输出实现。
        /// </summary>
        public static void Shutdown()
        {
            sLogger = null;
        }

        /// <summary>
        /// 调试日志。
        /// </summary>
        /// <param name="message">内容。</param>
        public static void Debug(string message)
        {
            sLogger?.Debug(message);
        }

        /// <summary>
        /// 普通信息。
        /// </summary>
        /// <param name="message">内容。</param>
        public static void Info(string message)
        {
            sLogger?.Info(message);
        }

        /// <summary>
        /// 警告。
        /// </summary>
        /// <param name="message">内容。</param>
        public static void Warn(string message)
        {
            sLogger?.Warn(message);
        }

        /// <summary>
        /// 错误。
        /// </summary>
        /// <param name="message">内容。</param>
        public static void Error(string message)
        {
            sLogger?.Error(message);
        }

        /// <summary>
        /// 带异常堆栈的错误。
        /// </summary>
        /// <param name="exception">异常。</param>
        /// <param name="message">内容。</param>
        public static void Exception(Exception exception, string message)
        {
            sLogger?.Exception(exception, message);
        }
    }
}
