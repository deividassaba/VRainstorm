using UnityEngine;
using System.Linq;

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

    void Start()
    {
        
        _renderer = _tip.GetComponent<Renderer>();
        _colors = Enumerable.Repeat(_renderer.material.color, _penSize* _penSize).ToArray();
        //_tipHeight = _tip.localScale.x;
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Draw();
        Contact();
    }
    private void Contact(){
    
    }
    private void Draw(){
        Debug.DrawRay(_tip.position, _tip.right * _tipHeight, Color.red);
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
}
