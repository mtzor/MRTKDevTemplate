using System;
using Unity.Netcode;
using UnityEngine;

public class BuildingArea : NetworkBehaviour
{
    [SerializeField] private string areaID; // Unique identifier for the area
    [SerializeField] private bool isOccupied = false;

    [SerializeField] private int floorNo;

    [SerializeField] private Material highlightMaterial; // Material to highlight the valid BuildingArea


    private Material originalAreaMaterial;   // To store the original material of the BuildingArea

    public NetworkVariable<bool> isOccupiedNet = new NetworkVariable<bool>();

    public int FloorNo { get => floorNo; set => floorNo = value; }

    public bool IsOccupied { get => isOccupied; set => isOccupied = value; }
    private Renderer areaRenderer;

    public string AreaID { get => areaID; set => areaID = value; }  // Property for the unique ID

    public override void OnNetworkSpawn()
    {
        isOccupiedNet.OnValueChanged += OnOccupiedChanged;
    }

    public void OnOccupiedChanged(bool previousValue, bool newValue)
    {
        isOccupied = newValue;
        Debug.Log("Is occupied changed from :" + previousValue + " to :" + newValue);
    }

    [ServerRpc(RequireOwnership =false)]
    public void OccupyServerRpc()
    {
        isOccupiedNet.Value = true;
    }

    [ServerRpc(RequireOwnership =false)]
    public void VacateServerRpc()
    {
        isOccupiedNet.Value = false;
    }

    [ClientRpc(RequireOwnership =false)]
    public void HighlightAreaMaterialClientRpc()
    {
        originalAreaMaterial = this.GetComponent<Renderer>().material;
        this.GetComponent<Renderer>().material = highlightMaterial;
        Debug.Log("Highlighting Area: "+areaID);
    }

    [ClientRpc(RequireOwnership = false)]
    public void RevertAreaMaterialClientRpc()
    {
        this.GetComponent<Renderer>().material = originalAreaMaterial;
        Debug.Log("Reverting area: "+areaID);
    }

}
