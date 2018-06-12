using System;
using System.Xml.Serialization;

namespace nspector.Common.Import
{
    [Serializable]
    public class ProfileSetting
    {
        public string SettingNameInfo = "";

        [XmlElement(ElementName = "SettingID")]
        public uint SettingId = 0;
        
        public uint SettingValue = 0;
    }
}