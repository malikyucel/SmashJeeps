using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyItem : MonoBehaviour
{
    [SerializeField] private TMP_Text _lobbyNameText;
    [SerializeField] private TMP_Text _lobbyPlayersText;
    [SerializeField] private Button _joinButton;

    private LobbiesListUI _lobbiesListUI1;
    private Lobby _lobby;

    private void Awake()
    {
        _joinButton.onClick.AddListener(OnJoinButtonClicked);
    }

    public void SetupLobbyItem(LobbiesListUI lobbiesListUI, Lobby lobby)
    {
        _lobbiesListUI1 = lobbiesListUI;
        _lobby = lobby;

        _lobbyNameText.text = lobby.Name;
        _lobbyPlayersText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
    }

    private void OnJoinButtonClicked()
    {
        _lobbiesListUI1.JoinAsync(_lobby);
    }
}
