
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using Bright.Serialization;
using SimpleJSON;

namespace cfg
{
   
public sealed class Tables
{
    public csvSound.TbSound TbSound {get; }

    public Tables(System.Func<string, JSONNode> loader)
    {
        var tables = new System.Collections.Generic.Dictionary<string, object>();
        TbSound = new csvSound.TbSound(loader("csvsound_tbsound")); 
        tables.Add("csvSound.TbSound", TbSound);

        TbSound.Resolve(tables); 
    }

    public void TranslateText(System.Func<string, string, string> translator)
    {
        TbSound.TranslateText(translator); 
    }
}

}