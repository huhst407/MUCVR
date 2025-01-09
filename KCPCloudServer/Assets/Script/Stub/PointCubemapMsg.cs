using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCubemapMsg : MsgBase
{
    public PointCubemapMsg() { protoName = "PointCubemapMsg"; }
    public float x;
    public float y;
    public float z;
    public int face;
    public byte[] jpg_bytes;
}
