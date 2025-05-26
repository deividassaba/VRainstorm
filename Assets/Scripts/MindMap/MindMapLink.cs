using UnityEngine;

public class MindMapLink : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] public bool isDeleting;
    [SerializeField] public bool Select;
    [SerializeField] public bool isSelected;
    [SerializeField] public bool isEditing;
    public int id;
    void Start()
    {

    }

    // Update is called once per frame
    /*
    spustelt ant linijos pasirenkama
    kiti du pasirinkimai ant burbulu pakeicia rysi  
    du spustelejimai ant linijos ja istrina
    */
    void Update()
    {
        if (Select)
        {
            if(!isSelected){
                isSelected=true;
            }
            else{
                //isEditing=true;
                isDeleting= true;
                isSelected=false;
            }

            Select=false;
            //Destroy(transform.gameObject);
        }
    }
}
