using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomContainer : MonoBehaviour
{
    [SerializeField] private bool isDouble;
    [SerializeField] private Transform spawnPos;
    [SerializeField] private int rotation;

    [SerializeField] private Renderer buttonRenderer; // Reference to the button's renderer (for color change)

    private Color defaultColor;// Default button color
    [SerializeField] private Color highlightColor = Color.yellow; // Highlighted button color

    void Awake()
    {
        // Optionally, get the Renderer component if not assigned in the inspector
        if (buttonRenderer == null)
            buttonRenderer = GetComponent<Renderer>();


        defaultColor= buttonRenderer.material.color;
    }

    public void onRoomContainerBtnPressed()
    {
        // Ask the LayoutManager to close previous open menus and open the current one
        if (CustomizeManager.Instance.GetCurrentLayoutManager() != null)
        {
            Debug.Log($"SPAWN POS ROOM CONTAINER {spawnPos.position.x}, {spawnPos.position.y}, {spawnPos.position.z}");

            // Open menu and set this button as pressed
            if (CustomizeManager.Instance.isCurrentLayoutManagerShared)
            {
                CustomizeManager.Instance.SharedLayoutManager().SetLastPressedButton(this);
                CustomizeManager.Instance.SharedLayoutManager().OpenMenu(isDouble, spawnPos, rotation);
            }
            else
            {
                CustomizeManager.Instance.PrivateLayoutManager().SetLastPressedButton(this);
                CustomizeManager.Instance.PrivateLayoutManager().OpenMenu(isDouble, spawnPos, rotation);
            }
        }
        else
        {
            Debug.LogError("CustomizeManager has no current LayoutManager!");
        }
    }

    // Change button to highlight color
    public void HighlightButton()
    {
        if (buttonRenderer != null)
        {
            buttonRenderer.material.color = highlightColor;
        }
    }

    // Reset button to default color
    public void ResetButton()
    {
        if (buttonRenderer != null)
        {
            buttonRenderer.material.color = defaultColor;
        }
    }
}


