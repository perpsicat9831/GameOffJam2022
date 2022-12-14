
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
    public csvCommon.TbCommon TbCommon {get; }
    public csvDelivery.TbDelivery TbDelivery {get; }
    public csvDeliveryFish.TbDeliveryFish TbDeliveryFish {get; }
    public csvFish.TbFish TbFish {get; }
    public csvFishingStage.TbFishingStage TbFishingStage {get; }
    public csvItem.TbItem TbItem {get; }
    public csvPlayerProp.TbPlayerProp TbPlayerProp {get; }

    public Tables(System.Func<string, JSONNode> loader)
    {
        var tables = new System.Collections.Generic.Dictionary<string, object>();
        TbSound = new csvSound.TbSound(loader("csvsound_tbsound")); 
        tables.Add("csvSound.TbSound", TbSound);
        TbCommon = new csvCommon.TbCommon(loader("csvcommon_tbcommon")); 
        tables.Add("csvCommon.TbCommon", TbCommon);
        TbDelivery = new csvDelivery.TbDelivery(loader("csvdelivery_tbdelivery")); 
        tables.Add("csvDelivery.TbDelivery", TbDelivery);
        TbDeliveryFish = new csvDeliveryFish.TbDeliveryFish(loader("csvdeliveryfish_tbdeliveryfish")); 
        tables.Add("csvDeliveryFish.TbDeliveryFish", TbDeliveryFish);
        TbFish = new csvFish.TbFish(loader("csvfish_tbfish")); 
        tables.Add("csvFish.TbFish", TbFish);
        TbFishingStage = new csvFishingStage.TbFishingStage(loader("csvfishingstage_tbfishingstage")); 
        tables.Add("csvFishingStage.TbFishingStage", TbFishingStage);
        TbItem = new csvItem.TbItem(loader("csvitem_tbitem")); 
        tables.Add("csvItem.TbItem", TbItem);
        TbPlayerProp = new csvPlayerProp.TbPlayerProp(loader("csvplayerprop_tbplayerprop")); 
        tables.Add("csvPlayerProp.TbPlayerProp", TbPlayerProp);

        TbSound.Resolve(tables); 
        TbCommon.Resolve(tables); 
        TbDelivery.Resolve(tables); 
        TbDeliveryFish.Resolve(tables); 
        TbFish.Resolve(tables); 
        TbFishingStage.Resolve(tables); 
        TbItem.Resolve(tables); 
        TbPlayerProp.Resolve(tables); 
    }

    public void TranslateText(System.Func<string, string, string> translator)
    {
        TbSound.TranslateText(translator); 
        TbCommon.TranslateText(translator); 
        TbDelivery.TranslateText(translator); 
        TbDeliveryFish.TranslateText(translator); 
        TbFish.TranslateText(translator); 
        TbFishingStage.TranslateText(translator); 
        TbItem.TranslateText(translator); 
        TbPlayerProp.TranslateText(translator); 
    }
}

}
