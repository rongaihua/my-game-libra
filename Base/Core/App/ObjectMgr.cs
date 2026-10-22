//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-22
// Desc: 按类型复用对象
//---------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace MyGame
{

    /// <summary>
    /// 按类型复用对象。取出的对象若曾被销毁，调用方要自己重新赋值。
    /// </summary>
    public sealed class ObjectMgr : Module
    {

        /// <summary>
        /// 按具体类型分的空闲栈。
        /// </summary>
        private readonly Dictionary<Type, Stack<GameObject>> mPools = new Dictionary<Type, Stack<GameObject>>();

        /// <summary>
        /// 取出一个对象。池里有就复用，没有就新建。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <returns>可用对象。</returns>
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

        /// <summary>
        /// 销毁对象并放回对应类型的池。
        /// </summary>
        /// <param name="target">要归还的对象。</param>
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

        /// <summary>
        /// 清空各类型的空闲栈。池中对象已在归还时销毁。
        /// </summary>
        protected override void OnDestroy()
        {
            mPools.Clear();
        }
    }
}
