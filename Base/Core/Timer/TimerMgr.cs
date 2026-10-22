//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-22
// Desc: 毫秒计时器
//---------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace MyGame
{

    /// <summary>
    /// 毫秒计时器。返回的 id 用于取消。
    /// </summary>
    public sealed class TimerMgr : Module
    {

        /// <summary>
        /// 一个计时槽。空闲槽复用，不从列表里删。
        /// </summary>
        private struct Slot
        {

            /// <summary>
            /// 槽是否在计时。
            /// </summary>
            public bool Active;

            /// <summary>
            /// 到期后是否按原间隔再计一次。
            /// </summary>
            public bool Repeat;

            /// <summary>
            /// 距到期还剩的毫秒。
            /// </summary>
            public int LeftMs;

            /// <summary>
            /// 重复计时的间隔毫秒。
            /// </summary>
            public int IntervalMs;

            /// <summary>
            /// 到期回调。
            /// </summary>
            public Action Callback;
        }

        /// <summary>
        /// 全部计时槽，下标即 id。
        /// </summary>
        private readonly List<Slot> mSlots = new List<Slot>();

        /// <summary>
        /// 添加一个计时器。
        /// </summary>
        /// <param name="delayMs">延迟毫秒。</param>
        /// <param name="callback">到期回调。</param>
        /// <param name="repeat">到期后是否重复。</param>
        /// <returns>计时器 id。参数无效时为 -1。</returns>
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

        /// <summary>
        /// 取消一个计时器。id 无效时不做事。
        /// </summary>
        /// <param name="id"><see cref="Add"/> 返回的 id。</param>
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

        /// <summary>
        /// 扣减剩余时间，到期的槽执行回调。
        /// </summary>
        /// <param name="deltaMs">距上一帧的毫秒数。</param>
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

        /// <summary>
        /// 清空全部计时槽。
        /// </summary>
        protected override void OnDestroy()
        {
            mSlots.Clear();
        }
    }
}
