using UnityEngine;

[CreateAssetMenu(fileName = "Mystery Box Skills", menuName = "Scriptable Objects/Mystery Box Skills")]
public class MyseryBoxSkillsSO : ScriptableObject
{
    [SerializeField] private string _skillName;
    [SerializeField] private Sprite _scillIcon;
    [SerializeField] private SkillType _skillType;
    [SerializeField] private SkellUsegeType _skellUsegeType;
    [SerializeField] private SkillDataSO _skillDataSO;

    public string SkillName => _skillName;
    public Sprite SkillIcon => _scillIcon;
    public SkillType SkillType => _skillType;
    public SkellUsegeType SkellUsegeType => _skellUsegeType;
    public SkillDataSO SkillData => _skillDataSO;
}
