using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Collections.Generic;

public class Whiteboard : NetworkBehaviour
{
    public Texture2D texture;
    public Vector2 textureSize = new Vector2(2048, 2048);

    void Start()
    {
        var r = GetComponent<Renderer>();
        texture = new Texture2D((int)textureSize.x, (int)textureSize.y);
        r.material.mainTexture = texture;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void UpdateTextureRPC(int x, int y, int penSize, Color[] colors)
    {
        if (texture != null)
        {
            texture.SetPixels(x, y, penSize, penSize, colors);
            texture.Apply();
        }
    }

    public void UpdateTextureOnClients(int x, int y, int penSize, Color[] colors)
    {
        UpdateTextureRPC(x, y, penSize, colors);
    }
}
