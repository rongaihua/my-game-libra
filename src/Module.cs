namespace MyGame
{
    /// <summary>
    /// 管理器基类。由 <see cref="App"/> 按创建、初始化、每帧更新、销毁的顺序驱动。
    /// </summary>
    public abstract class Module : GameObject
    {
        protected App App { get; private set; }

        internal void Attach(App app)
        {
            App = app;
            OnCreate();
        }

        internal bool Init()
        {
            return OnInit();
        }

        internal void Tick(int deltaMs)
        {
            if (!IsDestroyed)
            {
                OnUpdate(deltaMs);
            }
        }

        protected virtual void OnCreate()
        {
        }

        protected virtual bool OnInit()
        {
            return true;
        }

        protected virtual void OnUpdate(int deltaMs)
        {
        }
    }
}
