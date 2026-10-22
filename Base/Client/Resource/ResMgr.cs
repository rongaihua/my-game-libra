//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-22
// Desc: 按包内路径加载并持有资源
//---------------------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{

    /// <summary>
    /// 一次加载的持有。归还交给 <see cref="ResMgr.Release"/>。
    /// </summary>
    public sealed class ResHandle
    {

        /// <summary>
        /// 资源路径。
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// 加载到的资源。失败时为空。
        /// </summary>
        public Object Asset { get; internal set; }

        /// <summary>
        /// 仍被使用的次数。
        /// </summary>
        internal int RefCount;
    }

    /// <summary>
    /// 资源管理器。同一路径只加载一次，释放到零后从缓存去掉。
    /// </summary>
    public sealed class ResMgr : Module
    {

        /// <summary>
        /// 包资源根路径，对应语音房 Packages/libra/Res。
        /// </summary>
        public const string PackageRoot = "Packages/com.rongaihua.my-game-libra/";

        /// <summary>
        /// 已加载的资源，键是路径。
        /// </summary>
        private readonly Dictionary<string, ResHandle> mLoaded = new Dictionary<string, ResHandle>();

        /// <summary>
        /// 按路径取资源。已经加载过的只增加引用。
        /// </summary>
        /// <param name="path">以 Packages 开头的资源路径。</param>
        /// <returns>持有。路径为空时为空。</returns>
        public ResHandle Load(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            ResHandle handle;
            if (mLoaded.TryGetValue(path, out handle))
            {
                handle.RefCount++;
                return handle;
            }

            handle = new ResHandle();
            handle.Path = path;
            handle.Asset = LoadAsset(path);
            handle.RefCount = 1;
            mLoaded.Add(path, handle);
            return handle;
        }

        /// <summary>
        /// 归还一次引用。减到零后移出缓存。
        /// </summary>
        /// <param name="handle">要归还的持有。</param>
        public void Release(ResHandle handle)
        {
            if (handle == null || handle.RefCount <= 0)
            {
                return;
            }

            handle.RefCount--;
            if (handle.RefCount > 0)
            {
                return;
            }

            mLoaded.Remove(handle.Path);
            handle.Asset = null;
        }

        /// <summary>
        /// 清空缓存。实例由界面管理器先销毁。
        /// </summary>
        protected override void OnDestroy()
        {
            mLoaded.Clear();
        }

        /// <summary>
        /// 从工程路径读取资源。编辑器直接读包内文件。
        /// </summary>
        /// <param name="path">资源路径。</param>
        /// <returns>资源。读不到时为空。</returns>
        private static Object LoadAsset(string path)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(path);
#else
            return null;
#endif
        }
    }
}
