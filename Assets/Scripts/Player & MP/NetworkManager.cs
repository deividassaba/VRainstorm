using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    //creating a singleton
    public static NetworkManager Instance { get; private set; }

    [SerializeField]
    private GameObject _runnerPrefab;

    [SerializeField]
    private TMP_InputField playerNameInputField;

    [SerializeField]
    private TextMeshProUGUI outputField;

    [SerializeField]
    public TMP_Text roomCodeText;

    public PlayerSpawner playerSpawner;

    private void UpdateText(string text)
    {
        // Ensure the text change is done on the main thread
        // This can be called directly as it doesn't involve async calls
        outputField.text = text;
    }

    public string PlayerName { get; private set; }

    public NetworkRunner Runner { get; private set; }

    private void Awake()
    {
        if (playerNameInputField == null)
        {
            playerNameInputField = GameObject.Find("NameInputField")?.GetComponent<TMP_InputField>();
            if (playerNameInputField == null)
            {
                //Debug.LogError("NameInputField not found in the scene!");
            }
        }
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public void SetPlayerName(string name)
    {
        PlayerName = string.IsNullOrWhiteSpace(name) ? "Player_" + UnityEngine.Random.Range(1000, 9999) : playerNameInputField.text;
        Debug.Log($"SetPlayerName called with: {PlayerName}");
    }

    public string GetPlayerName()
    {
        return PlayerName;
    }

    public NetworkRunner GetNetworkRunner()
    {
        return Runner;
    }

    private void Start()
    {
        // fixing the server to a perticular region
        Fusion.Photon.Realtime.PhotonAppSettings.Global.AppSettings.FixedRegion = "eu";
        playerSpawner = FindAnyObjectByType<PlayerSpawner>();
        outputField.text = "";
    }

    public async void CreateSession(string roomCode)
    {
        if (roomCode == "" || roomCode == null)
        {
            UpdateText("Error: Lobby ID cannot be blank.");
            return;
        }
        if (false)
        {
            UpdateText("Error: Lobby ID already exists. Please select a different ID.");
            return;
        }
        //Create Runner
        CreateRunner();
        //Load Scene
        await LoadScene();
        //ConnectSession
        await Connect(roomCode);

        TMP_Text roomCodeText = GameObject.Find("RoomCodeText")?.GetComponent<TMP_Text>();
        if (roomCodeText != null)
        {
            roomCodeText.text = "Room Code: " + roomCode;
        }
        else
        {
            Debug.LogError("RoomCodeText object not found!");
        }
    }

    public async void JoinSession(string roomCode)
    {
        if (roomCode == "" || roomCode == null)
        {
            UpdateText("Error: Lobby ID cannot be blank.");
            return;
        }
        if (false)
        {
            UpdateText("Error: Lobby ID doesn't exist. Please select a different ID.");
            return;
        }

        //Create Runner
        CreateRunner();
        //Load Scene
        await LoadScene();
        //ConnectSession
        await Connect(roomCode);

        TMP_Text roomCodeText = GameObject.Find("RoomCodeText")?.GetComponent<TMP_Text>();
        if (roomCodeText != null)
        {
            roomCodeText.text = "Room Code: " + roomCode;
        }
        else
        {
            Debug.LogError("RoomCodeText object not found!");
        }
    }



    public void CreateRunner()
    {
        Runner = Instantiate(_runnerPrefab, transform).GetComponent<NetworkRunner>();
        Runner.AddCallbacks(this);
    }

    public async Task LoadScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1);

        while (!asyncLoad.isDone)
        {
            await Task.Yield();
        }
    }

    private async Task Connect(string SessionName)
    {
        var args = new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = SessionName,
            SceneManager = GetComponent<NetworkSceneManagerDefault>(),
            Scene = SceneRef.FromIndex(1)

        };
        await Runner.StartGame(args);
        playerSpawner = FindAnyObjectByType<PlayerSpawner>();
    }

    /*public void Disconnect()
    {
        Debug.Log("<<<<<Disconnect button pressed>>>>>>");
        StartCoroutine(ShutdownAndLoadStartScene());
    }*/

    public void Disconnect()
    {
        Debug.Log("<<<<< Disconnect button pressed >>>>>");

        if (Runner.IsSharedModeMasterClient) // If the host is disconnecting
        {
            Debug.Log("Host disconnecting—attempting host migration.");

            if (Runner.ActivePlayers.Count() > 1)
            {
                Debug.Log("Selecting a new host...");
                PlayerRef newHost = Runner.ActivePlayers.FirstOrDefault();
                OnHostMigration(Runner, newHost);
            }
            else
            {
                Debug.Log("No players left—shutting down session.");
                StartCoroutine(ShutdownAndLoadStartScene());
            }
        }
        else
        {
            Debug.Log("Client disconnecting normally.");
            Runner.Shutdown(true);
            StartCoroutine(ShutdownAndLoadStartScene());
        }
    }

    private IEnumerator ShutdownAndLoadStartScene()
    {
        if (NetworkManager.Instance.Runner != null)
        {
            yield return NetworkManager.Instance.Runner.Shutdown();
        }

        Destroy(NetworkManager.Instance.gameObject); // Fully remove NetworkManager

        //yield return new WaitForSeconds(1); // Give Unity time to clean up

        SceneManager.LoadScene("StartGame");
    }

    private Dictionary<PlayerRef, NetworkObject> _spawnedUsers = new Dictionary<PlayerRef, NetworkObject>();


    #region INetworkRunnerCallbacks
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("<<<<<<<< A new player joined to the session >>>>>>>");
        Debug.Log("<<<<<<< IsMasterClient >>>>>>>>" + player.IsMasterClient);
        Debug.Log("<<<<<<< PlayerID >>>>>>>>" + player.PlayerId);
        Debug.Log("<<<<<<< IsRealPlayer >>>>>>>>" + player.IsRealPlayer);


        if (runner.IsServer) // Only the server should spawn players
        {
            NetworkObject networkPlayerObject = runner.Spawn(_runnerPrefab, Vector3.zero, Quaternion.identity);

            _spawnedUsers.Add(player, networkPlayerObject);

            NetworkRig networkRig = networkPlayerObject.GetComponent<NetworkRig>();
            if (networkRig != null)
            {
                networkRig.SetPlayerNameRPC(NetworkManager.Instance.GetPlayerName());
            }
        }
    }

    /*public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("<<<<<<<< A player left the session >>>>>>>");
        Debug.Log("<<<<<<< IsMasterClient >>>>>>>>" + player.IsMasterClient);
        Debug.Log("<<<<<<< PlayerID >>>>>>>>" + player.PlayerId);
        Debug.Log("<<<<<<< IsRealPlayer >>>>>>>>" + player.IsRealPlayer);
    }*/

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player {player.PlayerId} left the game.");

        if (runner.IsSharedModeMasterClient)
        {
            Debug.Log("Current host left! Selecting a new host...");
            PlayerRef newHost = runner.ActivePlayers.FirstOrDefault();
            OnHostMigration(runner, newHost);
        }
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("<<<<<<< Runner Shutdown >>>>>>>>");

    }
    #endregion

    #region INetworkRunnerCallbacks (Unused)
    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    public void OnHostMigration(NetworkRunner runner, PlayerRef newHost)
    {
        Debug.Log($"Host migration triggered! New host: {newHost.PlayerId}");

        if (runner.IsSharedModeMasterClient) // Only execute if we are the new host
        {
            Debug.Log("Reassigning network authority to the new host...");

            foreach (var networkObject in runner.GetAllNetworkObjects())
            {
                if (networkObject.HasStateAuthority)
                {
                    Debug.Log($"Transferring authority for object: {networkObject.name}");
                    networkObject.AssignInputAuthority(newHost);
                }
            }
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }



    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }


    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        throw new NotImplementedException();
    }
    #endregion

}