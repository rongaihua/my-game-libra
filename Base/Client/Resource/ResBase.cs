//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-28
// Desc: 资源基类（引用计数 + 空闲时间）
//---------------------------------------------------------------------------------------

using System;
using UnityEngine;

namespace MyGame
{
    /// <summary>
    /// 资源基类。对照 ChatRoom 的 <c>ResBase</c>：缓存持有它，句柄通过引用计数决定何时能卸载。
    /// </summary>
    public abstract class ResBase
    {
        /// <summary>
        /// 加载后端。
        /// </summary>
        protected IResProvider Provider { get; }

        /// <summary>
        /// 资源名。
        /// </summary>
        public string ResName { get; internal set; }

        /// <summary>
        /// 当前状态。
        /// </summary>
        public EResStatus Status { get; internal set; }

        /// <summary>
        /// 引用计数。
        /// </summary>
        public int RefCount { get; private set; }

        /// <summary>
        /// 最后一次降到零的时间戳（UTC ticks）。
        /// </summary>
        public long LastReleaseTicks { get; private set; }

        /// <summary>
        /// 引用归零后是否立刻卸载。默认 false，交给空闲淘汰。
        /// </summary>
        public bool FreeWhenUnused { get; set; }

        /// <summary>
        /// 构造。
        /// </summary>
        /// <param name="provider">加载后端。</param>
        protected ResBase(IResProvider provider)
        {
            Provider = provider;
        }

        /// <summary>
        /// 资源本体。资源包这类没有显式对象的返回空。
        /// </summary>
        /// <returns>资源。</returns>
        public virtual UnityEngine.Object GetAsset()
        {
            return null;
        }

        /// <summary>
        /// 估算内存占用，给缓存预算用。
        /// </summary>
        /// <returns>字节数。</returns>
        public virtual long GetMemorySize()
        {
            return 0;
        }

        /// <summary>
        /// 执行加载。
        /// </summary>
        /// <returns>成功时为 true。</returns>
        internal virtual bool Load()
        {
            Status = EResStatus.Loaded;
            return true;
        }

        /// <summary>
        /// 加一次引用。
        /// </summary>
        internal void Retain()
        {
            ++RefCount;
            LastReleaseTicks = 0;
        }

        /// <summary>
        /// 减一次引用。归零时记时间戳，按需立刻卸载。
        /// </summary>
        internal void Release()
        {
            if (RefCount > 0)
            {
                --RefCount;
            }

            if (RefCount > 0)
            {
                return;
            }

            LastReleaseTicks = DateTime.UtcNow.Ticks;
            if (FreeWhenUnused)
            {
                Destroy();
            }
        }

        /// <summary>
        /// 是否已经空闲到可以卸载。
        /// </summary>
        /// <param name="nowTicks">当前时间戳。</param>
        /// <param name="idleTicks">空闲阈值。</param>
        /// <returns>可以卸载时为 true。</returns>
        public bool IsIdle(long nowTicks, long idleTicks)
        {
            return RefCount <= 0 && LastReleaseTicks > 0 && nowTicks - LastReleaseTicks >= idleTicks;
        }

        /// <summary>
        /// 卸载并置为已释放。重复调用无效。
        /// </summary>
        public void Destroy()
        {
            if (Status == EResStatus.Released)
            {
                return;
            }

            Status = EResStatus.Released;
            OnDestroy();
        }

        /// <summary>
        /// 派生类在这里真正卸载自己持有的东西。
        /// </summary>
        protected virtual void OnDestroy()
        {
        }
    }
}
