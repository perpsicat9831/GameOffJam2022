using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Framework
{
    public class Debuger
    {
        /// <summary>
        /// Check if a object null�������������ӡError�������жϵ�ǰ����
        /// </summary>
        public static bool Check(object obj, string formatStr = null, params object[] args)
        {
            if (obj != null) return true;

            if (string.IsNullOrEmpty(formatStr))
                formatStr = "[Check Null] Failed!";

            Log.Error("[!!!]" + formatStr, args);
            return false;
        }

        /// <summary>
        /// �����������ӡError�������жϵ�ǰ����
        /// </summary>
        public static bool Check(bool result, string formatStr = null, params object[] args)
        {
            if (result) return true;

            if (string.IsNullOrEmpty(formatStr))
                formatStr = "Check Failed!";

            Log.Error("[!!!]" + formatStr, args);
            return false;
        }

        /// <summary>
        /// ������������жϵ�ǰ����
        /// </summary>
        public static void Assert(bool result)
        {
            Assert(result, null);
        }

        /// <summary>
        /// ������������жϵ�ǰ����
        /// </summary>
        /// <param name="msg">����ʱ��error��־</param>
        public static void Assert(bool result, string msg, params object[] args)
        {
            if (!result)
            {
                string formatMsg = "Assert Failed! ";
                if (!string.IsNullOrEmpty(msg))
                    formatMsg += string.Format(msg, args);

                Log.LogErrorWithStack(formatMsg, 2);

                throw new Exception(formatMsg); // �жϵ�ǰ����
            }
        }

        /// <summary>
        /// ��ǰֵ�Ƿ�!=0
        /// </summary>
        public static void Assert(int result)
        {
            Assert(result != 0);
        }

        public static void Assert(Int64 result)
        {
            Assert(result != 0);
        }

        /// <summary>
        /// �������Ƿ�Ϊnull��������������жϵ�ǰ����
        /// </summary>
        public static void Assert(object obj)
        {
            Assert(obj != null);
        }
    }
}
