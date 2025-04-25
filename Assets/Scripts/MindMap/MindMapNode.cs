using UnityEngine;
using TMPro;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Fusion;
public class MindMapNode : MonoBehaviour
{

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private bool isTriggerPressed;

    [SerializeField] public bool isDeleting;
    [SerializeField] public bool Select;
    [SerializeField] public bool isSelected;
    [SerializeField] public bool isEditing;
    [SerializeField] public bool isCyclingColor;
    private bool isHovered;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Transform[] Nodes;
    private LineRenderer[] Lines;
    private int maxConnections=20;

    private MindMap mindMap;
    private GameObject parentGameObject;
    private GameObject formObject;
    private Color[]  Colors = new Color[]{
            Color.red,Color.blue,Color.green,Color.black
        };
    public int color_id;

    public int id;
    private MeshFilter meshFilter;
    public bool toggleShape = true;
    public int shape_id;
    private GameObject textObject;
    private TextMeshPro textTMP;
    void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grabInteractable.hoverEntered.AddListener(OnHoverEntered);
        grabInteractable.hoverExited.AddListener(OnHoverExited);

        isHovered=false;
        
        parentGameObject = this.transform.parent.gameObject;
        mindMap = GetComponentInParent<MindMap>();
        isSelected = false;
           

        textObject= transform.GetChild(0).gameObject;      
        textTMP = textObject.GetComponent<TextMeshPro>();
        SetName(transform.name);

        color_id=0;
        formObject= transform.GetChild(1).gameObject; 
        SetColor(0);
        
        meshFilter = formObject.GetComponent<MeshFilter>();
        shape_id=0;
        SetShape(0);
    }
    void Start(){
        
    }
    void Update()
    {
        if(isCyclingColor) SetColor();
        if(toggleShape) SetShape();
        if (isHovered)
        {
            InputDevice rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            InputDevice leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

            if (rightController.TryGetFeatureValue(CommonUsages.trigger, out float triggerAxisVal))
            {            
                if (triggerAxisVal>0 && !isTriggerPressed)
                {
                    Select=true;
                    isTriggerPressed = true;
                }
                else if (triggerAxisVal==0 && isTriggerPressed)
                {
                    isTriggerPressed = false;
                }
            }
        }
        if (Select)
        {
            if(!isSelected){
                isSelected=true;
            }
            else{
                isEditing=true;
                isSelected=false;
            }
            Select=false;
            //Destroy(transform.gameObject);
        }
    }

    public void SetColor(int c_id = -1){
        if(c_id == -1){
            color_id++;
            color_id%=Colors.Length;    
        }
        else color_id=c_id;
        formObject.GetComponent<Renderer>().material.color = Colors[color_id];
        isCyclingColor=false;
    }
    public void SetShape(int s_id = -1)
    {
        if(s_id == -1){
            shape_id++;
            shape_id%=2;
        }
        else shape_id=s_id;
        GameObject temp;
        temp = GameObject.CreatePrimitive(shape_id==0 ? PrimitiveType.Sphere : PrimitiveType.Cube);
        Mesh newMesh = temp.GetComponent<MeshFilter>().sharedMesh;
        Destroy(temp);
        meshFilter.mesh = newMesh;

        toggleShape=false;
    }
    public void SetName(string name){
        transform.name=name;
        textTMP.text = transform.name;
    }
        
    private void OnDestroy()
    {
        grabInteractable.hoverEntered.RemoveListener(OnHoverEntered);
        grabInteractable.hoverExited.RemoveListener(OnHoverExited);
    }
    void OnHoverEntered(HoverEnterEventArgs args){ isHovered=true;}
    void OnHoverExited(HoverExitEventArgs args){ isHovered=false;}
}
