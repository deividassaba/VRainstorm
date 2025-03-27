using UnityEngine;

using UnityEngine;
using UnityEngine.XR;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit;
using Fusion;

public class Eraser : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Transform _tip;
    [SerializeField] private int _penSize=20;
    [SerializeField] private float _tipHeight=0.01F;
    private Renderer _renderer;
    private Color[] _colors;
    //private float _tipHeight;
    private  bool _touchedLastFrame;
    private RaycastHit _touch;

    private Whiteboard _whiteboard;
    private Vector2 _touchPos;
    private Vector2 _lastTouchPos;
    private Quaternion _lastTouchRot;


    Rigidbody m_Rigidbody;

    private int color_id;
    private bool xButtonPressed;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    // Networked variables
    [Networked] private bool _touchedLastFrameNetworked { get; set; }
    [Networked] private Vector2 _lastTouchPosNetworked { get; set; }
    [Networked] private Quaternion _lastTouchRotNetworked { get; set; }


    void Start()
    {
        isGrabed=false;

    
        _renderer = _tip.GetComponent<Renderer>();
        color_id=0;
        m_Rigidbody = GetComponent<Rigidbody>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
        //ChangeColor();
        RightInput();
        Draw();

    }

    private void Draw()
    {
        if (Physics.Raycast(_tip.position, _tip.right, out _touch, _tipHeight))
        {
            if (_touch.transform.CompareTag("Whiteboard"))
            {
                if (_whiteboard == null)
                {
                    _whiteboard = _touch.transform.GetComponent<Whiteboard>();
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

            if (y >= 0 && y < _whiteboard.textureSize.y && x >= 0 && x < _whiteboard.textureSize.x)
            {
                if (_touchedLastFrame)
                {
                    // Call the method to update the whiteboard texture on all clients
                    Vector3 _colors = new Vector3(255,255,255);
                    _whiteboard.UpdateTextureOnClients(x, y, _penSize, _colors);
                    for (float f = 0.01F; f < 1.00F; f += 0.01F)
                    {
                        var lerpX = (int)Mathf.Lerp(_lastTouchPos.x, x, f);
                        var lerpY = (int)Mathf.Lerp(_lastTouchPos.y, y, f);
                        _whiteboard.UpdateTextureOnClients(lerpX, lerpY, _penSize, _colors);
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

    // Updates the whiteboard in theory
    [Rpc]
    private void UpdateWhiteboardTextureRPC(int x, int y, int penSize, Color[] colors)
    {
        // Ensure we have a reference to the whiteboard object
        if (_whiteboard != null)
        {
            // Update the whiteboard texture on this client
            _whiteboard.texture.SetPixels(x, y, penSize, penSize, colors);
            _whiteboard.texture.Apply();
        }
    }



    void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        // Add event listeners
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        //Debug.Log($"{gameObject.name} grabbed!");
        isGrabed=true;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        //Debug.Log($"{gameObject.name} released!");
        isGrabed=false;
    }
}
