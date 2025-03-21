using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

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

    // This method will be called to update the texture across all clients
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void UpdateTextureRPC(int x, int y, int penSize, Color[] colors)
    {
        // Update the texture with the provided pixel data (x, y, penSize, colors)
        if (texture != null)
        {
            texture.SetPixels(x, y, penSize, penSize, colors);
            texture.Apply(); // Apply the changes to the texture
        }
    }

    // This method will be called to apply texture updates across clients
    public void UpdateTextureOnClients(int x, int y, int penSize, Color[] colors)
    {
        // Since it's a global whiteboard, we don't need ownership checks
        UpdateTextureRPC(x, y, penSize, colors);
    }
}
