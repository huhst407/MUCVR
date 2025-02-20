using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class PointCubemapMsg : MsgBase {
    public PointCubemapMsg() { protoName = "PointCubemapMsg"; }
    public int x;
    public int y;
    public int z;
    public int face;
    public byte[] jpg_bytes;
}
