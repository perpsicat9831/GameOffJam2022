using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Logic
{
    /// <summary>
    /// ��װ��ܲ�Ķ����
    /// </summary>
    public class ObjectPool<T> : Framework.ObjectPool<T> where T : IPoolable, new()
    {

    }
}