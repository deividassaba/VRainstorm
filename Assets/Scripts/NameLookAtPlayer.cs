using UnityEngine;

public class NameLookAtPlayer : MonoBehaviour
{
    void Update()
    {
        if (Camera.main != null)
            transform.LookAt(transform.position + Camera.main.transform.forward);
    }
}
