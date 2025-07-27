using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class FakeBoxDamage : NetworkBehaviour, IDamageable
{
    [SerializeField] private MyseryBoxSkillsSO _mysteryBoxSkill;
    [SerializeField] private GameObject _explosionParticlesPrefab;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(OwnerClientId, out var client))
        {
            NetworkObject ownerNetworkObject = client.PlayerObject;
            PlayerVehicleController playerVehicleController = ownerNetworkObject.GetComponent<PlayerVehicleController>();
            playerVehicleController.OnVehicleCrashed += PlayerVehicleController_OnVehicleCrashed;
        }
    }

    private void PlayerVehicleController_OnVehicleCrashed()
    {
        DestroyRpc(false);
    }

    public void Damage(PlayerVehicleController playerVehicleController, string playerName)
    {
        playerVehicleController.CrashVehicle();
        KillScreenUI.Instance.SetSmashedUI(playerName, _mysteryBoxSkill.SkillData.RespawnTimer);
        DestroyRpc(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out ShieldController shieldController))
        {
            DestroyRpc(true);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DestroyRpc(bool isExploded)
    {
        if (IsServer)
        {
            if (isExploded)
            {
                GameObject explosionParticlesInstance = Instantiate(_explosionParticlesPrefab, transform.position, Quaternion.identity);
                explosionParticlesInstance.GetComponent<NetworkObject>().Spawn();
            }

            Destroy(gameObject);
        }
    }
    
    public ulong GetKillerClientId()
    {
        return OwnerClientId;
    }

    public int GetResapwnTİmer()
    {
        return _mysteryBoxSkill.SkillData.RespawnTimer;
    }
    
    public int GetDamageAmount()
    {
        return _mysteryBoxSkill.SkillData.DamageAmount;
    }

    public string GetKillerName()
    {
        ulong killerClientId = GetKillerClientId();

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(killerClientId, out var killerClient))
        {
            string playerName = killerClient.PlayerObject.GetComponent<PlayerNetworkController>().PlayerName.ToString();
            return playerName;
        }

        return string.Empty;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) { return; }

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(OwnerClientId, out var client))
        {
            NetworkObject ownerNetworkObject = client.PlayerObject;
            PlayerVehicleController playerVehicleController = ownerNetworkObject.GetComponent<PlayerVehicleController>();
            playerVehicleController.OnVehicleCrashed += PlayerVehicleController_OnVehicleCrashed;
        }
    }
}
