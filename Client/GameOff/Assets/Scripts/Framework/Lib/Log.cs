using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;


namespace Framework
{
    /// <summary>
    /// 自定义log标签
    /// </summary>
    public enum LogTag
    {
        None = 0,
    }
    /// <summary>
    /// log类型
    /// </summary>
    public enum LogType
    {
        Log = 0,
        Warning,
        Error,
    }
    /// <summary>
    /// debug封装层
    /// </summary>
    public class Log
    {
        /// <summary>
        /// 需要在游戏主循环的FixedUpdate里++
        /// </summary>
        public static long TotalFrame;
        private static int mainthreadid = System.Threading.Thread.CurrentThread.ManagedThreadId;

        public static void LogInfo(string err, params object[] args)
        {
            DoLog(err, args, LogType.Log);
        }
        public static void LogInfo(string err, LogTag logTag = LogTag.None, params object[] args)
        {
            DoLog(err, args, LogType.Log, logTag);
        }
        public static void Warning(string err, params object[] args)
        {
            DoLog(err, args, LogType.Warning);
        }
        public static void Warning(string err, LogTag logTag = LogTag.None, params object[] args)
        {
            DoLog(err, args, LogType.Warning, logTag);
        }
        public static void Error(string err, params object[] args)
        {
            LogErrorWithStack(string.Format(err, args), 2);
        }
        public static void Error(string err, LogTag logTag = LogTag.None, params object[] args)
        {
            LogErrorWithStack(string.Format(err, args), 2, logTag);
        }

        public static void LogException(Exception e)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Exception: {0}", e.Message);
            if (e.InnerException != null)
                sb.AppendFormat(" InnerException: {0}", e.InnerException.Message);

            LogErrorWithStack(sb.ToString() + " , " + e.StackTrace);
        }
        private static void DoLog(string szMsg, object[] args, LogType logType = LogType.Log, LogTag logTag = LogTag.None)
        {
            if (args != null)
                szMsg = string.Format(szMsg, args);
            szMsg = string.Format("[{0}] {1}(frame:{2},mem:{3:0.##}MB){4}",
                logTag, DateTime.Now.ToString("HH:mm:ss.fff"), TotalFrame, GetMonoUseMemory(), szMsg);
            switch (logType)
            {
                case LogType.Warning:
                    UnityEngine.Debug.LogWarning(szMsg);
                    break;
                case LogType.Error:
                    UnityEngine.Debug.LogError(szMsg);
                    break;
                default:
                    UnityEngine.Debug.Log(szMsg);
                    break;
            }
        }
        public static StackFrame GetTopStack(int stack = 2)
        {
            StackFrame[] stackFrames = new StackTrace(true).GetFrames();
            StackFrame sf = stackFrames[Math.Min(stack, stackFrames.Length - 1)];
            return sf;
        }
        public static void LogErrorWithStack(string err = "", int stack = 2, LogTag logTag = LogTag.None)
        {
            StackFrame sf = GetTopStack(stack);
            string log = string.Format("{0}\n\n{1}:{2}\t{3}", err, sf.GetFileName(), sf.GetFileLineNumber(),
                sf.GetMethod());
            //Console.Write(log);
            DoLog(log, null, LogType.Error, logTag);
        }
        public static double GetMonoUseMemory()
        {
            var ismain = mainthreadid == System.Threading.Thread.CurrentThread.ManagedThreadId;
            //乘法比除法快，所以/1024改成 *0.0009765625
            var memory = ismain ? UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong() * 0.0009765625 * 0.0009765625 : 0;
            return memory;
        }
    }
}
