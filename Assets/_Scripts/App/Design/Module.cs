using MixedReality.Toolkit.SpatialManipulation;
using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class Module : NetworkBehaviour
{
    public ModuleScriptable moduleData; // Information about the module, like its name or ID.

    public NetworkVariable<int> FloorNo = new NetworkVariable<int>(0);

    public Animator animator;
    public string clipName;

    private bool isPlaced = false;
    private Renderer moduleRenderer;

    [SerializeField] private Material fitMaterial;       // Material for the module when it fits
    [SerializeField] private Material nofitMaterial;     // Material for the module when it doesn't fit

    private BuildingArea lastValidArea;      // To store the last valid area entered
    private Vector3 initialPosition;         // To store the initial position in case no valid area is found
    private bool isInsideValidArea = false;  // Flag to determine if the module is in a valid area

    [SerializeField] private bool isRectangular = false; // Flag to determine if the module is rectangular

    public float positionThreshold = 0.001f;
    public float rotationThreshold = 0.1f; // Degrees

    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private Coroutine positionUpdateCoroutine;
    private int modCounter = 0;

    private void Start()
    {
        FloorNo.OnValueChanged += OnFloorNoChanged;
        moduleRenderer = GetComponent<Renderer>();
        animator = GetComponent<Animator>();
        animator.enabled = false;
        initialPosition = transform.position;
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    private bool HasTransformChanged()
    {
        float positionDelta = Vector3.Distance(lastPosition, transform.position);
        float rotationDelta = Quaternion.Angle(lastRotation, transform.rotation);
        return positionDelta > positionThreshold || rotationDelta > rotationThreshold;
    }

    private void OnFloorNoChanged(int oldValue, int newValue)
    {
        Debug.Log($"Floor No updated from {oldValue} to {newValue}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isPlaced) return;

        BuildingArea area = other.GetComponent<BuildingArea>();

        // Checks if area is a smaller area inside the occupied area; occupy this as well
        if (area != null && !area.IsOccupied && area.AreaID != this.moduleData.Name && IsRotationValid(area) && area.FloorNo == this.FloorNo.Value)
        {
            area.OccupyServerRpc();
        }

        if (area != null && !area.IsOccupied && area.AreaID == this.moduleData.Name && IsRotationValid(area) && area.FloorNo == this.FloorNo.Value)
        {
            SetLastValidArea(area);
            ChangeColor(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        BuildingArea area = other.GetComponent<BuildingArea>();

        if (area != null && area.IsOccupied && area.AreaID != this.moduleData.Name && IsRotationValid(area) && area.FloorNo == this.FloorNo.Value)
        {
            Debug.Log("Vacated fitting area encapsulated: " + area.AreaID);
            area.VacateServerRpc();
        }

        if (isPlaced) return;

        if (area != null && area.AreaID == this.moduleData.Name && IsRotationValid(area) && area.FloorNo == this.FloorNo.Value)
        {
            Debug.Log("Vacated fitting area encapsulated: " + area.AreaID);
            area.VacateServerRpc();
        }

        if (area != null && area == lastValidArea)
        {
            // If exiting the last valid area, revert its material and flag
            RevertLastValidArea();
            ChangeColor(false); // Ensure the module's material changes to nofitMaterial
            Debug.Log("Exited fitting area: " + area.AreaID);
            isInsideValidArea = false;
        }
    }

    public void OnManipulationEnded()
    {
        StopSendingPositionUpdates();

        if (isInsideValidArea && lastValidArea != null)
        {
            SnapToArea(lastValidArea);
        }
        else
        {
            ChangeColor(false);
        }

    }

    private void SetLastValidArea(BuildingArea area)
    {
        if (lastValidArea != null && lastValidArea != area)
        {
            RevertLastValidArea();
        }

        lastValidArea = area;
        area.HighlightAreaMaterialClientRpc();
        isInsideValidArea = true;
    }

    private void RevertLastValidArea()
    {
        if (lastValidArea != null)
        {
            Debug.Log($"Reverting area: {lastValidArea.AreaID}");
            lastValidArea.RevertAreaMaterialClientRpc();
            lastValidArea.VacateServerRpc();
            lastValidArea = null;
            isInsideValidArea = false;
        }
    }
    private void SnapToArea(BuildingArea area)
    {
        transform.position = area.transform.position;
        transform.rotation = area.transform.rotation;

        UpdateSnapOnServerRpc(area.transform.position, area.transform.rotation);

        isPlaced = true;
        area.OccupyServerRpc();
        area.RevertAreaMaterialClientRpc();

        DesignNetworkSyncScript.Instance.AddAreaServerRPC(0, moduleData.Area[0]);

        if (moduleData.Area.Length > 1)
        {
            DesignNetworkSyncScript.Instance.AddAreaServerRPC(FloorNo.Value, moduleData.Area[1]);
        }

        var rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
        GetComponent<ObjectManipulator>().enabled = false;

        Debug.Log("Module snapped successfully.");

        ModuleData data = new ModuleData(this.gameObject);
        DesignNetworkSyncScript.Instance.SaveModuleServerRPC(data);

        modCounter++;

        RevertLastValidArea();

        // This ensures that the current area is also reverted even if it wasn't the lastValidArea
        lastValidArea = null; // Reset the reference
        isInsideValidArea = false; // Clear the flag to avoid unexpected behaviors later
    }

    private void ChangeColor(bool fits)
    {
        if (moduleRenderer != null)
        {
            moduleRenderer.material = fits ? fitMaterial : nofitMaterial;
        }
    }

    private bool IsRotationValid(BuildingArea area)
    {
        float moduleYRotation = Mathf.Round(transform.eulerAngles.y / 90) * 90;
        float areaYRotation = Mathf.Round(area.transform.eulerAngles.y / 90) * 90;

        if (!isRectangular)
        {
            return Mathf.Abs(moduleYRotation - areaYRotation) < 1f;
        }

        return Mathf.Abs(moduleYRotation - areaYRotation) < 1f ||
               (Mathf.Abs(moduleYRotation - areaYRotation) > 90f && Mathf.Abs(moduleYRotation - areaYRotation) < 181f);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateSnapOnServerRpc(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;

        Debug.Log($"[Server] Snapped to position: {position}, rotation: {rotation}");

        UpdateClientsAfterSnapClientRpc(position, rotation);
    }

    [ClientRpc]
    private void UpdateClientsAfterSnapClientRpc(Vector3 position, Quaternion rotation)
    {
        if (!IsOwner)
        {
            transform.position = position;
            transform.rotation = rotation;
        }
    }

    public void StartSendingPositionUpdates()
    {
        if (IsOwner && positionUpdateCoroutine == null)
        {
            positionUpdateCoroutine = StartCoroutine(SendPositionUpdatesCoroutine());
        }
    }

    public void StopSendingPositionUpdates()
    {
        if (positionUpdateCoroutine != null)
        {
            StopCoroutine(positionUpdateCoroutine);
            positionUpdateCoroutine = null;
        }
    }

    private IEnumerator SendPositionUpdatesCoroutine()
    {
        while (true)
        {
            SendPositionToServer(transform.position);
            yield return new WaitForSeconds(0.05f);
        }
    }

    private void SendPositionToServer(Vector3 position)
    {
        UpdatePositionServerRpc(position);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePositionServerRpc(Vector3 position)
    {
        transform.position = position;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyModuleServerRPC()
    {
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }
}
