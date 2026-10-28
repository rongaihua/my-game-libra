//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-28
// Desc: 全局访问点
//---------------------------------------------------------------------------------------

namespace MyGame
{
    /// <summary>
    /// 全局访问点。参照 ChatRoom 的 <c>G</c>，把常用管理器收成静态属性，
    /// 业务代码里直接写 <c>G.UIMgr.OpenWindow&lt;XxxView&gt;()</c>。
    /// </summary>
    public static class G
    {
        /// <summary>
        /// 应用实例。未初始化时为空。
        /// </summary>
        public static App App { get; private set; }

        /// <summary>
        /// 事件管理器。
        /// </summary>
        public static EventMgr Events { get; private set; }

        /// <summary>
        /// 计时器管理器。
        /// </summary>
        public static TimerMgr Timer { get; private set; }

        /// <summary>
        /// 对象池管理器。
        /// </summary>
        public static ObjectMgr Objects { get; private set; }

        /// <summary>
        /// 资源管理器。
        /// </summary>
        public static ResMgr ResMgr { get; private set; }

        /// <summary>
        /// 界面管理器。
        /// </summary>
        public static UIMgr UIMgr { get; private set; }

        /// <summary>
        /// 音效管理器。
        /// </summary>
        public static SoundMgr SoundMgr { get; private set; }

        /// <summary>
        /// 是否已经初始化。
        /// </summary>
        public static bool IsReady => App != null;

        /// <summary>
        /// 创建应用并挂上客户端管理器。重复调用直接返回。
        /// </summary>
        /// <returns>初始化成功时为 true。</returns>
        public static bool Init()
        {
            if (App != null)
            {
                return true;
            }

            if (!Logger.IsInited)
            {
                Logger.Init(new UnityLogger());
            }

            var app = new App();
            var resMgr = app.Add<ResMgr>();
            var uiMgr = app.Add<UIMgr>();
            var soundMgr = app.Add<SoundMgr>();

            if (!app.Init())
            {
                app.Shutdown();
                return false;
            }

            App = app;
            Events = app.Events;
            Timer = app.Timer;
            Objects = app.Objects;
            ResMgr = resMgr;
            UIMgr = uiMgr;
            SoundMgr = soundMgr;
            return true;
        }

        /// <summary>
        /// 关闭应用并清空全部引用。
        /// </summary>
        public static void Destroy()
        {
            if (App == null)
            {
                return;
            }

            App.Shutdown();
            App = null;
            Events = null;
            Timer = null;
            Objects = null;
            ResMgr = null;
            UIMgr = null;
            SoundMgr = null;
        }

        /// <summary>
        /// 挂一个自定义管理器。
        /// </summary>
        /// <typeparam name="T">管理器类型。</typeparam>
        /// <returns>挂上的管理器。未初始化时为空。</returns>
        public static T Add<T>() where T : Module, new()
        {
            return App?.Add<T>();
        }

        /// <summary>
        /// 按类型取管理器。
        /// </summary>
        /// <typeparam name="T">管理器类型。</typeparam>
        /// <returns>找到的管理器。未初始化或没挂时为空。</returns>
        public static T Get<T>() where T : Module
        {
            return App?.Find<T>();
        }
    }
}
