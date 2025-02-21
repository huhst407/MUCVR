using kcp2k;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Sirenix.OdinInspector;
public class CENetworkManager : MonoBehaviour
{
    public static CENetworkManager instance;
    public KcpClient client;
    public Camera camm;
    public Cubemap cubemap;
    public int width = 1024;
    readonly static int MAX_MESSAGE_FIRE = 10;
    public MsgBase msg = new MsgBase();
    [ShowInInspector]
    public static List<MsgBase> msgList = new List<MsgBase>();
    public float updateInterval = 5f;

    public float thresholdDistance = 500;
    //todo:
    //将以camera为中心，threshold为半径的球形碰撞区域内的所有物体加入到一个列表中，同时设置为未激活状态
    //但物体移除时激活物体
    float updatetime = 0;

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
        camm = Camera.main;
        cubemap = new Cubemap(1024, TextureFormat.RGB24, false);
        InitClient();
        client.Connect("127.0.0.1", 7776);
    }

    // Update is called once per frame
    void Update()
    {
        client.Tick();
        if(client.connected)
            MsgUpdate();
        if (updatetime > 0) {
            updatetime -= Time.deltaTime;
        }


    }
    private void InitClient() {
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
        client = new KcpClient(
            () => { },//连接时回调
            (message, channel) => RecallReceived(message,channel),//接收到数据时回调
            () => { },//断开连接时回调
            ( error, reason) => Log.Info($"[KCP] OnServerError( {error}, {reason}"),
            config
        ); 
    }
    public void OnClick() {

        //if (camm.RenderToCubemap(cubemap)) {
        //    Texture2D tex = new Texture2D(1024, 1024, TextureFormat.RGB24, false);
        //    for (int i = 0; i < 6; i++) {
                
        //        tex.SetPixels(cubemap.GetPixels((CubemapFace) i), 0);
        //        tex.Apply();
        //        byte[] bytes = tex.EncodeToJPG();
        //        client.Send(new ArraySegment<byte>(bytes), KcpChannel.Reliable);
               
        //    }
        //    Destroy(tex);

        //}
        //else {
        //    Debug.Log("Failed to render cubemap");
        //}
        ////client.Send(new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes("hello")), KcpChannel.Reliable);
    }
    void RecallReceived( ArraySegment<byte> message, KcpChannel channel) {
        Log.Info($"[KCP] OnServerDataReceived(message.Offset: {message.Offset},message.Count{message.Count}bitll{BitConverter.ToString(message.Array, message.Offset, message.Count)} @ {channel})");

        if ((message.Count - message.Offset) < sizeof(Int32)) return;
        if (message.Count - message.Offset < BitConverter.ToInt32(message.Array, message.Offset)) return;
        byte[] bytes = new byte[message.Count - sizeof(Int32)];
        Array.Copy(message.Array, message.Offset + sizeof(Int32), bytes, 0, message.Count - sizeof(Int32));
       
        //Log.Info(System.Text.Encoding.UTF8.GetString(bytes));
        MsgBase reMsgbase = msg.Decode(bytes);
        msgList.Add(reMsgbase); 
    }
    public delegate void MsgListener(MsgBase msgBase);
    public static Dictionary<string, MsgListener> msgListeners = new Dictionary<string, MsgListener>();
    public  void AddMsgListener(string msgName, MsgListener listener) {

        if (msgListeners.ContainsKey(msgName)) {
            msgListeners[msgName] += listener;
        }

        else {
            msgListeners[msgName] = listener;
        }
    }

    public void RemoveMsgListener(string msgName, MsgListener listener) {
        if (msgListeners.ContainsKey(msgName)) {
            msgListeners[msgName] -= listener;
        }
    }
    private void FireMsg(string msgName, MsgBase msgBase) {
        if (msgListeners.ContainsKey(msgName)) {
            msgListeners[msgName](msgBase);
        }
    }
    public void MsgUpdate() {
        //初步判断，提升效率
        if (msgList.Count == 0) {
            return;
        }
        //重复处理消息
        for (int i = 0; i < MAX_MESSAGE_FIRE; i++) {
            //获取第一条消息
            MsgBase msgBase = null;
            lock (msgList) {
                if (msgList.Count > 0) {
                    msgBase = msgList[0];
                    msgList.RemoveAt(0);
                }
            }
            //分发消息
            if (msgBase != null) {
                FireMsg(msgBase.GetName(), msgBase);
            }
            //没有消息了
            else {
                break;
            }
        }
    }
    public void Send( MsgBase msg) {
        if (updatetime>0) return;
        byte[] bytes_context = msg.Encode();
        Int32 length = bytes_context.Length;
        byte[] length_bytes = BitConverter.GetBytes(length);
        byte[] bytes = length_bytes.Concat(bytes_context).ToArray();
        client.Send( new ArraySegment<byte>(bytes), KcpChannel.Reliable);
        updatetime = updateInterval;
    }
}
