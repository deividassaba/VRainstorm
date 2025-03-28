using UnityEngine;
using UnityEngine.XR;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit;
using Fusion;
using TMPro;
public class MindMap : MonoBehaviour
{
    
    private GameObject selected1;
    private GameObject selected2;
    private GameObject selectedLine;
    private LineRenderer[] Lines;

    [SerializeField] private int MaxConections=20;

    private GameObject[] Nodes1;
    private GameObject[] Nodes2;
    private int nodeCount;
    private int lineCount;
    [SerializeField] private bool doClone=false;
    [SerializeField] private GameObject objectToClone ;
    [SerializeField] Vector3 clonePosition ;

    private bool yButtonPressed;

    [SerializeField] private GameObject Canvas;
    [SerializeField] private GameObject text_input;
    MindMapNode MMN;
    void Start()
    {
        Canvas.SetActive(false);
        Nodes1 = new GameObject[MaxConections];
        Nodes2 = new GameObject[MaxConections];
        Lines = new LineRenderer[MaxConections];

        lineCount = 0;
        nodeCount = 0;

        yButtonPressed= false;
    }
    void Update()
    {
        RightInput();
        for(int i = 0 ; i < transform.childCount; i++){
            GameObject ChildGameObject = transform.GetChild(i).gameObject;
            MindMapNode mindMapNode = ChildGameObject.GetComponent<MindMapNode>();
            MindMapLink mindMapLink = ChildGameObject.GetComponent<MindMapLink>();
            if(mindMapNode!=null){
                if(mindMapNode.isSelected){
                    if(selected1==ChildGameObject){ //double click

                    }
                    if(selected1!=null && selected1 != ChildGameObject){
                        if(selected2 == null){
                            selected2 = ChildGameObject;
                            
                            MindMapNode mindMapNode1 = selected1.GetComponent<MindMapNode>();
                            MindMapNode mindMapNode2 = selected2.GetComponent<MindMapNode>();
                                
                            if(selectedLine != null){ //keisti
                                mindMapLink = selectedLine.GetComponent<MindMapLink>();
                                int id = mindMapLink.id;
                                Nodes1[id] = selected1;
                                Nodes2[id] = selected2;

                            }
                            else{ //naujas
                                Nodes1[lineCount] = selected1;
                                Nodes2[lineCount] = selected2;
                                lineCount++;
                            
                            }
                            mindMapNode1.isSelected=false;
                            mindMapNode2.isSelected=false;
                            
                            selected1=null;
                            selected2=null;
                             
                        }
                    }
                    else{
                        selected1 = ChildGameObject;
                    }
                }
                //
                if(mindMapNode.isEditing){
                    MMN=mindMapNode;
                    Canvas.SetActive(true);
                }
                //trinimas
                if(mindMapNode.isDeleting){
                    for(int ii=lineCount-1;ii>=0;ii-=1){
                            if(Nodes1[ii]== ChildGameObject || Nodes2[ii] ==ChildGameObject){
                                Destroy(Lines[ii]);
                                lineCount--;
                                for(int iii=ii;iii<(lineCount-1);iii++){
                                    Nodes1[iii]=Nodes1[iii+1];
                                    Nodes2[iii]=Nodes2[iii+1];
                                    
                                    Lines[iii]=Lines[iii+1];
                                }
                                
                            }
                        }
                    Destroy(ChildGameObject);
                }
            }
            if(mindMapLink!=null){
                if(mindMapLink.isDeleting){
                    lineCount-=1;
                    for(int ii=mindMapLink.id; ii < lineCount;ii++)
                    {
                        Lines[ii]=Lines[ii+1];
                    }
                    Destroy(ChildGameObject);
                }
                if(mindMapLink.isSelected){
                    selectedLine=ChildGameObject;
                }
            }
            
        }
        for(int i = 0 ; i < lineCount; i++){
            GameObject Node1 = Nodes1[i]; 
            GameObject Node2 = Nodes2[i];
            LineRenderer lineRenderer = Lines[i];
            if(lineRenderer ==null){
                GameObject lineObject = new GameObject("Line " + (i));
                lineObject.transform.SetParent(transform);
                lineRenderer = lineObject.AddComponent<LineRenderer>();
                MindMapLink link = lineObject.AddComponent<MindMapLink>();
                link.id=i;
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.SetColors(Color.red,Color.red ); 
                lineRenderer.startWidth = 0.05f;
                lineRenderer.endWidth = 0.01f;
                lineRenderer.positionCount = 2;
                Lines[i] = lineRenderer;
            }
            
            lineRenderer= Lines[i];
            lineRenderer.SetPosition(0, Node1.transform.position);
            lineRenderer.SetPosition(1, Node2.transform.position);
        }

        //klonuoti
        if (doClone)
        {
            // Create a copy of the object at the same position and rotation
            //GameObject clone = Instantiate(objectToClone, objectToClone.transform.position, objectToClone.transform.rotation, objectToClone.transform.parent);
            GameObject clone = Instantiate(objectToClone, clonePosition, objectToClone.transform.rotation, objectToClone.transform.parent);
            // Optional: Modify the clone, like setting a new name
            ///clone.name = objectToClone.name + "_Clone";
            nodeCount+=1;
            clone.name = "Node_" +nodeCount;
            doClone=false;
        }
        
        //double-click
        if(false){

        }
    }

    private void RightInput(){
        InputDevice rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (rightController.isValid)
        {
            if (rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out bool isPressed))
            {
                if (isPressed && !yButtonPressed)
                {
                    doClone = true;
                    yButtonPressed = true;
                }
                else if (!isPressed && yButtonPressed) {yButtonPressed = false;}
            }
        }
    }
    public void DoneButtonClick(){
        
        
        TMP_InputField  textTMP_input = text_input.GetComponent<TMP_InputField >();
        transform.name=textTMP_input.text;
        GameObject textObject= MMN.transform.GetChild(0).gameObject; 
        TextMeshPro textTMP = textObject.GetComponent<TextMeshPro >();
        textTMP.text = transform.name;
        
        
        MMN.isEditing=false;
        MMN=null;
        Canvas.SetActive(false);
    }
    public void DeleteButtonClick(){
        
        MMN.isDeleting=true;
        
        MMN.isEditing=false;
        MMN=null;
        Canvas.SetActive(false);
    }
}
