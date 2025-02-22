using kcp2k;
using Packages.Rider.Editor.Util;
using System;
using System.Collections;
using System.Collections.Generic;
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
    public Queue<TaskUnit> taskQueue = new Queue<TaskUnit>();

    Dictionary<VecInt3, PointMessage> pointMessageDic = new Dictionary<VecInt3, PointMessage>();


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
        cubemap = new Cubemap(width, TextureFormat.RGB24, false);
        tex = new Texture2D(width, width);
        camm = Camera.main;
        InitServer();

    }

    // Update is called once per frame
    void Update() {
        server.Tick();//kcp更新

        while (taskQueue.Count > 0) {
            TaskUnit taskUnit = taskQueue.Dequeue();
            PosMsg msg = (PosMsg)taskUnit.msg;
            VecInt3 pos = new VecInt3((int)msg.x, (int)msg.y, (int)msg.z);
            //检查是否已经有这个点的信息
            if(pointMessageDic.ContainsKey(pos)) {
                cubemap = pointMessageDic[pos].cubemap;
            }
            else {
                camm.transform.position = new Vector3(pos.x,pos.y,pos.z);

                TestLog.Log("开始渲染前："+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                if (camm.RenderToCubemap(cubemap)) {
                    TestLog.Log("渲染结束后：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                    //缓存点信息
                    PointMessage pointMessage = new PointMessage();
                    pointMessage.cubemap = cubemap;
                    pointMessage.pos = pos;
                    pointMessageDic.Add(pos, pointMessage);
                    //按照面发送 
                }
                else{
                    cubemap = null;
                    Log.Info("RenderToCubemap failed");
                }
            }
            if (cubemap == null) {
                Log.Info("cubemap is null");
                continue;
            }
            TestLog.Log("开始发送前："+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

            for (int i = 0; i < 6; i++) {
                tex.SetPixels(cubemap.GetPixels((CubemapFace)i), 0);
                tex.Apply();
                byte[] bytes = tex.EncodeToJPG();
                PointCubemapMsg pointCubemapMsg = new PointCubemapMsg();
                pointCubemapMsg.x = pos.x;
                pointCubemapMsg.y = pos.y;
                pointCubemapMsg.z = pos.z;
                pointCubemapMsg.face = i;
                pointCubemapMsg.jpg_bytes = bytes;
                Send(taskUnit.connectionId, pointCubemapMsg);
            }
            TestLog.Log("结束发送后：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));


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

        server.Start(7776);
    }
    void RecallReceived(int connectionId, ArraySegment<byte> message, KcpChannel channel) {
        Log.Info($"[KCP] OnServerDataReceived({connectionId},  message.Offset: {message.Offset},message.Count{message.Count}bitll{BitConverter.ToString(message.Array, message.Offset, message.Count)} @ {channel})");
        //if((message.Count - message.Offset) > sizeof(Int32)) return; 
        //if(BitConverter.ToInt32(message.Array, message.Offset) < message.Count - message.Offset)  return;
        //num++;
        //string assetpath = Application.dataPath + "/../test/" + num + ".png";
        new Task(() => {

            if ((message.Count - message.Offset) < sizeof(Int32)) return;
            if (message.Count - message.Offset < BitConverter.ToInt32(message.Array, message.Offset)) return;
            byte[] bytes = new byte[message.Count - sizeof(Int32)];
            Array.Copy(message.Array, message.Offset + sizeof(Int32), bytes, 0, message.Count - sizeof(Int32));

            MsgBase reMsgbase = msg.Decode(bytes);
            //HandleMsg(connectionId, reMsgbase);
            TaskUnit taskUnit = new TaskUnit();
            taskUnit.connectionId = connectionId;
            taskUnit.msg = reMsgbase;
            lock (taskQueue) {
                taskQueue.Enqueue(taskUnit);
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

