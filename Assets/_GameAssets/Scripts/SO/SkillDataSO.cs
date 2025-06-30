using UnityEngine;

[CreateAssetMenu(fileName = "Skill Data", menuName = "Scriptable Objects/Skill Data")]
public class SkillDataSO : ScriptableObject
{
    [SerializeField] private Transform _skillPrefab;
    [SerializeField] private Vector3 _skillOffset;
    [SerializeField] private int _spawnAmountOrTimer;
    [SerializeField] private bool _sholdBeAttachedToParent;
    [SerializeField] private int _respawnTimer;
    [SerializeField] private int _damageAmount;

    public Transform SkillPrefab => _skillPrefab;
    public Vector3 SkillOffset => _skillOffset;
    public int SpawnAmaountOrTimer => _spawnAmountOrTimer;
    public bool SholdBeAttachedToParent => _sholdBeAttachedToParent;
    public int RespawnTimer => _respawnTimer;
    public int DamageAmount => _damageAmount;
}
