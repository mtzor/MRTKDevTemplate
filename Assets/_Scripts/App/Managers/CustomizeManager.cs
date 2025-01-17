using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Unity.Netcode;
//using Unity.VisualScripting;
//using UnityEditor.PackageManager;
using UnityEngine;
using System.Linq;

public class CustomizeManager : NetworkBehaviour
{
    private static CustomizeManager _instance;
    [SerializeField] private GameObject customize_P1_UI;
    [SerializeField] private GameObject customize_P2_UI;

    [SerializeField] private GameObject roomMenuL;
    [SerializeField] private GameObject roomMenuS;

    [SerializeField] private Transform sharedViewPrefab;

    [SerializeField] private Transform layoutContainer;

    [SerializeField] private ModuleLayouts[] moduleLayouts = new ModuleLayouts[9];

    [SerializeField]private LayoutManager privateLayoutManager;
    [SerializeField]private LayoutManager sharedLayoutManager;

    private CustomizePhase privatePhase;
    private CustomizePhase sharedPhase;

    public int[] privateChoices = new int[2];//to debug
    public int[] sharedChoices = new int[2];


    public LayoutManager currentLayoutManager;// ???? public
    public string _selectedModule;
    public int selectedModuleIndex;

    private Dictionary<ulong, GameObject> playerObjects = new Dictionary<ulong, GameObject>();

    public Dictionary<ulong, GameObject> PlayerObjects { get { return playerObjects; } set { playerObjects = value; } }
    public string SelectedModule { set => _selectedModule = value; }
    public LayoutManager GetCurrentLayoutManager()
    {    return currentLayoutManager; }

    public void SetCurrentLayoutManager(bool isShared)
    {
        currentLayoutManager = (isShared) ? sharedLayoutManager : privateLayoutManager;
    }
    public LayoutManager PrivateLayoutManager(){return privateLayoutManager; }
    public LayoutManager SharedLayoutManager() { return sharedLayoutManager; }


public override void OnNetworkSpawn()
    {

    }
    public void SetChoice(bool isShared, int choice)
    {
        Debug.Log("Setting Choice");
        if (isShared)
        {
            if (sharedPhase == CustomizePhase.Choose_layout)
            {
                sharedChoices[0]=choice;
            }
            else if (sharedPhase==CustomizePhase.Choose_room_layout)
            {
                sharedChoices[1] = choice;
            }
        }
        else
        {
            if (privatePhase == CustomizePhase.Choose_layout)
            {
                privateChoices[0] = choice;
                Debug.Log("Setting Choice private choose layout");
            }
            else if (privatePhase == CustomizePhase.Choose_room_layout)
            {
                privateChoices[1] = choice;

                Debug.Log("Setting Choice private choose room layout");
            }
        }
    }
    public CustomizePhase PrivatePhase { get; set; }
    public CustomizePhase SharedPhase { get; set; }

    public void ToggleCustomize_P1_UI(bool toggle)
    {
        customize_P1_UI.SetActive(toggle);
    }

    public void ToggleCustomize_P2_UI(bool toggle)
    {
        customize_P2_UI.SetActive(toggle);
    }

    public static CustomizeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Find the UIManager in the scene
                _instance = FindObjectOfType<CustomizeManager>();

                if (_instance == null)
                {
                    // Create a new GameObject and attach UIManager if not found
                    GameObject customizeManagerObject = new GameObject("CustomizeManager");
                    _instance = customizeManagerObject.AddComponent<CustomizeManager>();
                }
            }

            return _instance;
        }
    }

    public enum CustomizePhase
    {
        Choose_layout,
        Choose_room_layout,
        Customize_layout,
        Customize_facade//??????????????when to save ?
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void SetupServerLayoutInterfaces()
    {
        Debug.Log("SELECTED MODULE: " + _selectedModule);

        sharedPhase = CustomizePhase.Choose_layout;
        privatePhase = CustomizePhase.Choose_layout;

        // Find layouts for the selected module
        List<Transform> selectedModuleLayouts = new List<Transform>();
        foreach (var module in moduleLayouts)
        {
            if (module.moduleName == _selectedModule)
            {
                selectedModuleLayouts = module.layouts;
                break;
            }
        }

        // Instantiate and configure the PrivateView (not networked)
        PrivateView layoutPrivate = new PrivateView(selectedModuleLayouts, layoutContainer);

        // Instantiate and configure the SharedView (networked)
        SharedView layoutShared = InstantiateSharedView(selectedModuleLayouts, layoutContainer);

        // Set the views in the ViewManager
        ViewManager.Instance.PrivateView = layoutPrivate;
        ViewManager.Instance.SharedView = layoutShared;

        ViewManager.Instance.SetPrivateView(layoutPrivate);
        ViewManager.Instance.SetSharedView(layoutShared);

        ViewManager.Instance.uiController.Initialize(layoutPrivate);
        ViewManager.Instance.InitializeViewManager();
        ViewManager.Instance.uiController.SetView(layoutPrivate);
    }

    private SharedView InstantiateSharedView(List<Transform> selectedModuleLayouts, Transform layoutContainer)
    {
        // Ensure SharedViewPrefab is set up in the inspector
        if (sharedViewPrefab == null)
        {
            Debug.LogError("SharedViewPrefab is not assigned in the inspector!");
            return null;
        }

        // Instantiate the prefab
        Transform sharedViewObject = Instantiate(sharedViewPrefab);

        // Configure the SharedView instance
        SharedView sharedView = sharedViewObject.GetComponent<SharedView>();
        sharedView.Initialize(selectedModuleLayouts, layoutContainer);

        // Spawn it over the network (only the server can do this)
        if (NetworkManager.Singleton.IsServer)
        {
            if (!sharedViewObject.GetComponent<NetworkObject>().IsSpawned)
            {
                Debug.Log("145 SPAWN");
                sharedViewObject.GetComponent<NetworkObject>().Spawn();
            }

            Debug.Log("146 SPAWN");
        }


        return sharedView;
    }

    public void SetupClientLayoutInterfaces()
    {
        Debug.Log("SELECTED MODULE: " + _selectedModule);

        sharedPhase = CustomizePhase.Choose_layout;
        privatePhase = CustomizePhase.Choose_layout;

        // Find layouts for the selected module
        List<Transform> selectedModuleLayouts = new List<Transform>();
        foreach (var module in moduleLayouts)
        {
            if (module.moduleName == _selectedModule)
            {
                selectedModuleLayouts = module.layouts;
                break;
            }
        }

        // Instantiate and configure the PrivateView (not networked)
        PrivateView layoutPrivate = new PrivateView(selectedModuleLayouts, layoutContainer);


        // Instantiate and configure the SharedView (networked)
        SharedView layoutShared = FindObjectOfType<SharedView>();
        if (layoutShared != null)
        {
            Debug.Log("SharedView found in the hierarchy: " + layoutShared.gameObject.name);
            layoutShared.SetItems(selectedModuleLayouts);
        }
        else
        {
            Debug.Log("SharedView not found in the hierarchy.");
        }

            // Set the views in the ViewManager
        ViewManager.Instance.PrivateView = layoutPrivate;
        ViewManager.Instance.SharedView = layoutShared;

        ViewManager.Instance.SetPrivateView(layoutPrivate);
        ViewManager.Instance.SetSharedView(layoutShared);

        ViewManager.Instance.uiController.Initialize(layoutPrivate);
        ViewManager.Instance.InitializeViewManager();
        ViewManager.Instance.uiController.SetView(layoutPrivate);
    }

    public void SetupInterface(bool isShared)
    {
        if (isShared)
        {
            if (sharedPhase == CustomizePhase.Choose_layout)
            {
                SetupRoomLayouts(isShared);
            }
            else if (sharedPhase == CustomizePhase.Choose_room_layout)
            {

               SetupCustomizeLayout(isShared);
            }
        }
        else
        {
            if (privatePhase == CustomizePhase.Choose_layout)
            {
                SetupRoomLayouts(isShared);

                Debug.Log("Setting private room layouta interface");
            }
            else if (privatePhase == CustomizePhase.Choose_room_layout)
            {
                SetupCustomizeLayout(isShared);
                Debug.Log("Setting private customize layouta interface");
            }
        }
    }
    public ModuleLayouts currModule; 

    public List<Transform> selectedModuleRoomLayouts = new List<Transform>();

    /*
    public void SetupRoomLayouts(bool shared)
    {
        foreach (var module in moduleLayouts)
        {

            if (module.moduleName == _selectedModule)
            {
                int selectedIndex;
                currModule= module;

                if (shared)
                {
                    selectedIndex = sharedChoices[0];
                    sharedPhase = CustomizePhase.Choose_room_layout;
                    selectedModuleRoomLayouts = module.roomLayouts[selectedIndex].roomLayouts;

                    sharedLayoutManager.SetLayout( module.layouts[selectedIndex]);
                   // ViewManager.Instance.SharedView.SetSharedItemsForClients(selectedIndex);

                }
                else
                {
                    Debug.Log("PRIVATE SETUP ROOM LAYOUTS");
                    selectedIndex = privateChoices[0];
                    privatePhase = CustomizePhase.Choose_room_layout;
                    selectedModuleRoomLayouts = module.roomLayouts[selectedIndex].roomLayouts;

                    privateLayoutManager.SetLayout(module.layouts[selectedIndex]);
                }


                Debug.Log("Selected index" + selectedIndex);
                //Debug.Log("SELECTED MODULE ROOM LAYOUTS" +);

                foreach(var layout in selectedModuleRoomLayouts)
                {

                    Debug.Log("Layout Name " + layout.name);
                }

                break;
            }
        }

        if (shared)
        {
            ViewManager.Instance.SharedView.SetItems(selectedModuleRoomLayouts);
        }
        else
        {
            ViewManager.Instance.PrivateView.SetItems(selectedModuleRoomLayouts);
        }
    }*/

    public void SetupRoomLayouts(bool shared)
    {
        // Get the index of the module with the matching name
        int selectedIndex = -1; // Default value if no match is found

        for (int i = 0; i < moduleLayouts.Count(); i++)
        {
            if (moduleLayouts[i].moduleName == _selectedModule)
            {
                selectedIndex = i;
                selectedModuleIndex = i;
                break; // Exit the loop as soon as a match is found
            }
        }

        if (selectedIndex == -1)
        {
            Debug.LogError($"Module with name {_selectedModule} not found.");
            return;
        }

        // Assign the matching module
        var currModule = moduleLayouts[selectedIndex];

        if (shared)
        {
            Debug.Log("Setting up shared room layouts...");
            int layoutIndex = sharedChoices[0];
            sharedPhase = CustomizePhase.Choose_room_layout;
            selectedModuleRoomLayouts = currModule.roomLayouts[layoutIndex].roomLayouts;

            sharedLayoutManager.SetLayout(currModule.layouts[layoutIndex]);
        }
        else
        {
            Debug.Log("Setting up private room layouts...");
            int layoutIndex = privateChoices[0];
            privatePhase = CustomizePhase.Choose_room_layout;
            selectedModuleRoomLayouts = currModule.roomLayouts[layoutIndex].roomLayouts;

            privateLayoutManager.SetLayout(currModule.layouts[layoutIndex]);
        }

        Debug.Log($"Selected module: {currModule.moduleName}, Selected index: {selectedIndex}");
        foreach (var layout in selectedModuleRoomLayouts)
        {
            Debug.Log("Layout Name: " + layout.name);
        }

        // Set items in the appropriate view
        if (shared)
        {
            ViewManager.Instance.SharedView.SetItems(selectedModuleRoomLayouts);
        }
        else
        {
            ViewManager.Instance.PrivateView.SetItems(selectedModuleRoomLayouts);
        }
    }


    public void SetupCustomizeLayout(bool shared)
    {
        //instantiate the correct interface based on choice[2]?

        //setup lists for shared or not of rooms and items

        List<Transform> itemList = new List<Transform>();

        foreach (var module in moduleLayouts)
        {

                if (module.moduleName == _selectedModule)
                {
                    int selectedIndex0;                
                    int selectedIndex1;
                    currModule = module;

                    if (shared)
                    {                    
                        selectedIndex0 = sharedChoices[0];
                        selectedIndex1 = sharedChoices[1];
                        sharedPhase = CustomizePhase.Customize_layout;

                        selectedModuleRoomLayouts = module.roomLayouts[selectedIndex0].roomLayouts;

                        sharedLayoutManager.SetRoomLayout(module.roomLayouts[selectedIndex0].customizationLayouts[selectedIndex1]);
                        
                        itemList.Add(module.roomLayouts[selectedIndex0].customizationLayouts[selectedIndex1]);

                        ViewManager.Instance.sharedView.SetItems(selectedModuleRoomLayouts);
                        //setLayoutManager(true);

                        if (IsServer)
                        {
                               // sharedLayoutManager.DisplayLayoutModelServerRPC();
                        }
                        // ViewManager.Instance.SharedView.SetSharedItemsForClients(selectedIndex);

                    }
                    else
                    {
                            selectedIndex0 = privateChoices[0];
                            selectedIndex1 = privateChoices[1];

                            privatePhase = CustomizePhase.Customize_layout;

                            selectedModuleRoomLayouts = module.roomLayouts[selectedIndex0].roomLayouts;

                            privateLayoutManager.SetRoomLayout(module.roomLayouts[selectedIndex0].customizationLayouts[selectedIndex1]);

                            Debug.Log("Name:"+module.roomLayouts[selectedIndex0].customizationLayouts[selectedIndex1].name);

                            itemList.Add(module.roomLayouts[selectedIndex0].customizationLayouts[selectedIndex1]);

                            ViewManager.Instance.privateView.SetItems(selectedModuleRoomLayouts);
                           // setLayoutManager(false);

                           // privateLayoutManager.DisplayLayoutModelServerRPC();
                    }
                 
                }
        }

            if (shared)
            {
                ViewManager.Instance.SharedView.SetItems(itemList);
            }
            else
            {
                ViewManager.Instance.PrivateView.SetItems(itemList);
            }
        
    }
    
    public void setLayoutManager(bool isShared)
    {
        if (!isShared)
        {
            currentLayoutManager = privateLayoutManager;
        }
        else
        {
            currentLayoutManager = sharedLayoutManager;
        }
    }

    public void ToggleLocalPlayer(bool value)
    {
        // Check if this is the local client
        ulong localClientId = NetworkManager.Singleton.LocalClientId;

        // Fetch the local player's NetworkObject
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(localClientId, out var localPlayerNetworkObject))
        {
            GameObject localPlayerObject = localPlayerNetworkObject.gameObject;

            // Access the PlayerController and toggle the body
            localPlayerObject.GetComponent<PlayerController>().ToggleBody(value);
            Debug.Log($"Local player's GameObject: {localPlayerObject.name}");
        }
        else
        {
            Debug.LogWarning("Local player not found in SpawnedObjects.");
        }
    }


    [ClientRpc(RequireOwnership = false)]
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
