using UnityEngine;
using UnityEngine.XR;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit;
using Fusion;
using TMPro;
using System.IO;
using System;
using System.Collections.Generic;
using Fusion;
public class MindMap : MonoBehaviour
{
    
    private GameObject selected1;
    private GameObject selected2;
    private GameObject selectedLine;
    private LineRenderer[] Lines;
    [SerializeField] private string MindMapFileName = "MyFile.txt";
    [SerializeField] private int MaxConections = 120;

    NetworkRunner Runner;

    private GameObject[] Nodes1;
    private GameObject[] Nodes2;
    private int nodeCount;
    private int lineCount;
    [SerializeField] private bool doClone = false;
    [SerializeField] private bool doExport = false;
    [SerializeField] private bool doImport = false;
    [SerializeField] private NetworkObject objectToClone;
    [SerializeField] private GameObject NodeContainer;
    [SerializeField] Vector3 clonePosition;
    [SerializeField] public GameObject rightController;
    [SerializeField] private float distance;
    private bool yButtonPressed;

    [SerializeField] public GameObject Canvas;
    [SerializeField] public GameObject keyboard;
    [SerializeField] private GameObject text_input;
    private MindMapNode MMN;
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
        isTriggerPressed = false;
        yButtonPressed = false;
        takeScreenshot = false;
        path = Application.dataPath + "/" + MindMapFileName;
        Debug.Log(path);
        deleted = false;
        current_id = 0;

        Runner = NetworkManager.Instance.GetNetworkRunner();
    }

    void Update()
    {

        Debug.Log("Current MMN: " + MMN);
        Debug.Log("The active mindmap in  Update() is: " + this);
        LineRay();
        RightInput();
        if (doClone) MakeNode();
        if (doExport) export();
        if (doImport && !deleted) deleteNodes();
        else if (doImport && deleted) import();
        if (takeScreenshot) TakeScreenshot();

        foreach (MindMapNode mindMapNode in FindObjectsOfType<MindMapNode>())
        {
            GameObject ChildGameObject = mindMapNode.gameObject;
            MindMapLink mindMapLink = ChildGameObject.GetComponent<MindMapLink>();

            if (mindMapNode.isSelected)
            {
                Debug.Log(selected1);
                Debug.Log(selected2);
                if (selected1 == ChildGameObject)
                {
                    // double click placeholder
                }
                if (selected1 != null && selected1 != ChildGameObject)
                {
                    //Debug.Log("testas4");
                    if (selected2 == null)
                    {
                        //Debug.Log("testas2");
                        selected2 = ChildGameObject;
                        MindMapNode mindMapNode1 = selected1.GetComponent<MindMapNode>();
                        MindMapNode mindMapNode2 = selected2.GetComponent<MindMapNode>();

                        if (selectedLine != null)
                        {
                            mindMapLink = selectedLine.GetComponent<MindMapLink>();
                            int id = mindMapLink.id;
                            Nodes1[id] = selected1;
                            Nodes2[id] = selected2;
                        }
                        else
                        {
                            Nodes1[lineCount] = selected1;
                            Nodes2[lineCount] = selected2;
                            lineCount++;
                        }

                        mindMapNode1.isSelected = false;
                        mindMapNode2.isSelected = false;

                        selected1 = null;
                        selected2 = null;
                    }
                    else
                    {
                        //Debug.Log("testas3");
                    }
                }
                else if (selected1 == null)
                {
                    //Debug.Log("testas1");
                    selected1 = ChildGameObject;
                }
            }

            ///EDITING SHEIZE
            if (mindMapNode.isEditing)
            {
                Debug.Log("MMN Set to: " + MMN);
                MMN = mindMapNode;
                MMN.isEditing = false;
                Canvas.SetActive(true);

                //Lets make the canvas location actually close to the controller
                Canvas.transform.position = rightController.transform.position + rightController.transform.forward - new Vector3(0, 0.5f, 0);
                Vector3 forward = rightController.transform.forward;
                forward.y = 0; // Ignore vertical tilt
                Canvas.transform.rotation = Quaternion.LookRotation(forward);
                //Keyboard needs to move as well
                keyboard.transform.position = Canvas.transform.position + Canvas.transform.forward * -0.3f - new Vector3(0, 0.2f, 0); // adjust 0.3f & 0.1f as needed
                keyboard.transform.rotation = Canvas.transform.rotation;
            }

            if (mindMapNode.isDeleting)
            {
                for (int ii = lineCount - 1; ii >= 0; ii--)
                {
                    if (Nodes1[ii] == ChildGameObject || Nodes2[ii] == ChildGameObject)
                    {
                        Destroy(Lines[ii]);
                        lineCount--;
                        for (int iii = ii; iii < lineCount; iii++)
                        {
                            Nodes1[iii] = Nodes1[iii + 1];
                            Nodes2[iii] = Nodes2[iii + 1];
                            Lines[iii] = Lines[iii + 1];
                        }
                    }
                }
                Destroy(ChildGameObject);
            }

            if (mindMapLink != null)
            {
                if (mindMapLink.isDeleting)
                {
                    lineCount -= 1;
                    for (int ii = mindMapLink.id; ii < lineCount; ii++)
                    {
                        Lines[ii] = Lines[ii + 1];
                    }
                    Destroy(ChildGameObject);
                }
                if (mindMapLink.isSelected)
                {
                    selectedLine = ChildGameObject;
                }
            }
        }
        // Draw or update lines
        for (int i = 0; i < lineCount; i++)
        {
            GameObject Node1 = Nodes1[i];
            GameObject Node2 = Nodes2[i];
            LineRenderer lineRenderer = Lines[i];
            if (lineRenderer == null)
            {
                GameObject lineObject = new GameObject("Line " + (i));
                lineObject.transform.SetParent(transform);
                lineRenderer = lineObject.AddComponent<LineRenderer>();

                BoxCollider boxCollider = lineObject.AddComponent<BoxCollider>();
                AddColliderToLine(boxCollider, Node1.transform.position, Node2.transform.position);

                lineObject.tag = "Line";

                MindMapLink link = lineObject.AddComponent<MindMapLink>();
                link.id = i;
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.startColor = Color.red;
                lineRenderer.endColor = Color.red;
                lineRenderer.startWidth = 0.05f;
                lineRenderer.endWidth = 0.01f;
                lineRenderer.positionCount = 2;
                Lines[i] = lineRenderer;
            }

            lineRenderer = Lines[i];
            lineRenderer.SetPosition(0, Node1.transform.position);
            lineRenderer.SetPosition(1, Node2.transform.position);
        }
    }

    private void RightInput()
    {
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
                else if (!isPressed && yButtonPressed)
                {
                    yButtonPressed = false;
                }
            }
        }
    }

    public void LineRay()
    {
        Vector3 forward = rightController.transform.TransformDirection(Vector3.forward) * 10;
        InputDevice rightControllerInput = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        RaycastHit hit;
        if (Physics.Raycast(rightController.transform.position, forward, out hit, 10))
        {
            if (hit.collider.CompareTag("Line"))
            {
                if (rightControllerInput.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue) || true)
                {
                    triggerValue = testTriggerValue;
                    if (triggerValue <= 0.1f && isTriggerPressed)
                    {
                        isTriggerPressed = false;
                    }
                    if (triggerValue > 0.1f && !isTriggerPressed)
                    {
                        MindMapLink mindMapLink = hit.transform.gameObject.GetComponent<MindMapLink>();
                        mindMapLink.Select = true;
                        isTriggerPressed = true;
                    }
                }
            }
        }
    }

    public NetworkObject MakeNode()
    {
        //Debug.Log("MAKE NODE WAS CALLED");
        Vector3 clonePos = rightController.transform.position + rightController.transform.forward * distance;
        NetworkObject clone = Runner.Spawn(objectToClone, clonePos, objectToClone.transform.rotation);
        nodeCount += 1;

        MindMapNode nodeScript = clone.GetComponent<MindMapNode>();
        //Debug.Log("Script attached:");
        Debug.Log(nodeScript.name);
        //Debug.Log("Spawning with Runner: " + Runner);

        nodeScript.id = current_id;
        nodeScript.SetName("Node" + current_id);

        current_id++;
        doClone = false;
        Debug.Log("Node created: " + clone);
        return clone;
    }

    public void DoneButtonClick()
    {
        TMP_InputField textTMP_input = text_input.GetComponent<TMP_InputField>();
        MMN.SetName(textTMP_input.text);
        MMN.isEditing = false;
        MMN = null;
        Canvas.SetActive(false);
        keyboard.SetActive(false);
    }

    public void DeleteButtonClick()
    {
        MMN.isDeleting = true;
        MMN.isEditing = false;
        MMN = null;
        Canvas.SetActive(false);
        keyboard.SetActive(false);
    }

    public void ImportButtonClick()
    {
        MMN = null;
        doImport = true;
        Canvas.SetActive(false);
        keyboard.SetActive(false);
    }

    public void ExportButtonClick()
    {
        MMN = null;
        doExport = true;
        Canvas.SetActive(false);
        keyboard.SetActive(false);
    }

    public void TakeScreenshotButtonClick()
    {
        MMN = null;
        takeScreenshot = true;
        Canvas.SetActive(false);
        keyboard.SetActive(false);
    }

    public void ColorButtonClick()
    {
        Debug.Log("MMN Currently set to: " + MMN);
        Debug.Log("The active mindmap in ColorButtonClick is: " + this);
        MMN.isCyclingColor = true;
        MMN.isEditing = false;
        //MMN = null;
    }

    public void ShapeButtonClick()
    {
        MMN.toggleShape = true;
        MMN.isEditing = false;
        //MMN = null;
    }

    void AddColliderToLine(BoxCollider boxCollider, Vector3 start, Vector3 end)
    {
        Vector3 direction = end - start;
        float length = direction.magnitude;
        boxCollider.size = new Vector3(length, 0.1f, 0.1f);
        boxCollider.center = Vector3.zero;
        Vector3 center = (start + end) / 2f;
        boxCollider.transform.position = center;
        Quaternion rotation = Quaternion.LookRotation(direction);
        boxCollider.transform.rotation = rotation * Quaternion.Euler(0, -90, 0);
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

    void export()
    {
        string path = Application.persistentDataPath + "/" + MindMapFileName;
        string json = "";

        json += string.Format("{0};{1}\n", nodeCount, lineCount);

        var allNodes = FindObjectsOfType<MindMapNode>();
        foreach (var node in allNodes)
        {
            int id = node.id;
            int color_id = node.color_id;
            int shape_id = node.shape_id;
            string name = node.name;
            Vector3 coords = node.transform.position;
            json += string.Format("{0};{1};{2};{3};{4};{5};{6}\n", id, name, color_id, shape_id, coords.x, coords.y, coords.z);
        }

        for (int i = 0; i < lineCount; i++)
        {
            int id1 = Nodes1[i].GetComponent<MindMapNode>().id;
            int id2 = Nodes2[i].GetComponent<MindMapNode>().id;
            json += string.Format("{0};{1}\n", id1, id2);
        }

        Debug.Log(json);
        WriteToFile(path, json);
        doExport = false;
    }

    void deleteNodes()
    {
        List<GameObject> children = new List<GameObject>();
        for (int i = 0; i < lineCount; i++)
        {
            children.Add(Lines[i].transform.gameObject);
        }
        for (int i = 0; i < lineCount; i++)
        {
            Nodes1[i] = null;
            Nodes2[i] = null;
        }
        foreach (GameObject child in children)
        {
            Destroy(child);
        }

        var allNodes = FindObjectsOfType<MindMapNode>();
        foreach (var node in allNodes)
        {
            Destroy(node.gameObject);
        }

        lineCount = 0;
        nodeCount = 0;
        current_id = 0;
        deleted = true;
    }

    void import()
    {
        string[] lines = ReadFromFile(path).Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        string[] header = lines[0].Split(';');
        lineCount = int.Parse(header[1]);
        int t_nodeCount = int.Parse(header[0]);

        for (int i = 0; i < t_nodeCount; i++)
        {
            string[] p = lines[i + 1].Split(';');
            int id = int.Parse(p[0]);
            string name = p[1];
            int color_id = int.Parse(p[2]);
            int shape_id = int.Parse(p[3]);
            Vector3 coords = new Vector3(float.Parse(p[4]), float.Parse(p[5]), float.Parse(p[6]));

            NetworkObject node_GO = MakeNode();
            MindMapNode node_MMN = node_GO.GetComponent<MindMapNode>();
            node_MMN.id = id;
            node_MMN.SetName(name);
            node_MMN.SetColor(color_id);
            node_MMN.SetShape(shape_id);
            node_GO.transform.position = coords;
        }

        for (int i = 0; i < lineCount; i++)
        {
            string[] p = lines[i + 1 + t_nodeCount].Split(';');
            int id1 = int.Parse(p[0]);
            int id2 = int.Parse(p[1]);
            Nodes1[i] = FindNodeByID(id1);
            Nodes2[i] = FindNodeByID(id2);
        }

        deleted = false;
        doImport = false;
    }

    private GameObject FindNodeByID(int id)
    {
        var allNodes = FindObjectsOfType<MindMapNode>();
        foreach (var node in allNodes)
        {
            if (node.id == id)
                return node.gameObject;
        }
        return null;
    }

    public void TakeScreenshot()
    {
        FocusCameraOnContainer();
        RenderTexture rt = new RenderTexture(width, height, 24);
        targetCamera.targetTexture = rt;
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);

        targetCamera.Render();

        RenderTexture.active = rt;
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshot.Apply();

        targetCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        byte[] bytes = screenshot.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/CameraScreenshot.png", bytes);
        Debug.Log("Screenshot saved to: " + Application.dataPath + "/CameraScreenshot.png");
        takeScreenshot = false;
    }

    public void FocusCameraOnContainer()
    {
        float padding = 1.2f;
        var allRenderers = FindObjectsOfType<Renderer>().Where(r => r.GetComponent<MindMapNode>() != null).ToArray();
        if (allRenderers.Length == 0) return;

        Bounds bounds = allRenderers[0].bounds;
        foreach (Renderer r in allRenderers)
            bounds.Encapsulate(r.bounds);

        Vector3 center = bounds.center;
        float radius = bounds.extents.magnitude;
        Vector3 direction = targetCamera.transform.forward * -1f;
        float distance = radius * padding;

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
