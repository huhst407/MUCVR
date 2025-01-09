using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PosMsg : MsgBase
{
    public PosMsg() { protoName = "PosMsg"; }
    public float x;
    public float y;
    public float z;
}

