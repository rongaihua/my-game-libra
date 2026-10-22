//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-22
// Desc: 管理器容器
//---------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace MyGame
{

    /// <summary>
    /// 管理器容器。先 <see cref="Add{T}"/>，再 <see cref="Init"/>，之后每帧调用 <see cref="Update"/>。
    /// </summary>
    public sealed class App
    {

        /// <summary>
        /// 已挂上的模块，按添加顺序。
        /// </summary>
        private readonly List<Module> mModules = new List<Module>();

        /// <summary>
        /// 事件管理器。
        /// </summary>
        public EventMgr Events { get; private set; }

        /// <summary>
        /// 计时器管理器。
        /// </summary>
        public TimerMgr Timer { get; private set; }

        /// <summary>
        /// 对象池管理器。
        /// </summary>
        public ObjectMgr Objects { get; private set; }

        /// <summary>
        /// 房间玩家管理器。
        /// </summary>
        public RoomMgr Room { get; private set; }

        /// <summary>
        /// 局域网连接管理器。
        /// </summary>
        public NetMgr Net { get; private set; }

        /// <summary>
        /// 创建并挂上内置管理器。
        /// </summary>
        public App()
        {
            Events = Add<EventMgr>();
            Timer = Add<TimerMgr>();
            Objects = Add<ObjectMgr>();
            Room = Add<RoomMgr>();
            Net = Add<NetMgr>();
        }

        /// <summary>
        /// 创建模块、挂到本应用并记入列表。
        /// </summary>
        /// <typeparam name="T">模块类型。</typeparam>
        /// <returns>新模块。</returns>
        public T Add<T>() where T : Module, new()
        {
            var module = new T();
            module.Attach(this);
            mModules.Add(module);
            return module;
        }

        /// <summary>
        /// 按类型查找已挂上的模块。
        /// </summary>
        /// <typeparam name="T">模块类型。</typeparam>
        /// <returns>找到的模块。没有时为空。</returns>
        public T Find<T>() where T : Module
        {
            for (var i = 0; i < mModules.Count; i++)
            {
                var found = mModules[i] as T;
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        /// <summary>
        /// 按添加顺序初始化全部模块。
        /// </summary>
        /// <returns>全部成功时为 true。</returns>
        public bool Init()
        {
            for (var i = 0; i < mModules.Count; i++)
            {
                if (!mModules[i].Init())
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 按添加顺序更新全部模块。
        /// </summary>
        /// <param name="deltaMs">距上一帧的毫秒数。</param>
        public void Update(int deltaMs)
        {
            for (var i = 0; i < mModules.Count; i++)
            {
                mModules[i].Tick(deltaMs);
            }
        }

        /// <summary>
        /// 按添加的反序销毁全部模块并清空引用。
        /// </summary>
        public void Shutdown()
        {
            for (var i = mModules.Count - 1; i >= 0; i--)
            {
                mModules[i].Destroy();
            }

            mModules.Clear();
            Events = null;
            Timer = null;
            Objects = null;
            Room = null;
            Net = null;
        }
    }
}
