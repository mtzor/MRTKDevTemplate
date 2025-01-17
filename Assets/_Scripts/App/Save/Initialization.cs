using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine.SceneManagement;

namespace App
{
    public class Initialization : MonoBehaviour
    {
        // Start is called before the first frame update
        async void Start()
        {
            await UnityServices.InitializeAsync();

            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                if (AuthenticationService.Instance.IsSignedIn)
                {
                    string username = PlayerPrefs.GetString("Username");
                    Debug.Log("Username " + username + "Signed in");

                    if (username == "")
                    {
                        username = "Player";

                        PlayerPrefs.SetString("Username", username);
                    }

                    SceneManager.LoadSceneAsync("MainMenu");
                }
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
