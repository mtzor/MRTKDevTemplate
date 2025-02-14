using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Netcode.Components;

public class PlayerController : NetworkBehaviour
{
    public NetworkTransform networkTransform;

    public Color defaultColor;
    public Color glowColor = Color.yellow;
    public float glowIntensity = 2.0f;

    public List<Renderer> bodyPartRenderers;
    public GameObject body;

    public Renderer faceRenderer;

    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private int frameCounter = 0;
    private int checkInterval = 60;
    private bool isWalking = false;

    // Network variable to sync color across clients
    private NetworkVariable<Color> playerColor = new NetworkVariable<Color>(Color.white);
    // Network variable to sync iridescent state across clients
    private NetworkVariable<bool> isIridescent = new NetworkVariable<bool>(false);
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Register the player object
            ulong clientId = NetworkManager.Singleton.LocalClientId;
            CustomizeManager.Instance.PlayerObjects[clientId] = gameObject;

            Debug.Log("Local client ID" + NetworkManager.Singleton.LocalClientId + " COLOR" + LobbyManager.Instance.GetPlayerData().Color);
            defaultColor = LobbyManager.Instance.GetPlayerData().Color;
            RequestChangeColorServerRpc(defaultColor); // Initialize the NetworkVariable

            UpdateMeshRenderer(false);
            
        }
    }

    public void Start()
    {
        if (IsOwner)
        {
            Debug.Log("Local client ID" + NetworkManager.Singleton.LocalClientId + " COLOR" + LobbyManager.Instance.GetPlayerData().Color);
            defaultColor = LobbyManager.Instance.GetPlayerData().Color;
            RequestChangeColorServerRpc(defaultColor); // Initialize the NetworkVariable
        }

        lastPosition = transform.position;
        lastRotation = transform.rotation;

        // Register callbacks for when color or iridescent state changes
        playerColor.OnValueChanged += OnPlayerColorChanged;
        isIridescent.OnValueChanged += OnIridescentChanged;
    }

    private void LateUpdate()
    {
        frameCounter++;
        if (frameCounter % checkInterval == 0)
        {
            if (HasTransformChanged() && !isWalking)
            {
                StartWalking();
                lastPosition = transform.position;
                lastRotation = transform.rotation;
            }
            else if (HasTransformChanged() && isWalking)
            {
                lastPosition = transform.position;
                lastRotation = transform.rotation;
            }
            else
            {
                StopWalking();
            }
        }
    }

    private bool HasTransformChanged()
    {
        float positionDelta = Vector3.Distance(lastPosition, transform.position);
        float rotationDelta = Quaternion.Angle(lastRotation, transform.rotation);
        return positionDelta > 0.000000001f || rotationDelta > 0.001f;
    }

    void Update()
    {
        if (IsOwner)
        {
            RotateBodyAndHead();
            HandManager.Instance.RotateHandsWithPlayer(NetworkManager.Singleton.LocalClientId, transform.rotation);
        }
    }

    private void RotateBodyAndHead()
    {
        if (Camera.main == null) return;

        float bodyYaw = Camera.main.transform.eulerAngles.y + 180f;
        Quaternion adjustedRotation = Quaternion.Euler(0f, bodyYaw, 0f);

        networkTransform.transform.rotation = adjustedRotation;
        networkTransform.transform.position = Camera.main.transform.position - new Vector3(0.67f, -0.6f, +0.1f);
    }

    public void StartWalking()
    {
        isWalking = true;

        if (IsOwner)
        {
            // Enable iridescent effect and apply glow color
            RequestIridescentChangeServerRpc(true);
            RequestChangeColorServerRpc(glowColor);
        }
    }

    public void StopWalking()
    {
        isWalking = false;

        if (IsOwner)
        {
            // Disable iridescent effect and reset to default color
            RequestIridescentChangeServerRpc(false);
            RequestChangeColorServerRpc(defaultColor);
        }
    }

    // Server RPC to request a color change
    [ServerRpc(RequireOwnership =false)]
    private void RequestChangeColorServerRpc(Color newColor, ServerRpcParams rpcParams = default)
    {
        playerColor.Value = newColor; // Update color network variable
    }

    // Server RPC to request an iridescent effect change
    [ServerRpc(RequireOwnership = false)]
    private void RequestIridescentChangeServerRpc(bool enableIridescent, ServerRpcParams rpcParams = default)
    {
        isIridescent.Value = enableIridescent;
    }

    // Callback to update material properties when the color changes
    private void OnPlayerColorChanged(Color oldColor, Color newColor)
    {
        UpdateMaterialProperties(newColor, isIridescent.Value);
    }

    // Callback to update material properties when the iridescent state changes
    private void OnIridescentChanged(bool oldValue, bool newValue)
    {
        UpdateMaterialProperties(playerColor.Value, newValue);
    }

    // Method to update material properties based on color and iridescent state
    private void UpdateMaterialProperties(Color color, bool useIridescent)
    {
        foreach (var renderer in bodyPartRenderers)
        {
            foreach (var material in renderer.materials)
            {
                material.color = color;  // Set the base color
                material.SetColor("_EmissionColor", color * glowIntensity);  // Set the emission color
                material.SetFloat("_Mode", useIridescent ? 1.0f : 0.0f);  // Toggle iridescent mode
                DynamicGI.SetEmissive(renderer, color * glowIntensity); // Update global illumination
            }
        }
    }

    private void UpdateMeshRenderer(bool isOn)
    {
        foreach (var renderer in bodyPartRenderers)
        {
            renderer.GetComponent<MeshRenderer>().enabled = isOn;
            faceRenderer.GetComponent<MeshRenderer>().enabled=isOn;
        }
    }

    private void OnDestroy()
    {
        playerColor.OnValueChanged -= OnPlayerColorChanged;
        isIridescent.OnValueChanged -= OnIridescentChanged;
    }

    public void ToggleBody(bool value)
    {
        body.SetActive(value);
    }

    public Quaternion GetPlayerAvatarRotation()
    {
        return this.transform.rotation;
    }
}
