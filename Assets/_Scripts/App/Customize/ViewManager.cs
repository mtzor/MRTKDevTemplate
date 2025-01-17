using MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using Unity.Netcode;


public class ViewManager : MonoBehaviour
{

    private static ViewManager _instance;

    [SerializeField] public IView sharedView;
    [SerializeField] public IView privateView;

    [SerializeField] public ViewUIController uiController;

    [SerializeField] public ProgressIndicator progressIndicator;

    [SerializeField] public PressableButton finalizeChoiceBtn;

    [SerializeField] private PressableButton sharedViewToggle;
    [SerializeField] private TMP_Text sharedViewText;

    [SerializeField] private GameObject privateSceneHelper;
    [SerializeField] private GameObject sharedSceneHelper;
    [SerializeField] private GameObject simpleSceneHelper;

    public IView currentIVew;

    private int selectedItem;
    private bool isShared = false;

    private bool isPrivateComplete = false;
    private bool isSharedComplete = false;

    public bool IsShared{ set; get; }

    public int SelectedItem { get; set; }
    private void Start()
    {
        sharedViewToggle.OnClicked.AddListener(OnSharedViewToggled);
        finalizeChoiceBtn.OnClicked.AddListener(OnFinalizeChoiceBtnPressed);
    }
   
    public static ViewManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ViewManager>();
                if (_instance == null)
                {
                    GameObject viewManager = new GameObject("ViewManager");
                    _instance = viewManager.AddComponent<ViewManager>();
                }
            }
            return _instance;
        }
    }

    public void SetPrivateView(IView view)
    {
        privateView = view;
    }

    public void SetSharedView(IView view)
    {
        sharedView = view;
    }

    public IView PrivateView { get; set; }
    public IView SharedView { get; set; }

    public void InitializeViewManager()
    {
        sharedViewText.text = "Private View";
        currentIVew = privateView;
        UIManager.Instance.SetCurrentSceneHelper(privateSceneHelper);
        CustomizeManager.Instance.ToggleLocalPlayer(false);
        // uiController.SetView(currentIVew);
    }
    private void OnSharedViewToggled()
    {
        Debug.Log("OnsharedViewToggled");
        currentIVew.DestroyCurrentItem();

        isShared = !isShared;

        // Handle view switch externally if necessary
        if (isShared) {
            if (SharedView ==null) {
                Debug.Log("Shared View is NULL");
            }
            sharedViewText.text = "Shared View";
            currentIVew = sharedView;
            CustomizeManager.Instance.SetCurrentLayoutManager(isShared);
            sharedView.ReportSharedViewState(true);
            uiController.SetView(sharedView);
            CustomizeManager.Instance.ToggleLocalPlayer(true);
            if (!currentIVew.IsComplete)
            {
                ToggleCompleteView(false);
                //sharedView.ShowCurrentItem();//?????
                ShowModel();
                UIManager.Instance.SetCurrentSceneHelper(sharedSceneHelper);
            }
            else
            {
                sharedView.DestroyCurrentItem();
                ToggleCompleteView(true);
                UIManager.Instance.SetCurrentSceneHelper(simpleSceneHelper);
            }
            uiController.ToggleCompareModeToggle(false);
        }
        else
        {
            sharedViewText.text = "Private View";
            sharedView.ReportSharedViewState(false);
            currentIVew = privateView;
            uiController.SetView(privateView);
            CustomizeManager.Instance.SetCurrentLayoutManager(isShared);
            CustomizeManager.Instance.ToggleLocalPlayer(false);
            if (!currentIVew.IsComplete)
            {
                ToggleCompleteView(false);
                // privateView.ShowCurrentItem();//?????
                ShowModel();
                UIManager.Instance.SetCurrentSceneHelper(privateSceneHelper);
            }
            else
            {
                privateView.DestroyCurrentItem();
                ToggleCompleteView(true);
                UIManager.Instance.SetCurrentSceneHelper(simpleSceneHelper);
            }
          
        }
    }
    public void ToggleCompleteView(bool isActive)
    {
        LoadingManager.Instance.SetLoadingText("Complete all views to move to the next stage");
        if (isActive)
        {
            LoadingManager.Instance.EnableLoadingScreen();
            currentIVew.DestroyCurrentItem();
        }
        else
        {
            LoadingManager.Instance.DisableLoadingScreen();
        }
        if (currentIVew==sharedView && CustomizeManager.Instance.SharedPhase==CustomizeManager.CustomizePhase.Customize_layout || 
            currentIVew == privateView && CustomizeManager.Instance.PrivatePhase == CustomizeManager.CustomizePhase.Customize_layout
            ) 
        {
            uiController.ToggleBtns(false);
            uiController.ToggleCompareModeToggle(false);
            finalizeChoiceBtn.gameObject.SetActive(!isActive);
        }
        else
        {
            uiController.ToggleBtns(!isActive);
            uiController.ToggleCompareModeToggle(!isActive);
            finalizeChoiceBtn.gameObject.SetActive(!isActive);
        }
    }
    private void OnFinalizeChoiceBtnPressed()
    {
        OnFinalizeChoiceBtnAsync();

        return;
    }

    private async Task OnFinalizeChoiceBtnAsync()
    {
        Debug.Log("FINALIZE CHOICE BTN PRESSED");
        finalizeChoiceBtn.gameObject.SetActive(false);

        await currentIVew.FinalizeChoice();       

        return;
    }

    public void SetNextCurrentViewPhase()
    {
        if (currentIVew.SelectedIndex() != -1)
        {
            selectedItem = currentIVew.SelectedIndex();

            if (currentIVew.IsShared)
            {
                if (CustomizeManager.Instance.SharedPhase == CustomizeManager.CustomizePhase.Choose_layout)
                {
                    CustomizeManager.Instance.SharedPhase = CustomizeManager.CustomizePhase.Choose_room_layout;
                    Debug.Log("Setting up room layouts");
                   CustomizeManager.Instance.SetupRoomLayouts(isShared);
                    finalizeChoiceBtn.gameObject.SetActive(true);
                    CustomizeManager.Instance.ToggleCustomize_P1_UI(true);
                    LoadingManager.Instance.DisableLoadingScreen();

                }
                else if (CustomizeManager.Instance.SharedPhase == CustomizeManager.CustomizePhase.Choose_room_layout )
                {
                    if (!isSharedComplete)
                    {
                        isSharedComplete = true;
                        ToggleCompleteView(true);
                        Debug.Log("PROGRESS INDICATOR SHARED");
                        progressIndicator.ToggleProgressIndicator(true,true);
                        CustomizeManager.Instance.ToggleCustomize_P1_UI(true);
                        UIManager.Instance.SetCurrentSceneHelper(simpleSceneHelper);
                        CustomizeManager.Instance.SharedPhase = CustomizeManager.CustomizePhase.Customize_layout;

                    }
                    
                    if(isSharedComplete && isPrivateComplete)
                    {
                        //CustomizeManager.Instance.SharedPhase = CustomizeManager.CustomizePhase.Customize_layout;
                        Debug.Log("Setting the next phase appmanager");
                        AppManager.Instance.setNextPhase();
                        //CustomizeManager.Instance.ToggleCustomize_P1_UI(false);
                        CustomizeManager.Instance.ToggleCustomize_P2_UI(true);
                        CustomizeManager.Instance.SetupCustomizeLayout(isShared);
                        CustomizeManager.Instance.SetupCustomizeLayout(!isShared);
                        LoadingManager.Instance.DisableLoadingScreen();

                        isPrivateComplete = false;
                        isSharedComplete= false;

                        SetupCustomizeP2Interface();

                    }
                }
                else if (CustomizeManager.Instance.PrivatePhase == CustomizeManager.CustomizePhase.Customize_layout)
                {
                    if (!isSharedComplete)
                    {
                        isSharedComplete = true;
                        ToggleCompleteView(true);

                        Debug.Log("PROGRESS INDICATOR PRIVATE");
                        progressIndicator.ToggleProgressIndicator(true, true);
                        UIManager.Instance.SetCurrentSceneHelper(simpleSceneHelper);

                        //CustomizeManager.Instance.PrivatePhase = CustomizeManager.CustomizePhase.Customize_layout;
                        //layout manager save the current design

                        CustomizeManager.Instance.currentLayoutManager.DespawnAllRooms();
                        CustomizeManager.Instance.currentLayoutManager.FinalizeCurrentLayout();
                    }

                    if (isSharedComplete && isPrivateComplete)
                    {
                        Debug.Log("Setting the next phase appmanager");
                        AppManager.Instance.setNextPhase();
                        CustomizeManager.Instance.ToggleCustomize_P1_UI(false);
                        CustomizeManager.Instance.ToggleCustomize_P2_UI(false);
                        LoadingManager.Instance.DisableLoadingScreen();
                        Debug.Log("BOTH VIEWS COMPLETE");
                    }
                }

            }
            else
            {
                Debug.Log("CustomizeManager.Instance.PrivatePhase" + CustomizeManager.Instance.PrivatePhase);
                if (CustomizeManager.Instance.PrivatePhase == CustomizeManager.CustomizePhase.Choose_layout)
                {
                    CustomizeManager.Instance.PrivatePhase = CustomizeManager.CustomizePhase.Choose_room_layout;
                    CustomizeManager.Instance.SetupRoomLayouts(isShared);
                    finalizeChoiceBtn.gameObject.SetActive(true);
                    CustomizeManager.Instance.ToggleCustomize_P1_UI(true);
                    Debug.Log("Setting up room layouts");
                }
                else if (CustomizeManager.Instance.PrivatePhase == CustomizeManager.CustomizePhase.Choose_room_layout)
                {
                    if (!isPrivateComplete)
                    {
                        isPrivateComplete = true;
                        ToggleCompleteView(true);

                        Debug.Log("PROGRESS INDICATOR PRIVATE");
                        progressIndicator.ToggleProgressIndicator(false, true);
                        UIManager.Instance.SetCurrentSceneHelper(simpleSceneHelper);

                        CustomizeManager.Instance.PrivatePhase = CustomizeManager.CustomizePhase.Customize_layout;
                        Debug.Log("IS SHARED NOT COMPLETE->COMPLETE");
                    }

                    if (isSharedComplete && isPrivateComplete)
                    {
                        Debug.Log("Setting the next phase appmanager");
                        // CustomizeManager.Instance.PrivatePhase = CustomizeManager.CustomizePhase.Customize_layout;
                        AppManager.Instance.setNextPhase();
                        //CustomizeManager.Instance.ToggleCustomize_P1_UI(false);
                        CustomizeManager.Instance.ToggleCustomize_P2_UI(true);
                        CustomizeManager.Instance.SetupCustomizeLayout(isShared);
                        CustomizeManager.Instance.SetupCustomizeLayout(!isShared);
                        LoadingManager.Instance.DisableLoadingScreen();
                        //next previous and compare button off

                        isPrivateComplete = false;
                        isSharedComplete = false;

                        SetupCustomizeP2Interface();
                    }
                }
                else if (CustomizeManager.Instance.PrivatePhase == CustomizeManager.CustomizePhase.Customize_layout)
                {
                    if (!isPrivateComplete)
                    {
                        isPrivateComplete = true;
                        ToggleCompleteView(true);

                        Debug.Log("PROGRESS INDICATOR PRIVATE");
                        progressIndicator.ToggleProgressIndicator(false, true);
                        UIManager.Instance.SetCurrentSceneHelper(simpleSceneHelper);

                        //CustomizeManager.Instance.PrivatePhase = CustomizeManager.CustomizePhase.Customize_layout;
                        //layout manager save the current design
                        CustomizeManager.Instance.currentLayoutManager.DespawnAllRooms();
                        CustomizeManager.Instance.currentLayoutManager.FinalizeCurrentLayout();
                        Debug.Log("IS PRIVATE NOT COMPLETE->COMPLETE");
                    }

                    if (isSharedComplete && isPrivateComplete)
                    {
                        Debug.Log("Setting the next phase appmanager");
                        AppManager.Instance.setNextPhase();
                        CustomizeManager.Instance.ToggleCustomize_P1_UI(false);
                        CustomizeManager.Instance.ToggleCustomize_P2_UI(false);
                        LoadingManager.Instance.DisableLoadingScreen();
                        Debug.Log("BOTH VIEWS COMPLETE");
                    }
                }
            }

        }
        else
        {
            finalizeChoiceBtn.gameObject.SetActive(true);
        }
    }
    
    public void SetupCustomizeP2Interface()
    {

        privateView.ResetCurrentIndex();
        sharedView.ResetCurrentIndex();
        CustomizeManager.Instance.ToggleCustomize_P1_UI(true);

        progressIndicator.ToggleProgressIndicator(false,false);
        progressIndicator.ToggleProgressIndicator(true,false);


        //next previous and compare button off
        uiController.ToggleBtns(false);
        uiController.ToggleCompareModeToggle(false);

        finalizeChoiceBtn.gameObject.SetActive(true);

        sharedView.DestroyCurrentItem();
        sharedView.IsComplete = false;

        privateView.DestroyCurrentItem();
        privateView.IsComplete = false;

        currentIVew.ShowCurrentItem();
    }

    public void ShowModel()
    {
        CustomizeManager.CustomizePhase currentPhase=CustomizeManager.CustomizePhase.Choose_layout;

        if (uiController.CurrentIview == sharedView)
        {
            currentPhase = CustomizeManager.Instance.SharedPhase;
        }
        else if(uiController.CurrentIview == privateView)
        {
            currentPhase = CustomizeManager.Instance.SharedPhase;
        }

        if (currentPhase == CustomizeManager.CustomizePhase.Customize_layout)
        {
            Debug.Log("Display CUSTOMIZE LAYOUT");
            //CustomizeManager.Instance.CurrentLayoutManager.DisplayLayoutModelServerRPC();

            currentIVew.ShowCurrentItem();
        }
        else if(currentPhase == CustomizeManager.CustomizePhase.Choose_layout || currentPhase == CustomizeManager.CustomizePhase.Choose_room_layout)
        {
            currentIVew.ShowCurrentItem();
        }
    }
}

