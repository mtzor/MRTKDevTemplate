using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class VisualizeViewUIController : NetworkBehaviour
{
    [SerializeField] public View view;
    [SerializeField] private RatingIndicator ratingIndicator;

    [SerializeField] private PressableButton nextItemButton;
    [SerializeField] private PressableButton previousItemButton;

    [SerializeField] private PressableButton confirmButton;

    public bool isCompareMode = false;
    public void SetView()
    {
        view.ShowCurrentItem();
    }

    private void Start()
    {
        // Hook up button and toggle events
        nextItemButton.OnClicked.AddListener(OnNextItemClicked);
        previousItemButton.OnClicked.AddListener(OnPreviousItemClicked);
        confirmButton.OnClicked.AddListener(OnConfirmButtonClicked);
    }
    public RatingIndicator RatingIndicator { get { return ratingIndicator; } }
    private void OnNextItemClicked()
    {
        if (view != null)
        {
            view.NextItem();
        }
        else { Debug.Log("CurrentView iS NULL"); }
    }

    private void OnPreviousItemClicked()
    {
        view.PreviousItem();
    }

    private void OnConfirmButtonClicked()
    {
        view.FinalizeChoice();
        confirmButton.gameObject.SetActive(false);
    }

    public void EnableConfirmButton() { confirmButton.gameObject.SetActive(true);}


}
