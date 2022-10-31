#if UNITY_EDITOR
using AK.Wwise;
using System;
using System.Collections;
using System.Collections.Generic;
using static AkWwiseProjectData;

namespace AudioStudio.Tools
{
    public static class AsToolWrapper
    {
        public static ArrayList GetWorkUnit<T>(this AkWwiseProjectData awData)
        {
            if (typeof(T) == typeof(AudioEvent) || typeof(T) == typeof(AudioEventExt))
                return ArrayList.Adapter(awData.EventWwu);
            if (typeof(T) == typeof(AudioBank))
                return ArrayList.Adapter(awData.BankWwu);
            if (typeof(T) == typeof(RTPCExt))
                return ArrayList.Adapter(awData.RtpcWwu);
            if (typeof(T) == typeof(AuxBusExt))
                return ArrayList.Adapter(awData.AuxBusWwu);
            if (typeof(T) == typeof(TriggerExt))
                return ArrayList.Adapter(awData.TriggerWwu);
            return null;
        }
        public static AkInformation FindWwiseObject<T>(this AkWwiseProjectData awData, string objectName) where T : BaseType
        {
            ArrayList infomations = awData.GetWorkUnit<T>();
            foreach (var t in infomations)
            {
                AkInformation info = t as AkInformation;
                if (info != null)
                {
                    if (info.Name.Equals(objectName))
                        return info;
                }
            }
            return null;
        }

        public static AkInformation FindWwiseObject<T>(this AkWwiseProjectData awData, Guid objectGuid) where T : BaseType
        {
            ArrayList infomations = awData.GetWorkUnit<T>();
            foreach (var t in infomations)
            {
                if (typeof(T) == typeof(AudioEvent) || typeof(T) == typeof(AudioEventExt))
                {
                    EventWorkUnit ewu = t as EventWorkUnit;
                    foreach (var e in ewu.List)
                    {
                        if (e != null)
                        {
                            if (e.Guid.Equals(objectGuid))
                                return e;
                        }
                    }
                }
                else
                {
                    AkInformation info = t as AkInformation;
                    if (info != null)
                    {
                        if (info.Guid.Equals(objectGuid))
                            return info;
                    }
                }
            }
            return null;
        }

        private static IEnumerable<GroupValWorkUnit> GetGroupWorkUnit<T>(AkWwiseProjectData awData)
        {
            if (typeof(T) == typeof(State) || typeof(T) == typeof(StateExt) || typeof(T) == typeof(ASState))
                return awData.StateWwu;
            if (typeof(T) == typeof(Switch) || typeof(T) == typeof(SwitchEx) || typeof(T) == typeof(AudioSwitch))
                return awData.SwitchWwu;
            return null;
        }

        public static GroupValue FindWwiseGroupUnit<T>(this AkWwiseProjectData awData, Guid groupGuid) where T : BaseGroupType
        {
            foreach (var t in GetGroupWorkUnit<T>(awData))
            {
                var s = t.List.Find(x => x.Guid.Equals(groupGuid));
                if (s != null) return s;
            }
            return null;
        }

        public static GroupValue FindWwiseGroupUnit<T>(this AkWwiseProjectData awData, string groupName) where T : BaseGroupType
        {
            foreach (var t in GetGroupWorkUnit<T>(awData))
            {
                var s = t.List.Find(x => x.Name.Equals(groupName));
                if (s != null) return s;
            }
            return null;
        }

        public static string FindWwiseGroupObjectChildName(Guid guid, GroupValue objGroup)
        {
            foreach (var value in objGroup.values)
                if (value.Guid == guid)
                    return value.Name;
            return string.Empty;
        }

        public static Guid FindWwiseGroupObjectChildGuid(string name, GroupValue objGroup)
        {
            foreach (var value in objGroup.values)
                if (value.Name == name)
                    return value.Guid;
            return Guid.Empty;
        }
    }
}
#endif