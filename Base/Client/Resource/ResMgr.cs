//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-28
// Desc: 资源管理器
//---------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace MyGame
{
    /// <summary>
    /// 资源管理器。对照 ChatRoom 的 <c>ResMgr</c>：
    /// 加载交给 <see cref="IResProvider"/>，缓存与卸载交给 <see cref="ResCache"/>，对外只发 <see cref="ResHandle"/>。
    /// </summary>
    public sealed class ResMgr : Module
    {
        /// <summary>
        /// 包资源根路径。
        /// </summary>
        public const string PackageRoot = "Packages/com.rongaihua.my-game-libra/";

        /// <summary>
        /// 资源缓存。
        /// </summary>
        private ResCache mCache;

        /// <summary>
        /// 当前加载后端。
        /// </summary>
        public IResProvider Provider { get; private set; }

        /// <summary>
        /// 空闲多少秒后卸载。默认 10 秒。
        /// </summary>
        public double IdleUnloadSeconds { get; set; } = 10d;

        /// <summary>
        /// 缓存内存预算（字节）。默认 256MB。
        /// </summary>
        public long MemoryBudget { get; set; } = 256L * 1024L * 1024L;

        /// <summary>
        /// 缓存里的资源数量。
        /// </summary>
        public int CachedCount => mCache == null ? 0 : mCache.Count;

        /// <summary>
        /// 预制实例池。对照 ChatRoom 的 <c>G.PrefabPool</c>。
        /// </summary>
        public PrefabPool Pool { get; } = new PrefabPool();

        /// <summary>
        /// 初始化：选后端、建缓存。
        /// </summary>
        /// <returns>成功时为 true。</returns>
        protected override bool OnInit()
        {
            Provider = CreateProvider();
            mCache = new ResCache(Provider);
            ApplySettings();
            Logger.Info($"ResMgr 后端：{Provider.Name}");
            return true;
        }

        /// <summary>
        /// 每帧驱动缓存淘汰。
        /// </summary>
        /// <param name="deltaMs">距上一帧的毫秒数。</param>
        protected override void OnUpdate(int deltaMs)
        {
            mCache?.Tick(DateTime.UtcNow.Ticks);
        }

        /// <summary>
        /// 销毁时清空缓存。
        /// </summary>
        protected override void OnDestroy()
        {
            if (mCache != null)
            {
                mCache.Clear();
                mCache = null;
            }

            Pool.Clear();
            Provider = null;
        }

        /// <summary>
        /// 加载资源，返回持有。
        /// </summary>
        /// <param name="resName">资源名。</param>
        /// <returns>持有。加载不到时为空。</returns>
        public ResHandle Load(string resName)
        {
            var res = mCache == null ? null : mCache.Load(resName, false);
            return res == null ? null : new ResHandle(res);
        }

        /// <summary>
        /// 加载资源，返回持有。对照 ChatRoom 的 <c>LoadRes</c>。
        /// </summary>
        /// <typeparam name="T">资源类型。</typeparam>
        /// <param name="resName">资源名。</param>
        /// <returns>持有。加载不到时为空。</returns>
        public ResHandle LoadRes<T>(string resName) where T : UnityEngine.Object
        {
            return Load(resName);
        }

        /// <summary>
        /// 同步加载，返回持有。对照 ChatRoom 的 <c>LoadResSync</c>。
        /// </summary>
        /// <typeparam name="T">资源类型。</typeparam>
        /// <param name="resName">资源名。</param>
        /// <returns>持有。加载不到时为空。</returns>
        public ResHandle LoadResSync<T>(string resName) where T : UnityEngine.Object
        {
            return Load(resName);
        }

        /// <summary>
        /// 资源是否已经在缓存里。
        /// </summary>
        /// <param name="resName">资源名。</param>
        /// <returns>在缓存里时为 true。</returns>
        public bool ContainsRes(string resName)
        {
            return mCache != null && mCache.Contains(resName);
        }

        /// <summary>
        /// 归还一次引用。归零后等空闲淘汰。
        /// </summary>
        /// <param name="handle">要归还的持有。</param>
        public void Release(ResHandle handle)
        {
            handle?.Release();
        }

        /// <summary>
        /// 批量归还。
        /// </summary>
        /// <param name="handles">要归还的持有。</param>
        public void Release(IList<ResHandle> handles)
        {
            if (handles == null)
            {
                return;
            }

            for (var i = 0; i < handles.Count; i++)
            {
                Release(handles[i]);
            }
        }

        /// <summary>
        /// 立刻卸载全部缓存。切场景前调用。
        /// </summary>
        public void UnloadAll()
        {
            mCache?.Clear();
        }

        /// <summary>
        /// 加载场景。
        /// </summary>
        /// <param name="sceneName">场景名或工程路径。</param>
        /// <param name="mode">加载方式。</param>
        /// <returns>持有。加载不到时为空。</returns>
        public ResHandle LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            var res = mCache == null ? null : mCache.LoadScene(sceneName, mode);
            return res == null ? null : new ResHandle(res);
        }

        /// <summary>
        /// 同步加载场景，返回持有。对照 ChatRoom 的 <c>LoadSceneSync</c>。
        /// </summary>
        /// <param name="sceneName">场景名或工程路径。</param>
        /// <param name="mode">加载方式。</param>
        /// <returns>持有。加载不到时为空。</returns>
        public ResHandle LoadSceneSync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            return LoadScene(sceneName, mode);
        }

        /// <summary>
        /// 清空预制实例池。
        /// </summary>
        public void ClearPool()
        {
            Pool.Clear();
        }

        /// <summary>
        /// 把对外配置同步到缓存。
        /// </summary>
        private void ApplySettings()
        {
            mCache.IdleUnloadTicks = (long)(IdleUnloadSeconds * TimeSpan.TicksPerSecond);
            mCache.MemoryBudget = MemoryBudget;
        }

        /// <summary>
        /// 选后端：编辑器读 AssetDatabase，真机读 Resources。
        /// </summary>
        /// <returns>加载后端。</returns>
        private static IResProvider CreateProvider()
        {
#if UNITY_EDITOR
            return new EditorResProvider();
#else
            return new ResourcesResProvider();
#endif
        }
    }
}
