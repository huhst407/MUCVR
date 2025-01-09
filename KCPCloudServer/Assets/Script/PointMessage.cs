using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointMessage
{
    public Vector3 pos;
    public Cubemap cubemap;
    static int width ;
    public bool[] isFace = new bool[6];
    public PointMessage() {
        width = CENetworkManager.instance.width;
        cubemap = new Cubemap(width, TextureFormat.RGB24, false);
    }
    #region 接收
    public void ResolveToPointMessage(Vector3 Coord, int CubeMapFace, Texture2D texture) {
        pos = Coord;
        cubemap.SetPixels(texture.GetPixels(), (CubemapFace)CubeMapFace);
        cubemap.Apply();
        isFace[CubeMapFace] = true;
    }
    Color[] ColorToCubemap(byte[] byt) {
        Texture2D textureTest = new Texture2D(width, width);
        textureTest.LoadImage(byt);
        textureTest.Apply();

        return textureTest.GetPixels();
    }
    #endregion

    public bool IsAllFace() {
        for (int i = 0; i < 6; i++) {
            if (!isFace[i]) {
                return false;
            }
        }
        return true;
    }
}
