using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using UnityEngine;
public class MsgBase {
    public string protoName = "null";
    public MsgBase() { }
    public virtual byte[] Encode() {
        byte[] name_bytes = AddString(protoName);
        //UnityEngine.Debug.Log(JsonConvert.SerializeObject(this)); 
        byte[] context_bytes = AddString(JsonConvert.SerializeObject(this));
        return name_bytes.Concat(context_bytes).ToArray();
    }
    public virtual MsgBase Decode(byte[] allBytes, int start = 0) {

        protoName = GetString(allBytes, start, ref start);
        string json = GetString(allBytes, start, ref start);
        
        return (MsgBase)JsonConvert.DeserializeObject(json, Type.GetType(protoName));

    }
    public virtual string GetName() {
        return protoName;
    }
    public byte[] AddString(string str) {
        byte[] context_bytes = System.Text.Encoding.UTF8.GetBytes(str);
        Int32 length = context_bytes.Length;
        byte[] length_bytes = BitConverter.GetBytes(length);
        return length_bytes.Concat(context_bytes).ToArray();
    }
    public string GetString(byte[] allBytes, int start, ref int end) {
        if (allBytes == null)
            return "";
        if (allBytes.Length < start + sizeof(Int32))
            return "";
        Int32 length = BitConverter.ToInt32(allBytes, start);
        if (allBytes.Length < start + sizeof(Int32) + length)
            return "";


        string str = System.Text.Encoding.UTF8.GetString(allBytes, start + sizeof(Int32), length);
        
        end = start + sizeof(Int32) + length; ;
        return str;
    }
}