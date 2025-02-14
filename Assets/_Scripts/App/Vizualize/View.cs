using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Threading.Tasks;
//using Unity.VisualScripting;
using System;

public class View : MonoBehaviour, IView
{
    public List<Transform> items; // Private list of items
    private int currentIndex;
    private int currentRating;

    public Transform currentItem;

    public int selectedIndex;


    [SerializeField] private Transform layoutContainer;

    public Transform GetLayoutContainer() { return layoutContainer; }

    public View(List<Transform> items, Transform layoutTransform)
    {
        items = items;
        layoutContainer = layoutTransform;
        selectedIndex = -1;
        currentIndex = 0;
        IsInCompareMode = false;
        IsComplete = false;
        VisualizeManager.Instance.SetSpawnedRoomData(currentIndex);
    }
    public void Start()
    {
        selectedIndex = -1;
        currentIndex = 0;
        IsInCompareMode = false;
        IsComplete = false;
    }
    public void Setup(List<Transform> items)
    {
        Debug.Log("GIVEN ITEMS LENGTH" + items.Count);
        this.items = items;
        VisualizeManager.Instance.SetSpawnedRoomData(currentIndex);
        ToggleNextPrevBtns();

    }
    public List<ulong> SharedClients()
    {
        return null;
    }
    public bool IsComplete { get; set; } //complete view flag
    public bool IsShared => false; // Private view flag
    public bool IsInCompareMode { get; set; }
    public Transform GetCurrentItem() { return currentItem; }

    public void ResetCurrentIndex() { currentIndex = 0; }
    public void SetItems(List<Transform> setItems)
    {
        items = setItems;
        Debug.Log("setting items in private view :" + items[0]);
    }
    public void NextItem()
    {
        if (currentIndex < items.Count - 1)
        {
            currentIndex = (currentIndex + 1);
        }
        Debug.Log("Next item" + currentIndex);
        VisualizeManager.Instance.SetSpawnedRoomData(currentIndex);
        ShowCurrentItem();
        ToggleNextPrevBtns();
    }

    public void PreviousItem()
    {
        if (currentIndex > 0)
        {
            currentIndex = (currentIndex - 1);
        }
        Debug.Log("previous item" + currentIndex);
        VisualizeManager.Instance.SetSpawnedRoomData(currentIndex);
        ShowCurrentItem();
        ToggleNextPrevBtns();

    }

    public void ToggleNextPrevBtns()
    {
        if (currentIndex == 0)
        {
            VisualizeManager.Instance.UIController().TogglePreviousBtn(false);
            VisualizeManager.Instance.UIController().ToggleNextBtn(true);
        }
        else if (currentIndex > 0 && currentIndex < items.Count - 1) {
            VisualizeManager.Instance.UIController().TogglePreviousBtn(true);
            VisualizeManager.Instance.UIController().ToggleNextBtn(true);
        }
        else
        {
            VisualizeManager.Instance.UIController().TogglePreviousBtn(true);
            VisualizeManager.Instance.UIController().ToggleNextBtn(false);
        }
    }
    public void DestroyCurrentItem()
    {
        if (layoutContainer == null)
        {
            Debug.LogError("LayoutContainer is not assigned. Cannot display items.");
            return;
        }

        // Destroy previously instantiated current item
        if (currentItem != null)
        {
            Destroy(currentItem.gameObject);
        }

    }
    public void ShowCurrentItem()
    {
        if (layoutContainer == null)
        {
            Debug.LogError("LayoutContainer is not assigned. Cannot display items.");
            return;
        }

        // Destroy previously instantiated current item
        if (currentItem != null)
        {
            Destroy(currentItem.gameObject);                       
            VisualizeManager.Instance.DespawnAllRooms();           
        }

        

        Debug.Log("CURRENTINDEX" + currentIndex);

        currentItem = Instantiate(items[currentIndex], layoutContainer);   
        VisualizeManager.Instance.SetSpawnedRoomData(currentIndex);
        VisualizeManager.Instance.RespawnAllRooms();

        VisualizeManager.Instance.SetRating(VisualizeManager.Instance.ratings[currentIndex]);
       

        if (VisualizeManager.Instance.CurrentPhase == VisualizeManager.VizualizePhase.Vizualize_layout)
        {
            currentItem.transform.localPosition = new Vector3(0.51f, 0.1f, 0.394f);
            currentItem.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            currentItem.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        }

        Debug.Log("Current Item" + currentItem);

    }

    public void CompareViewConvert()
    {
       
    }



    public async Task FinalizeChoice()
    {
        VisualizeManager.Instance.DespawnAllRooms();
        Destroy(currentItem.gameObject);

        DialogButtonType answer = await DialogManager.Instance.SpawnDialogWithAsync("Designs Rated!", "Would you like to submit your ratings ?", "YES", "NO");

        if (answer == DialogButtonType.Positive)
        {
            Debug.Log("POSITIVE");
            selectedIndex = currentIndex;

            VisualizeManager.Instance.SubmitRatings();

        }
        else if (answer == DialogButtonType.Negative)
        {
            ShowCurrentItem();
            VisualizeManager.Instance.RespawnAllRooms();
        }
         Debug.Log("NEGATIVE");
         VisualizeManager.Instance.UIController().EnableConfirmButton();
         selectedIndex = -1;
        
    }

    public void SetSelectedIndex(int value)
    {
        selectedIndex = value;
    }
    public int SelectedIndex()
    {
        return selectedIndex;
    }

    public void ReportSharedViewState(bool state)
    {

    }
    public void SetSharedItemsForClients(int index)
    {
    }

    public void RateCurrentDesign(int rating) 
    {

        VisualizeManager.Instance.ratings[currentIndex] = rating;
    }
}

