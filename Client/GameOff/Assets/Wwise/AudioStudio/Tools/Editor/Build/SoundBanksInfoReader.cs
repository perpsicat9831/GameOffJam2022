using System.IO;
using System.Xml.Linq;

namespace AudioStudio.Tools
{
    /// <summary>
    /// Read SoundBanksInfo.xml and get data from it.
    /// </summary>
    public static class SoundBanksInfoReader
    {
        private static XDocument XmlFile
        {
            get { return XDocument.Load(AsScriptingHelper.CombinePath(WwisePathSettings.Instance.EditorBankLoadPathFull, "SoundBanksInfo.xml")); }
        }

        public static XElement SoundBanksNode
        {
            get { return XmlFile.Root.Element("SoundBanks"); }
        }

        public static float GetMaxAttenuation(string eventName)
        {
            foreach (var xBank in SoundBanksNode.Elements())
            {
                var xEvents = xBank.Element("IncludedEvents");
                if (xEvents == null) continue;
                foreach (var xEvent in xEvents.Elements())
                {
                    if (xEvent.Attribute("Name").Value == eventName)
                    {
                        var maxAtt = xEvent.Attribute("MaxAttenuation");
                        if (maxAtt != null)
                            return float.Parse(maxAtt.Value);
                    }
                }   
            }
            return 0f;
        }
    }
}