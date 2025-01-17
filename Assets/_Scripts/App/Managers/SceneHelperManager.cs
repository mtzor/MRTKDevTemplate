using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneHelperManager : MonoBehaviour
{    
    // Array to store UI elements for each scene
    public GameObject[][] sceneUIElements;

    // Current scene index
    private int currentSceneIndex = 0;

    // Current UI element index within the scene
    private int currentUIElementIndex = 0;

    public void NextUIElement()
    {
        // Increment UI element index
        currentUIElementIndex++;

        // Wrap around if we reach the end of the UI elements for this scene
        if (currentUIElementIndex >= sceneUIElements[currentSceneIndex].Length)
        {
            currentUIElementIndex = 0;
        }

    }

    public void PreviousUIElement()
    {
        // Decrement UI element index
        currentUIElementIndex--;

        // Wrap around if we reach the beginning of the UI elements for this scene
        if (currentUIElementIndex < 0)
        {
            currentUIElementIndex = sceneUIElements[currentSceneIndex].Length - 1;
        }

    }

    public void NextScene()
    {
        // Increment scene index
        currentSceneIndex++;

        // Wrap around if we reach the end of the scenes
        if (currentSceneIndex >= sceneUIElements.Length)
        {
            currentSceneIndex = 0;
        }

        // Reset UI element index for the new scene
        currentUIElementIndex = 0;

    }

    private bool isActive = false;
    // (Optional) Helper function to update UI visibility
    private void ToggleSceneHelper()
    {
        isActive = !isActive;
        // Disable all UI elements in the current scene
        foreach (GameObject uiElement in sceneUIElements[currentSceneIndex])
        {
            uiElement.SetActive(isActive);
        }

    }

}
