using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using MixedReality.Toolkit.UX;

public class LobbyListSingleUI : MonoBehaviour {// class for the UI appearance of each lobby in the create join lobby UI
    
    [SerializeField] private TMP_Text lobbyNameText;
    [SerializeField] private TMP_Text playersText;
    [SerializeField] private TMP_Text gameModeText;

    private Lobby lobby;

    public Lobby GetLobby() { return lobby; }

    private void Awake() {

        //if (lobby!=null)
        //{
            GetComponent<PressableButton>().OnClicked.AddListener(() => { JoinLobby(); });
        //}
    
    }

    public void UpdateLobby(Lobby lobby) {
        this.lobby = lobby;

        lobbyNameText.text = lobby.Name;
        playersText.text = "<size=6><alpha=#88>"+lobby.Players.Count + "/" + lobby.MaxPlayers+ " </size>";
    }

    public void UpdateLobby(string lobbyName)
    {
        lobbyNameText.text = lobbyName;
        playersText.text = "<size=6><alpha=#88>" + 0 + "/" + 4 + " </size>";
    }

    public void JoinLobby()
    { 
        if (lobby!=null)
        {
            LobbyManager.Instance.JoinLobby(lobby);
        }
    }
}