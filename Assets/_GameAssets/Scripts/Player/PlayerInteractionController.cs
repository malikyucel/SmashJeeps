using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerInteractionController : NetworkBehaviour
{
    [SerializeField] private CameraShader _cameraShake;

    private PlayerSkillController _playerSkillController;
    private PlayerVehicleController _playerVehicleController;
    private PlayerHealthController _playerHealthController;
    private PlayerNetworkController _playerNetworkController;

    private bool _isCrashed;
    private bool _isShieldActive;
    private bool _isSpikeActive;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }

        _playerSkillController = GetComponent<PlayerSkillController>();
        _playerVehicleController = GetComponent<PlayerVehicleController>();
        _playerHealthController = GetComponent<PlayerHealthController>();
        _playerNetworkController = GetComponent<PlayerNetworkController>();

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
        if (GameManager.Instance.GetGameState() != GaemState.Playing) return;

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
        var playerName = _playerNetworkController.PlayerName.Value;

        _cameraShake.ShakeCamera(3f, 0.8f);
        damageable.Damage(_playerVehicleController, damageable.GetKillerName());
        _playerHealthController.TakeDamage(damageable.GetDamageAmount());
        SetSkillerUIRpc(damageable.GetKillerClientId(), playerName.ToString(),
            RpcTarget.Single(damageable.GetKillerClientId(), RpcTargetUse.Temp));
        SpawnerMnagager.Instance.RespawnPlaer(damageable.GetResapwnTİmer(), OwnerClientId);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void SetSkillerUIRpc(ulong killerClientId, FixedString32Bytes playerName, RpcParams rpcParams)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(killerClientId, out var killerCliend))
        {
            KillScreenUI.Instance.SetSmashUI(playerName.ToString());
            killerCliend.PlayerObject.GetComponent<PlayerScoreController>().AddScore(1);
        }
    }

    public void OnPlayerRespawn()
    {
        enabled = true;
        _isCrashed = false;
        _playerHealthController.RestartHealth();
    }

    public void SetShiledlActive(bool active) => _isShieldActive = active;

    public void SetSpikelActive(bool active) => _isSpikeActive = active;
}
