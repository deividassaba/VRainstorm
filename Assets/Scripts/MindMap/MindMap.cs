using UnityEngine;
using UnityEngine.XR;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit;
using Fusion;
using TMPro;
using System.IO;
using System;
using System.Collections.Generic;
public class MindMap : MonoBehaviour
{
    
    private GameObject selected1;
    private GameObject selected2;
    private GameObject selectedLine;
    private LineRenderer[] Lines;
    [SerializeField] private string MindMapFileName = "MyFile.txt";
    [SerializeField] private int MaxConections=20;

    private GameObject[] Nodes1;
    private GameObject[] Nodes2;
    private int nodeCount;
    private int lineCount;
    [SerializeField] private bool doClone=false;
    [SerializeField] private bool doExport=false;
    [SerializeField] private bool doImport=false;
    [SerializeField] private GameObject objectToClone ;
    [SerializeField] private GameObject NodeContainer ;
    [SerializeField] Vector3 clonePosition ;
    [SerializeField] private GameObject rightController ;
    [SerializeField] private float distance ;
    private bool yButtonPressed;

    [SerializeField] private GameObject Canvas;
    [SerializeField] private GameObject text_input;
    MindMapNode MMN;
    private bool isTriggerPressed;
    [SerializeField] private float testTriggerValue;
    private int current_id; 

         public Camera targetCamera;
    public int width = 1920;
    public int height = 1080;
    public bool takeScreenshot;
    private string path;
        
    private bool deleted; 
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
        takeScreenshot=false;
        //string path = Application.persistentDataPath + "/" + MindMapFileName;
        //Application.dataPath
        path =Application.dataPath + "/" + MindMapFileName;
        // WriteToFile(path, "Hello, Unity File System!");
        Debug.Log(path);
        deleted=false;
        current_id=0;
    }
    void Update()
    {
        LineRay();
        RightInput();
        if(doClone) MakeNode();
        if(doExport) export();
        if(doImport && !deleted) deleteNodes();
        else if(doImport && deleted) import();
        if(takeScreenshot) TakeScreenshot();
        for(int i = 1 ; i < transform.childCount; i++){
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
    
    public GameObject MakeNode()
    {
        Vector3 clonePos = rightController.transform.position;
        clonePos = clonePos +rightController.transform.forward * distance;
        GameObject clone = Instantiate(objectToClone, clonePos , objectToClone.transform.rotation, NodeContainer.transform);
        nodeCount+=1;
        clone.GetComponent<MindMapNode>().id=current_id;
        clone.GetComponent<MindMapNode>().SetName("Node_" +current_id);
        current_id++;
        doClone=false;
        return clone;
    }
    public void DoneButtonClick(){
        TMP_InputField  textTMP_input = text_input.GetComponent<TMP_InputField >();
        MMN.SetName(textTMP_input.text);
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
    public void ImportButtonClick(){
        MMN=null;
        doImport=true;
        Canvas.SetActive(false);
    }
    public void ExportButtonClick(){
        MMN=null;
        doExport=true;
        Canvas.SetActive(false);
    }
    public void TakeScreenshotButtonClick(){
        MMN=null;
        takeScreenshot= true;
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



    void WriteToFile(string path, string content)
    {
        try
        {
            File.WriteAllText(path, content);
            Debug.Log("Successfully wrote to: " + path);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error writing to file: " + e.Message);
        }
    }

    string ReadFromFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }
            else
            {
                Debug.LogWarning("File not found: " + path);
                return null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error reading file: " + e.Message);
            return null;
        }
    }
    void export(){
        // Nodes1[id] = selected1;
        // Nodes2[id] = selected2;
        //Lines
        string path = Application.persistentDataPath + "/" + MindMapFileName;
        //Nodes
        string json="";
        // for( int i=0 ; i < nodeCount; i++){
        //     int id;
        //     Vector3 coords;
        //     int color_id;
        //     int shape_id;
        //     string name;
        //     json += string.Format("{0};{1}\n",id,name);
        // }
        json += string.Format("{0};{1}\n",nodeCount,lineCount);
        for(int i = 0 ; i < NodeContainer.transform.childCount; i++){
            GameObject ChildGameObject = NodeContainer.transform.GetChild(i).gameObject;
            MindMapNode mindMapNode = ChildGameObject.GetComponent<MindMapNode>();
            if(mindMapNode){
                int id = mindMapNode.id;
                int color_id = mindMapNode.color_id;
                int shape_id = mindMapNode.shape_id;
                string name = ChildGameObject.name;
                Vector3 coords = ChildGameObject.transform.position;
                json += string.Format("{0};{1};{2};{3};{4};{5};{6}\n",id,name,color_id,shape_id,coords.x,coords.y,coords.z);
            }
        
        }
        for( int i=0 ; i < lineCount; i++){
            int id1=Nodes1[i].GetComponent<MindMapNode>().id;
            int id2=Nodes2[i].GetComponent<MindMapNode>().id;
            json += string.Format("{0};{1}\n",id1,id2);
        }
        Debug.Log(json);   
        WriteToFile(path,json);
        doExport = false;
    }
    void deleteNodes(){
        for(int i = 0 ; i <lineCount;i++)
        {
            Nodes1[i] = null; 
            Nodes2[i] = null;
            Destroy(Lines[i].transform.gameObject);
        }
        lineCount=0;
        nodeCount=0;
        current_id=0;
        // for(int i = 0 ; i <NodeContainer.transform.childCount;i++)
        // {
        //     Transform child = NodeContainer.transform.GetChild(i);
        //     Destroy(child.gameObject);
        // }
        List<GameObject> children = new List<GameObject>();
        for (int i = 0; i < NodeContainer.transform.childCount; i++) {
            children.Add(NodeContainer.transform.GetChild(i).gameObject);
        }

        foreach (GameObject child in children) {
            Destroy(child);
        }
        deleted = true;
    }
    void import(){
        
        
        
        //string path = Application.persistentDataPath + "/" + MindMapFileName;
        string[] lines = ReadFromFile(path).Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        string[] header = lines[0].Split(';');
        lineCount = int.Parse(header[1]);
        int t_nodeCount= int.Parse(header[0]);
        for(int i = 0; i < t_nodeCount ; i++){

            string[] p = lines[i+1].Split(';');
            int id = int.Parse(p[0]);
            string name = p[1];
            Debug.Log(lines[i+1]);
            int color_id = int.Parse(p[2]);
            int shape_id = int.Parse(p[3]);
            Vector3 coords = new Vector3(float.Parse(p[4]),float.Parse(p[5]),float.Parse(p[6]));
            GameObject node_GO = MakeNode();

            MindMapNode node_MMN = node_GO.GetComponent<MindMapNode>();
            node_MMN.id= id;
            node_MMN.SetName(name);
            node_MMN.SetColor(color_id);
            node_MMN.SetShape(shape_id);
            node_GO.transform.position = coords;
        }
        
        for(int i = 0; i < lineCount ; i++){
            string[] p = lines[i+1+nodeCount].Split(';');
            int id1 = int.Parse(p[0]);
            int id2 = int.Parse(p[1]);
            Nodes1[i] = findNodeByID(id1);
            Nodes2[i] = findNodeByID(id2);
        }
        deleted = false;
        doImport = false;
    }
    private GameObject findNodeByID(int id){
        for(int i = 0 ; i < NodeContainer.transform.childCount; i++){
            GameObject ChildGameObject = NodeContainer.transform.GetChild(i).gameObject;
            MindMapNode mindMapNode = ChildGameObject.GetComponent<MindMapNode>();
            if(mindMapNode && mindMapNode.id == id){
                return ChildGameObject;
            }
        }
        return null;
    }


    public void TakeScreenshot()
    {


        // Vector3 pos = NodeContainer.transform.position; 
        // targetCamera.transform.position = pos; 

        FocusCameraOnContainer();
        RenderTexture rt = new RenderTexture(width, height, 24);
        targetCamera.targetTexture = rt;
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);

        targetCamera.Render(); // Render camera manually

        RenderTexture.active = rt;
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshot.Apply();

        targetCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        // Optionally save to PNG
        byte[] bytes = screenshot.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.dataPath + "/CameraScreenshot.png", bytes);
        Debug.Log("Screenshot taken from " + targetCamera.name + " to : " + Application.dataPath + "/CameraScreenshot.png");
        takeScreenshot=false;
    }
    public void FocusCameraOnContainer()
    {
        float padding = 1.2f;
        Renderer[] renderers = NodeContainer.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        // 1. Get bounds that include all child renderers
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
            bounds.Encapsulate(r.bounds);

        Vector3 center = bounds.center;
        float radius = bounds.extents.magnitude;

        // 2. Move the camera to frame the bounds
        Vector3 direction = targetCamera.transform.forward * -1f; // Move opposite to where it's looking
        float distance = radius * padding;

        // Optional: Adjust distance based on camera's FOV
        if (targetCamera.orthographic)
        {
            targetCamera.orthographicSize = radius * padding;
        }
        else
        {
            float fovRad = Mathf.Deg2Rad * targetCamera.fieldOfView * 0.5f;
            distance = radius / Mathf.Tan(fovRad);
        }

        targetCamera.transform.position = center + direction.normalized * distance;
        targetCamera.transform.LookAt(center);
    }
}

