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
            LoadingManager.Instance.DisableLoadingScreen();
#if UNITY_EDITOR
            // If running in the Unity Editor, stop playing the scene
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
        else
        {

            UIManager.Instance.Show("MainMenu");
        }

    }
}
