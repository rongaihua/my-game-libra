//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-22
// Desc: 按消息类型分发的事件
//---------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace MyGame
{

    /// <summary>
    /// 按消息类型分发的事件。监听方在销毁前要 <see cref="Off{T}"/>。
    /// </summary>
    public sealed class EventMgr : Module
    {

        /// <summary>
        /// 每种消息的监听列表。
        /// </summary>
        private readonly Dictionary<Type, List<Delegate>> mHandlers = new Dictionary<Type, List<Delegate>>();

        /// <summary>
        /// 监听一种消息。
        /// </summary>
        /// <typeparam name="T">消息类型。</typeparam>
        /// <param name="handler">回调。</param>
        public void On<T>(Action<T> handler)
        {
            if (handler == null)
            {
                return;
            }

            List<Delegate> list;
            if (!mHandlers.TryGetValue(typeof(T), out list))
            {
                list = new List<Delegate>();
                mHandlers.Add(typeof(T), list);
            }

            list.Add(handler);
        }

        /// <summary>
        /// 取消一种消息的监听。
        /// </summary>
        /// <typeparam name="T">消息类型。</typeparam>
        /// <param name="handler">当初注册的回调。</param>
        public void Off<T>(Action<T> handler)
        {
            List<Delegate> list;
            if (handler == null || !mHandlers.TryGetValue(typeof(T), out list))
            {
                return;
            }

            list.Remove(handler);
        }

        /// <summary>
        /// 把消息发给当前全部监听者。
        /// </summary>
        /// <typeparam name="T">消息类型。</typeparam>
        /// <param name="message">消息。</param>
        public void Emit<T>(T message)
        {
            List<Delegate> list;
            if (!mHandlers.TryGetValue(typeof(T), out list))
            {
                return;
            }

            for (var i = list.Count - 1; i >= 0; i--)
            {
                var action = list[i] as Action<T>;
                if (action != null)
                {
                    action(message);
                }
            }
        }

        /// <summary>
        /// 清空全部监听。
        /// </summary>
        protected override void OnDestroy()
        {
            mHandlers.Clear();
        }
    }
}
