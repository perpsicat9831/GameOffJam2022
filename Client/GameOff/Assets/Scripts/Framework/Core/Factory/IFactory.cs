using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public interface IObjectFactory<T>
    {
        /// <summary>
        /// ��������
        /// </summary>
        /// <returns></returns>
        T Create();
    }
}
