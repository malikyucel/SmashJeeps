using System;
using System.Collections.Generic;
using Unity.Netcode;

public class CharacterSelectReady : NetworkBehaviour
{
    public static CharacterSelectReady Instance { get; private set; }

    public event Action OnReadyChanged;
    public event Action OnUnreadyChanged;
    public event Action OnAllPlayersReady;


    private Dictionary<ulong, bool> _palyerReadyDictionary = new Dictionary<ulong, bool>();

    private void Awake()
    {
        Instance = this;        
    }

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
    }

    private void OnClientDisconnectCallback(ulong clientId)
    {
        if (_palyerReadyDictionary.ContainsKey(clientId))
        {
            _palyerReadyDictionary.Remove(clientId);
            OnReadyChanged?.Invoke();
        }
    }

    private void OnClientConnectedCallback(ulong connectedClientId)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (IsPlayerReady(clientId))
            {
                SetPlayerReadyToAllRpc(clientId);
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void SetPlayerReadyRpc(RpcParams rpcParams = default)
    {
        SetPlayerReadyToAllRpc(rpcParams.Receive.SenderClientId);
        _palyerReadyDictionary[rpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!_palyerReadyDictionary.ContainsKey(clientId) || !_palyerReadyDictionary[clientId])
            {
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady)
        {
            OnAllPlayersReady?.Invoke();
        }
    }

    [Rpc(SendTo.Server)]
    private void SetPlayerUnreadyRpc(RpcParams rpcParams = default)
    {
        SetPlayerUnReadyToAllRpc(rpcParams.Receive.SenderClientId);

        if (_palyerReadyDictionary.ContainsKey(rpcParams.Receive.SenderClientId))
        {
            _palyerReadyDictionary[rpcParams.Receive.SenderClientId] = false;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetPlayerReadyToAllRpc(ulong clientId)
    {
        _palyerReadyDictionary[clientId] = true;
        OnReadyChanged?.Invoke();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetPlayerUnReadyToAllRpc(ulong clientId)
    {
        _palyerReadyDictionary[clientId] = false;
        OnReadyChanged?.Invoke();
        OnUnreadyChanged?.Invoke();
    }

    public bool IsPlayerReady(ulong clientId)
    {
        return _palyerReadyDictionary.ContainsKey(clientId) && _palyerReadyDictionary[clientId];
    }

    public bool AreAllPlayersReady()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!_palyerReadyDictionary.ContainsKey(clientId) || !_palyerReadyDictionary[clientId])
            {
                return false;
            }
        }

        return true;
    }

    public void SetPlayerReady()
    {
        SetPlayerReadyRpc();
    }

    public void SetPlayerUnready()
    {
        SetPlayerUnreadyRpc();
    }
}
