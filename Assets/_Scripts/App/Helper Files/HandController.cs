using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class HandController : NetworkBehaviour
{
    public Renderer handRenderer;  // Reference to Renderer for hand color
    private NetworkAnimator networkAnimator; // Reference to the NetworkAnimator

    private void Awake()
    {
        networkAnimator = GetComponent<NetworkAnimator>();
        if (handRenderer == null)
            handRenderer = GetComponentInChildren<Renderer>(); // Get the renderer
    }

    // Function to set the tint color over the network
    [ServerRpc(RequireOwnership = false)]
    public void SetTintColorServerRpc(Color color)
    {
        SetTintColorClientRpc(color);
    }

    [ClientRpc]
    public void SetTintColorClientRpc(Color color)
    {
        if (handRenderer != null && handRenderer.material != null)
        {
            handRenderer.material.color = color; // Apply the tint color
        }
    }

    // Function to trigger animations using the NetworkAnimator
    public void TriggerHandAnimation(string triggerName)
    {
        if (networkAnimator != null && IsOwner)
        {
            networkAnimator.SetTrigger(triggerName);
        }
        else if (networkAnimator == null)
        {
            Debug.LogError("NetworkAnimator is not set on this object.");
        }
    }
}
