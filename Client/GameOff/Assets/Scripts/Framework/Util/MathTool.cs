using System;
using System.Collections;
using System.Collections.Generic;

namespace Framework
{

    public class MathTool
    {
        public static int randomSeed = 0;
        /// <summary>
        /// �ܹ��������ֵ(�����)
        /// </summary>
        public static int GetRandom(int min, int max)
        {
            Random r = new Random(Guid.NewGuid().GetHashCode());
            return r.Next(min, max + 1);
        }
        /// <summary>
        /// �ܹ��������ֵ(�����)
        /// </summary>
        public static int GetRandom(int max)
        {
            Random r = new Random(Guid.NewGuid().GetHashCode());
            return r.Next(max + 1);
        }

        /// <summary>
        /// Ȩ�ؼ����� keyListȡֵ�б� wListȨ���б�
        /// </summary>
        public static int GetWeight(List<int> keyList, List<int> wList)
        {
            if (keyList.Count == wList.Count)
            {
                int sum = 0;
                for (int i = 0; i < wList.Count; i++)
                {
                    sum += wList[i];
                }
                int w = GetRandom(sum);
                int deltaW = 0;
                for (int i = 0; i < wList.Count; i++)
                {
                    deltaW += wList[i];
                    if (w <= deltaW)
                    {
                        return keyList[i];
                    }
                }
            }
            else
            {
                Log.Error("Ȩ�ؼ������ȡֵ�б� �� Ȩ���б� ���Ȳ����");
            }
            return 0;
        }

        /// <summary>
        /// �����ȡ�б��е�һ��ֵ
        /// </summary>
        public static int GetRadomListValue(List<int> keyList)
        {
            if (keyList != null && keyList.Count > 0)
            {
                int index = GetRandom(0, keyList.Count - 1);
                return keyList[index];
            }
            return 0;
        }
    }
}
