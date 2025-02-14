using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HandManager : NetworkBehaviour
{
    private static HandManager _instance;

    public GameObject handPrefab;

    public static HandManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<HandManager>();

                if (_instance == null)
                {
                    GameObject handManagerObject = new GameObject("HandManager");
                    _instance = handManagerObject.AddComponent<HandManager>();
                }
            }

            return _instance;
        }
    }

    private Dictionary<ulong, GameObject> playerHands = new Dictionary<ulong, GameObject>();

    [ServerRpc(RequireOwnership = false)]
    public void SpawnHandForPlayerServerRpc(Color playerColor, Vector3 spawnPosition, string animationTrigger, ServerRpcParams serverRpcParams = default)
    {
        ulong playerId = serverRpcParams.Receive.SenderClientId;

        if (playerHands.ContainsKey(playerId) && playerHands[playerId] != null)
        {
            GameObject oldHand = playerHands[playerId];
            NetworkObject oldHandNetObj = oldHand.GetComponent<NetworkObject>();
            if (oldHandNetObj != null && oldHandNetObj.IsSpawned)
            {
                oldHandNetObj.Despawn();
            }
            Destroy(oldHand);
        }

        GameObject newHandInstance = Instantiate(handPrefab, spawnPosition, Quaternion.identity);
        NetworkObject handNetObj = newHandInstance.GetComponent<NetworkObject>();
        handNetObj.SpawnWithOwnership(playerId);

        HandController handController = newHandInstance.GetComponent<HandController>();
        handController.SetTintColorServerRpc(playerColor);
        handController.TriggerHandAnimation(animationTrigger);

        playerHands[playerId] = newHandInstance;

        StartCoroutine(DestroyHandAfterSeconds(newHandInstance, 3f));
    }

    private IEnumerator DestroyHandAfterSeconds(GameObject hand, float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (hand != null)
        {
            NetworkObject handNetObj = hand.GetComponent<NetworkObject>();
            if (handNetObj != null && handNetObj.IsSpawned)
            {
                handNetObj.Despawn();
            }
            Destroy(hand);

            ulong ownerId = handNetObj.OwnerClientId;
            if (playerHands.ContainsKey(ownerId) && playerHands[ownerId] == hand)
            {
                playerHands.Remove(ownerId);
            }
        }
    }

    public void RotateHandsWithPlayer(ulong playerId, Quaternion playerRotation)
    {
        if (playerHands.ContainsKey(playerId) && playerHands[playerId] != null)
        {
            // Ensure the hand's rotation aligns with the player's rotation
            playerHands[playerId].transform.rotation = playerRotation;

            // Adjust for any prefab-specific offset (e.g., hands facing backward)
            playerHands[playerId].transform.Rotate(0, 180f, 0); // Adjust based on your prefab's orientation
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void DespawnAndDestroyAllHandsServerRpc()
    {
        foreach (var handEntry in playerHands.Values)
        {
            if (handEntry != null)
            {
                NetworkObject handNetObj = handEntry.GetComponent<NetworkObject>();
                if (handNetObj != null && handNetObj.IsSpawned)
                {
                    handNetObj.Despawn();
                }
                Destroy(handEntry);
            }
        }

        playerHands.Clear();
    }
}
