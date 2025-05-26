using UnityEngine;
using TMPro;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Fusion;
using Fusion.Sockets;

public class MindMapNode : NetworkBehaviour
{

    //Networked need be:
    [Networked]
    public int id { get; set; }
    [Networked]
    public int color_id { get; set; }
    [Networked]
    public int shape_id { get; set; }
    [Networked]
    public string nodeName {  get; set; }

    private int previousColorId = -1;
    private int previousShapeId = -1;
    private string previousNodeName = null;



    //for text rotation
    Camera mainCamera;

    [SerializeField]
    private TextMeshPro textTMP { get; set; }

    //Position should be maybe hopefully transformed automatically because
    // of Network Transform (Hopefully)

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private bool isTriggerPressed;

    // Under no circumstances network these.
    // Every player edits independantly.
    // This will lead to race conditions but whatever who cares xddd
    [SerializeField] public bool isDeleting;
    [SerializeField] public bool Select;
    [SerializeField] public bool isSelected;
    [SerializeField] public bool isEditing;
    [SerializeField] public bool isCyclingColor;
    private bool isHovered;

    // private Transform[] Nodes; not needed
    // private LineRenderer[] Lines; not needed
    // private int maxConnections=20; not used

    private MindMap mindMap;
    private GameObject parentGameObject;
    private GameObject formObject;
    private Color[]  Colors = new Color[]{
            Color.red,Color.blue,Color.green,Color.black
        };
    

    private MeshFilter meshFilter;
    public bool toggleShape = true;

    private GameObject textObject;

    public override void Spawned()
    {
        base.Spawned();
        Debug.Log("Node Spawned() called.");

        //get camera
        mainCamera = Camera.main;


        textObject = transform.GetChild(0).gameObject;
        textTMP = textObject.GetComponent<TextMeshPro>();
        previousNodeName = transform.name;
        SetName(transform.name);

        color_id = 0;
        previousColorId = 0;
        formObject = transform.GetChild(1).gameObject;

        SetColor(0);

        meshFilter = formObject.GetComponent<MeshFilter>();
        shape_id = 0;
        previousShapeId = 0;
        SetShape(0);
    }

    void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grabInteractable.hoverEntered.AddListener(OnHoverEntered);
        grabInteractable.hoverExited.AddListener(OnHoverExited);

        isHovered = false;

        // Come to think of it ive no clue what this does either
        // parentGameObject = this.transform.parent.gameObject;
        mindMap = GetComponentInParent<MindMap>();

        isSelected = false;
    }
    

    
    void Start()
    {
        //galimai per cia reikes spawnint bet teoriškai gal ne?    
    }
    void Update()
    {/*
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
        }*/
    }

    public override void FixedUpdateNetwork()
    {
        //Debug.Log("FixedNetworkUpdate is doing update things.");
        //for cam text rotate
        if (mainCamera != null && textObject != null)
        {
            textObject.transform.LookAt(mainCamera.transform);
            textObject.transform.rotation = Quaternion.LookRotation(mainCamera.transform.forward);
        }
        //for mp node attributes
        //SetColor(color_id);
        //SetShape(shape_id);
        //SetName(nodeName);
        if (isCyclingColor) SetColor();
        if (toggleShape) SetShape();
        if (isHovered)
        {
            InputDevice rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            InputDevice leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

            if (rightController.TryGetFeatureValue(CommonUsages.trigger, out float triggerAxisVal))
            {
                if (triggerAxisVal > 0 && !isTriggerPressed)
                {
                    Select = true;
                    isTriggerPressed = true;
                }
                else if (triggerAxisVal == 0 && isTriggerPressed)
                {
                    isTriggerPressed = false;
                }
            }
        }
        if (Select)
        {
            if (!isSelected)
            {
                isSelected = true;
            }
            else
            {
                //Debug.Log("Node " + id + " was selected for editing");
                isEditing = true;
                isSelected = false;
            }
            Select = false;
            //Destroy(transform.gameObject);
        }

        if (previousColorId != color_id)
        {
            SetColor(color_id);
            previousColorId = color_id;
        }

        // React to shape changes
        if (previousShapeId != shape_id)
        {
            SetShape(shape_id);
            previousShapeId = shape_id;
        }

        // React to name changes
        if (previousNodeName != nodeName)
        {
            SetName(nodeName);
            previousNodeName = nodeName;
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
<<<<<<< HEAD
        transform.name=name;
=======
        nodeName = name;
        transform.name=name;
        Debug.Log(transform.name);
>>>>>>> e2adf9c (manual download)
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
