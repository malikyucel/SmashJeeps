using UnityEngine;

[CreateAssetMenu(fileName = "Vehicle Settings", menuName = "Scriptable Object/Vehicle Settings")]
public class VehicleSettingsSO : ScriptableObject
{
    [Header("Wheel Paddings")]
    [SerializeField] private float _wheelsPaddingX;
    [SerializeField] private float _wheelsPaddingZ;

    [Header("Suspension")]
    [SerializeField] private float _springRestLenght;
    [SerializeField] private float _springStrength;
    [SerializeField] private float _springDample;

    [Header("Hendling")]
    [SerializeField] private float _steerAngle;
    [SerializeField] private float _frontWheelIsGripFactor;
    [SerializeField] private float _rearWheelIsGripFactor;

    [Header("Body")]
    [SerializeField] private float _tireMass;

    [Header("Power")]
    [SerializeField] private float _acceleratePower;
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _maxReverseSpeed;
    [SerializeField] private float _brakesPower;

    [Header("Air Resistance")]
    [SerializeField] private float _airResistance;

    public float WheelsPaddingX => _wheelsPaddingX;
    public float WheelsPaddingZ => _wheelsPaddingZ;


    public float SpringRestLenght => _springRestLenght;
    public float SpringStrength => _springStrength;
    public float SpringDample => _springDample;

    public float SteerAngle => _steerAngle;
    public float FrontWheelIsGripFactor => _frontWheelIsGripFactor;
    public float RearWheelIsGripFactor => _rearWheelIsGripFactor;

    public float TireMass => _tireMass;

    public float AcceleratePower => _acceleratePower;
    public float MaxSpeed => _maxSpeed;
    public float MaxReverseSpeed => _maxReverseSpeed;
    public float BrakesPower => _brakesPower;

    public float AirResistance => _airResistance;
}
