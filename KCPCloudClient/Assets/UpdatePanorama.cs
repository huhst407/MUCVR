using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class UpdatePanorama : MonoBehaviour
{
    public Dictionary<VecInt3, PointMessage> pointmessages = new Dictionary<VecInt3, PointMessage>();
    public Texture2D tex;
    public Camera camm;
    public VecInt3 lastPos;
    void Start() {
        int width = CENetworkManager.instance.width;
        CENetworkManager.instance.AddMsgListener("PointCubemapMsg", OnPointCubemapMsg);
        tex = new Texture2D(width, width);
        camm = Camera.main;

    }

    private void OnPointCubemapMsg(MsgBase msgBase) {
        PointCubemapMsg msg = (PointCubemapMsg)msgBase;
        VecInt3 pos = new VecInt3(msg.x, msg.y, msg.z);
        if (!pointmessages.ContainsKey(pos)) {
            PointMessage pointMessage = new PointMessage();
            pointmessages.Add(pos, pointMessage);
        }
        tex.LoadImage(msg.jpg_bytes);
        pointmessages[pos].ResolveToPointMessage(pos, msg.face, tex);

    }
    public void MoveUpdateCubemap() {
        VecInt3  temp_pos = new VecInt3((int)camm.transform.position.x, (int)camm.transform.position.y,(int)camm.transform.position.z);
        if (((int)temp_pos.x) == lastPos.x&&((int)temp_pos.y) == lastPos.y && ((int)temp_pos.z) == lastPos.z) return;//如果离散位置没有变化就不发送
        if (!pointmessages.ContainsKey(temp_pos)) {
            PosMsg posMsg = new PosMsg();
            posMsg.x = camm.transform.position.x;
            posMsg.y = camm.transform.position.y;
            posMsg.z = camm.transform.position.z;
            CENetworkManager.instance.Send(posMsg);
        }
        else {
            if (!pointmessages[temp_pos].IsAllFace()) {
                PosMsg posMsg = new PosMsg();
                posMsg.x = camm.transform.position.x;
                posMsg.y = camm.transform.position.y;
                posMsg.z = camm.transform.position.z;
                CENetworkManager.instance.Send(posMsg);
            }
            else {
                //pointmessages[temp_pos].cubemap;
                //todo:更新全景图
            }
        }

    }
    private void Update() {
        
    }
}
