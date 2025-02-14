using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Threading.Tasks;
//using Unity.VisualScripting;
using System;

public class PrivateView :MonoBehaviour, IView
{
    private List<Transform> items; // Private list of items
    private int currentIndex;

    private int compareIndex;
    private Transform compareItem;
    private Transform compareItemInstance;

    private Transform currentItem;
    private int selectedIndex;

    private Transform layoutContainer;

    public Transform GetLayoutContainer() { return layoutContainer; }

    public void Initialize(List<Transform> privateItems, Transform layoutTransform)
    {
        items = privateItems;
        layoutContainer = layoutTransform;
        selectedIndex = -1;
        currentIndex = 0;
        compareIndex = 0;
        IsInCompareMode = false;
        IsComplete = false;
    }
    public List<ulong> SharedClients()
    {
        return null;
    }
    public bool IsComplete { get; set; } //complete view flag
    public bool IsShared => false; // Private view flag
    public bool IsInCompareMode { get; set; }
    public Transform GetCurrentItem() {  return currentItem; }

    public void ResetCurrentIndex() { currentIndex = 0; }
    public void SetItems(List<Transform> privateItems) 
    { 
        items=privateItems;
        Debug.Log("setting items in private view :" + items[0]);
    }
    public void NextItem()
    {
        currentIndex = (currentIndex + 1) % items.Count; // Circular navigation
        Debug.Log("Next item"+currentIndex);
        ShowCurrentItem();
    }

    public void PreviousItem()
    {
        currentIndex = (currentIndex - 1 + items.Count) % items.Count; // Circular navigation
        Debug.Log("previous item"+currentIndex);
        ShowCurrentItem();
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

            if (CustomizeManager.Instance.PrivatePhase == CustomizeManager.CustomizePhase.Customize_layout)
            {
                CustomizeManager.Instance.SharedLayoutManager().DespawnAllRooms();
            }
        }

        // Destroy compare item if compare mode is toggled off
        if (!IsInCompareMode && compareItemInstance != null)
        {
            Destroy(compareItemInstance.gameObject);
            compareItemInstance = null;
        }

        
        Transform item = Instantiate(items[currentIndex], layoutContainer);
        currentItem = item;

        if (CustomizeManager.Instance.PrivatePhase == CustomizeManager.CustomizePhase.Customize_layout)
        {
            if (CustomizeManager.Instance.isCurrentLayoutManagerShared)
            {
                CustomizeManager.Instance.SharedLayoutManager().RespawnAllRooms();
            }
            else
            {
                CustomizeManager.Instance.PrivateLayoutManager().RespawnAllRooms();
            }
        }

        if (CustomizeManager.Instance.PrivatePhase == CustomizeManager.CustomizePhase.Customize_layout)
        {
            currentItem.transform.localPosition = new Vector3(0.51f, 0.1f, 0.394f);
            currentItem.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            currentItem.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        }

        Debug.Log("Current Item" + currentItem);

        if (IsInCompareMode)
        {
            Debug.Log("Compare Mode Enabled");

            // Ensure compareIndex is valid
            compareIndex = Mathf.Clamp(compareIndex, 0, items.Count - 1);

            // Destroy the previous compare item if it exists
            if (compareItemInstance != null)
            {
                Destroy(compareItemInstance.gameObject);
            }

            // Instantiate the compare item
            compareItemInstance = Instantiate(compareItem, layoutContainer);

            // Positioning
            float offset = 0.10f; // Adjust for desired spacing
            currentItem.localPosition = currentItem.localPosition+ new Vector3(offset, 0, 0); // Move slightly to the right
            compareItemInstance.localPosition = compareItemInstance.localPosition + new Vector3(-offset, 0, 0); // Move symmetrically to the left

            Debug.Log($"Compare mode displaying: {currentItem.name} (right) and {compareItem.name} (left)");
        }
    }

    public void CompareViewConvert()
    {
        if (IsInCompareMode)
        {
            // Entering compare mode
            compareIndex = currentIndex; // Set compareIndex to the current item index
            compareItem=items[compareIndex];
            items.RemoveAt(compareIndex);
            currentIndex=currentIndex%items.Count;
        }
        else
        {
            // Exiting compare mode, clean up compare item
            if (compareItemInstance != null)
            {
                items.Insert(compareIndex,compareItem);
                Destroy(compareItemInstance.gameObject);
                compareItemInstance = null;
            }
        }

        // Refresh the view
        ShowCurrentItem();
    }
    public async Task FinalizeChoice()
    {
        if (CustomizeManager.Instance.PrivatePhase == CustomizeManager.CustomizePhase.Customize_layout)
        {
            if (CustomizeManager.Instance.isCurrentLayoutManagerShared)
            {
                CustomizeManager.Instance.SharedLayoutManager().DespawnAllRooms();
            }
            else
            {
                CustomizeManager.Instance.PrivateLayoutManager().DespawnAllRooms();
            }
            Destroy(currentItem.gameObject);
        }
        if (currentItem != null)
        {
            Destroy(currentItem.gameObject);
        }

        DialogButtonType answer = await DialogManager.Instance.SpawnDialogWithAsync("Layout selected!", "Would you like to confirm your choice ?", "YES", "NO");

        if (answer == DialogButtonType.Positive)
        {
            Debug.Log("POSITIVE");
            selectedIndex = currentIndex;

            CustomizeManager.Instance.SetChoice(false,selectedIndex);

            //CustomizeManager.Instance.SetupInterface(false);//setting next phase for private view

            ViewManager.Instance.SetNextCurrentViewPhase();//calling next phase for this view
        }
        else if (answer == DialogButtonType.Negative)
        {
            if (CustomizeManager.Instance.PrivatePhase == CustomizeManager.CustomizePhase.Customize_layout)
            {
                ShowCurrentItem();
                if (CustomizeManager.Instance.isCurrentLayoutManagerShared)
                {
                    CustomizeManager.Instance.SharedLayoutManager().RespawnAllRooms();
                }
                else
                {
                    CustomizeManager.Instance.PrivateLayoutManager().RespawnAllRooms();
                }
            }
            Debug.Log("NEGATIVE");
            ViewManager.Instance.finalizeChoiceBtn.gameObject.SetActive(true);
            selectedIndex = -1;
            ShowCurrentItem();
        }
    }
    public void SetSelectedIndex(int value)
    {
            selectedIndex= value;
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
}

