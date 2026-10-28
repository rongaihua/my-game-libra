//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-28
// Desc: 资源包
//---------------------------------------------------------------------------------------

using UnityEngine;

namespace MyGame
{
    /// <summary>
    /// 资源包。对照 ChatRoom 的 <c>ResBundle</c>：卸载时调 <see cref="AssetBundle.Unload"/>。
    /// </summary>
    public class ResBundle : ResBase
    {
        /// <summary>
        /// 资源包本体。
        /// </summary>
        public AssetBundle Bundle { get; private set; }

        /// <summary>
        /// 构造。
        /// </summary>
        /// <param name="provider">加载后端。</param>
        public ResBundle(IResProvider provider) : base(provider)
        {
        }

        /// <summary>
        /// 资源包没有单独的资产对象。
        /// </summary>
        /// <returns>恒为空。</returns>
        public override Object GetAsset()
        {
            return null;
        }

        /// <summary>
        /// 粗估内存占用。
        /// </summary>
        /// <returns>字节数。</returns>
        public override long GetMemorySize()
        {
            return Bundle ? 65536L : 0L;
        }

        /// <summary>
        /// 执行加载。
        /// </summary>
        /// <returns>成功时为 true。</returns>
        internal override bool Load()
        {
            Status = EResStatus.Loading;
            Bundle = Provider.LoadBundle(ResName);
            Status = Bundle ? EResStatus.Loaded : EResStatus.Failed;
            return Bundle;
        }

        /// <summary>
        /// 卸载资源包。
        /// </summary>
        protected override void OnDestroy()
        {
            if (Bundle)
            {
                Provider.UnloadBundle(Bundle, true);
                Logger.Debug($"UnloadBundle: {ResName}");
            }

            Bundle = null;
            base.OnDestroy();
        }
    }
}
