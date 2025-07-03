using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class SpawnerMnagager : NetworkBehaviour
{
    public static SpawnerMnagager Instance { get; private set; }

    [Header("Player Prefab")]
    [SerializeField] private GameObject _playerPrefab;

    [Header("Transform List")]
    [SerializeField] private List<Transform> _spawnPointTransformList;
    [SerializeField] private List<Transform> _respawnPointTransfomList;

    private List<int> _availableSpawnIndexList = new();
    private List<int> _avaliableRespawnIndexList = new();

    private void Awake()
    {
        Instance = this;    
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) { return; }

        for (int i = 0; i < _spawnPointTransformList.Count; i++)
        {
            _availableSpawnIndexList.Add(i);
        }

        for (int i = 0; i < _respawnPointTransfomList.Count; i++)
        {
            _avaliableRespawnIndexList.Add(i);
        }

        NetworkManager.OnClientConnectedCallback += SpawnPlayer;
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (_availableSpawnIndexList.Count == 0)
        {
            Debug.LogError("No available Spawn Points!");
            return;
        }

        int randumIndex = Random.Range(0, _availableSpawnIndexList.Count);
        int spawnIndex = _availableSpawnIndexList[randumIndex];
        _availableSpawnIndexList.RemoveAt(randumIndex);

        Transform spawnPointTransform = _spawnPointTransformList[spawnIndex];
        GameObject playerInstance = Instantiate(_playerPrefab,
            spawnPointTransform.position, spawnPointTransform.rotation);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }

    public void RespawnPlaer(int respawnTimer, ulong clientId)
    {
        StartCoroutine(RespawnPlayerCoroutine(respawnTimer, clientId));
    }

    private IEnumerator RespawnPlayerCoroutine(int respawnTime, ulong clientId)
    {
        yield return new WaitForSeconds(respawnTime);
        
        if(GameManager.Instance.GetGameState() != GaemState.Playing) { yield break; }

        if (_spawnPointTransformList.Count == 0)
        {
            Debug.LogError("No available Respawn Points!");
            yield break;
        }

        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            Debug.LogError($"Client {clientId} not found");
            yield break;
        }

        if (_avaliableRespawnIndexList.Count == 0)
        {
            for (int i = 0; i < _respawnPointTransfomList.Count; i++)
            {
                _avaliableRespawnIndexList.Add(i);
            }
        }

        int randumIndex = Random.Range(0, _avaliableRespawnIndexList.Count);
        int spawnIndex = _avaliableRespawnIndexList[randumIndex];
        _avaliableRespawnIndexList.RemoveAt(randumIndex);

        Transform respawnPointTransform = _respawnPointTransfomList[spawnIndex];
        NetworkObject palyernetworkObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;

        if (palyernetworkObject == null)
        {
            Debug.LogError("Player Network Object is Null!");
            yield break;
        }

        if (palyernetworkObject.TryGetComponent<Rigidbody>(out var palyerRigidbody))
        {
            palyerRigidbody.isKinematic = true;
        }

        if (palyernetworkObject.TryGetComponent<NetworkTransform>(out var _palyernetwrokTransfom))
        {
            _palyernetwrokTransfom.Interpolate = false;
            _palyernetwrokTransfom.GetComponent<PlayerVehicleVisualController>().SetVehicleVisualActive(0.1f);

            palyernetworkObject.transform.SetPositionAndRotation(respawnPointTransform.position, respawnPointTransform.rotation);
        }

        yield return new WaitForSeconds(0.1f);

        palyerRigidbody.isKinematic = false;
        _palyernetwrokTransfom.Interpolate = true;

        if (palyernetworkObject.TryGetComponent<PlayerNetworkController>(out var playerNetworkController))
        {
            playerNetworkController.OnPlayerRrspawn();
        }
    }
}
