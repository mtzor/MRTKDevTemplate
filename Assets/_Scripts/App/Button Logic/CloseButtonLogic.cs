using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Search;
using UnityEngine;

using MixedReality.Toolkit.UX;

public class CloseButtonLogic : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async void showCloseDialogue()
    {
        UIManager.Instance.Hide("MainMenu");

        DialogButtonType choice = await DialogManager.Instance.SpawnDialogWithAsync("Quit the Application", "Would you like to quit the app ?", "Quit", "Cancel");

        if(choice == DialogButtonType.Positive)
        {

            // If we are running in a standalone build of the game
            #if UNITY_STANDALONE
                    Application.Quit();
            #endif

                        // If we are running in the editor
            #if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }

        UIManager.Instance.Show("MainMenu");

    }
}
