using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

public class WhiteboardSinglePlayer : MonoBehaviour
{
    public Texture2D texture;
    public Texture2D textureBase;
    public Vector2 textureSize;

    void Start()
    {
        var r = GetComponent<Renderer>();
        textureSize = new Vector2(2048, 2048);
        texture = new Texture2D((int)textureSize.x, (int)textureSize.y);
        textureBase = new Texture2D((int)textureSize.x, (int)textureSize.y);
        r.material.mainTexture = texture;
    }

}
