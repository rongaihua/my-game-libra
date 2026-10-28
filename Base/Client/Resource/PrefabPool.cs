//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-28
// Desc: 预制实例池
//---------------------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{
    /// <summary>
    /// 预制实例池。对照 ChatRoom 的 <c>PrefabPool</c>：同路径的实例归还后复用，不反复 Instantiate。
    /// 注意本文件里的 <c>GameObject</c> 全部写全名，避免被 <see cref="GameObject"/> 基类遮挡。
    /// </summary>
    public sealed class PrefabPool
    {
        /// <summary>
        /// 空闲实例，键是预制路径。
        /// </summary>
        private readonly Dictionary<string, Stack<UnityEngine.GameObject>> mIdle = new Dictionary<string, Stack<UnityEngine.GameObject>>();

        /// <summary>
        /// 实例对应的预制路径，归还时用。
        /// </summary>
        private readonly Dictionary<UnityEngine.GameObject, string> mPaths = new Dictionary<UnityEngine.GameObject, string>();

        /// <summary>
        /// 每个路径最多留几个空闲实例。
        /// </summary>
        public int MaxIdlePerPath { get; set; } = 8;

        /// <summary>
        /// 从池里取一个实例，没有就实例化。
        /// </summary>
        /// <param name="resMgr">资源管理器。</param>
        /// <param name="prefabPath">预制路径。</param>
        /// <param name="parent">父节点。</param>
        /// <returns>实例。取不到时为空。</returns>
        public UnityEngine.GameObject Create(ResMgr resMgr, string prefabPath, Transform parent)
        {
            if (string.IsNullOrEmpty(prefabPath))
            {
                return null;
            }

            Stack<UnityEngine.GameObject> stack;
            if (mIdle.TryGetValue(prefabPath, out stack) && stack.Count > 0)
            {
                var idle = stack.Pop();
                if (idle)
                {
                    idle.transform.SetParent(parent, false);
                    idle.SetActive(true);
                    return idle;
                }
            }

            var handle = resMgr.Load(prefabPath);
            var prefab = handle == null ? null : handle.Asset as UnityEngine.GameObject;
            if (!prefab)
            {
                resMgr.Release(handle);
                return null;
            }

            var instance = Object.Instantiate(prefab, parent, false);
            mPaths[instance] = prefabPath;
            resMgr.Release(handle);
            return instance;
        }

        /// <summary>
        /// 归还实例。超过上限的直接销毁。
        /// </summary>
        /// <param name="instance">实例。</param>
        public void Release(UnityEngine.GameObject instance)
        {
            if (!instance)
            {
                return;
            }

            string path;
            if (!mPaths.Remove(instance, out path))
            {
                Object.Destroy(instance);
                return;
            }

            instance.SetActive(false);
            instance.transform.SetParent(null, false);

            Stack<UnityEngine.GameObject> stack;
            if (!mIdle.TryGetValue(path, out stack))
            {
                stack = new Stack<UnityEngine.GameObject>();
                mIdle.Add(path, stack);
            }

            if (stack.Count >= MaxIdlePerPath)
            {
                Object.Destroy(instance);
                return;
            }

            stack.Push(instance);
        }

        /// <summary>
        /// 清空池，销毁全部空闲实例。
        /// </summary>
        public void Clear()
        {
            foreach (var pair in mIdle)
            {
                foreach (var instance in pair.Value)
                {
                    if (instance)
                    {
                        Object.Destroy(instance);
                    }
                }
            }

            mIdle.Clear();
            mPaths.Clear();
        }
    }
}
