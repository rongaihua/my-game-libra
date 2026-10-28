//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-28
// Desc: 按标记自动绑定子节点
//---------------------------------------------------------------------------------------

using System;
using System.Reflection;
using UnityEngine;

namespace MyGame
{
    /// <summary>
    /// 子节点自动绑定。遍历界面上带 <see cref="UIBindAttribute"/> 的字段，
    /// 按路径找子节点并赋值。字段已经有值（比如在 Inspector 里拖好了）时不覆盖。
    /// </summary>
    public static class UIBinder
    {
        /// <summary>
        /// 反射用的字段范围：公开和非公开的实例字段都算。
        /// </summary>
        private const BindingFlags FieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// 绑定一个界面上的全部标记字段。
        /// </summary>
        /// <param name="target">要绑定的界面。</param>
        public static void Bind(MonoBehaviour target)
        {
            if (target == null)
            {
                return;
            }

            var type = target.GetType();
            var fields = type.GetFields(FieldFlags);
            for (var i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var attribute = field.GetCustomAttribute<UIBindAttribute>();
                if (attribute == null)
                {
                    continue;
                }

                if (field.GetValue(target) != null)
                {
                    continue;
                }

                var node = target.transform.Find(attribute.Path);
                if (node == null)
                {
                    Logger.Warn($"UIBind 找不到节点 {attribute.Path}（{type.Name}.{field.Name}）");
                    continue;
                }

                var value = Resolve(node, field.FieldType);
                if (value == null)
                {
                    Logger.Warn($"UIBind 节点 {attribute.Path} 上没有 {field.FieldType.Name}（{type.Name}.{field.Name}）");
                    continue;
                }

                field.SetValue(target, value);
            }
        }

        /// <summary>
        /// 把节点转成字段需要的类型。
        /// </summary>
        /// <param name="node">节点。</param>
        /// <param name="fieldType">字段类型。</param>
        /// <returns>赋值对象。取不到时为空。</returns>
        private static object Resolve(Transform node, Type fieldType)
        {
            if (fieldType == typeof(UnityEngine.GameObject))
            {
                return node.gameObject;
            }

            if (fieldType == typeof(Transform))
            {
                return node;
            }

            if (fieldType == typeof(RectTransform))
            {
                return node as RectTransform;
            }

            return node.GetComponent(fieldType);
        }
    }
}
