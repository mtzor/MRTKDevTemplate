using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using MixedReality.Toolkit.SpatialManipulation;
using MixedReality.Toolkit.UX;

public class NetworkSpawner : NetworkBehaviour
{
    private static NetworkSpawner _instance;

    [SerializeField] private Transform buildingPrefab; // Prefab to spawn
    [SerializeField] private Transform buildingContainer; // Parent container for the spawned building
    [SerializeField] private PressableButton spawnButton; // UI Button to trigger the spawn

    private Transform spawnedBuilding;
    private NetworkObject networkObject;

    public static NetworkSpawner Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<NetworkSpawner>();
                if (_instance == null)
                {
                    GameObject networkSpawnerManagerObject = new GameObject("NetworkSpawner");
                    _instance = networkSpawnerManagerObject.AddComponent<NetworkSpawner>();
                }
            }
            return _instance;
        }
    }
    void Start()
    {
        // Ensure the button is set up to trigger the building spawn
        if (spawnButton != null)
        {
            spawnButton.OnClicked.AddListener(OnSpawnButtonClicked);
        }
        else
        {
            Debug.LogError("Spawn button is not assigned!");
        }
    }

    private void OnSpawnButtonClicked()
    {
        Debug.Log("Spawn button clicked!");

        // Check if this instance is running on the client
        if (IsClient)
        {
            Debug.Log("Client requesting to spawn building.");
            RequestSpawnBuildingServerRpc(); // Calls the ServerRpc to spawn on the server

            PropagatePhaseUpdateClientRpc();
        }
        else
        {
            Debug.LogWarning("Button click not processed on server. Only clients can request spawn.");
        }
    }

    [ClientRpc(RequireOwnership = false)]
    public void PropagatePhaseUpdateClientRpc()
    {
        AppManager.Instance.setNextPhase();
    }


    [ServerRpc]
    public void RequestSpawnBuildingServerRpc(ServerRpcParams rpcParams = default)
    {
        if (IsServer)
        {
            SpawnBuildingOnServer(rpcParams.Receive.SenderClientId);
        }
        else
        {
            Debug.LogWarning("Only the server can spawn buildings.");
        }
    }

    [ServerRpc]
    public void RequestDestroyBuildingServerRpc(ServerRpcParams rpcParams = default)
    {
        if (IsServer)
        {
            DestroyBuildingOnServer(rpcParams.Receive.SenderClientId);
        }
        else
        {
            Debug.LogWarning("Only the server can spawn buildings.");
        }
    }

    private void DestroyBuildingOnServer(ulong clientId)
    {
        networkObject.Despawn(destroy:true);

       // toggleBuildingClientRPC(false);
    }

    [ClientRpc(RequireOwnership =false)]
    public void toggleBuildingClientRPC(bool active)
    {
        spawnedBuilding.gameObject.SetActive(active);

    }
    private void SpawnBuildingOnServer(ulong clientId)
    {
        Debug.Log("Spawning building on the server...");

        // Instantiate the building prefab on the server
        spawnedBuilding = Instantiate(buildingPrefab, buildingContainer);
        networkObject = spawnedBuilding.GetComponent<NetworkObject>();

        if (networkObject != null)
        {
            networkObject.Spawn(true); // true allows visibility by all clients

            // Optionally transfer ownership to the client who requested it
            networkObject.ChangeOwnership(clientId);
            Debug.Log("Building spawned and ownership changed to client: " + clientId);

            // Attach manipulator logic
            AttachObjectManipulator();
        }
        else
        {
            Debug.LogError("NetworkObject component missing from the building prefab.");
        }
    }

    private void AttachObjectManipulator()
    {
        var manipulator = spawnedBuilding.gameObject.GetComponentInChildren<ObjectManipulator>();

        if (manipulator != null)
        {
            Debug.Log("ObjectManipulator component found in children! Setting it up.");

            // Ensure the manipulator is properly set up for both the server and clients
            manipulator.firstSelectEntered.AddListener((eventData) =>
            {
                Debug.Log("firstSelectEntered event triggered.");

                if (!IsOwner)
                {
                    RequestOwnershipServerRpc();
                }
            });
            manipulator.firstSelectEntered.AddListener((eventData) =>
            {
                Debug.Log("Second firstSelectEntered event triggered.");
                            });

            manipulator.lastSelectExited.AddListener((eventData) =>
            {
                Debug.Log("lastSelectExited event triggered.");
            });
        }
        else
        {
            Debug.LogError("ObjectManipulator component not found in children!");
        }
    }

    [ServerRpc]
    private void RequestOwnershipServerRpc(ServerRpcParams rpcParams = default)
    {
        networkObject.ChangeOwnership(rpcParams.Receive.SenderClientId);
        Debug.Log("Ownership transferred to client: " + rpcParams.Receive.SenderClientId);
    }


    [ClientRpc]
    public void DisableP2ComponentsClientRPC()
    {
        if (spawnedBuilding != null)
        {
            var syncScript = spawnedBuilding.GetComponent<DesignNetworkSyncScript>();
            if (syncScript != null)
            {
                syncScript.DisableP2Components();
                Debug.Log("P2 components disabled on the client.");
            }
            else
            {
                Debug.LogError("DesignNetworkSyncScript component missing from the spawned building.");
            }
        }
        else
        {
            Debug.LogError("Spawned building is null, cannot disable P2 components.");
        }
    }



}
