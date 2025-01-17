using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using MixedReality.Toolkit.UX;
using UnityEngine.Events;


public class HomeButtonLogic : MonoBehaviour
{
    public DialogManager dialogManager;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public async  void showHomeDialogue()
    {
        AppManager.Instance.UpdatePhase(AppManager.AppPhase.HomeDialogue);

        DialogButtonType choice= await  dialogManager.SpawnDialogWithAsync("HOME BUTTON","Would you like to return home? Any progress not saved will be lost","Yes","No");

       if (choice== DialogButtonType.Positive)
        {
            Debug.Log("Button pressed was yes " + choice);
            returnHome();
       }
       else if (choice == DialogButtonType.Negative)
        {
            Debug.Log("Button pressed was no " + choice);
            AppManager.Instance.UpdatePhase(AppManager.Instance.PreviousPhase());
           
        }


    }

    public void returnHome()
    {
        //save progress if possible
        //??????????

        //close intterface 
        //open home interface
        AppManager.Instance.UpdatePhase(AppManager.AppPhase.MainMenu);



    }
}
