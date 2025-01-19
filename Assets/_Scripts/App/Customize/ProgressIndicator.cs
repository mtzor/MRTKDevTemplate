using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressIndicator : MonoBehaviour
{

    [SerializeField] public PressableButton sharedCompleteToggle;
    [SerializeField] public PressableButton privateCompleteToggle;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void ToggleProgressIndicator(bool isShared,bool complete)
    {
        if (isShared)
        {
            sharedCompleteToggle.ForceSetToggled(complete);
        }
        else
        {
            privateCompleteToggle.ForceSetToggled(complete);
        }
    }
}
