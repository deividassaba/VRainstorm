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

    HardwareRig hardwareRig;

    [Header("Player Name Display")]
    [SerializeField]
    private TMP_Text playerNameText;

    [Networked] public string PlayerName { get; set; }
    
    //for hiding player body
    private GameObject vrHeadModel;
    private GameObject leftHandModel;
    private GameObject rightHandModel;

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void SetPlayerNameRPC(string name)
    {
        if (playerNameText == null)
        {
            Debug.LogError("playerNameText is NULL! Make sure it's assigned in the Inspector.");
            return;
        }

        PlayerName = name;
        playerNameText.text = name;
    }


    public override void Spawned()
    {
        base.Spawned();

        if (playerNameText == null)
        {
            playerNameText = GetComponentInChildren<TMP_Text>();
            if (playerNameText == null)
            {
                Debug.LogError("playerNameText still NULL! Make sure it's in the prefab.");
            }
        }


        if (IsLocalNetworkRig)
        {
            hardwareRig = FindAnyObjectByType<HardwareRig>();
            if (hardwareRig == null)
                Debug.LogError("Missing HardwareRig in the scene");

            string assignedName = string.IsNullOrWhiteSpace(NetworkManager.Instance.GetPlayerName())
        ? "Player_" + UnityEngine.Random.Range(1000, 9999)
        : NetworkManager.Instance.GetPlayerName();


            SetPlayerNameRPC(NetworkManager.Instance.GetPlayerName());

            foreach (var renderer in headTransform.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
            }
            foreach (var renderer in leftHandTransform.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
            }
            foreach (var renderer in rightHandTransform.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
            }

            Transform body = playerTransform.transform.Find("Player/Body");
            if (body != null)
            {
                foreach (var r in body.GetComponentsInChildren<Renderer>())
                    r.enabled = false;
            }
        }

        playerNameText.text = PlayerName;
        // else it means that this is a client
    }

    public override void FixedUpdateNetwork()
    {
        if (Runner.Tick % 20 != 0) return;

        base.FixedUpdateNetwork();

        if (GetInput<RigState>(out var input))
        {
            playerTransform.transform.SetPositionAndRotation(input.PlayerPosition, input.PlayerRotation);

            headTransform.transform.SetPositionAndRotation(input.HeadsetPosition, input.HeadsetRotation);

            leftHandTransform.transform.SetPositionAndRotation(input.LeftHandPosition, input.LeftHandRotation);

            rightHandTransform.transform.SetPositionAndRotation(input.RightHandPosition, input.RightHandRotation);

        }

        if (IsLocalNetworkRig && hardwareRig)
        {
            playerNameText.text = PlayerName;
        }
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