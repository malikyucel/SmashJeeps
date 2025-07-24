using System;
using TMPro;
using Unity.Cinemachine;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkController : NetworkBehaviour
{
    public static event Action<PlayerNetworkController> OnPlayerSapwned;
    public static event Action<PlayerNetworkController> OnPlayerDesapwned;

    [SerializeField] private CinemachineCamera _playerCamera;
    [SerializeField] private TMP_Text _palyerNameText;
    [SerializeField] private PlayerScoreController _palyerScoreController;

    private PlayerVehicleController _playerVehicleController;
    private PlayerSkillController _playerSkillController;
    private PlayerInteractionController _playerInteractionController;

    public NetworkVariable<FixedString32Bytes> PlayerName = new();

    public override void OnNetworkSpawn()
    {
        _playerCamera.gameObject.SetActive(IsOwner);

        if (IsServer)
        {
            UserData userData = HostSingleton.Instance.HostGameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            PlayerName.Value = userData.UserNane;
            SetPlayerNameRpc();

            OnPlayerSapwned?.Invoke(this);
        }

        if (!IsOwner) { return; }

        _playerVehicleController = GetComponent<PlayerVehicleController>();
        _playerInteractionController = GetComponent<PlayerInteractionController>();
        _playerSkillController = GetComponent<PlayerSkillController>();
    }

    public void OnPlayerRrspawn()
    {
        _playerVehicleController.OnPlayerRespawn();
        _playerSkillController.OnPlayerRespawn();
        _playerInteractionController.OnPlayerRespawn();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetPlayerNameRpc()
    {
        _palyerNameText.text = PlayerName.Value.ToString();
    }

    public PlayerScoreController GetPlayerScoreController()
    {
        return _palyerScoreController;
    }

    public override void OnNetworkDespawn()
    {
        if (IsSpawned)
        {
            OnPlayerDesapwned?.Invoke(this);
        }
    }
}
