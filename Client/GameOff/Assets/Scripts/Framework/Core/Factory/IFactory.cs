using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public interface IObjectFactory<T>
    {
        /// <summary>
        /// 创建对象
        /// </summary>
        /// <returns></returns>
        T Create();
    }
}
