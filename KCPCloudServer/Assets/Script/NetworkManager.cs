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
public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;
    public KcpServer server;
    MsgBase msg = new MsgBase();


    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else {
            Destroy(this);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        InitServer();
       
    }

    // Update is called once per frame
    void Update()
    {
        server.Tick();
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
            (connectionId, message, channel) => RecallReceived(connectionId, message,channel),//接收到数据时回调
            (connectionId) => { },//断开连接时回调
            (connectionId, error, reason) => Log.Info($"[KCP] OnServerError({connectionId}, {error}, {reason}"),
            config
        );

        server.Start(7777);
    }
    void RecallReceived(int connectionId,ArraySegment<byte> message,KcpChannel channel) {
        Log.Info($"[KCP] OnServerDataReceived({connectionId},  message.Offset: {message.Offset},message.Count{message.Count}bitll{BitConverter.ToString(message.Array, message.Offset, message.Count)} @ {channel})");
        //if((message.Count - message.Offset) > sizeof(Int32)) return; 
        //if(BitConverter.ToInt32(message.Array, message.Offset) < message.Count - message.Offset)  return;
        //num++;
        //string assetpath = Application.dataPath + "/../test/" + num + ".png";
        new Task(() => {
            //byte[] bytes = new byte[message.Count - message.Offset];
            //Array.Copy(message.Array, message.Offset, bytes, 0, message.Count - message.Offset);
            //File.WriteAllBytes(assetpath, bytes);

            if ((message.Count - message.Offset) < sizeof(Int32)) return;
            if (message.Count - message.Offset < BitConverter.ToInt32(message.Array, message.Offset)) return;
            byte [] bytes = new byte[message.Count - message.Offset- sizeof(Int32)];
            Array.Copy(message.Array, message.Offset + sizeof(Int32), bytes, 0, message.Count - message.Offset - sizeof(Int32));
            MsgBase reMsgbase = msg.Decode(bytes);
            HandleMsg(connectionId, reMsgbase);
        }).Start();
    }
    private void HandleMsg(int conn, MsgBase protoBase) {

        string methodName = protoBase.GetName();


        MethodInfo mm = Type.GetType("Handle").GetMethod(methodName);
        if (mm == null) {
            string str = "[警告]ConnHandleMsg没有处理连接方法 ";
            Console.WriteLine(str + methodName);
            return;
        }
        Action<int, MsgBase> updateDel = (Action<int, MsgBase>)Delegate.CreateDelegate(typeof(Action<int, MsgBase>), null, mm);

        updateDel(conn, protoBase);
        Log.Info("[处理连接消息]" + conn + " :" + methodName);


    }
}

