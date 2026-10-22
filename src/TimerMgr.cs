using System;
using System.Collections.Generic;

namespace MyGame
{
    /// <summary>
    /// 毫秒计时器。返回的 id 用于取消。
    /// </summary>
    public sealed class TimerMgr : Module
    {
        private struct Slot
        {
            public bool Active;
            public bool Repeat;
            public int LeftMs;
            public int IntervalMs;
            public Action Callback;
        }

        private readonly List<Slot> mSlots = new List<Slot>();

        public int Add(int delayMs, Action callback, bool repeat)
        {
            if (callback == null || delayMs < 0)
            {
                return -1;
            }

            var slot = new Slot
            {
                Active = true,
                Repeat = repeat,
                LeftMs = delayMs,
                IntervalMs = delayMs,
                Callback = callback
            };

            for (var i = 0; i < mSlots.Count; i++)
            {
                if (!mSlots[i].Active)
                {
                    mSlots[i] = slot;
                    return i;
                }
            }

            mSlots.Add(slot);
            return mSlots.Count - 1;
        }

        public void Cancel(int id)
        {
            if (id < 0 || id >= mSlots.Count)
            {
                return;
            }

            var slot = mSlots[id];
            slot.Active = false;
            slot.Callback = null;
            mSlots[id] = slot;
        }

        protected override void OnUpdate(int deltaMs)
        {
            if (deltaMs < 0)
            {
                return;
            }

            for (var i = 0; i < mSlots.Count; i++)
            {
                var slot = mSlots[i];
                if (!slot.Active)
                {
                    continue;
                }

                slot.LeftMs -= deltaMs;
                if (slot.LeftMs > 0)
                {
                    mSlots[i] = slot;
                    continue;
                }

                var callback = slot.Callback;
                if (slot.Repeat)
                {
                    slot.LeftMs += slot.IntervalMs;
                    if (slot.LeftMs < 0)
                    {
                        slot.LeftMs = 0;
                    }
                }
                else
                {
                    slot.Active = false;
                    slot.Callback = null;
                }

                mSlots[i] = slot;
                if (callback != null)
                {
                    callback();
                }
            }
        }

        protected override void OnDestroy()
        {
            mSlots.Clear();
        }
    }
}
