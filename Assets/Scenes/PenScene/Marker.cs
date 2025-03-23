/*using UnityEngine;
using UnityEngine.XR;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit;
public class Marker : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Transform _tip;
    [SerializeField] private int _penSize=5;
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
    int[,] Colors;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private bool isGrabed;
    void Start()
    {
        isGrabed=false;
        xButtonPressed = false;
        Colors = new int[4, 3]
        {
            { 0, 0, 0 },
            { 255, 0, 0 },
            { 0, 255, 0 },
            { 0, 0, 255 }
        };

        
        _renderer = _tip.GetComponent<Renderer>();
        color_id=0;
        ChangeColor();
        //_colors = Enumerable.Repeat(_renderer.material.color, _penSize* _penSize).ToArray();
        //_tipHeight = _tip.localScale.x;
        m_Rigidbody = GetComponent<Rigidbody>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
        //ChangeColor();
        RightInput();
        Draw();

    }
    private void RightInput(){
        InputDevice rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (rightController.isValid && isGrabed)
        {
        // Check button state
            if (rightController.TryGetFeatureValue(CommonUsages.primaryButton, out bool isPressed))
            {
                // Detect press (button was not pressed before but is pressed now)
                if (isPressed && !xButtonPressed)
                {
                    //Debug.Log("X Button Pressed!");
                    color_id++;
                    color_id = color_id % 4;
                    ChangeColor();
                    xButtonPressed = true;  // Mark as pressed
                }
                // Detect release (button was pressed before but is not pressed now)
                else if (!isPressed && xButtonPressed)
                {
                    //Debug.Log("X Button Released!");
                    xButtonPressed = false;  // Reset press state
                }
            }
        }
    }
    private void ChangeColor(){

        _tip.GetComponent<Renderer>().material.color = new Color(Colors[color_id,0],Colors[color_id,1],Colors[color_id,2]);//new Color(Random.Range(0, 255),Random.Range(0, 255),Random.Range(0, 255));
        //_tip.GetComponent<Renderer>().material.color = new Color(255,0,0);//new Color(Random.Range(0, 255),Random.Range(0, 255),Random.Range(0, 255));
        //_tip.GetComponent<Renderer>().material.color = new Color(Random.Range(0, 255),Random.Range(0, 255),Random.Range(0, 255));
        _colors = Enumerable.Repeat(_renderer.material.color, _penSize* _penSize).ToArray();

    }
    private void Draw(){
        //Debug.DrawRay(_tip.position, _tip.right * _tipHeight, Color.red);
        if(Physics.Raycast(_tip.position,_tip.right, out _touch, _tipHeight)){
            //Debug.Log("Raycast hit: " + _touch.transform.name);
            if(_touch.transform.CompareTag("Whiteboard")){
                if(_whiteboard == null){
                    _whiteboard = _touch.transform.GetComponent<Whiteboard>();
                }
                m_Rigidbody.freezeRotation = true;
            }
            else{
                return;
            }
            _touchPos = new Vector2(_touch.textureCoord.x, _touch.textureCoord.y);
            var x = (int)(_touchPos.x * _whiteboard.textureSize.x - (_penSize / 2));
            var y = (int)(_touchPos.y * _whiteboard.textureSize.y - (_penSize / 2));
            Debug.Log(string.Format("Raycast hit: ({0} : {1}) {2}",x,y,_touchedLastFrame));
            
            if(y >= 0 && y < _whiteboard.textureSize.y && x >= 0 && x < _whiteboard.textureSize.x )
            {
                
                if(_touchedLastFrame)
                {
                    
                    _whiteboard.texture.SetPixels(x,y,_penSize,_penSize,_colors);
                    for(float f=0.01F; f< 1.00F; f+=0.01F){
                        var lerpX = (int) Mathf.Lerp(_lastTouchPos.x,x,f);
                        var lerpY = (int) Mathf.Lerp(_lastTouchPos.y,y,f);
                        Debug.Log(string.Format("Raycast hit: ({0} : {1}) {2}",x,y,_touch.transform.name));
                        _whiteboard.texture.SetPixels(lerpX,lerpY,_penSize,_penSize,_colors);
                    }
                    
                    transform.rotation = _lastTouchRot;
                    _whiteboard.texture.Apply();
                }
                _lastTouchPos= new Vector2(x,y); //_touchPos;
                _lastTouchRot = transform.rotation;
                _touchedLastFrame = true;
                return;
            }
            
        }
        _whiteboard = null;
        _touchedLastFrame= false;
        m_Rigidbody.freezeRotation = false;
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
*/