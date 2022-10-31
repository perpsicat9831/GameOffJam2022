using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

namespace Framework
{
    public class FastEnumIntEqualityComparer<T> : IEqualityComparer<T> where T : struct
    {
        public bool Equals(T first, T second)
        {
            var firstParam = Expression.Parameter(typeof(T), "first");
            var secondParam = Expression.Parameter(typeof(T), "second");
            var equalExpression = Expression.Equal(firstParam, secondParam);

            return Expression.Lambda<Func<T, T, bool>>
                (equalExpression, firstParam, secondParam).
                Compile().Invoke(first, second);
        }
        public int GetHashCode(T instance)
        {
            var parameter = Expression.Parameter(typeof(T), "instance");
            var convertExpression = Expression.Convert(parameter, typeof(int));

            return Expression.Lambda<Func<T, int>>
                (convertExpression, parameter).
                Compile().Invoke(instance);
        }
    }
    public class EventEmitter<TEnum> where TEnum : struct
    {
        private Dictionary<TEnum, List<Delegate>> dictMessge = null;

        private void RegisterDelegate(TEnum eventType, Delegate hander)
        {
            if(hander == null)
            {
                return;
            }
            if(dictMessge == null)
            {
                dictMessge = new Dictionary<TEnum, List<Delegate>>(new FastEnumIntEqualityComparer<TEnum>());
            }
            List<Delegate> srcHander = null;
            if(!dictMessge.TryGetValue(eventType,out srcHander) || srcHander == null)
            {
                dictMessge[eventType] = new List<Delegate>(8) { hander };
            }
            else
            {
                if (srcHander.Contains(hander))
                {
                    //Debug.Log("")
                }
                else
                {
                    srcHander.Add(hander);
                }
            }
        }

        private void RemoveDelegate(TEnum eventType, Delegate hander)
        {

            if (hander == null)
            {
                return;
            }
            if (dictMessge == null)
            {
                return;
            }
            List<Delegate> srcHander = null;
            if (dictMessge.TryGetValue(eventType, out srcHander) || srcHander != null)
            {
                srcHander.Remove(hander);
                if(srcHander.Count == 0)
                {
                    dictMessge.Remove(eventType);
                }
            }
        }


        public void Handle(TEnum eventType,Action hander,bool isAdd)
        {
            if (isAdd)
            {
                RegisterDelegate(eventType, hander);
            }
            else
            {
                RemoveDelegate(eventType, hander);
            }
        }
        public void Handle<TArg0>(TEnum eventType, Action<TArg0> hander, bool isAdd)
        {
            if (isAdd)
            {
                RegisterDelegate(eventType, hander);
            }
            else
            {
                RemoveDelegate(eventType, hander);
            }
        }
        public void Handle<TArg0, TArg1>(TEnum eventType, Action<TArg0, TArg1> hander, bool isAdd)
        {
            if (isAdd)
            {
                RegisterDelegate(eventType, hander);
            }
            else
            {
                RemoveDelegate(eventType, hander);
            }
        }
        public void Handle<TArg0, TArg1, TArg2>(TEnum eventType, Action<TArg0, TArg1, TArg2> hander, bool isAdd)
        {
            if (isAdd)
            {
                RegisterDelegate(eventType, hander);
            }
            else
            {
                RemoveDelegate(eventType, hander);
            }
        }
        public void Handle<TArg0, TArg1, TArg2, TArg3>(TEnum eventType, Action<TArg0, TArg1, TArg2, TArg3> hander, bool isAdd)
        {
            if (isAdd)
            {
                RegisterDelegate(eventType, hander);
            }
            else
            {
                RemoveDelegate(eventType, hander);
            }
        }

        public void Trigger(TEnum eventType)
        {
            if(dictMessge == null)
            {
                return;
            }
            List<Delegate> hander = null;
            if(dictMessge.TryGetValue(eventType,out hander))
            {
                for (int i = 0; i < hander.Count; i++)
                {
                    try
                    {
                        (hander[i] as Action)?.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        Debug.LogError("EventError " + eventType.ToString());
                        RemoveDelegate(eventType, hander[i]);
                        i--;
                    }
                }
            }
        }
        public void Trigger<TArg0>(TEnum eventType, TArg0 arg)
        {
            if (dictMessge == null)
            {
                return;
            }
            List<Delegate> hander = null;
            if (dictMessge.TryGetValue(eventType, out hander))
            {
                for (int i = 0; i < hander.Count; i++)
                {
                    try
                    {
                        (hander[i] as Action<TArg0>)?.Invoke(arg);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        Debug.LogError("EventError " + eventType.ToString());
                        RemoveDelegate(eventType, hander[i]);
                        i--;
                    }
                }
            }
        }
        public void Trigger<TArg0, TArg1>(TEnum eventType, TArg0 arg, TArg1 arg1)
        {
            if (dictMessge == null)
            {
                return;
            }
            List<Delegate> hander = null;
            if (dictMessge.TryGetValue(eventType, out hander))
            {
                for (int i = 0; i < hander.Count; i++)
                {
                    try
                    {
                        (hander[i] as Action<TArg0, TArg1>)?.Invoke(arg, arg1);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        Debug.LogError("EventError " + eventType.ToString());
                        RemoveDelegate(eventType, hander[i]);
                        i--;
                    }
                }
            }
        }
        public void Trigger<TArg0, TArg1, TArg2>(TEnum eventType, TArg0 arg, TArg1 arg1, TArg2 arg2)
        {
            if (dictMessge == null)
            {
                return;
            }
            List<Delegate> hander = null;
            if (dictMessge.TryGetValue(eventType, out hander))
            {
                for (int i = 0; i < hander.Count; i++)
                {
                    try
                    {
                        (hander[i] as Action<TArg0, TArg1, TArg2>)?.Invoke(arg, arg1, arg2);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        Debug.LogError("EventError " + eventType.ToString());
                        RemoveDelegate(eventType, hander[i]);
                        i--;
                    }
                }
            }
        }
        public void Trigger<TArg0, TArg1, TArg2, TArg3>(TEnum eventType, TArg0 arg, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            if (dictMessge == null)
            {
                return;
            }
            List<Delegate> hander = null;
            if (dictMessge.TryGetValue(eventType, out hander))
            {
                for (int i = 0; i < hander.Count; i++)
                {
                    try
                    {
                        (hander[i] as Action<TArg0, TArg1, TArg2, TArg3>)?.Invoke(arg, arg1, arg2, arg3);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        Debug.LogError("EventError " + eventType.ToString());
                        RemoveDelegate(eventType, hander[i]);
                        i--;
                    }
                }
            }
        }
    }
}
