using System.Collections.Generic;

namespace MyGame
{
    /// <summary>
    /// 管理器容器。先 <see cref="Add{T}"/>，再 <see cref="Init"/>，之后每帧调用 <see cref="Update"/>。
    /// </summary>
    public sealed class App
    {
        private readonly List<Module> mModules = new List<Module>();

        public EventMgr Events { get; private set; }
        public TimerMgr Timer { get; private set; }
        public ObjectMgr Objects { get; private set; }
        public RoomMgr Room { get; private set; }
        public NetMgr Net { get; private set; }

        public App()
        {
            Events = Add<EventMgr>();
            Timer = Add<TimerMgr>();
            Objects = Add<ObjectMgr>();
            Room = Add<RoomMgr>();
            Net = Add<NetMgr>();
        }

        public T Add<T>() where T : Module, new()
        {
            var module = new T();
            module.Attach(this);
            mModules.Add(module);
            return module;
        }

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

        public void Update(int deltaMs)
        {
            for (var i = 0; i < mModules.Count; i++)
            {
                mModules[i].Tick(deltaMs);
            }
        }

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
