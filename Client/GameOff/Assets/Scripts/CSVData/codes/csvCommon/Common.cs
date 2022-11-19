
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



namespace cfg.csvCommon
{

public sealed class Common :  Bright.Config.BeanBase 
{
    public Common(JSONNode _json) 
    {
        { if(!_json["id"].IsNumber) { throw new SerializationException(); }  Id = _json["id"]; }
        { if(!_json["game_time"].IsNumber) { throw new SerializationException(); }  GameTime = _json["game_time"]; }
        { if(!_json["delivery_init"].IsNumber) { throw new SerializationException(); }  DeliveryInit = _json["delivery_init"]; }
        { if(!_json["delivery_durality"].IsNumber) { throw new SerializationException(); }  DeliveryDurality = _json["delivery_durality"]; }
        { if(!_json["delivery_refresh"].IsNumber) { throw new SerializationException(); }  DeliveryRefresh = _json["delivery_refresh"]; }
    }

    public Common(int id, int game_time, int delivery_init, int delivery_durality, int delivery_refresh ) 
    {
        this.Id = id;
        this.GameTime = game_time;
        this.DeliveryInit = delivery_init;
        this.DeliveryDurality = delivery_durality;
        this.DeliveryRefresh = delivery_refresh;
    }

    public static Common DeserializeCommon(JSONNode _json)
    {
        return new csvCommon.Common(_json);
    }

    /// <summary>
    /// ID
    /// </summary>
    public int Id { get; private set; }
    /// <summary>
    /// 游戏总时长（秒）
    /// </summary>
    public int GameTime { get; private set; }
    /// <summary>
    /// 首个空投时间（秒）
    /// </summary>
    public int DeliveryInit { get; private set; }
    /// <summary>
    /// 空投存在时间（秒）
    /// </summary>
    public int DeliveryDurality { get; private set; }
    /// <summary>
    /// 空投刷新间隔（秒）
    /// </summary>
    public int DeliveryRefresh { get; private set; }

    public const int __ID__ = -962901240;
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
        + "GameTime:" + GameTime + ","
        + "DeliveryInit:" + DeliveryInit + ","
        + "DeliveryDurality:" + DeliveryDurality + ","
        + "DeliveryRefresh:" + DeliveryRefresh + ","
        + "}";
    }
    }
}
