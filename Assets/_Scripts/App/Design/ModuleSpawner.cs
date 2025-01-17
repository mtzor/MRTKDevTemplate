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
        Debug.Log("Server spawning module: " + moduleIndex + " for client: " + clientId);

        float yval =  0.03f;

        Debug.Log("Spawning position: " + yval+ "Floor No:"+DesignNetworkSyncScript.Instance.floorNo);

        if (DesignNetworkSyncScript.Instance.FirstModuleOfFloor)
        {
            Debug.Log("First Module of the Floor No:" + DesignNetworkSyncScript.Instance.floorNo);
            moduleContainer.transform.position += new Vector3(0, yval, 0);
            DesignNetworkSyncScript.Instance.FirstModuleOfFloor = false;
        }
        // Instantiate the module prefab
         moduleInstance = Instantiate(modulePrefabs[moduleIndex], moduleContainer);

        // Spawn the object across the network
        moduleInstance.GetComponent<NetworkObject>().Spawn(true);

        moduleInstance.GetComponent<Module>().FloorNo.Value = DesignNetworkSyncScript.Instance.floorNo.Value;

        spawnedModules.Add(moduleInstance.gameObject);
        // Optionally transfer ownership to the client who requested the spawn
        //Debug.Log("Owner Client ID: " +moduleInstance.GetComponent<NetworkObject>().OwnerClientId);

        if (moduleInstance.GetComponent<NetworkObject>().OwnerClientId != clientId)
        {
            moduleInstance.GetComponent<NetworkObject>().ChangeOwnership(clientId);
        }
    }



}
