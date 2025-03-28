using UnityEngine;
using UnityEngine.XR;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit;
using Fusion;

public class EraserSingePlayer : MonoBehaviour
{
    [SerializeField] private Transform _tip;
    [SerializeField] private int _penSize=15;
    [SerializeField] private float _tipHeight=0.05F;
    private Renderer _renderer;
    private Color[] _colors;
    //private float _tipHeight;
    private  bool _touchedLastFrame;
    private RaycastHit _touch;

    private WhiteboardSinglePlayer _whiteboard;
    private Vector2 _touchPos;
    private Vector2 _lastTouchPos;
    private Quaternion _lastTouchRot;

    private Rigidbody m_Rigidbody;

    // Networked variables
    // [Networked] private bool _touchedLastFrameNetworked { get; set; }
    // [Networked] private Vector2 _lastTouchPosNetworked { get; set; }
    // [Networked] private Quaternion _lastTouchRotNetworked { get; set; }


    void Start()
    {
        _renderer = transform.GetComponent<Renderer>();
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        Erase();
    }
    private void Erase()
    {
        if (Physics.Raycast(transform.position, transform.right, out _touch, _tipHeight))
        {
            if (_touch.transform.CompareTag("Whiteboard"))
            {
                if (_whiteboard == null)
                {
                    _whiteboard = _touch.transform.GetComponent<WhiteboardSinglePlayer>();
                }
                m_Rigidbody.freezeRotation = true;
            }
            else
            {
                return;
            }

            _touchPos = new Vector2(_touch.textureCoord.x, _touch.textureCoord.y);
            
            var x = (int)(_touchPos.x * _whiteboard.textureSize.x - (_penSize / 2));

            var y = (int)(_touchPos.y * _whiteboard.textureSize.y - (_penSize / 2));

            if (y >= 0+_penSize && y < _whiteboard.textureSize.y-_penSize && x >= 0+_penSize && x < _whiteboard.textureSize.x-_penSize)
            {
                if (_touchedLastFrame)
                {
                    // Call the method to update the whiteboard texture on all clients
                    //_whiteboard.UpdateTextureOnClients(x, y, _penSize, _colors);
                    for (float f = 0.01F; f < 1.00F; f += 0.01F)
                    {
                        var lerpX = (int)Mathf.Lerp(_lastTouchPos.x, x, f);
                        var lerpY = (int)Mathf.Lerp(_lastTouchPos.y, y, f);
                        Color[] sourcePixels = _whiteboard.textureBase.GetPixels(lerpX, lerpY, _penSize, _penSize);
                        for (int i = 0; i < sourcePixels.Length; i++)
                        {
                            sourcePixels[i].a = 0f;
                        }
                        _whiteboard.texture.SetPixels(lerpX, lerpY, _penSize, _penSize, sourcePixels);

                    }

                    transform.rotation = _lastTouchRot;
                    _whiteboard.texture.Apply();
                }
                _lastTouchPos = new Vector2(x, y);
                _lastTouchRot = transform.rotation;
                _touchedLastFrame = true;
                return;
            }
        }
            
        _whiteboard = null;
        _touchedLastFrame = false;
        m_Rigidbody.freezeRotation = false;
    }

}
