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
    private bool isHovered;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Transform[] Nodes;
    private LineRenderer[] Lines;
    private int maxConnections=20;

    private MindMap mindMap;
    private GameObject parentGameObject;
    
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
    }

    void Update()
    {

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


    private void OnDestroy()
    {
        grabInteractable.hoverEntered.RemoveListener(OnHoverEntered);
        grabInteractable.hoverExited.RemoveListener(OnHoverExited);
    }
    void OnHoverEntered(HoverEnterEventArgs args){ isHovered=true;}
    void OnHoverExited(HoverExitEventArgs args){ isHovered=false;}
}
