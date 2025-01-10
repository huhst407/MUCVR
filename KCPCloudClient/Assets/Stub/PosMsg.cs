using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class PosMsg : MsgBase {
    public PosMsg() { protoName = "PosMsg"; }
    public float x;
    public float y;
    public float z;
}

public class TaskUnit {
    public int connectionId;
    public MsgBase msg;
}
public class VecInt3 {
    public int x;
    public int y;
    public int z;
    public VecInt3(int x, int y, int z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}