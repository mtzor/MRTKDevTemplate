using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
//using Unity.VisualScripting;
using UnityEngine;

public class DesignNetworkSyncScript : NetworkBehaviour // Implement IDisposable
{
    private static DesignNetworkSyncScript _instance;

    [SerializeField] Transform buildingTransform;
    [SerializeField] private GameObject NextPhaseButton;
    [SerializeField] private TMP_Text P12_text;
    [SerializeField] private GameObject manipulationBar;
    [SerializeField] private GameObject[] floors = new GameObject[4];
    [SerializeField] private GameObject buildingHollow;
    [SerializeField] private PressableButton NextFloorBtn;

    [SerializeField] private PressableButton CloseLobbyBtn;

    // Initialize placedModules to null initially
    [SerializeField] public NetworkVariable<int> floorNo = new NetworkVariable<int>();
    [SerializeField] public NetworkVariable<bool> floorBtnActiveNet = new NetworkVariable<bool>();
    [SerializeField] public NetworkVariable<bool> CloseLobbyBtnActiveNet = new NetworkVariable<bool>();

    [SerializeField] private GameObject Design_P2_interface;

    private int currentFloor = 0;
    public List<ModuleData> placedModules = new List<ModuleData>();
    private bool firstModuleOfFloor = false;
    private bool floorBtnActive = false;
    private bool closeLobbyBtnActive = false;


    public delegate void AreaUpdated(int area);
    public static event AreaUpdated OnAreaUpdatedEvent;

    public static int TOTAL_AREA = 240;
    public static int MAX_FLOORS = 4;
    [SerializeField] private int[] coveredArea;
    public bool FirstModuleOfFloor
    {
        set => firstModuleOfFloor = value;
        get => firstModuleOfFloor;
    }

    public static DesignNetworkSyncScript Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DesignNetworkSyncScript>();
                if (_instance == null)
                {
                    GameObject designNetworkSyncScriptManagerObject = new GameObject("DesignNetworkSyncScript");
                    _instance = designNetworkSyncScriptManagerObject.AddComponent<DesignNetworkSyncScript>();
                }
            }
            return _instance;
        }
    }

    public override void OnNetworkSpawn()
    {
       
        //comment if you want file to persist between sessions
        //SaveSystem.DeleteSaveFile();


        // Listen to app phase change
        if (AppManager.Instance != null)
        {
            coveredArea = new int[MAX_FLOORS];
            // AppManager.Instance.OnAppPhaseChanged += UpdateAppPhaseEvent;
            for (int i = 0; i < MAX_FLOORS; i++)
            {
                coveredArea[i] = 0;
            }
        }

        Debug.Log("OnNetworkSpawn called");
        // Initialize placedModules only once when the script is spawned
        /*if (placedModules == null)
        {
            placedModules = new NetworkList<ModuleData>();
        }
        */
        if (IsServer)
        {
            floorNo.Value = 0;
        }
        floorNo.OnValueChanged += OnFloorUpdate;
    }

    // Properly handling the list changes


    public void ToggleNextPhaseBtn(bool toggle)
    {
        NextPhaseButton.gameObject.SetActive(toggle);
    }
    public void ToggleP12Text(bool toggle)
    {
        P12_text.gameObject.SetActive(toggle);
    }

    private int currentCoveredArea = 0;

    [ServerRpc(RequireOwnership =false)]
    public void AddAreaServerRPC(int i, int area)
    {
        Debug.Log(floorNo.Value + i + " fLOOR");
        coveredArea[floorNo.Value + i] += area;
        TriggerAreaUpdatedEvent();
        Debug.Log("COVERED AREA ADDITION :" + coveredArea[floorNo.Value + i]);
    }

   
    private void TriggerAreaUpdatedEvent()
    {
        OnAreaUpdatedEvent?.Invoke(coveredArea[floorNo.Value]);

        // Additional logic can be added here if needed
        if (coveredArea[floorNo.Value] >= TOTAL_AREA)
        {   if(floorNo.Value < MAX_FLOORS-1)//0,1,2
            {
                Debug.Log("Building is full");
                ActivateNextFloorBtnServerRpc();
            }
            else if (floorNo.Value == MAX_FLOORS-1)
            {
                ActivateCloseLobbyBtnServerRpc();
            }
        }
        else
        {

            DeactivateNextFloorBtnServerRpc();
        }
    }

    public void OnFloorUpdate(int previousValue, int newValue)
    {
        currentFloor = newValue;
        Debug.Log("Current Floor No changed from :" + previousValue + " to :" + newValue);

        // Deactivate previous floor GameObject
        if (previousValue >= 0) 
        {
            Debug.Log("Deactivating Floor :"+ previousValue);
           floors[previousValue].SetActive(false);
        }
           

        // Activate current floor GameObject
        if (currentFloor < floors.Length)
            floors[currentFloor].SetActive(true);

        DesignManager.Instance.FloorCount = newValue;
    }

  
    #region Next Floor Button Logic

    [ServerRpc(RequireOwnership = false)]
    public void DeactivateNextFloorBtnServerRpc()
    {
        floorBtnActive = false; // Deactivate next floor button
        floorBtnActiveNet.Value = floorBtnActive; // Updating network variable
        NextFloorBtn.gameObject.SetActive(floorBtnActive);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ActivateNextFloorBtnServerRpc()
    {
        floorBtnActive = true; // Activate next floor button
        floorBtnActiveNet.Value = floorBtnActive; // Updating network variable
        NextFloorBtn.gameObject.SetActive(floorBtnActive);
    }

    public void OnNextFloorBtnPressed()
    {
        // Deactivate next floor button and add the next floor
        DeactivateNextFloorBtnServerRpc();
        AddFloorServerRpc();

        Debug.Log("Next Floor Button Pressed");
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddFloorServerRpc()
    {
        firstModuleOfFloor = true;
        currentFloor++; // Increasing floor number
        floorNo.Value = currentFloor; // Updating floor number network variable
    }

    #endregion

    #region Close Lobby Button Logic
    [ServerRpc(RequireOwnership = false)]
    public void DeactivateCloseLobbyBtnServerRpc()
    {
        closeLobbyBtnActive = false; // Deactivate close lobby button
        CloseLobbyBtnActiveNet.Value = closeLobbyBtnActive; // Updating network variable
        CloseLobbyBtn.gameObject.SetActive(closeLobbyBtnActive);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ActivateCloseLobbyBtnServerRpc()
    {
        closeLobbyBtnActive = true; // Deactivate close lobby button
        CloseLobbyBtnActiveNet.Value = closeLobbyBtnActive; // Updating network variable
        CloseLobbyBtn.gameObject.SetActive(closeLobbyBtnActive);
    }

    public void OnCloseButtonPressed()
    {
        _ = OnCloseLobbyBtnPressedAsync();
    }

    public async Task OnCloseLobbyBtnPressedAsync()
    {
        Debug.Log("In closse lobby async!1!");
        //deactivate design interface
        Design_P2_interface.SetActive(false);

        // Deactivate next floor button and add the next floor
        DeactivateCloseLobbyBtnServerRpc();

        PropagatePhaseUpdateClientRpc(AppManager.AppPhase.Saving_Design);

        Debug.Log("Saving Design");
        saveDesignServerRPC();

        Debug.Log("Destroying Modules");
        ModuleSpawner.Instance.DestroySpawnedModulesServerRPC();

        Debug.Log("Destroying Hands");
        HandManager.Instance.DespawnAndDestroyAllHandsServerRpc();

        Debug.Log("Toggling off Design");
        DesignNetworkSyncScript.Instance.buildingHollow.SetActive(false);
        DesignNetworkSyncScript.Instance.Design_P2_interface.SetActive(false);


        NetworkSpawner.Instance.RequestDestroyBuildingServerRpc();

        CloseLobbyServerRPC();

        Debug.Log("Close Lobby Button Pressed");
    }




    [ServerRpc(RequireOwnership = false)]
    public void saveDesignServerRPC()
    {
       saveClientClientRPC();
    }

    [ClientRpc(RequireOwnership = false)]
    public void saveClientClientRPC()
    {
        SaveClient();
    }


    public async void SaveClient()
    {
        await SaveSystem.SaveAllModulesAsync(placedModules);
        await SaveSystem.SaveDesignFileAsync();
    }

    [ServerRpc(RequireOwnership =false)]
    private void CloseLobbyServerRPC()
    {
         //LobbyManager.Instance.CloseLobby();
    }
    #endregion
    void Update()
    {
        if (AppManager.Instance.CurrentPhase() != AppManager.AppPhase.Design_P2 && AppManager.Instance.CurrentPhase() != AppManager.AppPhase.Design_P2_Host)
        {
            if (DesignNetworkSyncScript.Instance.Design_P2_interface.gameObject.activeSelf)
            {
                Debug.Log("Design_P2_interface is active. Deactivating");
                DesignNetworkSyncScript.Instance.Design_P2_interface.gameObject.SetActive(false);
            }
        }
    }

    #region UI Phase Handling
    [ClientRpc(RequireOwnership = false)]
    public void SetNextPhaseClientRPC()
    {
        AppManager.Instance.setNextPhase();
        DesignNetworkSyncScript.Instance.NextPhaseButton.SetActive(false);
        DesignNetworkSyncScript.Instance.manipulationBar.SetActive(false);
        DesignNetworkSyncScript.Instance.floors[currentFloor].SetActive(true);
        DesignNetworkSyncScript.Instance.buildingHollow.SetActive(false);
        DesignNetworkSyncScript.Instance.Design_P2_interface.SetActive(true);
        Debug.Log("SetNextPhaseClientRPC called.");
    }

    public void DisableP2Components()
    {
        DesignNetworkSyncScript.Instance.Design_P2_interface.SetActive(false);
        Debug.Log("DisableP2Components called." + NetworkObjectId);
    }

    #endregion

    #region Save& Load Module Logic

    [ServerRpc(RequireOwnership = false)]
    public void SaveModuleServerRPC(ModuleData newModuleData)
    {
        // Add to the placedModules network list
        SaveModuleClientRPC(newModuleData);
    }

    [ClientRpc(RequireOwnership = false)]
    public void SaveModuleClientRPC(ModuleData newModuleData)
    {
        // Add to the placedModules network list
        placedModules.Add(newModuleData);
    }


    [ServerRpc(RequireOwnership = false)]
    public void LoadAllModulesServerRPC()
    {
        placedModules.Clear();
        StartCoroutine(LoadModulesCoroutine());
    }

    private IEnumerator LoadModulesCoroutine()
    {
        // Asynchronously wait for the returnModules task to complete
        Task<List<ModuleData>> loadModulesTask = returnModules();

        // Wait for the task to complete
        while (!loadModulesTask.IsCompleted)
        {
            yield return null;
        }

        if (loadModulesTask.Exception != null)
        {
            Debug.LogError($"Error loading modules: {loadModulesTask.Exception}");
            yield break;
        }

        // Get the result once the task is completed
        placedModules = loadModulesTask.Result;

        // Now you can use placedModules safely
        foreach (ModuleData moduleData in placedModules)
        {
            Debug.Log($"Module ID: {moduleData.moduleID} Owner ID: Player{moduleData.ownerID} Position X: {moduleData.positionX} Y: {moduleData.positionY} Z: {moduleData.positionZ}");
        }
    }

    public async Task<List<ModuleData>> returnModules()
    {
        return await SaveSystem.LoadAllModulesAsync();
    }
    #endregion

    [ClientRpc(RequireOwnership =false)]
    public void PropagatePhaseUpdateClientRpc(AppManager.AppPhase phase)
    {
       AppManager.Instance.UpdatePhase(phase);
    }


    [ClientRpc(RequireOwnership = false)]
    public void MainMenuClientRpc()
    {
        AppManager.Instance.UpdatePhase(AppManager.AppPhase.MainMenu);
    }

}
