using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Fusion;
using System.Collections.Generic;
using System.Linq;

public class Marker : NetworkBehaviour
{
    [SerializeField] private Transform _tip;
    [SerializeField] private int _penSize = 5;
    [SerializeField] private float _tipHeight = 0.01F;
    private Renderer _renderer;
    private Color[] _colors;
    private bool _touchedLastFrame;
    private RaycastHit _touch;

    private Whiteboard _whiteboard;
    private Vector2 _touchPos;
    private Vector2 _lastTouchPos;
    private Quaternion _lastTouchRot;

    Rigidbody m_Rigidbody;

    private int color_id;
    private bool xButtonPressed;
    private int[,] Colors;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private bool isGrabed;

    // Networked variables
    [Networked] private bool _touchedLastFrameNetworked { get; set; }
    [Networked] private Vector2 _lastTouchPosNetworked { get; set; }
    [Networked] private Quaternion _lastTouchRotNetworked { get; set; }

    // Optimized drawing variables
    private float drawCooldown = 0.05f; // Shorter cooldown for faster updates
    private float timeSinceLastUpdate = 0.0f;
    private List<(int x, int y, int penSize, Color[] colors)> drawBuffer = new List<(int, int, int, Color[])>();
    private float movementThreshold = 0.002f; // Lower the threshold for detecting small movements

    // New variable for ensuring consistent drawing
    private float movementThresholdForFastDraw = 0.02f; // Larger threshold for fast drawing

    // Buffer time before applying to texture
    private float textureApplyCooldown = 0.1f; // Reduce how often we apply to the texture
    private float timeSinceLastTextureUpdate = 0.0f;

    void Start()
    {
        isGrabed = false;
        xButtonPressed = false;
        Colors = new int[4, 3]
        {
            { 0, 0, 0 },
            { 255, 0, 0 },
            { 0, 255, 0 },
            { 0, 0, 255 }
        };

        _renderer = _tip.GetComponent<Renderer>();
        color_id = 0;
        ChangeColor();
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        RightInput();
        Draw();

        // Apply buffered drawings after the cooldown has elapsed
        timeSinceLastUpdate += Time.deltaTime;
        if (timeSinceLastUpdate >= drawCooldown && drawBuffer.Count > 0)
        {
            ApplyBufferedDrawings();
            timeSinceLastUpdate = 0.0f;
        }

        // Apply texture updates periodically, reducing the call frequency
        timeSinceLastTextureUpdate += Time.deltaTime;
        if (timeSinceLastTextureUpdate >= textureApplyCooldown && drawBuffer.Count > 0)
        {
            ApplyBufferedDrawings();
            timeSinceLastTextureUpdate = 0.0f;
        }
    }

    private void RightInput()
    {
        InputDevice rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (rightController.isValid && isGrabed)
        {
            if (rightController.TryGetFeatureValue(CommonUsages.primaryButton, out bool isPressed))
            {
                if (isPressed && !xButtonPressed)
                {
                    color_id++;
                    color_id = color_id % 4;
                    ChangeColor();
                    xButtonPressed = true;
                }
                else if (!isPressed && xButtonPressed)
                {
                    xButtonPressed = false;
                }
            }
        }
    }

    private void ChangeColor()
    {
        _tip.GetComponent<Renderer>().material.color = new Color(Colors[color_id, 0], Colors[color_id, 1], Colors[color_id, 2]);
        _colors = Enumerable.Repeat(_renderer.material.color, _penSize * _penSize).ToArray();
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

            // Only update if position is within bounds
            if (y >= 0 && y < _whiteboard.textureSize.y && x >= 0 && x < _whiteboard.textureSize.x)
            {
                // Only update the drawing buffer if there’s enough movement (skip small or minor changes)
                if (_touchedLastFrame)
                {
                    float moveDistance = Vector2.Distance(_lastTouchPos, new Vector2(x, y));

                    // Apply faster drawing for long strokes
                    if (moveDistance > movementThresholdForFastDraw)
                    {
                        // Skip some intermediate points for fast drawing to reduce load
                        DrawLine(_lastTouchPos, new Vector2(x, y));
                    }

                    // Apply buffer if distance threshold is crossed
                    if (moveDistance > movementThreshold)
                    {
                        drawBuffer.Add((x, y, _penSize, _colors));
                    }
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

    private void DrawLine(Vector2 start, Vector2 end)
    {
        // Interpolation: Only calculate steps for very long distances
        int steps = Mathf.CeilToInt(Vector2.Distance(start, end) / 10); // Larger step size for long lines
        for (int i = 0; i <= steps; i++)
        {
            float lerpFactor = i / (float)steps;
            int x = (int)Mathf.Lerp(start.x, end.x, lerpFactor);
            int y = (int)Mathf.Lerp(start.y, end.y, lerpFactor);
            drawBuffer.Add((x, y, _penSize, _colors));
        }
    }

    private void ApplyBufferedDrawings()
    {
        if (_whiteboard != null)
        {
            // Apply all buffered updates at once
            foreach (var drawData in drawBuffer)
            {
                _whiteboard.UpdateTextureOnClients(drawData.x, drawData.y, drawData.penSize, drawData.colors);
            }
            drawBuffer.Clear();
        }
    }

    [Rpc]
    private void UpdateWhiteboardTextureRPC(int x, int y, int penSize, Color[] colors)
    {
        if (_whiteboard != null)
        {
            _whiteboard.texture.SetPixels(x, y, penSize, penSize, colors);
            _whiteboard.texture.Apply();
        }
    }

    void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        isGrabed = true;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isGrabed = false;
    }
}
