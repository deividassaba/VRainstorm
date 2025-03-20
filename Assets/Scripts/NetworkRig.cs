using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

public class NetworkRig : NetworkBehaviour
{
    public bool IsLocalNetworkRig => Object.HasStateAuthority;

    [Header("RigComponents")]
    [SerializeField]
    private NetworkTransform playerTransform;

    [SerializeField]
    private NetworkTransform headTransform;

    [SerializeField]
    private NetworkTransform leftHandTransform;

    [SerializeField]
    private NetworkTransform rightHandTransform;

    [Header("Player Name Display")]
    [SerializeField]
    private TMP_Text playerNameText;

    HardwareRig hardwareRig;

    [Networked] public string PlayerName { get; set; }

    public override void Spawned()
    {
        base.Spawned();

        if (IsLocalNetworkRig)
        {
            hardwareRig = FindObjectOfType<HardwareRig>();
            if (hardwareRig == null)
                Debug.LogError("Missing HardwareRig in the scene");

            // Assign a random name only for the local player
            PlayerName = "Player_" + Random.Range(1000, 9999);

            // Disable the networked visual components for the local player
            headTransform.gameObject.SetActive(false);
            leftHandTransform.gameObject.SetActive(false);
            rightHandTransform.gameObject.SetActive(false);

            // Inform all clients about the player name
            SetPlayerNameRPC(PlayerName);
        }

        // Display the player name (for local and remote players)
        playerNameText.text = PlayerName;
    }

    // RPC to sync player name across all clients
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void SetPlayerNameRPC(string name)
    {
        PlayerName = name;
        playerNameText.text = name;
    }

    [SerializeField]
    private float moveSpeed = 2f;

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        if (GetInput<RigState>(out var input))
        {
            Vector3 inputPosition = input.PlayerPosition * moveSpeed * Time.deltaTime;
            Quaternion inputRotation = input.PlayerRotation;

            // Set position and rotation for the player
            playerTransform.transform.SetPositionAndRotation(inputPosition, inputRotation);
            headTransform.transform.SetPositionAndRotation(input.HeadsetPosition, input.HeadsetRotation);
            leftHandTransform.transform.SetPositionAndRotation(input.LeftHandPosition, input.LeftHandRotation);
            rightHandTransform.transform.SetPositionAndRotation(input.RightHandPosition, input.RightHandRotation);
        }

        // Always update player name text
        playerNameText.text = PlayerName;
    }

    public override void Render()
    {
        base.Render();
        if (IsLocalNetworkRig)
        {
            playerTransform.transform.SetPositionAndRotation(hardwareRig.playerTransform.position, hardwareRig.playerTransform.rotation);
            headTransform.transform.SetPositionAndRotation(hardwareRig.headTransform.position, hardwareRig.headTransform.rotation);
            leftHandTransform.transform.SetPositionAndRotation(hardwareRig.leftHandTransform.position, hardwareRig.leftHandTransform.rotation);
            rightHandTransform.transform.SetPositionAndRotation(hardwareRig.rightHandTransform.position, hardwareRig.rightHandTransform.rotation);
        }
    }
}
