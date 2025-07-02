using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerInteractionController : NetworkBehaviour
{
    private PlayerSkillController _playerSkillController;
    private PlayerVehicleController _playerVehicleController;

    private bool _isCrashed;
    private bool _isShieldActive;
    private bool _isSpikeActive;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }

        _playerSkillController = GetComponent<PlayerSkillController>();
        _playerVehicleController = GetComponent<PlayerVehicleController>();

        _playerVehicleController.OnVehicleCrashed += PlayerVehicleController_OnVehicleCrashed;
    }

    private void PlayerVehicleController_OnVehicleCrashed()
    {
        enabled = false;
        _isCrashed = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckCollision(other);
    }

    private void OnTriggerStay(Collider other)
    {
        CheckCollision(other);
    }

    private void CheckCollision(Collider other)
    {
        if (!IsOwner) { return; }
        if (_isCrashed) { return; }

        CheckCollectibleCollision(other);
        ChackDamageableCollision(other);
    }

    private void CheckCollectibleCollision(Collider other)
    {
        if (other.gameObject.TryGetComponent(out Collectibles collectibles))
        {
            collectibles.Collect(_playerSkillController);
        }
    }

    private void ChackDamageableCollision(Collider other)
    {
        if (other.gameObject.TryGetComponent(out IDamageable damageable))
        {
            if (_isShieldActive)
            {
                Debug.Log("Sheald Active: Damage Blocked");
                return;
            }

            CrashTheVehicle(damageable);
        }
    }

    private void CrashTheVehicle(IDamageable damageable)
    {
        damageable.Damage(_playerVehicleController);
        SetSkillerUIRpc(damageable.GetKillerClientId(),
            RpcTarget.Single(damageable.GetKillerClientId(), RpcTargetUse.Temp));
        SpawnerMnagager.Instance.RespawnPlaer(damageable.GetResapwnTİmer(), OwnerClientId);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void SetSkillerUIRpc(ulong killerClientId, RpcParams rpcParams)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(killerClientId, out var killerCliend))
        {
            KillScreenUI.Instance.SetSmashUI("Malik");
        }
    }

    public void OnPlayerRespawn()
    {
        enabled = true;
        _isCrashed = false;
    }

    public void SetShiledlActive(bool active) => _isShieldActive = active;

    public void SetSpikelActive(bool active) => _isSpikeActive = active;
}
