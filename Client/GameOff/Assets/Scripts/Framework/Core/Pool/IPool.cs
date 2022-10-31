using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public interface IPool<T>
    {
        /// <summary>
        /// 分配对象
        /// </summary>
        /// <returns></returns>
        T Allocate();

        /// <summary>
        /// 回收对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        bool Recycle(T obj);
    }
    public interface IPoolable
    {
        /// <summary>
        /// 回收(成功被回收到对象池时调用)
        /// </summary>
        void OnRecycled();
        /// <summary>
        /// 已经被回收
        /// </summary>
        bool IsRecycled { get; set; }
        /// <summary>
        /// 销毁 (回收失败，对象池clear的时候会调用)
        /// </summary>
        void OnDestroy();
    }
    public interface IPoolType
    {
        /// <summary>
        /// 回收对象
        /// </summary>
        void Recycle2Cache();
    }
}
