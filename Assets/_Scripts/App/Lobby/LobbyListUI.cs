using MixedReality.Toolkit.UX;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
//using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListUI : MonoBehaviour {//class implementing  create /join lobby ui function


    public static LobbyListUI Instance { get; private set; }



    [SerializeField] private Transform lobbySingleTemplate;
    [SerializeField] private Transform container;
    [SerializeField] private PressableButton refreshButton;
    [SerializeField] private PressableButton createLobbyButton;

    public string lobbyName = "Lobby";
    private bool isPrivate=false;
    private int maxPlayers = 4;
    private LobbyManager.SessionMode sessionMode= LobbyManager.SessionMode.Customize;

    private List<string> lobbyBtns = new List<string>();

    public void SetLobbyName(string name)
    {
        lobbyName = name;
    }

    public LobbyManager.SessionMode GetSessionMode()
    {
        return sessionMode; // Getter
    }

    public void SetSessionMode(string value)
    {
        Debug.Log("String value: " + value.ToString());

        if (value == LobbyManager.SessionMode.Customize.ToString())
        {
            sessionMode = LobbyManager.SessionMode.Customize;
            createLobbyButton.gameObject.SetActive(false);
            LobbyManager.Instance.RefreshLobbyList(sessionMode);
        }
        else if (value == LobbyManager.SessionMode.Design.ToString())
        {
            sessionMode = LobbyManager.SessionMode.Design;
            lobbyBtns.Clear();
            createLobbyButton.gameObject.SetActive(true);
            LobbyManager.Instance.RefreshLobbyList(sessionMode);
        }else if (value == LobbyManager.SessionMode.Visualize.ToString())
        {
            sessionMode = LobbyManager.SessionMode.Visualize;
            lobbyBtns.Clear();
            createLobbyButton.gameObject.SetActive(false);
            LobbyManager.Instance.RefreshLobbyList(sessionMode);

        }
    }

    private void Awake() {
        
        Instance = this;
        lobbyName = "Lobby "+UnityEngine.Random.Range(1, 1000).ToString();

        lobbySingleTemplate.gameObject.SetActive(false);

        refreshButton.OnClicked.AddListener(RefreshButtonClick);
        createLobbyButton.OnClicked.AddListener(CreateLobbyButtonClick);
    }

    private void Start() {
        LobbyManager.Instance.OnLobbyListChanged += LobbyManager_OnLobbyListChanged;
        LobbyManager.Instance.OnJoinedLobby += LobbyManager_OnJoinedLobby;
        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnKickedFromLobby;
    }

    private void LobbyManager_OnKickedFromLobby(object sender, LobbyManager.LobbyEventArgs e) {
        Show();
    }

    private void LobbyManager_OnLeftLobby(object sender, EventArgs e) {
        Show();
    }

    private void LobbyManager_OnJoinedLobby(object sender, LobbyManager.LobbyEventArgs e) {
        Hide();
    }

    private void LobbyManager_OnLobbyListChanged(object sender, LobbyManager.OnLobbyListChangedEventArgs e) {
        UpdateLobbyList(e.lobbyList);
    }

    private bool lobbiesRefreshed = false;
    public bool LobbiesRefreshed { set { lobbiesRefreshed = value; } get { return lobbiesRefreshed; } }
    private void UpdateLobbyList(List<Lobby> lobbyList) {
        foreach (Transform child in container) {
            if (child == lobbySingleTemplate) continue;

            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in lobbyList) {
            Debug.Log("Lobby name :" + lobby.Name);
            Transform lobbySingleTransform = Instantiate(lobbySingleTemplate, container);
            lobbySingleTransform.gameObject.SetActive(true);
            LobbyListSingleUI lobbyListSingleUI = lobbySingleTransform.GetComponent<LobbyListSingleUI>();
            lobbyListSingleUI.UpdateLobby(lobby);

            if (lobbyBtns.Contains(lobby.Name))
            {
                lobbyBtns.Remove(lobby.Name);
            }
        }

        if (lobbyBtns.Count > 0)
        {
            foreach (string lobbyName in lobbyBtns)
            {
                CreateCustomizeLobbyButton(lobbyName);
            }
        }

            lobbiesRefreshed = true;
    }

    private void RefreshButtonClick() {
        LobbyManager.Instance.RefreshLobbyList();
    }
    public void CreateLobbyButtonClick() {
            LobbyManager.Instance.CreateLobby(
                lobbyName,
                maxPlayers,
                isPrivate,
                sessionMode
            );
            Hide();
           // LobbyCreateUI.Instance.Show();
    }

    private bool _lobbyCreated=false;
    /*
    public void CreateCustomizeLobbyButton(string name)
    {
        if (!lobbyBtns.Contains(name))
        {
            lobbyBtns.Add(lobbyName);
        }
        Debug.Log("Lobby name :" + lobbyName);
        Transform lobbySingleTransform = Instantiate(lobbySingleTemplate, container);
        lobbySingleTransform.gameObject.SetActive(true);
        LobbyListSingleUI lobbyListSingleUI = lobbySingleTransform.GetComponent<LobbyListSingleUI>();
        lobbyListSingleUI.UpdateLobby(lobbyName);
        lobbyListSingleUI.GetComponent<PressableButton>().OnClicked.AddListener(CreateLobbyButtonClick);
        lobbyListSingleUI.GetComponent<PressableButton>().OnClicked.AddListener(() => { SaveSystem.LastLobbyName = name; });

    }*/

    public void CreateCustomizeLobbyButton(string name)
    {
        if (!lobbyBtns.Contains(name)) // Ensure the name is not already added
        {
            lobbyBtns.Add(name);
        }

        Debug.Log("Lobby name: " + name);

        Transform lobbySingleTransform = Instantiate(lobbySingleTemplate, container);
        lobbySingleTransform.gameObject.SetActive(true);

        LobbyListSingleUI lobbyListSingleUI = lobbySingleTransform.GetComponent<LobbyListSingleUI>();
        lobbyListSingleUI.UpdateLobby(name);

        PressableButton button = lobbyListSingleUI.GetComponent<PressableButton>();

        // Capture the current value of `name` in a local variable
        string capturedName = name;

        button.OnClicked.AddListener(CreateLobbyButtonClick);
        button.OnClicked.AddListener(() => { SaveSystem.LastLobbyName = capturedName; });
    }
    private void Hide() {
        gameObject.SetActive(false);
    }
    private void Show() {
        gameObject.SetActive(true);
    }
}
