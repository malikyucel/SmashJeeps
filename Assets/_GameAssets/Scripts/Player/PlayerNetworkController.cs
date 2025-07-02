using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkController : NetworkBehaviour
{
    [SerializeField] private CinemachineCamera _playerCamera;

    private PlayerVehicleController playerVehicleController;
    private PlayerSkillController playerSkillController;
    private PlayerInteractionController playerInteractionController;

    public override void OnNetworkSpawn()
    {
        _playerCamera.gameObject.SetActive(IsOwner);

        if (!IsOwner) { return; }

        playerVehicleController = GetComponent<PlayerVehicleController>();
        playerInteractionController = GetComponent<PlayerInteractionController>();
        playerSkillController = GetComponent<PlayerSkillController>();
    }

    public void OnPlayerRrspawn()
    {
        playerVehicleController.OnPlayerRespawn();
        playerSkillController.OnPlayerRespawn();
        playerInteractionController.OnPlayerRespawn();
    }
}
