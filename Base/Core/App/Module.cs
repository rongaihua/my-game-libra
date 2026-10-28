//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-22
// Desc: 管理器基类
//---------------------------------------------------------------------------------------

namespace MyGame
{

    /// <summary>
    /// 管理器基类。由 <see cref="App"/> 按创建、初始化、每帧更新、销毁的顺序驱动。
    /// </summary>
    public abstract class Module : GameObject
    {

        /// <summary>
        /// 所属应用。
        /// </summary>
        protected App App { get; private set; }

        /// <summary>
        /// 挂到应用上并进入创建回调。
        /// </summary>
        /// <param name="app">所属应用。</param>
        internal void Attach(App app)
        {
            App = app;
            OnCreate();
        }

        /// <summary>
        /// 执行初始化。
        /// </summary>
        /// <returns>初始化成功时为 true。</returns>
        internal bool Init()
        {
            return OnInit();
        }

        /// <summary>
        /// 全部模块初始化完成后执行一次启动。对照 ChatRoom 的 <c>OnStart</c>：
        /// 需要「别的管理器也已就绪」的逻辑放这里，别放 OnInit。
        /// </summary>
        internal void Start()
        {
            if (!IsDestroyed)
            {
                OnStart();
            }
        }

        /// <summary>
        /// 推进一帧。已销毁的模块不再更新。
        /// </summary>
        /// <param name="deltaMs">距上一帧的毫秒数。</param>
        internal void Tick(int deltaMs)
        {
            if (!IsDestroyed)
            {
                OnUpdate(deltaMs);
            }
        }

        /// <summary>
        /// 挂到应用之后、初始化之前。
        /// </summary>
        protected virtual void OnCreate()
        {
        }

        /// <summary>
        /// 初始化。
        /// </summary>
        /// <returns>成功时为 true。</returns>
        protected virtual bool OnInit()
        {
            return true;
        }

        /// <summary>
        /// 每帧更新。
        /// </summary>
        /// <param name="deltaMs">距上一帧的毫秒数。</param>
        protected virtual void OnUpdate(int deltaMs)
        {
        }

        /// <summary>
        /// 全部模块初始化完成后调用一次。
        /// </summary>
        protected virtual void OnStart()
        {
        }
    }
}
