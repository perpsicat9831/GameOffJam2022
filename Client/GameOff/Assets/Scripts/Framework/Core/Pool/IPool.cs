using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public interface IPool<T>
    {
        /// <summary>
        /// �������
        /// </summary>
        /// <returns></returns>
        T Allocate();

        /// <summary>
        /// ���ն���
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        bool Recycle(T obj);
    }
    public interface IPoolable
    {
        /// <summary>
        /// ����(�ɹ������յ������ʱ����)
        /// </summary>
        void OnRecycled();
        /// <summary>
        /// �Ѿ�������
        /// </summary>
        bool IsRecycled { get; set; }
        /// <summary>
        /// ���� (����ʧ�ܣ������clear��ʱ������)
        /// </summary>
        void OnDestroy();
    }
    public interface IPoolType
    {
        /// <summary>
        /// ���ն���
        /// </summary>
        void Recycle2Cache();
    }
}
