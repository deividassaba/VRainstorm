using UnityEngine;

public class Whiteboard : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Texture2D texture;
    public Vector2 textureSize = new Vector2(2048,2048);
    void Start()
    {
        var r = GetComponent<Renderer>();
        texture = new Texture2D((int)textureSize.x,(int)textureSize.y);
        r.material.mainTexture= texture;
    }


}
