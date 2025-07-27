using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class SkillManager : NetworkBehaviour
{
    public static SkillManager Instance { get; private set; }

    public event Action OnMineCountReducet;

    [SerializeField] private MyseryBoxSkillsSO[] _myseryBoxSkills;
    [SerializeField] private LayerMask _groundLayer, _hillLayer;

    private Dictionary<SkillType, MyseryBoxSkillsSO> _skillsDictionary;

    private void Awake()
    {
        Instance = this;

        _skillsDictionary = new Dictionary<SkillType, MyseryBoxSkillsSO>();

        foreach (MyseryBoxSkillsSO skill in _myseryBoxSkills)
        {
            _skillsDictionary[skill.SkillType] = skill;
        }
    }

    public void ActivateSkill(SkillType skillType, Transform palyerTransform, ulong spawnerClientId)
    {
        SkillTransformDataSerializable skillTransformData =
            new SkillTransformDataSerializable(palyerTransform.position, palyerTransform.rotation,
                skillType, palyerTransform.GetComponent<NetworkObject>());

        if (!IsServer)
        {
            RequestSpawnRpc(skillTransformData, spawnerClientId);
            return;
        }

        SpawnSkill(skillTransformData, spawnerClientId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void RequestSpawnRpc(SkillTransformDataSerializable skillTransformDataSerializable,
        ulong spawnerClientId)
    {
        SpawnSkill(skillTransformDataSerializable, spawnerClientId);
    }

    private async void SpawnSkill(SkillTransformDataSerializable skillTransformDataSerializable,
        ulong spawnerClientId)
    {
        if (!_skillsDictionary.TryGetValue(skillTransformDataSerializable.SkillType, out MyseryBoxSkillsSO skillData))
        {
            Debug.LogError($"Spawn Skill: {skillTransformDataSerializable.SkillType} not found!");
            return;
        }

        if (skillTransformDataSerializable.SkillType == SkillType.Mine)
        {
            Vector3 spawnPosition = skillTransformDataSerializable.Position;
            Vector3 spawnDirection = skillTransformDataSerializable.Rotation * Vector3.forward;

            for (int i = 0; i < skillData.SkillData.SpawnAmaountOrTimer; i++)
            {
                Vector3 offset = spawnDirection * (i * 3f);
                skillTransformDataSerializable.Position = spawnPosition + offset;

                Spawn(skillTransformDataSerializable, spawnerClientId, skillData);
                await UniTask.Delay(200);
                OnMineCountReducet?.Invoke();
            }
        }
        else
        {
            Spawn(skillTransformDataSerializable, spawnerClientId, skillData);
        }
    }

    private void Spawn(SkillTransformDataSerializable skillTransformDataSerializable,
        ulong spawnerClientId, MyseryBoxSkillsSO skilldata)
    {
        if (IsServer)
        {
            Transform skillInstance = Instantiate(skilldata.SkillData.SkillPrefab);
            skillInstance.SetPositionAndRotation(skillTransformDataSerializable.Position, skillTransformDataSerializable.Rotation);
            var networkObject = skillInstance.GetComponent<NetworkObject>();
            networkObject.SpawnWithOwnership(spawnerClientId);

            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(spawnerClientId, out var client))
            {
                if (skilldata.SkillType != SkillType.Rocket)
                {
                    networkObject.TrySetParent(client.PlayerObject);
                }
                else
                {
                    PlayerSkillController playerSkillController = client.PlayerObject.GetComponent<PlayerSkillController>();
                    networkObject.transform.localPosition = playerSkillController.GetRocketLaunchPosition();
                    return;
                }

                if (skilldata.SkillData.SholdBeAttachedToParent)
                {
                    networkObject.transform.localPosition = Vector3.zero;
                }

                PositionDataSerializable positionDataSerializable = new PositionDataSerializable(
                    skillInstance.transform.localPosition + skilldata.SkillData.SkillOffset
                );
                UpdateSkillPositionRpc(networkObject.NetworkObjectId, positionDataSerializable, false);

                if (!skilldata.SkillData.SholdBeAttachedToParent)
                {
                    networkObject.TryRemoveParent();
                }

                if (skilldata.SkillType == SkillType.FakeBox)
                {
                    float groundHeight = GetGroundHeight(skilldata, skillInstance.position);

                    positionDataSerializable = new PositionDataSerializable(new Vector3(
                        skillInstance.transform.position.x,
                        groundHeight,
                        skillInstance.transform.position.z));

                    UpdateSkillPositionRpc(networkObject.NetworkObjectId, positionDataSerializable, true);
                }
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateSkillPositionRpc(ulong objectId, PositionDataSerializable positionDataSerializable, bool isSpeciaPosition)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out var networkObject))
        {
            if (isSpeciaPosition)
            {
                networkObject.transform.position = positionDataSerializable.Position;
            }
            else
            {
                networkObject.transform.localPosition = positionDataSerializable.Position;
            }
        }
    }

    private float GetGroundHeight(MyseryBoxSkillsSO skillData, Vector3 position)
    {
        if (Physics.Raycast(new Vector3(position.x, position.y, position.z), Vector3.down,
            out RaycastHit hi2, 10f, _hillLayer))
        {
            return 3f;
        }

        if (Physics.Raycast(new Vector3(position.x, position.y, position.z), Vector3.down,
            out RaycastHit hit, 10f, _groundLayer))
        {
            return skillData.SkillData.SkillOffset.y;
        }

        return skillData.SkillData.SkillOffset.y;
    }
}
