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
    private Color[] Colors;
    private int color_id;


    private MeshFilter meshFilter;
    public bool toggleShape = true;
    private int shape_id;

    void Start()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grabInteractable.hoverEntered.AddListener(OnHoverEntered);
        grabInteractable.hoverExited.AddListener(OnHoverExited);

        isHovered=false;
        
        parentGameObject = this.transform.parent.gameObject;
        mindMap = GetComponentInParent<MindMap>();
        isSelected = false;
        GameObject textObject= transform.GetChild(0).gameObject;         
        TextMeshPro textTMP = textObject.GetComponent<TextMeshPro >();
        textTMP.text = transform.name;

        Colors = new Color[]{
            Color.red,Color.blue,Color.green,Color.black
        };
        color_id=0;
        formObject= transform.GetChild(1).gameObject; 
        CycleColor();

        meshFilter = formObject.GetComponent<MeshFilter>();
        shape_id=0;
        ToggleShape();
    }

    void Update()
    {
        if(isCyclingColor) CycleColor();
        if(toggleShape) ToggleShape();
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

    private void CycleColor(){
        formObject.GetComponent<Renderer>().material.color = Colors[color_id];
        isCyclingColor=false;
        color_id++;
        color_id%=Colors.Length;
    }
    public void ToggleShape()
    {
        GameObject temp = GameObject.CreatePrimitive(shape_id==0 ? PrimitiveType.Sphere : PrimitiveType.Cube);
        Mesh newMesh = temp.GetComponent<MeshFilter>().sharedMesh;
        Destroy(temp);
        meshFilter.mesh = newMesh;
        shape_id++;
        shape_id%=2;
        toggleShape=false;
    }    
    private void OnDestroy()
    {
        grabInteractable.hoverEntered.RemoveListener(OnHoverEntered);
        grabInteractable.hoverExited.RemoveListener(OnHoverExited);
    }
    void OnHoverEntered(HoverEnterEventArgs args){ isHovered=true;}
    void OnHoverExited(HoverExitEventArgs args){ isHovered=false;}
}
