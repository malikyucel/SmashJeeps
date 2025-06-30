using Unity.Netcode;
using UnityEngine;

public class MysteryBoxCollectible : NetworkBehaviour, Collectibles
{
    [Header("Reference")]
    [SerializeField] private MyseryBoxSkillsSO[] _mysteryBoxSkills;
    [SerializeField] private Animator _boxAnimator;
    [SerializeField] private Collider _collider;

    [Header("Settings")]
    [SerializeField] private float _respawnTimer;

    public void Collect(PlayerSkillController playerSkillController)
    {
        if(playerSkillController.HasSkillAlready()) { return; }

        MyseryBoxSkillsSO skill = GetRandumSkill();
        SkillsUI.Instance.SetSkill(skill.SkillName, skill.SkillIcon, skill.SkellUsegeType, skill.SkillData.SpawnAmaountOrTimer);
        playerSkillController.SetupSkill(skill);

        CollectRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void CollectRpc()
    {
        AnimateCollection();
        Invoke(nameof(Respawn), _respawnTimer);
    }

    private void AnimateCollection()
    {
        _collider.enabled = false;
        _boxAnimator.SetTrigger(Const.BoxAnimations.IS_COLLECTED);
    }

    private void Respawn()
    {
        _boxAnimator.SetTrigger(Const.BoxAnimations.IS_RESPAWED);
        _collider.enabled = true;
    }

    private MyseryBoxSkillsSO GetRandumSkill()
    {
        int randumIndex = Random.Range(0, _mysteryBoxSkills.Length);
        return _mysteryBoxSkills[randumIndex];
    }
}
