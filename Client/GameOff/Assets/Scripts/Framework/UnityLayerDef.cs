using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnityLayerDef
{
    Default = 0,
    TransparentFX = 1,
    IgnoreRaycast = 2,

    Water = 4,
    UI = 5,

    //以下为自定义
    Hidden = 8,
}
