using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostGameManager: IDisposable
{
    private const int MAX_CONNECTIONS = 4;

    public NetworkServer NetworkServer { get; private set; }

    private Allocation _allocation;
    private string _joinCode;
    private string _lobbyID;

    public async UniTask StartHostAsync()
    {
        try
        {
            _allocation = await RelayService.Instance.CreateAllocationAsync(MAX_CONNECTIONS);
        }
        catch (Exception exception)
        {
            Debug.LogError(exception);
            return;
        }

        try
        {
            _joinCode = await RelayService.Instance.GetJoinCodeAsync(_allocation.AllocationId);
            Debug.Log(_joinCode);
        }
        catch (Exception exception)
        {
            Debug.LogError(exception);
            return;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(AllocationUtils.ToRelayServerData(_allocation, "dtls"));

        try
        {
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions();
            createLobbyOptions.IsPrivate = false;
            createLobbyOptions.Data = new Dictionary<string, DataObject>()
            {
                {
                    "JoinCode", new DataObject
                    (
                        visibility: DataObject.VisibilityOptions.Member,
                        value: _joinCode
                    )
                }
            };

            string playerName = PlayerPrefs.GetString(Const.PlayerData.PLAYER_NAME, "NoName");

            Lobby lobby
                = await LobbyService.Instance.CreateLobbyAsync
                ($"{playerName}'s Lobby", MAX_CONNECTIONS, createLobbyOptions);

            _lobbyID = lobby.Id;

            HostSingleton.Instance.StartCoroutine(HartbeatLobby(15));
        }
        catch (LobbyServiceException lobbyServiceException)
        {
            Debug.LogError(lobbyServiceException);
            return;
        }

        NetworkServer = new NetworkServer(NetworkManager.Singleton);

        UserData userData = new UserData
        {
            UserNane = PlayerPrefs.GetString(Const.PlayerData.PLAYER_NAME, "NONAME"),
            UserAuthId = AuthenticationService.Instance.PlayerId
        };

        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        NetworkManager.Singleton.StartHost();

        NetworkManager.Singleton.SceneManager.LoadScene(Const.SceneName.CHARACTER_SELECT_SCENE, LoadSceneMode.Single);
    }

    private IEnumerator HartbeatLobby(float waitTimeSecound)
    {
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(waitTimeSecound);

        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(_lobbyID);
            yield return delay;
        }
    }

    public string GetJoinCode()
    {
        return _joinCode;
    }

    public async void ShutDown()
    {
        HostSingleton.Instance.StopCoroutine(nameof(HartbeatLobby));

        if (!string.IsNullOrEmpty(_lobbyID))
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(_lobbyID);
            }
            catch (LobbyServiceException lobbyServiceException)
            {
                Debug.Log(lobbyServiceException);
            }

            _lobbyID = string.Empty;
        }

        NetworkServer?.Dispose();
    }

    public void Dispose()
    {
        ShutDown();
    }
}
