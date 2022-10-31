using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Framework
{
    public class Debuger
    {
        /// <summary>
        /// Check if a object null，条件不满足打印Error，不会中断当前调用
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
        /// 条件不满足打印Error，不会中断当前调用
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
        /// 条件不满足会中断当前调用
        /// </summary>
        public static void Assert(bool result)
        {
            Assert(result, null);
        }

        /// <summary>
        /// 条件不满足会中断当前调用
        /// </summary>
        /// <param name="msg">出错时的error日志</param>
        public static void Assert(bool result, string msg, params object[] args)
        {
            if (!result)
            {
                string formatMsg = "Assert Failed! ";
                if (!string.IsNullOrEmpty(msg))
                    formatMsg += string.Format(msg, args);

                Log.LogErrorWithStack(formatMsg, 2);

                throw new Exception(formatMsg); // 中断当前调用
            }
        }

        /// <summary>
        /// 当前值是否!=0
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
        /// 检查参数是否为null，条件不满足会中断当前调用
        /// </summary>
        public static void Assert(object obj)
        {
            Assert(obj != null);
        }
    }
}
