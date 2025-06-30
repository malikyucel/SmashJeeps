using System;
using Unity.Netcode;
using UnityEngine;

public class ShieldController : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        PlayerSkillController.OnTimerFineshed += PlayerSlillController_OnTimerFinished;
    }

    private void PlayerSlillController_OnTimerFinished()
    {
        DestroyRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DestroyRpc()
    {
        if (IsServer)
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkDespawn()
    {
        PlayerSkillController.OnTimerFineshed -= PlayerSlillController_OnTimerFinished;
    }
}