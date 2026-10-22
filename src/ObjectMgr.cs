using System;
using System.Collections.Generic;

namespace MyGame
{
    /// <summary>
    /// 按类型复用对象。取出的对象若曾被销毁，调用方要自己重新赋值。
    /// </summary>
    public sealed class ObjectMgr : Module
    {
        private readonly Dictionary<Type, Stack<GameObject>> mPools = new Dictionary<Type, Stack<GameObject>>();

        public T Create<T>() where T : GameObject, new()
        {
            Stack<GameObject> pool;
            if (mPools.TryGetValue(typeof(T), out pool) && pool.Count > 0)
            {
                var reused = (T)pool.Pop();
                reused.Revive();
                return reused;
            }

            return new T();
        }

        public void Release(GameObject target)
        {
            if (target == null || target.IsDestroyed)
            {
                return;
            }

            target.Destroy();
            Stack<GameObject> pool;
            var type = target.GetType();
            if (!mPools.TryGetValue(type, out pool))
            {
                pool = new Stack<GameObject>();
                mPools.Add(type, pool);
            }

            pool.Push(target);
        }

        protected override void OnDestroy()
        {
            mPools.Clear();
        }
    }
}
