using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static partial class Handle {
    public static void PosMsg(int conn, MsgBase msgBase) {
        PosMsg msg = (PosMsg)msgBase;
        Camera.main.transform.position = new Vector3(msg.x, msg.y, msg.z);
        
    }

}
