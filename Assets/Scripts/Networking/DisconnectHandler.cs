using UnityEngine;

public class DisconnectHandler : MonoBehaviour
{
    public void CallDisconnect()
    {
        Debug.Log(" CallDisconnect triggered.");
        if (NetworkManager.Instance == null)
        {
            Debug.LogWarning(" NetworkManager.Instance is NULL!");
            return;
        }

        Debug.Log("Disconnect button pressed.");
        NetworkManager.Instance?.Disconnect();
    }
}
