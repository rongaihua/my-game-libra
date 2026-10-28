//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-28
// Desc: 日志接口
//---------------------------------------------------------------------------------------

using System;

namespace MyGame
{
    /// <summary>
    /// 日志接口。对照 ChatRoom 的 <c>ILogger</c>，去掉 format 参数，统一用字符串插值。
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// 调试日志。
        /// </summary>
        /// <param name="message">内容。</param>
        void Debug(string message);

        /// <summary>
        /// 普通信息。
        /// </summary>
        /// <param name="message">内容。</param>
        void Info(string message);

        /// <summary>
        /// 警告。
        /// </summary>
        /// <param name="message">内容。</param>
        void Warn(string message);

        /// <summary>
        /// 错误。
        /// </summary>
        /// <param name="message">内容。</param>
        void Error(string message);

        /// <summary>
        /// 带异常堆栈的错误。
        /// </summary>
        /// <param name="exception">异常。</param>
        /// <param name="message">内容。</param>
        void Exception(Exception exception, string message);
    }
}
