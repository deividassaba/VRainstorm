using UnityEngine;
using TMPro;
public class MindMapNode : MonoBehaviour
{
    [SerializeField] public bool isDeleting;
    [SerializeField] public bool Select;
    [SerializeField] public bool isSelected;
    [SerializeField] public bool isEditing;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Transform[] Nodes;
    private LineRenderer[] Lines;
    private int maxConnections=20;

    private MindMap mindMap;
    private GameObject parentGameObject;
    
    void Start()
    {
        parentGameObject = this.transform.parent.gameObject;
        mindMap = GetComponentInParent<MindMap>();
        isSelected = false;
        GameObject textObject= transform.GetChild(0).gameObject; 
        TextMeshPro textTMP = textObject.GetComponent<TextMeshPro >();
        textTMP.text = transform.name;
    }

    // Update is called once per frame
    void Update()
    {
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
}
