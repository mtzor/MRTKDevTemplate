using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using static AppManager;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;

    [SerializeField] private List<GameObject> uiElements;  // List of UI elements
    [SerializeField] private List<GameObject> sceneHelpers;   // List of scene helpers
    private Dictionary<string, GameObject> uiElementsDict;
    private Dictionary<string, GameObject> sceneHelperElementsDict;

    private string previousInterface;
    public string currentInterface;
    public GameObject currentSceneHelper;


    public List<GameObject> additionalSceneHelpers;
    // Singleton instance
    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UIManager>() ?? new GameObject("UIManager").AddComponent<UIManager>();
            }
            return _instance;
        }
    }

    public void Start()
    {
        InitializeUIElements();

        if (AppManager.Instance != null)
        {
            AppManager.Instance.OnAppPhaseChanged += UpdateAppPhaseEvent;
        }
    }

    #region UI init & toggling

    private void InitializeUIElements()
    {
        uiElementsDict = new Dictionary<string, GameObject>();
        sceneHelperElementsDict = new Dictionary<string, GameObject>();

        for (int i = 0; i < uiElements.Count; i++)
        {
            if (uiElements[i] != null)
            {
                uiElementsDict.Add(uiElements[i].name, uiElements[i]);
                if (i < sceneHelpers.Count)
                {
                    sceneHelperElementsDict.Add(uiElements[i].name, sceneHelpers[i]);
                }
                uiElements[i].SetActive(false); // Start with all UI elements hidden
            }
        }
    }

    public void Show(string uiElementName)
    {
        if (uiElementsDict.ContainsKey(uiElementName))
        {
            uiElementsDict[uiElementName].SetActive(true);
            UpdateSceneHelper(uiElementName);
        }
        else
        {
            Debug.LogWarning($"UI Element {uiElementName} not found.");
        }
    }

    public void Hide(string uiElementName)
    {
        if (uiElementsDict.ContainsKey(uiElementName))
        {
            uiElementsDict[uiElementName].SetActive(false);
        }
        else
        {
            Debug.LogWarning($"UI Element {uiElementName} not found.");
        }
    }

    public void HideAll()
    {
        foreach (var uiElement in uiElementsDict.Values)
        {
            uiElement.SetActive(false);
        }

        Show("HandMenu");
    }
    #endregion

    private async void UpdateAppPhaseEvent(AppManager.AppPhaseChangeEvent e)
    {
        previousInterface = currentInterface;

        switch (e.newPhase)
        {
            case AppManager.AppPhase.Startup:
                InitializeUIElements();
                HideAll();
                break;

            case AppManager.AppPhase.Tutorial:
                HideAll();
                break;

            case AppManager.AppPhase.MainMenu:
                HideAll();
                Show("MainMenu");
                currentInterface = "MainMenu";
                break;

            case AppManager.AppPhase.Lobby_List_Customize:
                HideAll();
                Show("Join/Create Lobby Interface");
                currentInterface = "Join/Create Lobby Interface";

                await SaveSystem.LoadDesignLobbiesFromCloudAsync();

                LobbyManager.Instance.RefreshLobbyList();
                break;
            case AppManager.AppPhase.Lobby_List_Visualize:
                HideAll();
                Show("Join/Create Lobby Interface");
                currentInterface = "Join/Create Lobby Interface";

                await SaveSystem.LoadCustomizeLobbiesFromCloudAsync();

                LobbyManager.Instance.RefreshLobbyList();
                break;
            case AppManager.AppPhase.Lobby_List_Design:
                HideAll();
                Show("Join/Create Lobby Interface");
                currentInterface = "Join/Create Lobby Interface";
                LobbyManager.Instance.RefreshLobbyList();
                break;

            case AppManager.AppPhase.Lobby_Customize:
                Debug.Log(" UIManager CUSTOMIZE LOBBY");
                HideAll();
                Show("Lobby UI");
                currentInterface = "Lobby UI";
                break;

            case AppManager.AppPhase.Lobby_Design:
                HideAll();
                Show("Lobby UI");
                currentInterface = "Lobby UI";
                break;
            case AppManager.AppPhase.Lobby_Visualize:
                HideAll();
                Show("Lobby UI");
                currentInterface = "Lobby UI";
                break;


            case AppManager.AppPhase.Customize_Module_Selection:

                currentInterface = "Dwelling Selection UI";
                Debug.Log(" Dwelling Selection UI");

                //HideAll();
                //Show("Module Selection Interface");

                Debug.Log(" UIManager CUSTOMIZE SELECTION AFTER SHOW");

                break;

            case AppManager.AppPhase.Visualize_Module_Selection:

                currentInterface = "Rating Apartment Selection UI";
                Debug.Log("Rating Apartment Selection UI");

                //HideAll();
                //Show("Module Selection Interface");

                Debug.Log(" UIManager vizualize SELECTION AFTER SHOW");

                break;
            case AppManager.AppPhase.Customize_P1:
                HideAll();
                Show("Customize_P1 UI");
                currentInterface = "Customize_P1 UI";
                break;

            case AppManager.AppPhase.Customize_P2:
                HideAll();
                Show("Customize_P2 UI");
                currentInterface = "Customize_P2 UI";

                uiElementsDict["Customize_P1 UI"].SetActive(true);

                break;
            case AppManager.AppPhase.Design_P1:
                HideAll();
                Show("Design_P1 UI");
                currentInterface = "Design_P1 UI";

                break;

            case AppManager.AppPhase.Design_P1_Host:
                HideAll();
                Show("Design_P1 UI");
                currentInterface = "Design_P1 UI";

                break;

            case AppManager.AppPhase.Design_P12:
                HideAll();
                currentInterface = "Design_P1 UI";

                AddSceneHelpers();

                currentInterface = "Design_P1 UI ";

                UpdateSceneHelper("Design P1");

                break;

            case AppManager.AppPhase.Design_P12_Host:
                HideAll();
                currentInterface = "Design_P1 UI";

                AddSceneHelpers();

                currentInterface = "Design_P1 UI Host";

                UpdateSceneHelper("Design P1 Host");

                break;

            case AppManager.AppPhase.Design_P2:
                HideAll();
                Show("Design_P2 UI");
                currentInterface = "Design_P2 UI";

                UpdateSceneHelper("Design P2");
                break;

            case AppManager.AppPhase.Design_P2_Host:
                HideAll();
                Show("Design_P2 UI");

                AddSceneHelpers();

                currentInterface = "Design_P2 UI Host";

                UpdateSceneHelper("Design P2 Host");
                break;

            case AppManager.AppPhase.Visualize:
                HideAll();
                Show("Visualize Interface");
                currentInterface = "Visualize Interface";

                break;
            case AppManager.AppPhase.Saving_Design:
                HideAll();
                Show("Saving_Design");
                AddSceneHelpers();

                currentInterface = "Saving_Design";
                break;

            case AppManager.AppPhase.HomeDialogue:
                HideAll();
                Show("HomeDialogue");
                currentInterface = "HomeDialogue";
                break;


            default:
                Debug.LogWarning("Unhandled AppPhase: " + e.newPhase);
                break;
        }
    }

    private void UpdateSceneHelper(string uiElementName)
    {
        if (sceneHelperElementsDict.ContainsKey(uiElementName))
        {
            if (currentSceneHelper != null) currentSceneHelper.SetActive(false);
            currentSceneHelper = sceneHelperElementsDict[uiElementName];
            currentSceneHelper.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"No Scene Helper associated with UI element {uiElementName} Trying to find one in the additional ");

            foreach (var sceneHelper in additionalSceneHelpers)
            {
                sceneHelper.SetActive(true);
            }

            GameObject requesteSceneHelper= GameObject.FindGameObjectWithTag(uiElementName);
            Debug.Log("RequestedSceneHelper" + requesteSceneHelper + " found");

            foreach (var sceneHelper in additionalSceneHelpers)
            {
                sceneHelper.SetActive(false);
            }

            if (currentSceneHelper != null) currentSceneHelper.SetActive(false);
            currentSceneHelper = requesteSceneHelper;
            currentSceneHelper.SetActive(true);
        }
    }
    private bool isActive=false;
    public void ToggleCurrentSceneHelper()
    {
        if (currentSceneHelper == null)
        {
            Debug.LogWarning("No current scene helper to toggle.");
            return;
        }
        isActive=!isActive;

        currentSceneHelper.SetActive(isActive);
        foreach (Transform child in currentSceneHelper.transform)
        {
            child.gameObject.SetActive(isActive);
            Debug.Log("tOGGLING CHILDREN ON");
        }
    }

    void OnDestroy()
    {
        if (AppManager.Instance != null)
        {
            AppManager.Instance.OnAppPhaseChanged -= UpdateAppPhaseEvent;
        }
    }

    public void AddSceneHelpers()
    {

        var objectsWithName = FindObjectsOfType<GameObject>()
                           .Where(go => go.name == "SceneHelper");

        foreach (var obj in objectsWithName)
        {
            if (!additionalSceneHelpers.Contains(obj))
            {
                additionalSceneHelpers.Add(obj); // Add each found object to the sceneHelpers list
                Debug.Log("Found object with name: " + obj.name);
            }
           
        }
    }


    public void SetCurrentSceneHelper(GameObject sceneHelper)
    {
        currentSceneHelper = sceneHelper;
        //sceneHelper.gameObject.SetActive(true);
    }
}
