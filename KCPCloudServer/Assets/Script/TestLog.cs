using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public static class  TestLog {
    static string path = "Assets/Script/TestLog.log";
    public static void Log(string log) {
        using (StreamWriter sw = new StreamWriter(path,true,System.Text.Encoding.UTF8)) {
            sw.WriteLine(log);
        }
    }
}
