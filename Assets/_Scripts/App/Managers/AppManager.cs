using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using MixedReality.Toolkit.UX;
using Unity.Netcode;


public class AppManager : MonoBehaviour
{
    private static AppManager _instance;

    public AppPhase currentPhase;
    private AppPhase previousPhase;

    #region AppPhases & AppPhaseChangeEvent
    public enum AppPhase
    {
        Startup,
        Role_Choice,
        MainMenu,
        Tutorial,
        HomeDialogue,
        Lobby_List_Design,
        Lobby_List_Customize,
        Lobby_List_Visualize,
        Lobby_Design,
        Lobby_Customize,
        Lobby_Visualize,
        Design_P1_Host,
        Design_P1,
        Design_P12_Host,
        Design_P12,
        Design_P2_Host,
        Design_P2,
        Customize_Module_Selection,
        Customize_P1,
        Customize_P2,
        Saving_Design,
        Visualize_Module_Selection,
        Visualize

    }
    //Get current phase of the app
    public AppPhase CurrentPhase()
    {
        return currentPhase;
    }
    //Get the previous App Phase
    public AppPhase PreviousPhase()
    {
        return previousPhase;
    }

    //custom event fired when the app changes phase
    public class AppPhaseChangeEvent
    {
        public AppManager.AppPhase newPhase; // The new game phase

        public AppPhaseChangeEvent(AppManager.AppPhase phase)
        {
            newPhase = phase;
        }
    }

    public delegate void OnAppPhaseChange(AppPhaseChangeEvent e);
    public OnAppPhaseChange OnAppPhaseChanged; // Event variable

    #endregion


    // Start is called before the first frame update
    void Start()
    {
        UpdatePhase(AppPhase.Startup);
    }

    public static AppManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Find the UIManager in the scene
                _instance = FindObjectOfType<AppManager>();

                if (_instance == null)
                {
                    // Create a new GameObject and attach UIManager if not found
                    GameObject appManagerObject = new GameObject("AppManager");
                    _instance = appManagerObject.AddComponent<AppManager>();
                }
            }

            return _instance;
        }
    }

    public async Task UpdatePhase(AppPhase nextPhase)
    {
        previousPhase = currentPhase;//updating previous phase

        switch (nextPhase)
        {
            case AppPhase.Startup:
                // Perform startup tasks
                currentPhase = AppPhase.Startup;

                DialogButtonType result = await StartApp();

                if(result == DialogButtonType.Positive)
                {
                    UpdatePhase(AppPhase.Tutorial);//calling update phase for the tutorial
                }
                else
                {
                    UpdatePhase(AppPhase.Role_Choice);//calling update phase for the tutorial
                }
                break;

            case AppPhase.Role_Choice:
                // Perform startup tasks
                currentPhase = AppPhase.Role_Choice;
                Debug.Log("ROLE CHOICE APPPHASE");

                result = await RoleChoice();

                if (result == DialogButtonType.Positive)
                {
                    LobbyManager.Instance.SetPlayerType(LobbyManager.PlayerType.Expert);
                }
                else
                {
                    LobbyManager.Instance.SetPlayerType(LobbyManager.PlayerType.NonExpert);
                }
                
                UpdatePhase(AppPhase.MainMenu);//calling update phase for the tutorial
                TriggerAppPhaseChange();
                break;

            case AppPhase.Tutorial:
                currentPhase = AppPhase.Tutorial;

                TriggerAppPhaseChange();

                //running tutorial task
                int exitCode = await TutorialManager.Instance.runTutorial();

                Debug.Log("TUTORIAL ENDED CODE "+exitCode);
                UpdatePhase(AppPhase.Role_Choice);

                break;

            case AppPhase.MainMenu:

                currentPhase = AppPhase.MainMenu;

                Debug.Log("Update phase Main menu");
                TriggerAppPhaseChange();

                break;

            case AppPhase.Lobby_List_Design:
                // Handle tutorial logic
                currentPhase = AppPhase.Lobby_List_Design;

                LoadingManager.Instance.SetLoadingText("Loading Lobbies");
                LoadingManager.Instance.EnableLoadingScreen();
                               
                    await Task.Delay(3000);
               
                LoadingManager.Instance.DisableLoadingScreen();
                TriggerAppPhaseChange();

                break;

            case AppPhase.Lobby_List_Customize:
                // Handle tutorial logic
                currentPhase = AppPhase.Lobby_List_Customize;
                TriggerAppPhaseChange();


                LoadingManager.Instance.SetLoadingText("Loading Lobbies");
                LoadingManager.Instance.EnableLoadingScreen();

                await Task.Delay(1000);

                LoadingManager.Instance.DisableLoadingScreen();

                break;
            case AppPhase.Lobby_List_Visualize:
                // Handle tutorial logic
                currentPhase = AppPhase.Lobby_List_Visualize;
                TriggerAppPhaseChange();


                LoadingManager.Instance.SetLoadingText("Loading Designs");
                LoadingManager.Instance.EnableLoadingScreen();

                await Task.Delay(1000);

                LoadingManager.Instance.DisableLoadingScreen();

                break;

            case AppPhase.Lobby_Customize:
                // Handle tutorial logic
                currentPhase = AppPhase.Lobby_Customize;

                LoadingManager.Instance.SetLoadingText("Loading Lobby");
                LoadingManager.Instance.EnableLoadingScreen();

                while (!LobbyManager.Instance.LobbyCreated && !LobbyManager.Instance.LobbyJoined)
                {
                    await Task.Delay(200);
                }

                LoadingManager.Instance.DisableLoadingScreen();
                LobbyManager.Instance.UpdatePlayerCharacter(LobbyManager.Instance.GetPlayerType());

                TriggerAppPhaseChange();

                break;

            case AppPhase.Lobby_Visualize:
                // Handle tutorial logic
                currentPhase = AppPhase.Lobby_Visualize;

                LoadingManager.Instance.SetLoadingText("Loading Lobby");
                LoadingManager.Instance.EnableLoadingScreen();

                while (!LobbyManager.Instance.LobbyCreated && !LobbyManager.Instance.LobbyJoined)
                {
                    await Task.Delay(200);
                }

                LoadingManager.Instance.DisableLoadingScreen();
                LobbyManager.Instance.UpdatePlayerCharacter(LobbyManager.Instance.GetPlayerType());

                TriggerAppPhaseChange();

                setPhase(true);
                LobbyManager.Instance.StartSession();
                LobbyUI.Instance.Hide();

                break;

            case AppPhase.Lobby_Design:
                // Handle tutorial logic
                currentPhase = AppPhase.Lobby_Design;

                LoadingManager.Instance.SetLoadingText("Loading Lobby");
                LoadingManager.Instance.EnableLoadingScreen();

                while (!LobbyManager.Instance.LobbyCreated && !LobbyManager.Instance.LobbyJoined)
                {
                    await Task.Delay(200);
                }
                
                LoadingManager.Instance.DisableLoadingScreen();
                LobbyManager.Instance.UpdatePlayerCharacter(LobbyManager.Instance.GetPlayerType());

                TriggerAppPhaseChange();
                break;

            case AppPhase.Customize_Module_Selection:
                // Handle tutorial logic
                currentPhase = AppPhase.Customize_Module_Selection;
               // UIManager.Instance.Show("Module Selection Interface");

                LoadingManager.Instance.SetLoadingText("Loading Dwelling Selection Interface");
                LoadingManager.Instance.EnableLoadingScreen();

                await Task.Delay(2000);

                LoadingManager.Instance.DisableLoadingScreen();

                TriggerAppPhaseChange();

                //TriggerAppPhaseChange();

                break;

            case AppPhase.Visualize_Module_Selection:
                // Handle tutorial logic
                currentPhase = AppPhase.Visualize_Module_Selection;
                // UIManager.Instance.Show("Module Selection Interface");

                LoadingManager.Instance.SetLoadingText("Loading Vizualize Selection Interface");
                LoadingManager.Instance.EnableLoadingScreen();

                await Task.Delay(2000);

                LoadingManager.Instance.DisableLoadingScreen();

                TriggerAppPhaseChange();

                //TriggerAppPhaseChange();

                break;

            case AppPhase.Customize_P1:

                TriggerAppPhaseChange();
                // Handle tutorial logic
                currentPhase = AppPhase.Customize_P1;

                TriggerAppPhaseChange();

                break;

            case AppPhase.Customize_P2:
                // Handle tutorial logic
                currentPhase = AppPhase.Customize_P2;

                TriggerAppPhaseChange();

                break;

            case AppPhase.Design_P1_Host:
                // Handle tutorial logic
                currentPhase = AppPhase.Design_P1_Host;


                LoadingManager.Instance.SetLoadingText("Loading Design Interface");
                LoadingManager.Instance.EnableLoadingScreen();

                await Task.Delay(3000);

                LoadingManager.Instance.DisableLoadingScreen();

                Debug.Log("Design_p1 PHASE");

                //TriggerAppPhaseChange();
                break;

            case AppPhase.Design_P1:
                // Handle tutorial logic
                currentPhase = AppPhase.Design_P1;


                LoadingManager.Instance.SetLoadingText("Waiting for host to create a new Design.");
                LoadingManager.Instance.EnableLoadingScreen();

                await Task.Delay(3000);

                LoadingManager.Instance.DisableLoadingScreen();

                break;

            case AppPhase.Design_P12_Host:
                // Handle tutorial logic
                currentPhase = AppPhase.Design_P12_Host;

                TriggerAppPhaseChange();
                break;

            case AppPhase.Design_P12:
                // Handle tutorial logic
                currentPhase = AppPhase.Design_P12;

                TriggerAppPhaseChange();
                break;

            case AppPhase.Design_P2:
                // Handle tutorial logic
                currentPhase = AppPhase.Design_P2;

                TriggerAppPhaseChange();

                break;
            case AppPhase.Design_P2_Host:
                // Handle tutorial logic
                currentPhase = AppPhase.Design_P2_Host;

                TriggerAppPhaseChange();

                break;

            case AppPhase.Visualize:

                TriggerAppPhaseChange();
                // Handle tutorial logic
                currentPhase = AppPhase.Visualize;

                TriggerAppPhaseChange();

                break;

            case AppPhase.Saving_Design:
                // Handle tutorial logic
                currentPhase = AppPhase.Saving_Design;

                LoadingManager.Instance.SetLoadingText("Saving Design");
                LoadingManager.Instance.EnableLoadingScreen();

                await Task.Delay(5000);

                LoadingManager.Instance.DisableLoadingScreen();

                UpdatePhase(AppPhase.MainMenu);

                //TriggerAppPhaseChange();

                break;

            case AppPhase.HomeDialogue:
                // Handle tutorial logic
                currentPhase = AppPhase.HomeDialogue;

                break;
                // Add similar cases for other phases
        }

        //trigger app phase changed event
        TriggerAppPhaseChange();

    }

    private void TriggerAppPhaseChange()
    {
        if (OnAppPhaseChanged != null)
        {
            OnAppPhaseChanged(new AppPhaseChangeEvent(currentPhase)); // Trigger the event with a new AppPhaseChangeEvent object
        }
        else
        {
            Debug.Log("OnAppPhaseChanged NULL");
        }
    }

    #region setPhase Functions

    //Changes AppPhase Main Menu -> Lobby_list
    public void setPhaseLobbyList()
    {
        if(LobbyManager.Instance.sessionMode == LobbyManager.SessionMode.Design)
        {
            UpdatePhase(AppPhase.Lobby_List_Design);
        }
        else if(LobbyManager.Instance.sessionMode == LobbyManager.SessionMode.Customize)
        {
            UpdatePhase(AppPhase.Lobby_List_Customize);
        }
        else if(LobbyManager.Instance.sessionMode == LobbyManager.SessionMode.Visualize)
        {
            UpdatePhase(AppPhase.Lobby_List_Visualize);
        }
    }

    //Changes AppPhase Lobby_list -> Lobby
    public void setPhaseLobby()
    {
        if (LobbyManager.Instance.sessionMode == LobbyManager.SessionMode.Design)
        {
            UpdatePhase(AppPhase.Lobby_Design);
        }
        else if (LobbyManager.Instance.sessionMode == LobbyManager.SessionMode.Customize)
        {
            UpdatePhase(AppPhase.Lobby_Customize);
        }
        else if (LobbyManager.Instance.sessionMode == LobbyManager.SessionMode.Visualize)
        {
            UpdatePhase(AppPhase.Lobby_Visualize);
        }
    }

    //Changes AppPhase Lobby -> P1 (Design or Customize)
    public void setPhase(bool isHost)
    {
        if (LobbyManager.Instance.sessionMode == LobbyManager.SessionMode.Design && isHost)
        {
            UpdatePhase(AppPhase.Design_P1_Host);
            Debug.Log("SET PHASE DESIGN HOST");
        }
        else if (LobbyManager.Instance.sessionMode == LobbyManager.SessionMode.Design && !isHost)
        {
            Debug.Log("SET PHASE DESIGN ");
            UpdatePhase(AppPhase.Design_P1);
        }
        else if (LobbyManager.Instance.sessionMode == LobbyManager.SessionMode.Customize && currentPhase!=AppPhase.Customize_Module_Selection)
        {
            Debug.Log("SET PHASE CUSTOMIZE MODULE SELECTION");
            UpdatePhase(AppPhase.Customize_Module_Selection);
        }
        else if (LobbyManager.Instance.sessionMode == LobbyManager.SessionMode.Visualize && currentPhase != AppPhase.Visualize_Module_Selection)
        {
            Debug.Log("SET PHASE VIZUALIZE MODULE SELECTION");
            UpdatePhase(AppPhase.Visualize_Module_Selection);
        }
    }

    //Changes AppPhase P1 (Design or Customize) -> P2
    public void setNextPhase()
    {
        if (LobbyManager.Instance.sessionMode == LobbyManager.SessionMode.Design)
        {
            if (currentPhase == AppManager.AppPhase.Design_P1_Host)
            {
                UpdatePhase(AppPhase.Design_P12_Host);
            }
            else if (currentPhase == AppManager.AppPhase.Design_P1)
            { 
                UpdatePhase(AppPhase.Design_P12); 
            }
            else if (currentPhase == AppManager.AppPhase.Design_P12_Host)
            {
                UpdatePhase(AppPhase.Design_P2_Host);
            }
            else if (currentPhase == AppManager.AppPhase.Design_P12)
            {
                UpdatePhase(AppPhase.Design_P2);
            }
        }

        else if (currentPhase == AppManager.AppPhase.Customize_Module_Selection)
        {
            UpdatePhase(AppPhase.Customize_P1);
        }
        else if (currentPhase == AppManager.AppPhase.Visualize_Module_Selection)
        {
            UpdatePhase(AppPhase.Visualize);
        }
        else if (currentPhase == AppManager.AppPhase.Customize_P1)
        {
            UpdatePhase(AppPhase.Customize_P2);
        }
        else if (currentPhase == AppManager.AppPhase.Customize_P2)
        {
            UpdatePhase(AppPhase.MainMenu);
        }
    }
    
    #endregion
    public async Task<DialogButtonType> StartApp()
    {

        DialogButtonType result = await DialogManager.Instance.SpawnDialogWithAsync("Welcome to the AR-App application !", "Would you like to view the application tutorial ?", "VIEW","CANCEL");
        return result;
    }

    public async Task<DialogButtonType> RoleChoice()
    {

        DialogButtonType result = await DialogManager.Instance.SpawnDialogWithAsync("Chose your role .", "Are you a non expert user or an expert user (architect,engineer) ?", "EXPERT", "NON-EXPER");
        return result;
    }
    public void TutorialComplete()
    {
        // Log completion
        Debug.Log("Tutorial completed, returning to Role Choice.");
        UpdatePhase(AppPhase.Role_Choice);
    }
}
