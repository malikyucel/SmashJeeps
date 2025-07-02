using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerSkillController : NetworkBehaviour
{
    public static event Action OnTimerFineshed;

    [Header("Referance")]
    [SerializeField] private Transform _rocketLauncherTransform;
    [SerializeField] private Transform _rocketLaunchPoint;

    [Header("Settings")]
    [SerializeField] private bool _hasSkillAlready;
    [SerializeField] private float _resetDelay;

    private PlayerVehicleController _playerVehicleController;
    private PlayerInteractionController _playerInteractionController;

    private MyseryBoxSkillsSO _mysteryBoxSkill;
    private bool isSkillUsed;
    private bool _hasTimerStarted;
    private float _timer;
    private float _timerMax;
    private int _mineAmountCounter;

    public override void OnNetworkSpawn()
    {
        _playerVehicleController = GetComponent<PlayerVehicleController>();
        _playerInteractionController = GetComponent<PlayerInteractionController>();

        _playerVehicleController.OnVehicleCrashed += PlaayerVehicleController_OnVehicleCrashed;
    }

    private void PlaayerVehicleController_OnVehicleCrashed()
    {
        enabled = false;
        SkillsUI.Instance.SetSkillToNone();
        _hasTimerStarted = false;
        _hasSkillAlready = false;
        SetRocketLauncherActiveRpc(false);
    }

    private void Update()
    {
        if (!IsOwner) { return; }

        if (Input.GetKey(KeyCode.Space) && !isSkillUsed)
        {
            ActivateSkill();
            isSkillUsed = true;
        }

        if (_hasTimerStarted)
        {
            _timer -= Time.deltaTime;
            SkillsUI.Instance.SetTimeCounterText((int)_timer);

            if (_timer <= 0f)
            {
                OnTimerFineshed?.Invoke();
                SkillsUI.Instance.SetSkillToNone();
                _hasTimerStarted = false;
                _hasSkillAlready = false;

                if (_mysteryBoxSkill.SkillType == SkillType.Shield)
                {
                    _playerInteractionController.SetShiledlActive(false);
                }

                if (_mysteryBoxSkill.SkillType == SkillType.Spike)
                {
                    _playerInteractionController.SetSpikelActive(false);
                }
            }
        }
    }

    public void SetupSkill(MyseryBoxSkillsSO skill)
    {
        _mysteryBoxSkill = skill;

        if (_mysteryBoxSkill.SkillType == SkillType.Rocket)
        {
            SetRocketLauncherActiveRpc(true);
        }

        _hasSkillAlready = true;
        isSkillUsed = false;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetRocketLauncherActiveRpc(bool active)
    {
        _rocketLauncherTransform.gameObject.SetActive(active);
    }

    private IEnumerator ResetRocketLauncher()
    {
        yield return new WaitForSeconds(_resetDelay);
        SetRocketLauncherActiveRpc(false);
    }

    public void ActivateSkill()
    {
        if (!_hasSkillAlready) { return; }

        SkillManager.Instance.ActivateSkill(_mysteryBoxSkill.SkillType, transform, OwnerClientId);
        SetSkillToNone();

        if (_mysteryBoxSkill.SkillType == SkillType.Rocket)
        {
            StartCoroutine(ResetRocketLauncher());
        }

        if (_mysteryBoxSkill.SkillType == SkillType.Shield)
        {
            _playerInteractionController.SetShiledlActive(true);
        }

        if (_mysteryBoxSkill.SkillType == SkillType.Spike)
        {
            _playerInteractionController.SetSpikelActive(true);
        }
    }

    private void SetSkillToNone()
    {
        if (_mysteryBoxSkill.SkellUsegeType == SkellUsegeType.None)
        {
            _hasSkillAlready = false;
            SkillsUI.Instance.SetSkillToNone();
        }

        if (_mysteryBoxSkill.SkellUsegeType == SkellUsegeType.Timer)
        {
            _hasTimerStarted = true;
            _timerMax = _mysteryBoxSkill.SkillData.SpawnAmaountOrTimer;
            _timer = _timerMax;
        }

        if (_mysteryBoxSkill.SkellUsegeType == SkellUsegeType.Amount)
        {
            _mineAmountCounter = _mysteryBoxSkill.SkillData.SpawnAmaountOrTimer;

            SkillManager.Instance.OnMineCountReducet += SkillManager_OnMineCountReduced;
        }
    }

    private void SkillManager_OnMineCountReduced()
    {
        _mineAmountCounter--;
        SkillsUI.Instance.SetTimeCounterText(_mineAmountCounter);

        if (_mineAmountCounter <= 0)
        {
            _hasSkillAlready = false;
            SkillsUI.Instance.SetSkillToNone();
            SkillManager.Instance.OnMineCountReducet -= SkillManager_OnMineCountReduced;
        }
    }

    public bool HasSkillAlready()
    {
        return _hasSkillAlready;
    }

    public Vector3 GetRocketLaunchPosition()
    {
        return _rocketLaunchPoint.position;
    }

    public void OnPlayerRespawn() => enabled = true;
}
