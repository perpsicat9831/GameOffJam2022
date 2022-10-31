using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// Some tool function for time, bytes, MD5, or something...
    /// </summary>
    public class Tool
    {
        private static readonly Dictionary<string, Shader> CacheShaders = new Dictionary<string, Shader>();
        // Shader.Find��һ���ǳ����ĵĺ�������˾�����������

        /// <summary>
        /// Whether In Wifi or Cable Network
        /// </summary>
        /// <returns></returns>
        public static bool IsWifi()
        {
            return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
        }


        /// <summary>
        /// ��ȡ�����2�η�
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static int GetNearestPower2(int num)
        {
            return (int)(Mathf.Pow(2, Mathf.Ceil(Mathf.Log(num) / Mathf.Log(2))));
        }

        /// <summary>
        /// �ж�һ�����Ƿ�2�Ĵη�
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static bool CheckPow2(int num)
        {
            int i = 1;
            while (true)
            {
                if (i > num)
                    return false;
                if (i == num)
                    return true;
                i = i * 2;
            }
        }

        /// <summary>
        /// ģ�� NGUISelectionTool��ͬ����������λ����ת��������
        /// </summary>
        /// <param name="t"></param>
        public static void ResetLocalTransform(Transform t)
        {
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        // ���Լ��
        public static int GetGCD(int a, int b)
        {
            if (a < b)
            {
                int t = a;
                a = b;
                b = t;
            }
            while (b > 0)
            {
                int t = a % b;
                a = b;
                b = t;
            }
            return a;
        }

        /// <summary>
        /// Find Type from every assembly
        /// </summary>
        /// <param name="type"></param>
        /// <param name="qualifiedTypeName"></param>
        /// <returns></returns>
        public static Type FindType(string qualifiedTypeName)
        {
            Type t = Type.GetType(qualifiedTypeName);

            if (t != null)
            {
                return t;
            }
            else
            {
                Assembly[] Assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int n = 0; n < Assemblies.Length; n++)
                {
                    Assembly asm = Assemblies[n];
                    t = asm.GetType(qualifiedTypeName);
                    if (t != null)
                        return t;
                }
                return null;
            }
        }


        /// <summary>
        /// ���һ��GameObject�������еĺ���
        /// </summary>
        /// <param name="go"></param>
        public static void DestroyGameObjectChildren(GameObject go)
        {
            var tran = go.transform;

            while (tran.childCount > 0)
            {
                var child = tran.GetChild(0);

                if (Application.isEditor && !Application.isPlaying)
                {
                    child.parent = null; // ��ո�, ��Ϊ.Destroy��ͬ����
                    GameObject.DestroyImmediate(child.gameObject);
                }
                else
                {
                    GameObject.Destroy(child.gameObject);
                    // Ԥ�����������OnEnable����Destroy
                    child.parent = null; // ��ո�, ��Ϊ.Destroy��ͬ����
                }
            }
        }

        /// <summary>
        /// �ֵ�ת���ַ���A:1|B:2|C:3����
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <param name="dict"></param>
        /// <param name="delimeter1"></param>
        /// <param name="delimeter2"></param>
        /// <returns></returns>
        public static string DictToSplitStr<T, K>(Dictionary<T, K> dict, char delimeter1 = '|', char delimeter2 = ':')
        {
            var sb = new StringBuilder();
            foreach (var kv in dict)
            {
                sb.AppendFormat("{0}{1}{2}{3}", kv.Key, delimeter2, kv.Value, delimeter1);
            }
            return sb.ToString();
        }

        /// <summary>
        /// A:1|B:2|C:3�����ַ���ת���ֵ�
        /// </summary>
        /// <typeparam name="T">string</typeparam>
        /// <typeparam name="K">string</typeparam>
        /// <param name="str">ԭʼ�ַ���</param>
        /// <param name="delimeter1">�ָ���1</param>
        /// <param name="delimeter2">�ָ���2</param>
        /// <returns></returns>
        public static Dictionary<T, K> SplitToDict<T, K>(string str, char delimeter1 = '|', char delimeter2 = ':')
        {
            var dict = new Dictionary<T, K>();
            if (!string.IsNullOrEmpty(str))
            {
                string[] strs = str.Split(delimeter1);
                foreach (string s in strs)
                {
                    string trimS = s.Trim();
                    if (!string.IsNullOrEmpty(trimS))
                    {
                        string[] strs2 = trimS.Split(delimeter2);
                        K valK = default(K);
                        T valT = default(T);
                        if (strs2.Length > 0)
                        {
                            valT = (T)Convert.ChangeType(strs2[0], typeof(T));
                        }
                        if (strs2.Length == 2)
                        {
                            valK = (K)Convert.ChangeType(strs2[1], typeof(K));
                        }
                        dict[valT] = valK;
                    }
                }
            }
            return dict;
        }

        /// <summary>
        /// �ض��ַ����������
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static List<T> Split<T>(string str, params char[] args)
        {
            if (args.Length == 0)
            {
                args = new[] { '|' }; // Ĭ��
            }

            var retList = new List<T>();
            if (!string.IsNullOrEmpty(str))
            {
                string[] strs = str.Split(args);

                foreach (string s in strs)
                {
                    string trimS = s.Trim();
                    if (!string.IsNullOrEmpty(trimS))
                    {
                        T val = (T)Convert.ChangeType(trimS, typeof(T));
                        if (val != null)
                        {
                            retList.Add(val);
                        }
                    }
                }
            }
            return retList;
        }

        /// <summary>
        /// ��һ��List�������ȡ
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T GetRandomItemFromList<T>(IList<T> list)
        {
            if (list.Count == 0)
                return default(T);

            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        /// <summary>
        /// ���������������
        /// </summary>
        /// <param name="waveNumberStr"></param>
        /// <returns></returns>
        public static int GetWaveRandomNumberInt(string waveNumberStr)
        {
            return Mathf.RoundToInt(GetWaveRandomNumber(waveNumberStr));
        }

        /// <summary>
        /// ��ȡ���������,   ���1�����1~2���������ַ����з���һ������
        /// ����"1"��ֱ�ӷ���1
        /// �����"1~10"�����ģ���ô�������1~10�м�һ����
        /// </summary>
        /// <param name="waveNumberStr"></param>
        /// <returns></returns>
        public static float GetWaveRandomNumber(string waveNumberStr)
        {
            if (string.IsNullOrEmpty(waveNumberStr))
                return 0;

            var strs = waveNumberStr.Split('-', '~');
            if (strs.Length == 1)
            {
                return waveNumberStr.ToFloat();
            }

            return UnityEngine.Random.Range(strs[0].ToFloat(), strs[1].ToFloat());
        }


        public struct FromToNumber
        {
            public float From;
            public float To;
        }

        /// <summary>
        /// ��ȡ����������������С
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static FromToNumber ParseMinMaxNumber(string str)
        {
            var rangeArr = Tool.Split<float>(str, '~', '-');
            var number = new FromToNumber();
            if (rangeArr.Count > 0)
            {
                number.From = rangeArr[0];
            }
            if (rangeArr.Count > 1)
            {
                number.To = rangeArr[1];
            }
            return number;
        }

        /// <summary>
        /// �Ƿ��ڲ�����֮��
        /// </summary>
        /// <param name="waveNumberStr"></param>
        /// <param name="testNumber"></param>
        /// <returns></returns>
        public static bool IsBetweenWave(string waveNumberStr, int testNumber)
        {
            if (string.IsNullOrEmpty(waveNumberStr))
                return false;

            var strs = waveNumberStr.Split('~');
            if (strs.Length == 1)
            {
                return strs[0].ToInt32() == testNumber;
            }
            var min = strs[0].ToInt32();
            var max = strs[1].ToInt32();
            return testNumber >= min && testNumber <= max;
        }

        /// <summary>
        /// �Ƿ�����ڶ���������
        /// </summary>
        /// <param name="numberStr">�����ַ���</param>
        /// <param name="testValue">������ֵ</param>
        /// <param name="sp">����ָ���</param>
        /// <returns></returns>
        public static bool IsContains(string numberStr, string testValue, char sp = ',')
        {
            if (string.IsNullOrEmpty(numberStr))
                return false;

            var strs = numberStr.Split(sp);
            return strs.KContains(testValue);
        }

        public static Shader FindShader(string shaderName)
        {
            Shader shader;
            if (!CacheShaders.TryGetValue(shaderName, out shader))
            {
                shader = Shader.Find(shaderName);
                CacheShaders[shaderName] = shader;
                if (shader == null)
                    Log.Error("ȱ��Shader��{0}  �� ���Graphics Settings��Ԥ��shader", shaderName);
            }

            return shader;
        }

        public static byte[] StructToBytes(object structObject)
        {
            int structSize = Marshal.SizeOf(structObject);
            byte[] bytes = new byte[structSize];
            GCHandle bytesHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            IntPtr bytesPtr = bytesHandle.AddrOfPinnedObject();
            Marshal.StructureToPtr(structObject, bytesPtr, false);

            if (bytesHandle.IsAllocated)
                bytesHandle.Free();

            return bytes;
        }

        public static T BytesToStruct<T>(byte[] bytes, int offset = 0)
        {
            GCHandle bytesHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            IntPtr bytesPtr = (IntPtr)(bytesHandle.AddrOfPinnedObject().ToInt32() + offset);

            T structObject = (T)Marshal.PtrToStructure(bytesPtr, typeof(T));

            if (bytesHandle.IsAllocated)
                bytesHandle.Free();

            return structObject;
        }

        // ���ַ���ת��ָ�����͵����� , ��Ԫ������Test_StrBytesToArray
        public static T[] StrBytesToArray<T>(string str, int arraySize)
        {
            int typeSize = Marshal.SizeOf(typeof(T));
            byte[] strBytes = Encoding.Unicode.GetBytes(str);
            byte[] bytes = new byte[typeSize * arraySize]; // ǿ�������С

            for (int k = 0; k < strBytes.Length; k++)
                bytes[k] = strBytes[k]; // copy

            T[] tArray = new T[bytes.Length / typeSize]; // ���ֽ� ���� ���ͳ��� = �ж��ٸ����Ͷ���

            int offset = 0;
            for (int i = 0; i < tArray.Length; i++)
            {
                object convertedObj = null;
                TypeCode typeCode = Type.GetTypeCode(typeof(T));
                switch (typeCode)
                {
                    case TypeCode.Byte:
                        convertedObj = bytes[offset];
                        break;
                    case TypeCode.Int16:
                        convertedObj = BitConverter.ToInt16(bytes, offset);
                        break;
                    case TypeCode.Int32:
                        convertedObj = BitConverter.ToInt32(bytes, offset);
                        break;
                    case TypeCode.Int64:
                        convertedObj = BitConverter.ToInt64(bytes, offset);
                        break;
                    case TypeCode.UInt16:
                        convertedObj = BitConverter.ToUInt16(bytes, offset);
                        break;
                    case TypeCode.UInt32:
                        convertedObj = BitConverter.ToUInt32(bytes, offset);
                        break;
                    case TypeCode.UInt64:
                        convertedObj = BitConverter.ToUInt64(bytes, offset);
                        break;
                    default:
                        Log.Error("Unsupport Type {0} in StrBytesToArray(), You can custom this.", typeCode);
                        Debuger.Assert(false);
                        break;
                }

                tArray[i] = (T)(convertedObj);
                offset += typeSize;
            }

            return tArray;
        }

        public static uint MakeDword(ushort high, ushort low)
        {
            return ((uint)(((ushort)(((uint)(low)) & 0xffff)) | ((uint)((ushort)(((uint)(high)) & 0xffff))) << 16));
        }

        public static ushort LoWord(uint low)
        {
            return ((ushort)(((uint)(low)) & 0xffff));
        }

        public static ushort HiWord(uint high)
        {
            return ((ushort)((((uint)(high)) >> 16) & 0xffff));
        }

        public static string FormatToAssetUrl(string filePath)
        {
            var newFilePath1 = filePath.Replace("\\", "/");
            var newFilePath2 = newFilePath1.Replace("//", "/").Trim();
            newFilePath2 = newFilePath2.Replace("///", "/").Trim();
            newFilePath2 = newFilePath2.Replace("\\\\", "/").Trim();
            return newFilePath2;
        }

        #region Int + Int = Long

        public static ulong MakeLong(uint high, uint low)
        {
            return ((ulong)high) << 32 | low;
        }
        public static uint HiInt(ulong l)
        {
            return (uint)(l >> 32);
        }

        public static uint LowInt(ulong l)
        {
            return (uint)l;
        }

        #endregion

        public static string HumanizeTimeString(int seconds)
        {
            TimeSpan ts = TimeSpan.FromSeconds(seconds);
            string timeStr = string.Format("{0}{1}{2}{3}",
                ts.Days == 0 ? "" : ts.Days + "��",
                ts.Hours == 0 ? "" : ts.Hours + "Сʱ",
                ts.Minutes == 0 ? "" : ts.Minutes + "����",
                ts.Seconds < 0 ? "0��" : ts.Seconds + "��");

            return timeStr;
        }

        // ͬLua, Lib:GetUtcDay
        public static int GetUtcDay()
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            DateTime now = DateTime.UtcNow;
            var span = now - origin;

            return span.Days;
        }

        public static int GetDeltaMinutes(DateTime origin)
        {
            DateTime now = DateTime.UtcNow;
            var span = now - origin;

            return span.Minutes;
        }

        public static int GetDeltaHours(DateTime origin)
        {
            DateTime now = DateTime.UtcNow;
            var span = now - origin;

            return span.Hours;
        }

        public static int GetDeltaDay(DateTime origin)
        {
            DateTime now = DateTime.UtcNow;
            var span = now - origin;

            return span.Days;
        }

        /// <summary>
        /// Utc����תUtcʱ��
        /// </summary>
        /// <param name="utcTime"></param>
        /// <param name="zone">Ĭ��0ʱ��</param>
        /// <returns></returns>
        public static DateTime GetDateTimeByUtcMilliseconds(long utcTime, int zone = 0)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddMilliseconds(utcTime).AddHours(zone);
        }

        /// <summary>
        /// Utc��תUtcʱ��
        /// </summary>
        /// <param name="unixTimeStamp"></param>
        /// <param name="zone">Ĭ��0ʱ��</param>
        /// <returns></returns>
        public static DateTime GetDateTimeByUtcSeconds(double unixTimeStamp, int zone = 0)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(unixTimeStamp).AddHours(zone);
        }

        /// <summary>
        /// Unix�r�g�����딵
        /// </summary>
        /// <returns></returns>
        public static double GetUtcMilliseconds()
        {
            return GetUtcMilliseconds(DateTime.UtcNow);
        }

        /// <summary>
        /// Unix�r�g�����딵
        /// </summary>
        /// <returns></returns>
        public static double GetUtcMilliseconds(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date - origin;
            return diff.TotalMilliseconds;
        }

        /// <summary>
        /// Unix�r�g���딵
        /// </summary>
        /// <returns></returns>
        public static double GetUtcSeconds()
        {
            return GetUtcSeconds(DateTime.UtcNow);
        }

        /// <summary>
        /// Unix�r�g���딵
        /// </summary>
        /// <returns></returns>
        public static double GetUtcSeconds(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date - origin;
            return diff.TotalSeconds;
        }

        /*  Need CronTab
        /// <summary>
        /// ����cron�Ƿ񴥷�(��ȷ������)
        /// </summary>
        /// <returns></returns>
        public static bool TestCron(string cron)
        {
            var nCron = NCrontab.CrontabSchedule.Parse(cron);
            var now = DateTime.Now; // �����漰���ֻ�����ʱ��, ����ʹ��UtcNow
            var next = nCron.GetNextOccurrence(now, DateTime.Now.AddDays(1));
            Log.DoLog("Cron:{0}, now: {1}, next: {2}", cron, now, next);
            var span = next - now;
            Log.DoLog(span.TotalMinutes.ToString());
            return span.TotalMinutes < 1;
        }
        */

        /// <summary>
        /// ���Ի�������ʾ������ǧ����
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string HumanizeNumber(int number)
        {
            if (number > 100000000)
            {
                return string.Format("{0}{1}", number / 100000000, "��");
            }
            else if (number > 10000000)
            {
                return string.Format("{0}{1}", number / 10000000, "ǧ��");
            }
            else if (number > 1000000)
            {
                return string.Format("{0}{1}", number / 1000000, "����");
            }
            else if (number > 10000)
            {
                return string.Format("{0}{1}", number / 10000, "��");
            }

            return number.ToString();
        }

        public static float GetPercent(float lhs, float rhs)
        {
            if (Math.Abs(lhs) < 0.0000001f || Math.Abs(rhs) < 0.0000001f) return 0;
            return lhs / rhs;
        }

        public static float GetPercent(long lhs, long rhs)
        {
            if (Math.Abs(lhs) < 0.0000001f || Math.Abs(rhs) < 0.0000001f) return 0;
            return (float)lhs / (float)rhs;
        }

        public static float GetPercent(int lhs, int rhs)
        {
            if (Math.Abs(lhs) < 0.0000001f || Math.Abs(rhs) < 0.0000001f) return 0;
            return (float)lhs / (float)rhs;
        }

        // �����ڲ���
        public static string[] Match(string find, string pattern)
        {
            int resultCount = 0;
            string[] result;
            Regex regex;
            Match match;

            regex = new Regex(pattern);
            match = regex.Match(find);

            resultCount = match.Groups.Count - 1;
            if (resultCount < 1)
                return null;

            result = new string[resultCount];
            for (int i = 1; match.Groups[i].Value != ""; i++)
                result[i - 1] = match.Groups[i].Value;

            return result;
        }

        /// <summary>
        /// ģ������
        /// </summary>
        /// <param name="source">ģ������</param>
        /// <param name="data">������Դ[����]</param>
        /// <returns></returns>
        /// <summary>
        /// ģ������
        /// </summary>
        /// <param name="source">ģ������</param>
        /// <param name="datas">ģ���-ֵ��Ӧ����[key1,value1,key2,value2,...]</param>
        /// <returns></returns>
        /// <summary>
        /// ���ģ��
        /// </summary>
        /// <param name="source">ģ������</param>
        /// <param name="data">������Դ[����]</param>
        /// <param name="args">������Դ[����]</param>
        /// <returns></returns>
        //public static string Template(string source, object data, object[] args)
        //{
        //    return FormatArgs(Template(source, data), args);
        //}
        public static string Format(string source, params object[] args)
        {
            return FormatArgs(source, args);
        }

        /// <summary>
        /// ģ���ȡ
        /// </summary>
        /// <param name="source">ģ������</param>
        /// <param name="args">������Դ[����]</param>
        /// <returns></returns>
        public static string FormatArgs(string source, object[] args)
        {
            if (args == null) return source;
            var result = source;
            var regex = new Regex(@"\{\d+\}");
            var matches = regex.Matches(source);
            foreach (Match match in matches)
            {
                var paramRex = match.Value;
                var paramKey = paramRex.Substring(1, paramRex.Length - 2).Trim(); // ��ͷ��β
                try
                {
                    var index = paramKey.ToInt32();
                    if (args.Length > index)
                    {
                        var paramValue = args[index].ToString();
                        result = result.Replace(paramRex, paramValue);
                    }
                }
                catch (Exception)
                {
                    Log.Error("not find argument index: \"{0}\" in array: {1}", paramKey, args);
                }
            }

            return result;
        }

        /// <summary>
        /// Recursively set the game object's layer.
        /// </summary>
        public static void SetLayer(GameObject go, int layer)
        {
            go.layer = layer;

            var t = go.transform;

            for (int i = 0, imax = t.childCount; i < imax; ++i)
            {
                var child = t.GetChild(i);
                SetLayer(child.gameObject, layer);
            }
        }

        /// <summary>
        /// ����uriѰ��ָ���ؼ�
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="findTrans"></param>
        /// <param name="uri"></param>
        /// <param name="isLog"></param>
        /// <returns></returns>
        public static T GetChildComponent<T>(Transform findTrans, string uri, bool isLog = true) where T : Component
        {
            Transform trans = findTrans.Find(uri);
            if (trans == null)
            {
                if (isLog)
                    Log.Error("Get Child Error: " + uri);
                return default(T);
            }

            return (T)trans.GetComponent(typeof(T));
        }

        public static T GetChildComponent<T>(string uri, Transform findTrans, bool isLog = true) where T : Component
        {
            if (findTrans == null)
                return default(T);
            Transform trans = findTrans.Find(uri);
            if (trans == null)
            {
                if (isLog)
                    Log.Error("Get Child Error: " + uri);
                return default(T);
            }

            return (T)trans.GetComponent(typeof(T));
        }

        public static GameObject DFSFindObject(Transform parent, string name)
        {
            for (int i = 0; i < parent.childCount; ++i)
            {
                Transform node = parent.GetChild(i);
                if (node.name == name)
                    return node.gameObject;

                GameObject target = DFSFindObject(node, name);
                if (target != null)
                    return target;
            }

            return null;
        }

        public static GameObject GetGameObject(string name, Transform findTrans, bool isLog = true)
        {
            GameObject obj = DFSFindObject(findTrans, name);
            if (obj == null)
            {
                Log.Error("Find GemeObject Error: " + name);
                return null;
            }

            return obj;
        }

        public static void SetChild(GameObject child, GameObject parent, bool selfPos = false, bool selfRotation = false, bool selfScale = false)
        {
            SetChild(child.transform, parent.transform, selfPos, selfRotation, selfScale);
        }

        public static void SetChild(Transform child, Transform parent, bool selfPos = false, bool selfRotation = false, bool selfScale = false)
        {
            child.SetParent(parent);
            ResetTransform(child, selfPos, selfRotation, selfScale);
        }

        public static void ResetTransform(UnityEngine.Transform transform, bool selfPos = false, bool selfRotation = false, bool selfScale = false)
        {
            if (!selfPos)
                transform.localPosition = UnityEngine.Vector3.zero;
            if (!selfRotation)
                transform.localEulerAngles = UnityEngine.Vector3.zero;

            if (!selfScale)
                transform.localScale = UnityEngine.Vector3.one;
        }

        //��ȡ�Ӹ��ڵ㵽�Լ�������·��
        public static string GetRootPathName(UnityEngine.Transform transform)
        {
            var pathName = "/" + transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                pathName += "/" + transform.name;
            }
            return pathName;
        }

        // ��ȡָ������MD5
        public static string MD5_Stream(Stream stream)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider get_md5 =
                new System.Security.Cryptography.MD5CryptoServiceProvider();

            byte[] hash_byte = get_md5.ComputeHash(stream);

            string resule = System.BitConverter.ToString(hash_byte);

            resule = resule.Replace("-", "");

            return resule;
        }

        public static string MD5_File(string filePath)
        {
            try
            {
                using (FileStream get_file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    return MD5_Stream(get_file);
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public static byte[] MD5_bytes(string str)
        {
            // MD5 �ļ���
            var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            return md5.ComputeHash(System.Text.Encoding.Unicode.GetBytes(str));
        }

        // �ַ���16λ MD5
        public static string MD5_16bit(string str)
        {
            byte[] md5bytes = MD5_bytes(str);
            str = System.BitConverter.ToString(md5bytes, 4, 8);
            str = str.Replace("-", "");
            return str;
        }

        public static T ToEnum<T>(string e)
        {
            return (T)Enum.Parse(typeof(T), e);
        }


        // ���ν�����+1��-1 , kk
        public static int IntLerp(int from, int to)
        {
            if (from > to)
                return from - 1;
            else if (from < to)
                return from + 1;
            else
                return from;
        }
#if UNITY_5
        // ������Ч��������
        public static void ScaleParticleSystem(GameObject gameObj, float scale)
        {
            var notFind = true;
            foreach (ParticleSystem p in gameObj.GetComponentsInChildren<ParticleSystem>(true))
            {
                notFind = false;
                p.startSize *= scale;
                p.startSpeed *= scale;
                p.startRotation *= scale;
                p.transform.localScale *= scale;
            }
            if (notFind)
            {
                gameObj.transform.localScale = new Vector3(scale, scale, 1);
            }
        }
#endif
        //��������ϵͳ��RenderQueue
        public static void SetParticleSystemRenderQueue(Transform parent, int renderQueue = 3900)
        {
            int childCount = parent.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.childCount > 0)
                    SetParticleSystemRenderQueue(child, renderQueue);
                if (child.GetComponent<ParticleSystem>() != null)
                {
                    var particleSystem = child.GetComponent<ParticleSystem>();
                    if (particleSystem.GetComponent<Renderer>().sharedMaterial != null)
                        particleSystem.GetComponent<Renderer>().sharedMaterial.renderQueue = renderQueue;
                }
            }
            if (parent.GetComponent<ParticleSystem>() != null)
            {
                var particleSystem = parent.GetComponent<ParticleSystem>();
                //bug ��ͬһ�������ж��ʹ����ͬ��Materialʱ�����������Material�ڹرպ�ᱻ�ͷ�
                if (particleSystem.GetComponent<Renderer>().sharedMaterial != null)
                    particleSystem.GetComponent<Renderer>().sharedMaterial.renderQueue = renderQueue;
            }
        }

        public static void CopyTransformToTarget(Transform sourceTrans, Transform targetTrans)
        {
            targetTrans.localPosition = sourceTrans.localPosition;
            targetTrans.localRotation = sourceTrans.localRotation;
            targetTrans.localScale = sourceTrans.localScale;
        }

        // ��������дȨ��
        public static bool HasWriteAccessToFolder(string folderPath)
        {
            try
            {
                string tmpFilePath = Path.Combine(folderPath, Path.GetRandomFileName());
                using (
                    FileStream fs = new FileStream(tmpFilePath, FileMode.CreateNew, FileAccess.ReadWrite,
                        FileShare.ReadWrite))
                {
                    StreamWriter writer = new StreamWriter(fs);
                    writer.Write("1");
                }
                File.Delete(tmpFilePath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void SetStaticRecursively(GameObject obj)
        {
            LinkedList<Transform> transList = new LinkedList<Transform>();
            transList.AddLast(obj.transform);

            while (transList.Count > 0)
            {
                Transform trans = transList.First.Value;
                transList.RemoveFirst();

                trans.gameObject.isStatic = true;

                for (int i = 0; i < trans.childCount; ++i)
                {
                    transList.AddLast(trans.GetChild(i));
                }
            }
        }

        // ��׼�Ƕȣ��Ƕȸ�����ת��
        public static float GetNormalizedAngle(float _anyAngle)
        {
            _anyAngle = _anyAngle % 360;
            if (_anyAngle < 0)
            {
                _anyAngle += 360;
            }

            return _anyAngle;
        }

        // ���������collider��ָ��ȫ�����
        public static void AdjustCollidersCenterZ(GameObject gameObj, float z = 0)
        {
            foreach (Collider collider in gameObj.GetComponentsInChildren<Collider>())
            {
                if (collider is BoxCollider)
                {
                    BoxCollider boxC = (BoxCollider)collider;
                    float gameObjectZ = boxC.center.z + collider.gameObject.transform.position.z - z;
                    boxC.center = new Vector3(boxC.center.x, boxC.center.y, -gameObjectZ); // �������-1��colliderŪ��1��ƽ����
                }
                else if (collider is SphereCollider)
                {
                    SphereCollider sphereC = (SphereCollider)collider;
                    float globalZ = sphereC.center.z + collider.gameObject.transform.position.z - z;
                    sphereC.center = new Vector3(sphereC.center.x, sphereC.center.y, -globalZ);
                }
            }
        }

        // ����ĸ��д
        public static string ToTitleCase(string word)
        {
            return word.Substring(0, 1).ToUpper() + (word.Length > 1 ? word.Substring(1).ToLower() : "");
        }

        // ����ĸ��д���»���
        public static string ToSentenceCase(string str)
        {
            str = char.ToLower(str[0]) + str.Substring(1);
            return Regex.Replace(str, "[a-z][A-Z]",
                (m) => { return char.ToLower(m.Value[0]) + "_" + char.ToLower(m.Value[1]); });
        }

        // ���ʣ��ٷֱ�, // ע�⣬0��ʱ����100%
        public static bool Probability(float chancePercent)
        {
            var chance = UnityEngine.Random.Range(0f, 100f);

            if (chance <= chancePercent) // ����
            {
                return true;
            }

            return false;
        }

        public static bool Probability(byte chancePercent)
        {
            int chance = UnityEngine.Random.Range(1, 101);

            if (chance <= chancePercent) // ����
            {
                return true;
            }

            return false;
        }

        // ���Mֵ���^
        public static bool ArraysEqual<T>(T[] a1, T[] a2)
        {
            if (ReferenceEquals(a1, a2))
                return true;

            if (a1 == null || a2 == null)
                return false;

            if (a1.Length != a2.Length)
                return false;

            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < a1.Length; i++)
            {
                if (!comparer.Equals(a1[i], a2[i])) return false;
            }
            return true;
        }

        /// <summary>
        /// �ʻ��Σ�����һ���ο�����ݽǶȺͰ뾶���������λ�õ�����
        /// </summary>
        /// <param name="nNum">��Ҫ������</param>
        /// <param name="pAnchorPos">ê����/�ο���</param>
        /// <param name="fAngle">�Ƕ�</param>
        /// <param name="nRadius">�뾶</param>
        /// <returns></returns>
        public static Vector3[] GetSmartNpcPoints(Vector3 startDirection, int nNum, Vector3 pAnchorPos, float fAngle,
            float nRadius)
        {
            bool bPlural = nNum % 2 == 0 ? true : false; // �Ƿ���ģʽ
            Vector3 vDir = startDirection;
            int nMidNum = bPlural ? nNum / 2 : nNum / 2 + 1; // �м���, ѭ�������м�������һ���������Ų�
            Vector3 vRPos = vDir * nRadius; //// ����ֱ����Բ���ϵĶ��� �뾶�Ǳ��־���
            Vector3[] targetPos = new Vector3[nNum];
            for (int i = 1; i <= nNum; i++)
            {
                float nAddAngle = 0;

                if (bPlural) // ����ģʽ
                {
                    if (i > nMidNum)
                        nAddAngle = fAngle * ((i % nMidNum) + 1) - fAngle / 2;
                    else
                        nAddAngle = -fAngle * ((i % nMidNum) + 1) + fAngle / 2; // ����2����Ϊ�˶���NPC�����Ų� by KK
                }
                else // ����ģʽ
                {
                    // �ж��Ƿ�����м���
                    if (i > nMidNum)
                    {
                        nAddAngle = fAngle * (i % nMidNum); // ���NPC�ĽǶ�
                    }
                    else if (i < nMidNum) // �Ǹ���ģʽ�� �м���NPC ����������
                    {
                        nAddAngle = -fAngle * (i % nMidNum); // ������Ƕ�
                    }
                    else
                        nAddAngle = 0; // ������
                }

                Vector3 vTargetPos = pAnchorPos + Quaternion.AngleAxis(nAddAngle, Vector3.forward) * vRPos;
                targetPos[i - 1] = vTargetPos;
            }
            return targetPos;
        }

        /// ����localPoint
        public static Vector3[] GetParallelPoints(Vector3 startPos, Vector3 startDirection, int nNum, float meterInterval)
        {
            bool bPlural = nNum % 2 == 0 ? true : false; // �Ƿ���ģʽ
            int nMidNum = bPlural ? nNum / 2 : nNum / 2 + 1; // �м���, ѭ�������м�������һ���������Ų�
            Vector3[] targetPos = new Vector3[nNum];
            for (int i = 1; i <= nNum; i++)
            {
                float fAddInterval = 0;

                if (bPlural) // ����ģʽ
                {
                    if (i > nMidNum)
                        fAddInterval = meterInterval * ((i % nMidNum) + 1) - meterInterval / 2;
                    else
                        fAddInterval = -meterInterval * ((i % nMidNum) + 1) + meterInterval / 2; // ����2����Ϊ�˶���NPC�����Ų� by KK
                }
                else // ����ģʽ
                {
                    // �ж��Ƿ�����м���
                    if (i > nMidNum)
                    {
                        fAddInterval = meterInterval * ((i % nMidNum) + 1); // ���NPC�ĽǶ�
                    }
                    else if (i < nMidNum) // �Ǹ���ģʽ�� �м���NPC ����������
                    {
                        fAddInterval = -meterInterval * ((i % nMidNum) + 1); // ������Ƕ�
                    }
                    else
                        fAddInterval = 0; // ������
                }

                // 90����ת��ֱ
                Vector3 vTargetPos = startPos + Quaternion.AngleAxis(90, Vector3.forward) * startDirection * fAddInterval;
                //Vector3 vTargetPos = direction*fAddInterval;
                targetPos[i - 1] = vTargetPos;
            }
            return targetPos;
        }

        // ���߽��㣨���Գ��ȣ�
        public static bool LineIntersectionPoint(out Vector2 intersectPoint, Vector2 ps1, Vector2 pe1, Vector2 ps2,
            Vector2 pe2)
        {
            intersectPoint = Vector2.zero;

            // Get A,B,C of first line - points : ps1 to pe1
            float A1 = pe1.y - ps1.y;
            float B1 = ps1.x - pe1.x;
            float C1 = A1 * ps1.x + B1 * ps1.y;

            // Get A,B,C of second line - points : ps2 to pe2
            float A2 = pe2.y - ps2.y;
            float B2 = ps2.x - pe2.x;
            float C2 = A2 * ps2.x + B2 * ps2.y;

            // Get delta and check if the lines are parallel
            float delta = A1 * B2 - A2 * B1;
            if (delta == 0)
                return false;

            // now return the Vector2 intersection point
            intersectPoint = new Vector2(
                (B2 * C1 - B1 * C2) / delta,
                (A1 * C2 - A2 * C1) / delta
                );
            return true;
        }

        /// <summary>
        /// �ж��ַ����Ƿ�������
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNumber(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                Log.Warning("�����ֵΪ�գ�����");
                return false;
            }
            var pattern = @"^\d*$";
            return Regex.IsMatch(str, pattern);
        }


        /// <summary>
        /// ��ȡ��Բ�ϵ�ĳһ�㣬�������
        /// </summary>
        /// <param name="�����ἴĿ�����"></param>
        /// <param name="�̰���"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static Vector2 GetRelativePositionOfEllipse(float �����ἴĿ�����, float �̰���, float angle)
        {
            var rad = angle * Mathf.Deg2Rad; // ����
            var newPos = new Vector2(�����ἴĿ����� * Mathf.Cos(rad), �̰��� * Mathf.Sin(rad));
            return newPos;
        }

        public static float Angle(Vector2 from, Vector2 to)
        {
            return Quaternion.FromToRotation(from.normalized, to.normalized).eulerAngles.z;
        }

        /// <summary>
        /// �����ָ�ʽ������λ , �ָ�
        /// </summary>
        public static string NumberFormatTo3(Int64 num, string sp = ",")
        {
            return num.ToString("##" + sp + "###", System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// ȡ��������ip
        /// </summary>
        /// <returns></returns>
        public static string GetLocalIP()
        {
            try
            {
                string HostName = Dns.GetHostName(); //�õ�������
                IPHostEntry IpEntry = Dns.GetHostEntry(HostName);
                for (int i = 0; i < IpEntry.AddressList.Length; i++)
                {
                    //��IP��ַ�б���ɸѡ��IPv4���͵�IP��ַ
                    //AddressFamily.InterNetwork��ʾ��IPΪIPv4,AddressFamily.InterNetworkV6��ʾ�˵�ַΪIPv6����
                    if (IpEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        return IpEntry.AddressList[i].ToString();
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Get IPAdress from IpHostEntry,  ���GetIpAddress
        /// </summary>
        /// <param name="ipHostEntry"></param>
        /// <returns></returns>
        public static IPAddress GetIpAddressFromIpHostEntry(IPHostEntry ipHostEntry)
        {
            var addresses = ipHostEntry.AddressList;

            foreach (var item in addresses)
            {
                if (item.AddressFamily == AddressFamily.InterNetwork)
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        /// Async Get IPAdress
        /// </summary>
        /// <param name="host"></param>
        /// <param name="callback"></param>
        public static void GetIpAddress(string host, Action<IPAddress> callback = null)
        {
            IPAddress ipAddress = null;
            if (!IPAddress.TryParse(host, out ipAddress))
            {
                Dns.BeginGetHostAddresses(host, new AsyncCallback((asyncResult) =>
                {
                    IPAddress[] addrs = Dns.EndGetHostAddresses(asyncResult);
                    if (callback != null)
                    {
                        if (addrs.Length > 0)
                            ipAddress = addrs[0];
                        callback(ipAddress);
                    }
                }), null);
            }
            else
            {
                if (callback != null)
                    callback(ipAddress);
            }
        }

        public static List<GameObject> GetAllChild(GameObject obj)
        {
            List<GameObject> objList = new List<GameObject>();
            objList.Add(obj);
            var childCount = obj.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var child = obj.transform.GetChild(i).gameObject;
                var childList = GetAllChild(child);
                objList.AddRange(childList);
            }

            return objList;
        }

        /// <summary>
        /// �ļ���С��ʽ����ʾ��KB��MB,GB
        /// </summary>
        /// <param name="size">�ֽ�</param>
        public static String FormatFileSize(long size)
        {
            int GB = 1024 * 1024 * 1024;
            int MB = 1024 * 1024;
            int KB = 1024;

            if (size / GB >= 1)
            {
                return Math.Round(size / (float)GB, 2) + "GB";
            }

            if (size / MB >= 1)
            {
                return Math.Round(size / (float)MB, 2) + "MB";
            }

            if (size / KB >= 1)
            {
                return Math.Round(size / (float)KB, 2) + "KB";
            }

            return size + "B";
        }

        public static void ExitGame()
        {
            if (Application.isEditor)
            {
                Log.LogInfo("�༭���²������˳���Ϸ");
            }
            else
            {
                Application.Quit();
            }
        }

        /// <summary>
        /// ����·����ȡ�ļ���
        /// </summary>
        public static string GetFileNameByPath(string path)
        {
            var str1 = path.Split('/');
            return str1[str1.Length - 1].Split('.')[0];
        }

        /// <summary>
        /// HexתColor
        /// </summary>
        public static Color HexToColor(string hex)
        {
            hex = hex.Replace("0x", string.Empty);
            hex = hex.Replace("#", string.Empty);
            byte a = byte.MaxValue;
            byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);
            }
            //Debug.Log("HexToColor " + r + " | " + g + " | " + b + " | " + a);
            return new Color32(r, g, b, a);
        }
    }

    #region XMemoryParser
    public class XMemoryParser<T>
    {
        private readonly int MaxCount;
        private readonly byte[] SourceBytes;
        private readonly GCHandle SourceBytesHandle;
        private readonly IntPtr SourceBytesPtr;

        public XMemoryParser(byte[] bytes, int maxCount)
        {
            SourceBytes = bytes;
            MaxCount = maxCount;
            SourceBytesHandle = GCHandle.Alloc(SourceBytes, GCHandleType.Pinned);
            SourceBytesPtr = SourceBytesHandle.AddrOfPinnedObject();
        }

        public XMemoryParser(IntPtr bytesPtr, int maxCount)
        {
            MaxCount = maxCount;
            SourceBytesPtr = bytesPtr;
        }

        ~XMemoryParser()
        {
            if (SourceBytesHandle.IsAllocated)
                SourceBytesHandle.Free();
        }

        public T this[int index]
        {
            get
            {
                Debuger.Assert(index < MaxCount);
                IntPtr p = (IntPtr)(SourceBytesPtr.ToInt32() + Marshal.SizeOf(typeof(T)) * index);
                return (T)Marshal.PtrToStructure(p, typeof(T));
            }
        }
        
    }
    #endregion
}
