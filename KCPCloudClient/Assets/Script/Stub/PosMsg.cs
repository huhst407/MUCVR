using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using static Sirenix.Utilities.Editor.MultilineWrapLayoutUtility;
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
[Serializable]
public class VecInt3 {
    public int x;
    public int y;
    public int z;
    public VecInt3(int x, int y, int z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    public override bool Equals(object obj) {
        if (obj == null || GetType() != obj.GetType()) {
            return false;
        }
        VecInt3 other = (VecInt3)obj;
        return x == other.x && y == other.y && z == other.z;
    }

    public override int GetHashCode() {
        // 使用一个简单的哈希算法来组合三个整数的哈希码
        unchecked {
            int hash = 17;
            hash = hash * 23 + x.GetHashCode();
            hash = hash * 23 + y.GetHashCode();
            hash = hash * 23 + z.GetHashCode();
            return hash;
        }
    }
}