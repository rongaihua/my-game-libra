//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-28
// Desc: 资源句柄
//---------------------------------------------------------------------------------------

using UnityEngine;

namespace MyGame
{
    /// <summary>
    /// 一次加载的持有。用完调 <see cref="Release"/>，或交给 <see cref="ResMgr.Release(ResHandle)"/>。
    /// 对照 ChatRoom 的 <c>ResHandle</c>。
    /// </summary>
    public sealed class ResHandle
    {
        /// <summary>
        /// 对应的缓存资源。
        /// </summary>
        private readonly ResBase mRes;

        /// <summary>
        /// 是否已经归还，防止重复释放。
        /// </summary>
        private bool mReleased;

        /// <summary>
        /// 构造。只有资源管理器能创建。
        /// </summary>
        /// <param name="res">缓存资源。</param>
        internal ResHandle(ResBase res)
        {
            mRes = res;
            Path = res.ResName;
            mRes.Retain();
        }

        /// <summary>
        /// 资源路径。
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// 资源本体。资源包这类返回空。
        /// </summary>
        public UnityEngine.Object Asset => mRes.GetAsset();

        /// <summary>
        /// 当前引用计数。
        /// </summary>
        public int RefCount => mRes.RefCount;

        /// <summary>
        /// 归还一次引用。重复调用无效。
        /// </summary>
        public void Release()
        {
            if (mReleased)
            {
                return;
            }

            mReleased = true;
            mRes.Release();
        }
    }
}
