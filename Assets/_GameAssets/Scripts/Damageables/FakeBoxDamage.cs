using System;
using Unity.Netcode;
using UnityEngine;

public class FakeBoxDamage : NetworkBehaviour, IDamageable
{
    [SerializeField] private MyseryBoxSkillsSO _mysteryBoxSkill;

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
        DestroyRpc();
    }

    public void Damage(PlayerVehicleController playerVehicleController)
    {
        playerVehicleController.CrashVehicle();
        KillScreenUI.Instance.SetSmashedUI("Malik", _mysteryBoxSkill.SkillData.RespawnTimer);
        DestroyRpc();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out ShieldController shieldController))
        {
            DestroyRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DestroyRpc()
    {
        if (IsServer)
        {
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
