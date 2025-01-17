using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoadingManager : MonoBehaviour
{

    private static LoadingManager _instance;

    [SerializeField] private GameObject _loadingGameObject;
    [SerializeField] private TMP_Text _loadingText;

    public static LoadingManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Find the UIManager in the scene
                _instance = FindObjectOfType<LoadingManager>();

                if (_instance == null)
                {
                    // Create a new GameObject and attach DesignManager if not found
                    GameObject loadingManagerObject = new GameObject("LoadingManager");
                    _instance = loadingManagerObject.AddComponent<LoadingManager>();
                }
            }

            return _instance;
        }
    }
   
    public void SetLoadingText(string loadingText)
    {
        _loadingText.text=loadingText;
    }

    public void EnableLoadingScreen() {
    _loadingGameObject.SetActive(true);
    }

    public void DisableLoadingScreen()
    {
        _loadingGameObject.SetActive(false);
    }


}
