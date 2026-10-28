//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-28
// Desc: 逻辑对象基类
//---------------------------------------------------------------------------------------

namespace MyGame
{
    /// <summary>
    /// 逻辑对象基类。对照 ChatRoom 的 <c>Libra.Client.Object</c>，
    /// 提供 <see cref="G"/> 与按类型取管理器的便捷入口。
    /// </summary>
    public abstract class BaseObject : GameObject
    {
        /// <summary>
        /// 应用实例。
        /// </summary>
        protected static App App => G.App;

        /// <summary>
        /// 按类型取管理器。
        /// </summary>
        /// <typeparam name="T">管理器类型。</typeparam>
        /// <returns>找到的管理器。</returns>
        protected static T Get<T>() where T : Module
        {
            return G.Get<T>();
        }
    }
}
