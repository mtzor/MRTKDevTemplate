using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MixedReality.Toolkit.UX;
using TMPro;
using Unity.Netcode;



public class ViewUIController : NetworkBehaviour
{
    [SerializeField] public GameObject compareViewUI;
    [SerializeField] public PressableButton compareModeToggle;
    [SerializeField] public TMP_Text compareViewText;

    [SerializeField] private PressableButton nextItemButton;
    [SerializeField] private PressableButton previousItemButton;


    private GameObject currentView;
    private IView currentIView;

    private bool isCurrentShared = false;
    public bool isCompareMode=false;

    public void Initialize( IView initialIVew)
    {
        SetView(initialIVew);
    }

    public IView CurrentIview{ set; get; }
    public void SetView( IView ivew)
    {
        currentIView = ivew;
        ivew.ShowCurrentItem();
        isCurrentShared = ivew.IsShared;

        ToggleInterfaceBTNS(ivew);

    }

    private void Start()
    {
        // Hook up button and toggle events
        nextItemButton.OnClicked.AddListener(OnNextItemClicked);
        previousItemButton.OnClicked.AddListener(OnPreviousItemClicked);
        compareModeToggle.OnClicked.AddListener(OnCompareModeToggled);
    }
    public void ToggleCompareModeToggle(bool active)
    {
        compareViewUI.SetActive(active);
    }
    public void ToggleBtns(bool active)
    {
        nextItemButton.gameObject.SetActive(active);

        previousItemButton.gameObject.SetActive(active);
    }
    private void OnNextItemClicked()
    {   
        if (currentIView != null)
        {
            currentIView.NextItem();

            Debug.Log("NEXT ITEM PRESSED");
        }
        else { Debug.Log("CurrentView iS NULL"); }
    }

    private void OnPreviousItemClicked()
    {
        currentIView.PreviousItem();
        Debug.Log("PREVIOUS ITEM PRESSED");
    }

    public void OnCompareModeToggled()
    {
        isCompareMode=!isCompareMode;//toggling bool

        currentIView.IsInCompareMode = isCompareMode;

        if (isCompareMode)
        {
            compareViewText.text="Compare View On.";
        }
        else
        {
            compareViewText.text = "Compare View Off.";
        }

        currentIView.CompareViewConvert();
    }

    public void ToggleInterfaceBTNS(IView iview)
    {
        if (iview.IsShared)
        {
            Debug.Log("SHARED VIEW PHASE "+ CustomizeManager.Instance.SharedPhase);
            if (CustomizeManager.Instance.SharedPhase == CustomizeManager.CustomizePhase.Choose_layout || CustomizeManager.Instance.SharedPhase == CustomizeManager.CustomizePhase.Choose_room_layout)
            {
                previousItemButton.gameObject.SetActive(true);
                nextItemButton.gameObject.SetActive(true);
                ToggleCompareModeToggle(false);
            }
            else
            {
                Debug.Log("Toggle btns SHARED ELSE");
                //spawn all items of the view
                previousItemButton.gameObject.SetActive(false);
                nextItemButton.gameObject.SetActive(false);
                ToggleCompareModeToggle(false);
            }
        }
        else if (!iview.IsShared) 
        {

            Debug.Log("PRIVATE VIEW PHASE " + CustomizeManager.Instance.PrivatePhase);
            if (CustomizeManager.Instance.PrivatePhase == CustomizeManager.CustomizePhase.Choose_layout || CustomizeManager.Instance.PrivatePhase == CustomizeManager.CustomizePhase.Choose_room_layout)
            {
                previousItemButton.gameObject.SetActive(true);
                nextItemButton.gameObject.SetActive(true);
                ToggleCompareModeToggle(true);
            }
            else
            {

                Debug.Log("Toggle btns PRIVATE ELSE");
                //spawn all items of the view
                previousItemButton.gameObject.SetActive(false);
                nextItemButton.gameObject.SetActive(false);
                ToggleCompareModeToggle(false);
            }

        }
    }
}
