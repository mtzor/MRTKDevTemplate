using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ModuleSpawner : NetworkBehaviour
{
    private static ModuleSpawner _instance;
    [SerializeField] private Transform[] modulePrefabs; // Prefab registered in NetworkManager
    [SerializeField] private Transform moduleContainer; // Parent container for spawned objects

    public List<GameObject> spawnedModules = new List<GameObject>();
    private bool firstModuleofFloor=false;

    public static ModuleSpawner Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ModuleSpawner>();
                if (_instance == null)
                {
                    GameObject moduleSpawnerObject = new GameObject("ModuleSpawner");
                    _instance = moduleSpawnerObject.AddComponent<ModuleSpawner>();
                }
            }
            return _instance;
        }
    }
    // Method called by clients to request the server to spawn a module
    public void OnSpawnButtonPressed(int moduleIndex)
    {
        if (IsClient)
        {
            // Call the ServerRpc to ask the server to spawn the module
            RequestSpawnModuleServerRpc(moduleIndex, NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc(RequireOwnership =false)]
    public void DestroySpawnedModulesServerRPC()
    {
        Debug.Log($"Client {NetworkManager.Singleton.LocalClientId} requesting module destruction");

        foreach (var module in spawnedModules)
        {
            if (module != null)
            {
                var networkObject = module.GetComponent<NetworkObject>();

                if (IsServer)
                {
                    // If on the server, destroy the module directly
                    networkObject.Despawn(destroy: true);
                    Debug.Log("Server destroying module: " + module.name);
                }
              
            }
        }
    }

    private Transform moduleInstance;

    // ServerRpc method to spawn the module on the server
    [ServerRpc(RequireOwnership = false)]
    public void RequestSpawnModuleServerRpc(int moduleIndex, ulong clientId)
    {
        Debug.Log($"Server spawning module: {moduleIndex} for client: {clientId}");

        float yval = 0.03f;

        if (DesignNetworkSyncScript.Instance.FirstModuleOfFloor)
        {
            Debug.Log($"First Module of Floor No: {DesignNetworkSyncScript.Instance.floorNo}");
            moduleContainer.transform.position += new Vector3(0, yval, 0);
            DesignNetworkSyncScript.Instance.FirstModuleOfFloor = false;
        }

        // Instantiate the module prefab
        var moduleInstance = Instantiate(modulePrefabs[moduleIndex], moduleContainer);

        // Spawn the object across the network
        moduleInstance.GetComponent<NetworkObject>().Spawn(true);

        // Set the FloorNo value on the module
        moduleInstance.GetComponent<Module>().FloorNo.Value = DesignNetworkSyncScript.Instance.floorNo.Value;

        // Add the module to the list of spawned modules
        spawnedModules.Add(moduleInstance.gameObject);

        // Transfer ownership to the client
        var networkObject = moduleInstance.GetComponent<NetworkObject>();
        if (networkObject.OwnerClientId != clientId)
        {
            networkObject.ChangeOwnership(clientId);
            Debug.Log($"Ownership of module {moduleIndex} transferred to Client ID: {clientId}");
        }
        else
        {
            Debug.Log($"Client ID: {clientId} already owns the module.");
        }

        Debug.Log($"Owner of the module after spawn: {networkObject.OwnerClientId}");


        // Set the isKinematic property on the client
        SetIsKinematicClientRPC(networkObject.NetworkObjectId, true);
    }


    [ClientRpc(RequireOwnership = false)]
    public void SetIsKinematicClientRPC(ulong networkObjectId, bool isKinematic)
    {
        // Find the network object by ID
        var networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];

        if (networkObject != null)
        {
            var rb = networkObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = isKinematic;
                Debug.Log($"Set Rigidbody isKinematic to {isKinematic} on Client for object {networkObjectId}");
            }
        }
        else
        {
            Debug.LogWarning($"NetworkObject with ID {networkObjectId} not found on client.");
        }
    }

}
