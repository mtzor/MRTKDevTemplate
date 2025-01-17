using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using MixedReality.Toolkit.UX;

public class LobbyPlayerSingleUI : MonoBehaviour {//class representing a lobby paricipant in Lobby UI


    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text playerTypeText;
    [SerializeField] private PressableButton kickPlayerButton;


    private Player player;


    private void Awake() {
        kickPlayerButton.OnClicked.AddListener(KickPlayer);
    }

    public void SetKickPlayerButtonVisible(bool visible) {
        kickPlayerButton.gameObject.SetActive(visible);
    }

    public void UpdatePlayer(Player player) {
        this.player = player;
        playerNameText.text = player.Data[LobbyManager.PLAYER_NAME_KEY].Value;
        playerTypeText.text = "<size=6><alpha=#88>"+ player.Data[LobbyManager.KEY_PLAYER_TYPE].Value+ "</size>";
    }

    private void KickPlayer() {
        if (player != null) {
            LobbyManager.Instance.KickPlayer(player.Id);
        }
    }


}