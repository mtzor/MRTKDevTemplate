using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
//using Unity.VisualScripting;
using UnityEngine;

public class HandManager : NetworkBehaviour
{
    private static HandManager _instance;

    public GameObject handPrefab; // Reference to the hand prefab
    public static HandManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Find the HandManager in the scene
                _instance = FindObjectOfType<HandManager>();

                if (_instance == null)
                {
                    // Create a new GameObject and attach HandManager if not found
                    GameObject handManagerObject = new GameObject("HandManager");
                    _instance = handManagerObject.AddComponent<HandManager>();
                }
            }

            return _instance;
        }
    }

    // This class stores a player's hands (current and previous)
    private class PlayerHandData
    {
        public GameObject currentHand;
        public GameObject previousHand;
    }

    // Dictionary to keep track of player hands by their network ID
    private Dictionary<ulong, PlayerHandData> playerHands = new Dictionary<ulong, PlayerHandData>();

    // Spawns a new hand for a player and manages their hand data
    [ServerRpc(RequireOwnership = false)]
    public void SpawnHandForPlayerServerRpc(Color playerColor, Vector3 spawnPosition, string animationTrigger, ServerRpcParams serverRpcParams = default)
    {
        // Use the requesting client's ID
        ulong playerId = serverRpcParams.Receive.SenderClientId; // The client that sent the ServerRpc

        // Ensure player has a tracking data structure
        if (!playerHands.ContainsKey(playerId))
        {
            playerHands[playerId] = new PlayerHandData();
        }

        // Get the player's current hand data
        PlayerHandData handData = playerHands[playerId];

        // If the player already has a current hand, move it to the previous hand
        if (handData.currentHand != null)
        {
            if (handData.previousHand != null)
            {
                // Despawn the oldest hand (previous hand)
                NetworkObject prevHandNetObj = handData.previousHand.GetComponent<NetworkObject>();
                prevHandNetObj.Despawn();
                Destroy(handData.previousHand);
            }

            // The current hand becomes the previous hand
            handData.previousHand = handData.currentHand;
        }

        // Instantiate and spawn the new current hand
        GameObject newHandInstance = Instantiate(handPrefab, spawnPosition, Quaternion.identity);
        NetworkObject handNetObj = newHandInstance.GetComponent<NetworkObject>();

        // Spawn with ownership assigned to the requesting client
        handNetObj.SpawnWithOwnership(playerId); // Spawn the hand for the specific client

        // Set the tint color and trigger the animation on the new hand
        HandController handController = newHandInstance.GetComponent<HandController>();
        handController.SetTintColorServerRpc(playerColor);
        handController.TriggerHandAnimation(animationTrigger);

        // Set the newly spawned hand as the current hand
        handData.currentHand = newHandInstance;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DespawnAndDestroyAllHandsServerRpc()
    {
        // Loop through each player's hand data
        foreach (var playerHandData in playerHands.Values)
        {
            // Despawn and destroy the current hand if it exists
            if (playerHandData.currentHand != null)
            {
                NetworkObject currentHandNetObj = playerHandData.currentHand.GetComponent<NetworkObject>();
                if (currentHandNetObj != null && currentHandNetObj.IsSpawned)
                {
                    currentHandNetObj.Despawn();
                }
                Destroy(playerHandData.currentHand);
                playerHandData.currentHand = null;
            }

            // Despawn and destroy the previous hand if it exists
            if (playerHandData.previousHand != null)
            {
                NetworkObject previousHandNetObj = playerHandData.previousHand.GetComponent<NetworkObject>();
                if (previousHandNetObj != null && previousHandNetObj.IsSpawned)
                {
                    previousHandNetObj.Despawn();
                }
                Destroy(playerHandData.previousHand);
                playerHandData.previousHand = null;
            }
        }

        // Clear the playerHands dictionary after all hands are removed
        playerHands.Clear();
    }
}
