using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MultiplayerGameManager : NetworkBehaviour
{
    public static MultiplayerGameManager Instance { get; private set; }

    public event Action OnPlayerDataNetwrokListChange;

    [SerializeField] private List<Color> _playerColorList;

    private NetworkList<PlayerDataSerializable> _playerDataNetworkList = new NetworkList<PlayerDataSerializable>();

    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);
        _playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _playerDataNetworkList.Clear();

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectedCallback;
        }
    }

    private void OnClientDisconnectedCallback(ulong clientId)
    {
        for (int i = 0; i < _playerDataNetworkList.Count; i++)
        {
            PlayerDataSerializable playerData = _playerDataNetworkList[i];
            if (playerData.CliendId == clientId)
            {
                _playerDataNetworkList.RemoveAt(i);
            }
        }
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        for (int i = 0; i < _playerDataNetworkList.Count; i++)
        {
            if (_playerDataNetworkList[i].CliendId == clientId)
            {
                _playerDataNetworkList.RemoveAt(i);
            }
        }

        _playerDataNetworkList.Add(new PlayerDataSerializable
        {
            CliendId = clientId,
            ColorId = GetFirstUnusedColorId()
        });
    }

    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerDataSerializable> changeEvent)
    {
        OnPlayerDataNetwrokListChange?.Invoke();
    }

    public bool IsPlayerIndexConnected(int playerIndex)
    {
        return playerIndex < _playerDataNetworkList.Count;
    }

    public PlayerDataSerializable GetPlayerDataFromPlayerIndex(int playerIndex)
    {
        return _playerDataNetworkList[playerIndex];
    }

    public void ChangePlayerColor(int colorId)
    {
        ChangePlayerColorRpc(colorId);
    }

    [Rpc(SendTo.Server)]
    private void ChangePlayerColorRpc(int colorId, RpcParams rpcParams = default)
    {
        if (!IsColorAvailable(colorId))
        {
            return;
        }

        int playerDataIndex = GetPlayerDataIndexFromClientId(rpcParams.Receive.SenderClientId);
        PlayerDataSerializable palyerData = _playerDataNetworkList[playerDataIndex];
        palyerData.ColorId = colorId;
        _playerDataNetworkList[playerDataIndex] = palyerData;
    }

    private int GetPlayerDataIndexFromClientId(ulong clientId)
    {
        for (int i = 0; i < _playerDataNetworkList.Count; i++)
        {
            if (_playerDataNetworkList[i].CliendId == clientId)
            {
                return i;
            }
        }

        return -1;
    }

    public Color GetPlayerColor(int colorId)
    {
        return _playerColorList[colorId];
    }

    private int GetFirstUnusedColorId()
    {
        for (int i = 0; i < _playerColorList.Count; i++)
        {
            if (IsColorAvailable(i))
            {
                return i;
            }
        }

        return -1;
    }

    private bool IsColorAvailable(int colorId)
    {
        foreach (PlayerDataSerializable palyerData in _playerDataNetworkList)
        {
            if (palyerData.ColorId == colorId)
            {
                return false;
            }
        }

        return true;
    }

    public PlayerDataSerializable GetPlayerDataFromClientId(ulong clientId)
    {
        foreach (PlayerDataSerializable playerData in _playerDataNetworkList)
        {
            if (playerData.CliendId == clientId)
            {
                return playerData;
            }
        }

        return default;
    }

    public PlayerDataSerializable GetPlayerData()
    {
        return GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId);
    }

    public void KickPlayer(ulong clientId)
    {
        NetworkManager.Singleton.DisconnectClient(clientId);
        OnClientDisconnectedCallback(clientId);
    }

    public override void OnDestroy()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectedCallback;
    }
}
