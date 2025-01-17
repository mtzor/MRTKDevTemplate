using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using MixedReality.Toolkit.UX;
using UnityEngine.Events;

public class DialogManager : MonoBehaviour
{
    public DialogPool DialogPool;

    private static DialogManager _instance;

    protected virtual void Awake()
    {
        if (DialogPool == null)
        {
            DialogPool = GetComponent<DialogPool>();
        }
    }
    public static DialogManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Find the UIManager in the scene
                _instance = FindObjectOfType<DialogManager>();

                if (_instance == null)
                {
                    // Create a new GameObject and attach UIManager if not found
                    GameObject dialogueManagerObject = new GameObject("DialogManager");
                    _instance = dialogueManagerObject.AddComponent<DialogManager>();
                }
            }

            return _instance;
        }
    }
    // Start is called before the first frame update
    public void SpawnNeutralDialogFromCode(string Header, string Body)
    {
        IDialog dialog = DialogPool.Get()
            .SetHeader(Header)
            .SetBody(Body);
            
        dialog.Show();
    }
    public void ModifylDialogFromCode(string Header, string Body)
    {
        IDialog dialog = DialogPool.Get()
            .SetHeader(Header)
            .SetBody(Body);

    }

    public Task<DialogButtonType> SpawnDialogWithAsync(string Header, string Body, string Neutral)
    {
        return ShowAsyncDialog(Header, Body, Neutral);
    }

    public Task<DialogButtonType> SpawnDialogWithAsync(string Header, string Body, string Positive, string Negative)
    {
        return ShowAsyncDialog(Header, Body, Positive, Negative);
    }

    private async Task<DialogButtonType> ShowAsyncDialog(string Header, string Body, string Neutral)
    {

          // Build and show the dialog.
        DialogDismissedEventArgs result = await DialogPool.Get()
            .SetHeader(Header)
            .SetBody(Body)
            .SetNeutral(Neutral)
            .ShowAsync();
        
        Debug.Log("Async dialog says " + result.Choice?.ButtonText);
        return result.Choice.ButtonType;
    }


    private async Task<DialogButtonType> ShowAsyncDialog(string Header, string Body, string Positive, string Negative)
    {

        // Build and show the dialog.
        DialogDismissedEventArgs result = await DialogPool.Get()
            .SetHeader(Header)
            .SetBody(Body)
            .SetPositive(Positive)
            .SetNegative(Negative)
            .ShowAsync();
        
        Debug.Log("Async dialog says " + result.Choice.ButtonType);
        return result.Choice.ButtonType;
    }

}
