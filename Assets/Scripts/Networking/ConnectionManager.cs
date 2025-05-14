using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField playerNameInputField; // For player name
    [SerializeField] private TMP_InputField roomCodeInputField;   // For room code

    void Start()
    {
        // If you want to dynamically assign these:
        if (playerNameInputField == null)
        {
            GameObject playerNameObject = GameObject.Find("NameInputField"); // Name of the input field GameObject in the hierarchy
            if (playerNameObject != null)
            {
                playerNameInputField = playerNameObject.GetComponent<TMP_InputField>();
            }
            else
            {
                Debug.LogError("PlayerNameInput not found!");
            }
        }

        if (roomCodeInputField == null)
        {
            GameObject roomCodeObject = GameObject.Find("InputField (TMP)"); // Name of the room code input field GameObject
            if (roomCodeObject != null)
            {
                roomCodeInputField = roomCodeObject.GetComponent<TMP_InputField>();
            }
            else
            {
                Debug.LogError("RoomCodeInput not found!");
            }
        }
    }

    public void CreateRoom()
    {
        string playerName = playerNameInputField.text; // Get the player's name
        string roomCode = roomCodeInputField.text;     // Get the room code

        NetworkManager.Instance.SetPlayerName(playerName);  // Store the player name in NetworkManager
        NetworkManager.Instance.CreateSession(roomCode);    // Create the session with the room code
    }

    public void JoinRoom()
    {
        string playerName = playerNameInputField.text; // Get the player's name
        string roomCode = roomCodeInputField.text;     // Get the room code

        NetworkManager.Instance.SetPlayerName(playerName);  // Store the player name in NetworkManager
        NetworkManager.Instance.JoinSession(roomCode);      // Join the session with the room code
    }


}