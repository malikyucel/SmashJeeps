using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkServer : IDisposable
{
    private NetworkManager _networkManager;

    private Dictionary<ulong, string> _clientIdToAuthDictionary = new Dictionary<ulong, string>();
    private Dictionary<string, UserData> _authIdToUserDataDictionary = new Dictionary<string, UserData>();

    public NetworkServer(NetworkManager networkManager)
    {
        _networkManager = networkManager;

        networkManager.ConnectionApprovalCallback += ApprovalChack;
        networkManager.OnServerStarted += OnServerReady;
    }

    private void OnServerReady()
    {
        _networkManager.OnClientDisconnectCallback += OnColientDisconmectCallbakc;
    }

    private void OnColientDisconmectCallbakc(ulong clientId)
    {
        if (_clientIdToAuthDictionary.TryGetValue(clientId, out string authId))
        {
            _clientIdToAuthDictionary.Remove(clientId);
            _authIdToUserDataDictionary.Remove(authId);
        }
    }

    private void ApprovalChack(NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response)
    {
        string payload = System.Text.Encoding.UTF8.GetString(request.Payload);
        UserData userData = JsonUtility.FromJson<UserData>(payload);

        _clientIdToAuthDictionary[request.ClientNetworkId] = userData.UserAuthId;
        _authIdToUserDataDictionary[userData.UserAuthId] = userData;

        response.Approved = true;
        response.CreatePlayerObject = true;
    }

    public void Dispose()
    {
        if (_networkManager == null) { return; }

        _networkManager.ConnectionApprovalCallback -= ApprovalChack;
        _networkManager.OnServerStarted -= OnServerReady;
        _networkManager.OnClientDisconnectCallback += OnColientDisconmectCallbakc;

        if (_networkManager.IsListening)
        {
            _networkManager.Shutdown();
        }
    }
}

[Serializable]
public class UserData
{
    public string UserMane;
    public string UserAuthId;
}
