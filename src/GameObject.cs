namespace MyGame
{
    /// <summary>
    /// 可销毁对象。派生类在 <see cref="OnDestroy"/> 里清空自己的字段。
    /// </summary>
    public abstract class GameObject
    {
        public bool IsDestroyed { get; private set; }

        internal void Destroy()
        {
            if (IsDestroyed)
            {
                return;
            }

            IsDestroyed = true;
            OnDestroy();
        }

        internal void Revive()
        {
            IsDestroyed = false;
        }

        protected virtual void OnDestroy()
        {
        }
    }
}
