//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-28
// Desc: 批量资源加载
//---------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace MyGame
{
    /// <summary>
    /// 批量加载器。对照 ChatRoom 的 <c>BatchResLoader</c>：先攒路径，再一次性加载，
    /// 全部到位后回调。本工程加载是同步的，所以回调在 <see cref="Load"/> 里立即触发。
    /// </summary>
    public sealed class BatchResLoader
    {
        /// <summary>
        /// 待加载路径。
        /// </summary>
        private readonly List<string> mNames = new List<string>();

        /// <summary>
        /// 已经拿到的持有。
        /// </summary>
        private readonly List<ResHandle> mHandles = new List<ResHandle>();

        /// <summary>
        /// 待加载数量。
        /// </summary>
        public int Count => mNames.Count;

        /// <summary>
        /// 加载完成的持有，顺序与添加顺序一致（失败的不在里面）。
        /// </summary>
        public IReadOnlyList<ResHandle> Handles => mHandles;

        /// <summary>
        /// 添加一个路径。重复添加会被忽略。
        /// </summary>
        /// <param name="resName">资源名。</param>
        public void Add(string resName)
        {
            if (string.IsNullOrEmpty(resName) || mNames.Contains(resName))
            {
                return;
            }

            mNames.Add(resName);
        }

        /// <summary>
        /// 执行加载。
        /// </summary>
        /// <param name="resMgr">资源管理器。</param>
        /// <param name="onCompleted">全部加载完后的回调，可为空。</param>
        public void Load(ResMgr resMgr, Action onCompleted)
        {
            if (resMgr == null)
            {
                onCompleted?.Invoke();
                return;
            }

            for (var i = 0; i < mNames.Count; i++)
            {
                var handle = resMgr.Load(mNames[i]);
                if (handle != null)
                {
                    mHandles.Add(handle);
                }
            }

            onCompleted?.Invoke();
        }

        /// <summary>
        /// 归还全部持有。
        /// </summary>
        /// <param name="resMgr">资源管理器。</param>
        public void Release(ResMgr resMgr)
        {
            if (resMgr != null)
            {
                resMgr.Release(mHandles);
            }

            mHandles.Clear();
        }
    }
}
