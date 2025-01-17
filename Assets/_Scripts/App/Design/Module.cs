using MixedReality.Toolkit.SpatialManipulation;
using Unity.Netcode;
using UnityEngine.Events;
using UnityEngine;

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
    private int frameCounter = 0; // Frame counter to control when the check happens
    private int checkInterval = 10; // Check every 10 frames

    private bool isBeingManipulated = false;
    private void Start()
    {
        FloorNo.OnValueChanged += OnFloorNoChanged;
        modCounter = 0;
        moduleRenderer = GetComponent<Renderer>();
        animator = GetComponent<Animator>();

        animator.enabled = false;
        initialPosition = transform.position;  // Store the initial position

        lastPosition = transform.position;
        lastRotation = transform.rotation;


    }

    public void AddedTest() { }

    private bool HasTransformChanged()
    {
        // Check if the position or rotation has changed beyond the set thresholds
        float positionDelta = Vector3.Distance(lastPosition, transform.position);
        float rotationDelta = Quaternion.Angle(lastRotation, transform.rotation);

        return positionDelta > positionThreshold || rotationDelta > rotationThreshold;
    }

    private void OnFloorNoChanged(int oldValue, int newValue)
    {
        // Update the module state or any UI elements based on the new FloorNo
        Debug.Log($"Floor No updated from {oldValue} to {newValue}");
        // Add additional logic here if needed
    }
    private void OnTriggerEnter(Collider other)
    {
        if (isPlaced) return;

        BuildingArea area = other.GetComponent<BuildingArea>();

        //checks if area is a smaller area inside the occupied area occupy this as well
        if (area != null && !area.IsOccupied && area.AreaID != this.moduleData.Name && IsRotationValid(area) && area.FloorNo == this.FloorNo.Value)
        {
            //Debug.Log("Entered fitting area Encapsulated: " + area.AreaID);
            area.OccupyServerRpc();

        }

        if (area != null && !area.IsOccupied && area.AreaID == this.moduleData.Name && IsRotationValid(area) && area.FloorNo == this.FloorNo.Value)
        {
            // DesignManager.Instance.AddArea(moduleData.Area);
            SetLastValidArea(area);
            ChangeColor(true);
            //Debug.Log("Entered fitting area: " + area.AreaID);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (isPlaced) return;

        BuildingArea area = other.GetComponent<BuildingArea>();

        // Debug.Log("Trigger stay: " + area.AreaID+" Area occupied"+area.IsOccupied);
        if (area != null && !area.IsOccupied && area.AreaID == this.moduleData.Name && IsRotationValid(area) && area.FloorNo == this.FloorNo.Value)
        {
            if (area != lastValidArea)
            {
                SetLastValidArea(area);
            }
            ChangeColor(true);
            isInsideValidArea = true;
        }
        else if (area == lastValidArea)
        {
            // If the area was the last valid but is no longer valid, revert it
            RevertLastValidArea();
            isInsideValidArea = false;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        BuildingArea area = other.GetComponent<BuildingArea>();

        if (area != null && area.IsOccupied && area.AreaID != this.moduleData.Name && IsRotationValid(area) && area.FloorNo == this.FloorNo.Value)
        {
            Debug.Log("Vacated fitting area encaapsulated: " + area.AreaID);
            area.VacateServerRpc();

        }

        if (isPlaced) return;

        if (area != null && area.AreaID == this.moduleData.Name && IsRotationValid(area) && area.FloorNo == this.FloorNo.Value)
        {
            Debug.Log("Vacated fitting area encaapsulated: " + area.AreaID);
            area.VacateServerRpc();
            //DesignManager.Instance.SubtractArea(moduleData.Area);

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

        // When manipulation ends, only snap to the last valid area if still inside it and rotation is valid
        if (isInsideValidArea && lastValidArea != null)
        {
            if (IsRotationValid(lastValidArea))
            {
                SnapToArea(lastValidArea);
            }
            else
            {
                // If rotation is not valid, reset color and do not snap
                ChangeColor(false);
                // Debug.Log("Rotation not valid, module not snapped.");
            }
        }
        else
        {
            // If no valid area or exited, do not snap and revert to nofitMaterial
            ChangeColor(false);
        }

        isBeingManipulated = false;
    }

    private void SetLastValidArea(BuildingArea area)
    {
        // Revert the material of the previous valid area if there was one
        if (lastValidArea != null && lastValidArea != area)
        {
            RevertLastValidArea();
        }

        // Set the new last valid area and highlight it
        lastValidArea = area;
        area.HighlightAreaMaterialClientRpc();
        isInsideValidArea = true;
    }

    private void RevertLastValidArea()
    {
        if (lastValidArea != null)
        {
            lastValidArea.RevertAreaMaterialClientRpc();
            lastValidArea.VacateServerRpc();
            lastValidArea = null;
            isInsideValidArea = false;
        }
    }

    private int modCounter;



    private void SnapToArea(BuildingArea area)
    {
        // Snap the module to the exact position and rotation of the BuildingArea
        transform.position = area.transform.position;
        transform.rotation = area.transform.rotation;

        // Mark the module as placed and the area as occupied
        isPlaced = true;
        area.OccupyServerRpc();

        // Revert the area to its original material after snapping
        area.RevertAreaMaterialClientRpc();

        // Make the module stationary
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<ObjectManipulator>().enabled = false;

        // Add the area (of the same floor) when the module is fully snapped in place
        Debug.Log("SNAPPPING");
        DesignNetworkSyncScript.Instance.AddAreaServerRPC(0, moduleData.Area[0]);

        ModuleData data = new ModuleData(this.gameObject);
        DesignNetworkSyncScript.Instance.SaveModuleServerRPC(data);

        if (modCounter > 0)
        {
            //  DesignNetworkSyncScript.Instance.LoadAllModulesServerRPC();
        }

        if (moduleData.Area.Length > 1)//when the module has a second floor
        {   // Add the area of the next floor when the module is fully snapped in place
            DesignNetworkSyncScript.Instance.AddAreaServerRPC(1, moduleData.Area[0]);
        }

        modCounter++;

        RevertLastValidArea();

        // This ensures that the current area is also reverted even if it wasn't the lastValidArea
        lastValidArea = null; // Reset the reference
        isInsideValidArea = false; // Clear the flag to avoid unexpected behaviors later
    }



    private void ChangeColor(bool fits)
    {
        // Change the material of the module based on whether it fits or not
        if (moduleRenderer != null)
        {
            moduleRenderer.material = fits ? fitMaterial : nofitMaterial;
        }
    }

    private bool IsRotationValid(BuildingArea area)
    {
        // Check if the Y rotation of the module matches the area's Y rotation within 90-degree increments
        float moduleYRotation = Mathf.Round(transform.eulerAngles.y / 90) * 90;
        float areaYRotation = Mathf.Round(area.transform.eulerAngles.y / 90) * 90;

        if (!isRectangular)
        {
            //Debug.Log("NOT RECTANGULAR Module Y Rotation: " + moduleYRotation + " Area Y Rotation: " + areaYRotation + "The result is " + Mathf.Abs(moduleYRotation - areaYRotation));
            return Mathf.Abs(moduleYRotation - areaYRotation) < 1f; // Allow small tolerance due to floating point errors
        }
        else
        {
            //Debug.Log("Module Y Rotation: " + moduleYRotation + " Area Y Rotation: " + areaYRotation+"The result is "+ Mathf.Abs(moduleYRotation - areaYRotation));
            return (Mathf.Abs(moduleYRotation - areaYRotation) < 1f || (Mathf.Abs(moduleYRotation - areaYRotation) > 90f && Mathf.Abs(moduleYRotation - areaYRotation) < 181f));
        } // Allow small tolerance due to floating point errors
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
