using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class VisualizeModule : NetworkBehaviour
{
    private Transform modulePosition; // Point where the hand will spawn relative to the object
    public Vector3 handOffset; // Offset from the spawn point for the hand

    private string storedPlayerID;

    private Color localPlayerColor;
    public void Start()
    {
        modulePosition = this.transform;
    }

    public override void OnNetworkSpawn()
    {
        storedPlayerID = PlayerPrefs.GetString("PlayerID", null);

        if (string.IsNullOrEmpty(storedPlayerID))
        {
            PlayerPrefs.SetString("PlayerID", NetworkManager.Singleton.LocalClientId.ToString());
            Debug.Log("Storing client player ID");
        }
        else
        {
            Debug.Log("Stored player ID: " + storedPlayerID);
        }

        localPlayerColor = LobbyManager.Instance.GetPlayerColor();
        Debug.Log("Player ID" + NetworkManager.Singleton.LocalClientId + "PlayerName: " + LobbyManager.Instance.GetPlayerData((int)NetworkManager.Singleton.LocalClientId).playerName);
    }


    // Call this when the module is interacted with
    public void OnGrabInteract()
    {
        if (IsClient)
        {
            // Calculate the spawn position based on the module's position and offset
            Vector3 spawnPosition = modulePosition.position + handOffset;

            // Ask the HandManager to spawn the player's hand at the given position
            HandManager.Instance.SpawnHandForPlayerServerRpc(localPlayerColor, spawnPosition, "Grab");
        }
    }

    public void OnPokeInteract()
    {
        if (IsClient)
        {
            storedPlayerID = PlayerPrefs.GetString("PlayerID", null);

            // Calculate the spawn position based on the module's position and offset
            Vector3 spawnPosition = modulePosition.position + handOffset;

            localPlayerColor = LobbyManager.Instance.GetPlayerData((int)NetworkManager.Singleton.LocalClientId).Color;

            Debug.Log("LocalPlayerColor:" + localPlayerColor + "player ID:" + storedPlayerID);

            // Ask the HandManager to spawn the player's hand at the given position
            HandManager.Instance.SpawnHandForPlayerServerRpc(localPlayerColor, spawnPosition, "Poke");
        }
    }

    public void SelectModule()
    {
        VisualizeDwellingSelector.Instance.SelectModuleDialogServerRPC(gameObject.name);
    }


}


