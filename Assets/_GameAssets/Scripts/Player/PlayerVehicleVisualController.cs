using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerVehicleVisualController : NetworkBehaviour
{
    [SerializeField] private PlayerVehicleController _playerVehicleController;
    [SerializeField] private CharacterSelectVisual _characterSelectVisual;
    [SerializeField] private Transform _jeepVisualTransform;
    [SerializeField] private Collider _playerCollider;
    [SerializeField] private Transform _wheelFrontLeft, _wheelFrontRight, _wheelBackLeft, _wheelBackRight;
    [SerializeField] private float _wheelSpinSpeed, _wheelYWhenSpringMin, _wheelYWhenSpringMax;

    private Quaternion _wheelFrontLeftRoll;
    private Quaternion _wheelFrontRightRoll;

    private float _springRestLength;
    private float _forwardSpeed;
    private float _steerInput;
    private float _steerAngle;

    private Dictionary<WheelType, float> _springsCurrentLength = new()
    {
        { WheelType.FrontLeft, 0 },
        { WheelType.FrontRight, 0 },
        { WheelType.BackLeft, 0 },
        { WheelType.BackRight, 0 }
    };

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }

        _playerVehicleController.OnVehicleCrashed += PlayerVehicleController_OnVehicleCrashed;
    }

    private void PlayerVehicleController_OnVehicleCrashed()
    {
        enabled = false;
    }

    private void Start()
    {
        _wheelFrontLeftRoll = _wheelFrontLeft.localRotation;
        _wheelFrontRightRoll = _wheelFrontRight.localRotation;

        _springRestLength = _playerVehicleController.Settings.SpringRestLenght;
        _steerAngle = _playerVehicleController.Settings.SteerAngle;

        PlayerDataSerializable playerData = MultiplayerGameManager.Instance.GetPlayerDataFromClientId(OwnerClientId);
        _characterSelectVisual.SetPlayerColor(MultiplayerGameManager.Instance.GetPlayerColor(playerData.ColorId));
    }

    private void Update()
    {
        if (!IsOwner) return;
        if(GameManager.Instance.GetGameState() != GaemState.Playing) return;

        UpdateVisulaStates();
        RotateWheels();
        SetSuspension();
    }

    private void UpdateVisulaStates()
    {
        _steerInput = Input.GetAxis("Horizontal");

        _forwardSpeed = Vector3.Dot(_playerVehicleController.Forward, _playerVehicleController.Velocity);

        _springsCurrentLength[WheelType.FrontLeft] = _playerVehicleController.GetSpringCurrentLength(WheelType.FrontLeft);
        _springsCurrentLength[WheelType.FrontRight] = _playerVehicleController.GetSpringCurrentLength(WheelType.FrontRight);
        _springsCurrentLength[WheelType.BackLeft] = _playerVehicleController.GetSpringCurrentLength(WheelType.BackLeft);
        _springsCurrentLength[WheelType.BackRight] = _playerVehicleController.GetSpringCurrentLength(WheelType.BackRight);
    }

    private void RotateWheels()
    {
        if (_springsCurrentLength[WheelType.FrontLeft] < _springRestLength)
        {
            _wheelFrontLeftRoll *= Quaternion.AngleAxis(_forwardSpeed * _wheelSpinSpeed * Time.deltaTime, Vector3.right);
        }

        if (_springsCurrentLength[WheelType.FrontRight] < _springRestLength)
        {
            _wheelFrontRightRoll *= Quaternion.AngleAxis(_forwardSpeed * _wheelSpinSpeed * Time.deltaTime, Vector3.right);
        }

        if (_springsCurrentLength[WheelType.BackLeft] < _springRestLength)
        {
            _wheelBackLeft.localRotation *= Quaternion.AngleAxis(_forwardSpeed * _wheelSpinSpeed * Time.deltaTime, Vector3.right);
        }

        if (_springsCurrentLength[WheelType.BackRight] < _springRestLength)
        {
            _wheelBackRight.localRotation *= Quaternion.AngleAxis(_forwardSpeed * _wheelSpinSpeed * Time.deltaTime, Vector3.right);
        }

        _wheelFrontLeft.localRotation = Quaternion.AngleAxis(_steerInput * _steerAngle, Vector3.up) * _wheelFrontLeftRoll;
        _wheelFrontRight.localRotation = Quaternion.AngleAxis(_steerInput * _steerAngle, Vector3.up) * _wheelFrontRightRoll;
    }

    private void SetSuspension()
    {
        float springFrontLeftRatio = _springsCurrentLength[WheelType.FrontLeft] / _springRestLength;
        float springFrontRightRatio = _springsCurrentLength[WheelType.FrontRight] / _springRestLength;
        float springBackLeftRatio = _springsCurrentLength[WheelType.BackLeft] / _springRestLength;
        float springBackRightRatio = _springsCurrentLength[WheelType.BackRight] / _springRestLength;

        _wheelFrontLeft.localPosition = new Vector3(_wheelFrontLeft.localPosition.x,
            _wheelYWhenSpringMin + (_wheelYWhenSpringMax - _wheelYWhenSpringMin) * springFrontLeftRatio,
            _wheelFrontLeft.localPosition.z);

        _wheelFrontRight.localPosition = new Vector3(_wheelFrontRight.localPosition.x,
            _wheelYWhenSpringMin + (_wheelYWhenSpringMax - _wheelYWhenSpringMin) * springFrontRightRatio,
            _wheelFrontRight.localPosition.z);

        _wheelBackLeft.localPosition = new Vector3(_wheelBackLeft.localPosition.x,
            _wheelYWhenSpringMin + (_wheelYWhenSpringMax - _wheelYWhenSpringMin) * springBackLeftRatio,
            _wheelBackLeft.localPosition.z);

        _wheelBackRight.localPosition = new Vector3(_wheelBackRight.localPosition.x,
            _wheelYWhenSpringMin + (_wheelYWhenSpringMax - _wheelYWhenSpringMin) * springBackRightRatio,
            _wheelBackRight.localPosition.z);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetJeepVisualActiveRpc(bool isActive)
    {
        _jeepVisualTransform.gameObject.SetActive(isActive);
    }

    private IEnumerator SetVehicleVisualActiveCoroutime(float delay)
    {
        SetJeepVisualActiveRpc(false);
        _playerCollider.enabled = false;

        yield return new WaitForSeconds(delay);

        SetJeepVisualActiveRpc(true);
        _playerCollider.enabled = true;
    }

    public void SetVehicleVisualActive(float delay)
    {
        StartCoroutine(SetVehicleVisualActiveCoroutime(delay));
    }
}
