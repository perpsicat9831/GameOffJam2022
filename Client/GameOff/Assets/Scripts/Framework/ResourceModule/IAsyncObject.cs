using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public interface IAsyncObject
    {
        /// <summary>
        /// ����Э�̣��̣߳����������
        /// </summary>
        public interface IAsyncObject
        {
            /// <summary>
            /// ���ռ��ؽ������Դ
            /// </summary>
            object AsyncResult { get; }

            /// <summary>
            /// �Ƿ��Ѿ���ɣ����Ĵ�����Loader��������Э��StartCoroutine
            /// </summary>
            bool IsCompleted { get; }

            /// <summary>
            /// ����WWW, IsFinished���ж��Ƿ��д���
            /// </summary>
            bool IsError { get; }

            /// <summary>
            /// ������Ϣ
            /// </summary>
            string AsyncMessage { get; }

            /// <summary>
            /// �Ƿ�ɹ�
            /// </summary>
            bool IsSuccess { get; }
        }

    }
}
