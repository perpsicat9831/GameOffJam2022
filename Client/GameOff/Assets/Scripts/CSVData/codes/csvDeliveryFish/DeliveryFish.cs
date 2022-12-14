
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using Bright.Serialization;
using System.Collections.Generic;
using SimpleJSON;



namespace cfg.csvDeliveryFish
{

public sealed class DeliveryFish :  Bright.Config.BeanBase 
{
    public DeliveryFish(JSONNode _json) 
    {
        { if(!_json["id"].IsNumber) { throw new SerializationException(); }  Id = _json["id"]; }
        { var _json1 = _json["fish_id"]; if(!_json1.IsArray) { throw new SerializationException(); } FishId = new System.Collections.Generic.List<int>(_json1.Count); foreach(JSONNode __e in _json1.Children) { int __v;  { if(!__e.IsNumber) { throw new SerializationException(); }  __v = __e; }  FishId.Add(__v); }   }
        { var _json1 = _json["fish_num"]; if(!_json1.IsArray) { throw new SerializationException(); } FishNum = new System.Collections.Generic.List<int>(_json1.Count); foreach(JSONNode __e in _json1.Children) { int __v;  { if(!__e.IsNumber) { throw new SerializationException(); }  __v = __e; }  FishNum.Add(__v); }   }
    }

    public DeliveryFish(int id, System.Collections.Generic.List<int> fish_id, System.Collections.Generic.List<int> fish_num ) 
    {
        this.Id = id;
        this.FishId = fish_id;
        this.FishNum = fish_num;
    }

    public static DeliveryFish DeserializeDeliveryFish(JSONNode _json)
    {
        return new csvDeliveryFish.DeliveryFish(_json);
    }

    /// <summary>
    /// ID
    /// </summary>
    public int Id { get; private set; }
    /// <summary>
    /// 对应鱼类ID
    /// </summary>
    public System.Collections.Generic.List<int> FishId { get; private set; }
    /// <summary>
    /// 对应鱼数量
    /// </summary>
    public System.Collections.Generic.List<int> FishNum { get; private set; }

    public const int __ID__ = 783949672;
    public override int GetTypeId() => __ID__;

    public  void Resolve(Dictionary<string, object> _tables)
    {
    }

    public  void TranslateText(System.Func<string, string, string> translator)
    {
    }

    public override string ToString()
    {
        return "{ "
        + "Id:" + Id + ","
        + "FishId:" + Bright.Common.StringUtil.CollectionToString(FishId) + ","
        + "FishNum:" + Bright.Common.StringUtil.CollectionToString(FishNum) + ","
        + "}";
    }
    }
}
