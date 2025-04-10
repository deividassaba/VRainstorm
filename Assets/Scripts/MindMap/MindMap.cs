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
    [SerializeField] private GameObject rightController ;
    [SerializeField] private float distance ;
    private bool yButtonPressed;

    [SerializeField] private GameObject Canvas;
    [SerializeField] private GameObject text_input;
    MindMapNode MMN;
    private bool isTriggerPressed;
    [SerializeField] private float testTriggerValue;
    void Start()
    {
        Canvas.SetActive(false);
        Nodes1 = new GameObject[MaxConections];
        Nodes2 = new GameObject[MaxConections];
        Lines = new LineRenderer[MaxConections];

        lineCount = 0;
        nodeCount = 0;
        isTriggerPressed =false;
        yButtonPressed= false;
    }
    void Update()
    {
        LineRay();
        RightInput();
        if(doClone) DoClone();
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
        //brazomos/redaguojamos linijos naujos
        for(int i = 0 ; i < lineCount; i++){
            GameObject Node1 = Nodes1[i]; 
            GameObject Node2 = Nodes2[i];
            LineRenderer lineRenderer = Lines[i];
            if(lineRenderer ==null){
                GameObject lineObject = new GameObject("Line " + (i));
                lineObject.transform.SetParent(transform);
                lineRenderer = lineObject.AddComponent<LineRenderer>();
                
                BoxCollider BoxCollider = lineObject.AddComponent<BoxCollider>();
                //BoxCollider.size = new Vector3(0.2f, 0.1f, 0.1f);
                AddColliderToLine(BoxCollider,Node1.transform.position,Node2.transform.position );

                lineObject.tag = "Line";

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
        
        
        //double-click
        if(false){

        }
    }
    /*

    yButtonPressed paspaudus mygtuka padaromas klonas
    
    */
    private void RightInput(){
        InputDevice rightControllerInput = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (rightControllerInput.isValid)
        {
            if (rightControllerInput.TryGetFeatureValue(CommonUsages.secondaryButton, out bool isPressed))
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
    public void LineRay(){
        Vector3 forward = rightController.transform.TransformDirection(Vector3.forward) * 10;
        //Debug.DrawRay(rightController.transform.position, forward, Color.green);
        InputDevice rightControllerInput = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        RaycastHit hit;
        if (Physics.Raycast(rightController.transform.position, rightController.transform.forward, out hit, 10))
        {
            if(hit.collider.CompareTag("Line")){
                

                //Debug.Log(hit.transform.gameObject.name);
                if (rightControllerInput.TryGetFeatureValue(CommonUsages.trigger , out float triggerValue) || true)
                {
                    triggerValue=testTriggerValue;
                    if(triggerValue <= 0.1f && isTriggerPressed){
                        Debug.Log("Trigger not pressed");
                        isTriggerPressed=false;
                    }
                    //Debug.Log(hit.transform.gameObject.name);
                    if (triggerValue > 0.1f && !isTriggerPressed) 
                    {
                        Debug.Log("selected");
                        MindMapLink mindMapLink = hit.transform.gameObject.GetComponent<MindMapLink>();
                        mindMapLink.Select=true;
                        isTriggerPressed=true;
                    }
                }

            }
        }
    }
    //klonuoti
    public void DoClone()
    {
        Vector3 clonePos = rightController.transform.position;
        //clonePos.x+=distance;
        clonePos = clonePos +rightController.transform.forward * distance;
        //GameObject clone = Instantiate(objectToClone, objectToClone.transform.position, objectToClone.transform.rotation, objectToClone.transform.parent);
        GameObject clone = Instantiate(objectToClone, clonePos , objectToClone.transform.rotation, objectToClone.transform.parent);
        
        // Optional: Modify the clone, like setting a new name
        ///clone.name = objectToClone.name + "_Clone";
        nodeCount+=1;
        clone.name = "Node_" +nodeCount;
        doClone=false;
        
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
    public void ColorButtonClick(){
        MMN.isCyclingColor=true;
        
        MMN.isEditing=false;
        MMN=null;
        Canvas.SetActive(false);
    }
    public void ShapeButtonClick(){
        MMN.toggleShape=true;
        
        MMN.isEditing=false;
        MMN=null;
        Canvas.SetActive(false);
    }
    void AddColliderToLine(BoxCollider boxCollider, Vector3 start, Vector3 end)
    {
        Vector3 direction = end - start;
        float length = direction.magnitude;

        // Set collider size (length along X, thin height/depth)
        boxCollider.size = new Vector3(length, 0.1f, 0.1f);
        boxCollider.center = new Vector3(0f,0f,0f);
        // Position: center of the line
        Vector3 center = (start + end) / 2f;
        boxCollider.transform.position = center;

        // Rotation: align the collider's X-axis with the direction
        Quaternion rotation = Quaternion.LookRotation(direction);
        boxCollider.transform.rotation = rotation * Quaternion.Euler(0, -90, 0); // Adjust for local X alignment
    }
}
