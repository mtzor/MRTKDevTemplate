using MixedReality.Toolkit.SpatialManipulation;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ModuleNetworkManager : NetworkBehaviour
{
    private NetworkObject networkObject;
    private bool attachedObjectManipulator = false;

    private void Awake()
    {
        networkObject = GetComponent<NetworkObject>();
    }

    public void Update()
    {

        if (IsServer && attachedObjectManipulator == false)
        {
            AttachObjectManipulator();
            attachedObjectManipulator = true;
        }
    }

    public void Added() { }
    private void AttachObjectManipulator()
    {
        var manipulator = GetComponentInChildren<ObjectManipulator>();

        if (manipulator != null)
        {
            Debug.Log("ObjectManipulator found. Setting up interaction listeners.");

            manipulator.firstSelectEntered.AddListener((eventData) =>
            {
                if (!IsOwner)
                {
                    RequestOwnershipServerRpc();
                }
            });
        }
        else
        {
            Debug.LogError("ObjectManipulator component not found!");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestOwnershipServerRpc(ServerRpcParams rpcParams = default)
    {
        Debug.Log("Requesting ownership from client: " + rpcParams.Receive.SenderClientId);
        networkObject.ChangeOwnership(rpcParams.Receive.SenderClientId);
    }


}
