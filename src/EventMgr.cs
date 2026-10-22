using System;
using System.Collections.Generic;

namespace MyGame
{
    /// <summary>
    /// 按消息类型分发的事件。监听方在销毁前要 <see cref="Off{T}"/>。
    /// </summary>
    public sealed class EventMgr : Module
    {
        private readonly Dictionary<Type, List<Delegate>> mHandlers = new Dictionary<Type, List<Delegate>>();

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

        public void Off<T>(Action<T> handler)
        {
            List<Delegate> list;
            if (handler == null || !mHandlers.TryGetValue(typeof(T), out list))
            {
                return;
            }

            list.Remove(handler);
        }

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

        protected override void OnDestroy()
        {
            mHandlers.Clear();
        }
    }
}
