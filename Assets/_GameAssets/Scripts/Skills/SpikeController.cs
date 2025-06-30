using System;
using Unity.Netcode;
using UnityEngine;

public class SpikeController : NetworkBehaviour
{
    [SerializeField] private Collider _spikeCollider;

    public override void OnNetworkSpawn()
    {
        PlayerSkillController.OnTimerFineshed += PlayerSkillController_OnTimerFinshed;

        if (IsOwner)
        {
            SetOvnerVisualsRpc();
        }
    }

    private void PlayerSkillController_OnTimerFinshed()
    {
        DestroyRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DestroyRpc()
    {
        if(IsServer)
            Destroy(gameObject);
    }

    [Rpc(SendTo.Owner)]
    private void SetOvnerVisualsRpc()
    {
        _spikeCollider.enabled = false;
    }

    public override void OnNetworkDespawn()
    {
        PlayerSkillController.OnTimerFineshed -= PlayerSkillController_OnTimerFinshed;
    }
}   
