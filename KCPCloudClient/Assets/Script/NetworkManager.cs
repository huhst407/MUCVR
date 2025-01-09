using kcp2k;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;
    public KcpClient client;
    public Camera camm;
    public Cubemap cubemap;
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
        client.Connect("127.0.0.1", 7777);
    }

    // Update is called once per frame
    void Update()
    {
        client.Tick();
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
            (message, channel) => Log.Info($"[KCP] OnServerDataReceived({BitConverter.ToString(message.Array, message.Offset, message.Count)} @ {channel})"),//接收到数据时回调
            () => { },//断开连接时回调
            ( error, reason) => Log.Info($"[KCP] OnServerError( {error}, {reason}"),
            config
        ); 
    }
    public void OnClick() {
        if (camm.RenderToCubemap(cubemap)) {
            Texture2D tex = new Texture2D(1024, 1024, TextureFormat.RGB24, false);
            for (int i = 0; i < 6; i++) {
                
                tex.SetPixels(cubemap.GetPixels((CubemapFace) i), 0);
                tex.Apply();
                byte[] bytes = tex.EncodeToJPG();
                client.Send(new ArraySegment<byte>(bytes), KcpChannel.Reliable);
               
            }
            Destroy(tex);

        }
        else {
            Debug.Log("Failed to render cubemap");
        }
        //client.Send(new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes("hello")), KcpChannel.Reliable);
    }
}
