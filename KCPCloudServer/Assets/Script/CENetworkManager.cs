using kcp2k;
using Packages.Rider.Editor.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;


public class CENetworkManager : MonoBehaviour {
    public static CENetworkManager instance;
    public KcpServer server;
    MsgBase msg = new MsgBase();
    Camera camm;
    Cubemap cubemap; 
    public int width = 1024;
    public Texture2D tex;
    Queue<TaskUnit> taskQueue = new Queue<TaskUnit>();



    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else {
            Destroy(this);
        }
    }
    // Start is called before the first frame update
    void Start() {
        tex = new Texture2D(width, width);
        camm = Camera.main;
        InitServer();

    }

    // Update is called once per frame
    void Update() {
        server.Tick();

        while (taskQueue.Count > 0) {
            TaskUnit taskUnit = taskQueue.Dequeue();
            PosMsg msg = (PosMsg)taskUnit.msg;
            camm.transform.position = new Vector3(msg.x, msg.y, msg.z);
            if (camm.RenderToCubemap(cubemap)) {
                for (int i = 0; i < 6; i++) {
                    tex.SetPixels(cubemap.GetPixels((CubemapFace)i), 0);
                    tex.Apply();
                    byte[] bytes = tex.EncodeToJPG();
                    PointCubemapMsg pointCubemapMsg = new PointCubemapMsg();
                    pointCubemapMsg.x = msg.x;
                    pointCubemapMsg.y = msg.y;
                    pointCubemapMsg.z = msg.z;
                    pointCubemapMsg.face = i;
                    pointCubemapMsg.jpg_bytes = bytes;
                    Send(taskUnit.connectionId, pointCubemapMsg);
                }
            }
        }

    }
    private void InitServer() {
        KcpConfig config = new KcpConfig(
            // force NoDelay and minimum interval.
            // this way UpdateSeveralTimes() doesn't need to wait very long and
            // tests run a lot faster.
            NoDelay: true,
            // not all platforms support DualMode.
            // run tests without it so they work on all platforms.
            DualMode: false,
            Interval: 1, // 1ms so at interval code at least runs.
            Timeout: 2000,

            // large window sizes so large messages are flushed with very few
            // update calls. otherwise tests take too long.
            SendWindowSize: Kcp.WND_SND * 1000,
            ReceiveWindowSize: Kcp.WND_RCV * 1000,

            // congestion window _heavily_ restricts send/recv window sizes
            // sending a max sized message would require thousands of updates.
            CongestionWindow: false,

            // maximum retransmit attempts until dead_link detected
            // default * 2 to check if configuration works
            MaxRetransmits: Kcp.DEADLINK * 2
        );

        // create server
        server = new KcpServer(
            (connectionId) => { },//连接时回调
            (connectionId, message, channel) => RecallReceived(connectionId, message, channel),//接收到数据时回调
            (connectionId) => { },//断开连接时回调
            (connectionId, error, reason) => Log.Info($"[KCP] OnServerError({connectionId}, {error}, {reason}"),
            config
        );

        server.Start(7777);
    }
    void RecallReceived(int connectionId, ArraySegment<byte> message, KcpChannel channel) {
        Log.Info($"[KCP] OnServerDataReceived({connectionId},  message.Offset: {message.Offset},message.Count{message.Count}bitll{BitConverter.ToString(message.Array, message.Offset, message.Count)} @ {channel})");
        //if((message.Count - message.Offset) > sizeof(Int32)) return; 
        //if(BitConverter.ToInt32(message.Array, message.Offset) < message.Count - message.Offset)  return;
        //num++;
        //string assetpath = Application.dataPath + "/../test/" + num + ".png";
        new Task(() => {
            //byte[] bytes = new byte[message.Count - message.Offset];
            //Array.Copy(message.Array, message.Offset, bytes, 0, message.Count - message.Offset);
            //File.WriteAllBytes(assetpath, bytes);
            try {
                if ((message.Count - message.Offset) < sizeof(Int32)) return;
                if (message.Count - message.Offset < BitConverter.ToInt32(message.Array, message.Offset)) return;
                byte[] bytes = new byte[message.Count - message.Offset - sizeof(Int32)];
                Array.Copy(message.Array, message.Offset + sizeof(Int32), bytes, 0, message.Count - message.Offset - sizeof(Int32));
                MsgBase reMsgbase = msg.Decode(bytes);
                //HandleMsg(connectionId, reMsgbase);
                TaskUnit taskUnit = new TaskUnit();
                taskUnit.connectionId = connectionId;
                taskUnit.msg = reMsgbase;
                lock (taskQueue) {
                    taskQueue.Enqueue(taskUnit);
                }
            }
            catch (Exception e) {
                Log.Info($"connectionId:{connectionId},e.Message:{e.Message}");
            }
        }).Start();
    }
    public void Send(int connectionId, MsgBase msg) {
        byte[] bytes_context = msg.Encode();
        Int32 length = bytes_context.Length;
        byte[] length_bytes = BitConverter.GetBytes(length);
        byte[] bytes = length_bytes.Concat(bytes_context).ToArray();
        server.Send(connectionId, new ArraySegment<byte>(bytes), KcpChannel.Reliable);

    }

}

