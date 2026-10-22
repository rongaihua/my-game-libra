//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-22
// Desc: 可销毁对象基类
//---------------------------------------------------------------------------------------

namespace MyGame
{

    /// <summary>
    /// 可销毁对象。派生类在 <see cref="OnDestroy"/> 里清空自己的字段。
    /// </summary>
    public abstract class GameObject
    {

        /// <summary>
        /// 是否已经销毁。
        /// </summary>
        public bool IsDestroyed { get; private set; }

        /// <summary>
        /// 标记销毁并调用 <see cref="OnDestroy"/>。重复调用无效。
        /// </summary>
        internal void Destroy()
        {
            if (IsDestroyed)
            {
                return;
            }

            IsDestroyed = true;
            OnDestroy();
        }

        /// <summary>
        /// 从对象池取出时清掉销毁标记。
        /// </summary>
        internal void Revive()
        {
            IsDestroyed = false;
        }

        /// <summary>
        /// 销毁时清空派生类自己的字段。
        /// </summary>
        protected virtual void OnDestroy()
        {
        }
    }
}
