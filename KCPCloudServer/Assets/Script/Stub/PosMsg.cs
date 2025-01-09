using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class PosMsg : MsgBase
{
    public PosMsg() { protoName = "PosMsg"; }
    public float x;
    public float y;
    public float z;
}

public class TaskUnit {
    public int connectionId;
    public MsgBase msg;
}