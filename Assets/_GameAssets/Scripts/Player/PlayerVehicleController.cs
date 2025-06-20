using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class PlayerVehicleController : MonoBehaviour
{
    [Header("Referances")]
    [SerializeField] private VehicleSettingsSO _vehicleSettings;
    [SerializeField] private Rigidbody _vehicleRigidbody;
    [SerializeField] private BoxCollider _vehicelCollider;

    public class SpringData
    {
        public float _currentLenght;
        public float _currentVelocty;
    }

    private static readonly WheelType[] _wheels = new WheelType[]
    {
        WheelType.FrontLeft, WheelType.FrontRight, WheelType.BackLeft, WheelType.BackRight
    };

    private static readonly WheelType[] _backWheels = new WheelType[]
    {
        WheelType.BackLeft, WheelType.BackRight
    };

    private Dictionary<WheelType, SpringData> _springDatas;
    private float _steerInput;
    private float _accelerationInput;

    public Vector3 Velocity => _vehicleRigidbody.linearVelocity;
    public Vector3 Forward => transform.forward;
    public VehicleSettingsSO Settings => _vehicleSettings;

    private void Awake()
    {
        _springDatas = new Dictionary<WheelType, SpringData>();

        foreach (WheelType wheelType in _wheels)
        {
            _springDatas.Add(wheelType, new());
        }
    }

    private void Update()
    {
        SetSteerInput(Input.GetAxis("Horizontal"));
        SetAccelerateInput(Input.GetAxis("Vertical"));
    }

    private void FixedUpdate()
    {
        UpdateSuspension();
        UpdateSteering();
        UpdateAcceleration();
        UpdateBrakes();
        UpdateAirResistance();
    }

    private void SetSteerInput(float steerInput)
    {
        _steerInput = Math.Clamp(steerInput, -1f, 1f);
    }

    private void SetAccelerateInput(float accelerateInput)
    {
        _accelerationInput = Mathf.Clamp(accelerateInput, -1f, 1f);
    }

    private void UpdateSuspension()
    {
        foreach (WheelType id in _springDatas.Keys)
        {
            CastSpring(id);
            float currentVelocty = _springDatas[id]._currentVelocty;
            float currentLenght = _springDatas[id]._currentLenght;

            float force = SpringMathExtensions.CalculateForceDamped(currentLenght, currentVelocty,
                _vehicleSettings.SpringRestLenght, _vehicleSettings.SpringStrength, _vehicleSettings.SpringDample);

            _vehicleRigidbody.AddForceAtPosition(force * transform.up, GetSpringPosition(id));
        }
    }

    private void UpdateSteering()
    {
        foreach (WheelType wheelType in _wheels)
        {
            if (!IsGrounded(wheelType))
            {
                continue;
            }

            Vector3 springPosition = GetSpringPosition(wheelType);

            Vector3 slideDirection = GetWheelSlieDirection(wheelType);
            float slideVelocity = Vector3.Dot(slideDirection, _vehicleRigidbody.GetPointVelocity(springPosition));

            float desiredVelocityChange = GetWheelGripFactor(wheelType) * -slideVelocity;
            float desiredAcceleration = desiredVelocityChange / Time.fixedDeltaTime;

            Vector3 force = desiredAcceleration * slideDirection * _vehicleSettings.TireMass;
            _vehicleRigidbody.AddForceAtPosition(force, GetWheelTorquePosition(wheelType));
        }
    }

    private void UpdateAcceleration()
    {
        if (Mathf.Approximately(_accelerationInput, 0f))
        {
            return;
        }


        float forwardSpeed = Vector3.Dot(transform.forward, _vehicleRigidbody.linearVelocity);
        bool movingForwaard = forwardSpeed > 0f;
        float speed = Mathf.Abs(forwardSpeed);

        if (movingForwaard && speed > _vehicleSettings.MaxSpeed)
        {
            return;
        }
        else if (!movingForwaard && speed > _vehicleSettings.MaxReverseSpeed)
        {
            return;
        }

        foreach (WheelType wheelType in _wheels)
        {
            if (!IsGrounded(wheelType))
            {
                continue;
            }

            Vector3 position = GetWheelTorquePosition(wheelType);
            Vector3 wheelForward = GetWheelRollDirection(wheelType);
            _vehicleRigidbody.AddForceAtPosition(_accelerationInput * wheelForward * _vehicleSettings.AcceleratePower, position);
        }
    }

    private void UpdateBrakes()
    {
        float forwardSpeed = Vector3.Dot(transform.forward, _vehicleRigidbody.linearVelocity);
        float speed = Mathf.Abs(forwardSpeed);
        float brakesRatio;

        const float ALMOST_STOPPING_SPEED = 2f;
        bool almostStopping = speed < ALMOST_STOPPING_SPEED;

        if (almostStopping)
        {
            brakesRatio = 1f;
        }
        else
        {
            bool accelerateContrary =
                !Mathf.Approximately(_accelerationInput, 0f) &&
                Vector3.Dot(_accelerationInput * transform.forward, _vehicleRigidbody.linearVelocity) < 0f;

            if (accelerateContrary)
            {
                brakesRatio = 1f;
            }
            else if (Mathf.Approximately(_accelerationInput, 0f))
            {
                brakesRatio = 0.1f;
            }
            else
            {
                return;
            }
        }

        foreach (WheelType wheelType in _backWheels)
        {
            if (!IsGrounded(wheelType))
            {
                continue;
            }

            Vector3 springPosition = GetSpringPosition(wheelType);
            Vector3 rollDirection = GetWheelRollDirection(wheelType);
            float rollVelocity = Vector3.Dot(rollDirection, _vehicleRigidbody.GetPointVelocity(springPosition));

            float desiredVelocityChange = -rollVelocity * brakesRatio * _vehicleSettings.BrakesPower;
            float desiredAcceleration = desiredVelocityChange / Time.fixedDeltaTime;

            Vector3 force = desiredAcceleration * _vehicleSettings.TireMass * rollDirection;
            _vehicleRigidbody.AddForceAtPosition(force, GetWheelTorquePosition(wheelType));
        }
    }

    private void UpdateAirResistance()
    {
        _vehicleRigidbody.AddForce(_vehicelCollider.size.magnitude * -_vehicleRigidbody.linearVelocity
            * _vehicleSettings.AirResistance);
    }

    private void CastSpring(WheelType wheelType)
    {
        Vector3 position = GetSpringPosition(wheelType);

        float previousLenght = _springDatas[wheelType]._currentLenght;
        float currentLenght;

        if (Physics.Raycast(position, -transform.up, out var hit, _vehicleSettings.SpringRestLenght))
        {
            currentLenght = hit.distance;
        }
        else
        {
            currentLenght = _vehicleSettings.SpringRestLenght;
        }

        _springDatas[wheelType]._currentVelocty = (currentLenght - previousLenght) / Time.fixedDeltaTime;
        _springDatas[wheelType]._currentLenght = currentLenght;
    }

    private Vector3 GetSpringPosition(WheelType wheelType)
    {
        return transform.localToWorldMatrix.MultiplyPoint3x4(GetSpringRelativePosition(wheelType));
    }

    private Vector3 GetSpringRelativePosition(WheelType wheelType)
    {
        Vector3 boxSize = _vehicelCollider.size;
        float boxBottom = boxSize.y * -0.5f;

        float paddingX = _vehicleSettings.WheelsPaddingX;
        float paddingZ = _vehicleSettings.WheelsPaddingZ;

        return wheelType switch
        {
            WheelType.FrontLeft => new Vector3(boxSize.x * (paddingX - 0.5f), boxBottom, boxSize.z * (0.5f - paddingZ)),
            WheelType.FrontRight => new Vector3(boxSize.x * (0.5f - paddingX), boxBottom, boxSize.z * (0.5f - paddingZ)),
            WheelType.BackLeft => new Vector3(boxSize.x * (paddingX - 0.5f), boxBottom, boxSize.z * (paddingZ - 0.5f)),
            WheelType.BackRight => new Vector3(boxSize.x * (0.5f - paddingX), boxBottom, boxSize.z * (paddingZ - 0.5f)),
            _ => default
        };
    }

    private Vector3 GetWheelTorquePosition(WheelType wheelType)
    {
        return transform.localToWorldMatrix.MultiplyPoint3x4(GetWheelRelativeTorquePosition(wheelType));
    }

    private Vector3 GetWheelRelativeTorquePosition(WheelType wheelType)
    {
        Vector3 boxSize = _vehicelCollider.size;

        float paddingX = _vehicleSettings.WheelsPaddingX;
        float paddingZ = _vehicleSettings.WheelsPaddingZ;

        return wheelType switch
        {
            WheelType.FrontLeft => new Vector3(boxSize.x * (paddingX - 0.5f), 0f, boxSize.z * (0.5f - paddingZ)),
            WheelType.FrontRight => new Vector3(boxSize.x * (0.5f - paddingX), 0f, boxSize.z * (0.5f - paddingZ)),
            WheelType.BackLeft => new Vector3(boxSize.x * (paddingX - 0.5f), 0f, boxSize.z * (paddingZ - 0.5f)),
            WheelType.BackRight => new Vector3(boxSize.x * (0.5f - paddingX), 0f, boxSize.z * (paddingZ - 0.5f)),
            _ => default
        };
    }

    private Vector3 GetWheelSlieDirection(WheelType wheelType)
    {
        Vector3 forward = GetWheelRollDirection(wheelType);
        return Vector3.Cross(transform.up, forward);
    }

    private Vector3 GetWheelRollDirection(WheelType wheelType)
    {
        bool frontWheels = wheelType == WheelType.FrontLeft || wheelType == WheelType.FrontRight;

        if (frontWheels)
        {
            var steerQuaternion = Quaternion.AngleAxis(_steerInput * _vehicleSettings.SteerAngle, Vector3.up);
            return steerQuaternion * transform.forward;
        }
        else
        {
            return transform.forward;
        }
    }

    private float GetWheelGripFactor(WheelType wheelType)
    {
        bool frontWheels = wheelType == WheelType.FrontLeft || wheelType == WheelType.FrontRight;
        return frontWheels ? _vehicleSettings.FrontWheelIsGripFactor : _vehicleSettings.RearWheelIsGripFactor;
    }

    private bool IsGrounded(WheelType wheelType)
    {
        return _springDatas[wheelType]._currentLenght < _vehicleSettings.SpringRestLenght;
    }

    public float GetSpringCurrentLength(WheelType wheelType)
    {
        return _springDatas[wheelType]._currentLenght;
    }
}

public static class SpringMathExtensions
{
    public static float CalculateForceDamped(float currentLenght, float lengthVelocity,
        float restLength, float strength, float damper)
    {
        float lengthOffset = restLength - currentLenght;
        return (lengthOffset * strength) - (lengthVelocity * damper);
    }
}
