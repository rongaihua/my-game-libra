//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-28
// Desc: 资源缓存（空闲淘汰 + 内存预算）
//---------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace MyGame
{
    /// <summary>
    /// 资源缓存。对照 ChatRoom 的 <c>ResCache</c>：
    /// 引用归零的资源不立刻卸载，等空闲超过阈值或内存超预算时才真正卸载。
    /// </summary>
    public sealed class ResCache : GameObject
    {
        /// <summary>
        /// 已加载的资源，键是资源名。
        /// </summary>
        private readonly Dictionary<string, ResBase> mCache = new Dictionary<string, ResBase>();

        /// <summary>
        /// 加载后端。
        /// </summary>
        private readonly IResProvider mProvider;

        /// <summary>
        /// 构造。
        /// </summary>
        /// <param name="provider">加载后端。</param>
        public ResCache(IResProvider provider)
        {
            mProvider = provider;
        }

        /// <summary>
        /// 缓存数量。
        /// </summary>
        public int Count => mCache.Count;

        /// <summary>
        /// 空闲多久后卸载（ticks）。默认 10 秒。
        /// </summary>
        public long IdleUnloadTicks { get; set; } = TimeSpan.TicksPerSecond * 10;

        /// <summary>
        /// 缓存内存预算（字节）。默认 256MB。
        /// </summary>
        public long MemoryBudget { get; set; } = 256L * 1024L * 1024L;

        /// <summary>
        /// 资源是否在缓存里。
        /// </summary>
        /// <param name="resName">资源名。</param>
        /// <returns>在缓存里时为 true。</returns>
        public bool Contains(string resName)
        {
            return !string.IsNullOrEmpty(resName) && mCache.ContainsKey(resName);
        }

        /// <summary>
        /// 加载资源。已在缓存且加载成功时直接复用。
        /// </summary>
        /// <param name="resName">资源名。</param>
        /// <param name="isBundle">是否按资源包加载。</param>
        /// <returns>缓存里的资源。加载失败时为空。</returns>
        public ResBase Load(string resName, bool isBundle)
        {
            if (string.IsNullOrEmpty(resName))
            {
                return null;
            }

            ResBase res;
            if (mCache.TryGetValue(resName, out res) && res.Status == EResStatus.Loaded)
            {
                return res;
            }

            if (res != null)
            {
                mCache.Remove(resName);
                res.Destroy();
            }

            res = isBundle ? new ResBundle(mProvider) : new ResObject(mProvider);
            res.ResName = resName;
            mCache[resName] = res;
            if (res.Load())
            {
                return res;
            }

            mCache.Remove(resName);
            res.Destroy();
            Logger.Warn($"加载资源失败：{resName}");
            return null;
        }

        /// <summary>
        /// 加载场景。同名同模式且已加载时直接复用。
        /// </summary>
        /// <param name="resName">场景名或工程路径。</param>
        /// <param name="mode">加载方式。</param>
        /// <returns>缓存里的场景资源。加载失败时为空。</returns>
        public ResBase LoadScene(string resName, LoadSceneMode mode)
        {
            if (string.IsNullOrEmpty(resName))
            {
                return null;
            }

            ResBase res;
            if (mCache.TryGetValue(resName, out res))
            {
                var cached = res as ResScene;
                if (cached != null && cached.Mode == mode && cached.Status == EResStatus.Loaded)
                {
                    return res;
                }
            }

            if (res != null)
            {
                mCache.Remove(resName);
                res.Destroy();
            }

            var sceneRes = new ResScene(mProvider, mode);
            sceneRes.ResName = resName;
            mCache[resName] = sceneRes;
            if (sceneRes.Load())
            {
                return sceneRes;
            }

            mCache.Remove(resName);
            sceneRes.Destroy();
            Logger.Warn($"加载场景失败：{resName}");
            return null;
        }

        /// <summary>
        /// 每帧淘汰：先按空闲时长，再按内存预算从最老的开始丢。
        /// </summary>
        /// <param name="nowTicks">当前时间戳。</param>
        public void Tick(long nowTicks)
        {
            if (mCache.Count == 0)
            {
                return;
            }

            UnloadIdle(nowTicks);
            UnloadOverBudget();
        }

        /// <summary>
        /// 全部卸载。
        /// </summary>
        public void Clear()
        {
            foreach (var pair in mCache)
            {
                pair.Value.Destroy();
            }

            mCache.Clear();
        }

        /// <summary>
        /// 卸载空闲超时的资源。
        /// </summary>
        /// <param name="nowTicks">当前时间戳。</param>
        private void UnloadIdle(long nowTicks)
        {
            List<string> expired = null;
            foreach (var pair in mCache)
            {
                if (!pair.Value.IsIdle(nowTicks, IdleUnloadTicks))
                {
                    continue;
                }

                if (expired == null)
                {
                    expired = new List<string>();
                }

                expired.Add(pair.Key);
            }

            if (expired == null)
            {
                return;
            }

            foreach (var key in expired)
            {
                var res = mCache[key];
                mCache.Remove(key);
                res.Destroy();
                Logger.Debug($"UnloadIdle: {key}");
            }
        }

        /// <summary>
        /// 内存超预算时按最后释放时间从老到新卸载。
        /// </summary>
        private void UnloadOverBudget()
        {
            var total = 0L;
            foreach (var pair in mCache)
            {
                total += pair.Value.GetMemorySize();
            }

            if (total <= MemoryBudget)
            {
                return;
            }

            var candidates = new List<ResBase>();
            foreach (var pair in mCache)
            {
                if (pair.Value.RefCount <= 0)
                {
                    candidates.Add(pair.Value);
                }
            }

            candidates.Sort(CompareByLastRelease);
            foreach (var res in candidates)
            {
                if (total <= MemoryBudget)
                {
                    break;
                }

                total -= res.GetMemorySize();
                mCache.Remove(res.ResName);
                res.Destroy();
                Logger.Debug($"UnloadOverBudget: {res.ResName}");
            }
        }

        /// <summary>
        /// 按最后释放时间升序比较，越早释放越先淘汰。
        /// </summary>
        /// <param name="left">左。</param>
        /// <param name="right">右。</param>
        /// <returns>比较结果。</returns>
        private static int CompareByLastRelease(ResBase left, ResBase right)
        {
            return left.LastReleaseTicks.CompareTo(right.LastReleaseTicks);
        }
    }
}
