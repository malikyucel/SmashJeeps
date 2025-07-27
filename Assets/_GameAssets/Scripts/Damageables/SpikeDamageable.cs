using Unity.Netcode;
using UnityEngine;

public class SpikeDamageable : NetworkBehaviour, IDamageable
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
        DestroyRpc();
    }

    public void Damage(PlayerVehicleController playerVehicleController, string playerName)
    {
        playerVehicleController.CrashVehicle();
        PlayerParticalesRpc(playerVehicleController.transform.position);
        KillScreenUI.Instance.SetSmashedUI(playerName, _mysteryBoxSkill.SkillData.RespawnTimer);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DestroyRpc()
    {
        if (IsServer)
        {
            Destroy(gameObject);
        }
    }

    [Rpc(SendTo.Server)]
    private void PlayerParticalesRpc(Vector3 vehiclePosition = default)
    {
        if(!IsServer) { return; }

        GameObject explosionParticlesInstance = Instantiate(_explosionParticlesPrefab, vehiclePosition, Quaternion.identity);
        explosionParticlesInstance.GetComponent<NetworkObject>().Spawn();
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
