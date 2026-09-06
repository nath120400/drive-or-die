using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class CarManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputActionReference _moveInputActionReference;
    [SerializeField] private CarType _carType;

    [Header("Physical")]
    [SerializeField] private float _forwardSpeed = 20f;
    [SerializeField] private float _wheelbase = 2.5f;
    [SerializeField] private float _steeringSpeed = 150f;
    [SerializeField] private float _maxWheelAngle = 35f;
    [SerializeField] private float _maxDiagonalAngle = 25f;

    [Header("Visual")]
    [SerializeField] private float _wheelRadius = 0.3f;
    [SerializeField] private float _rollMultiplier = 0.02f;
    [SerializeField] private float _shakeSpeedMultiplier = 1f;
    [SerializeField] private float _shakeRotAmplitude = 1.5f;
    [SerializeField] private float _shakePosAmplitude = 0.05f;

    // Run state
    public float Health;
    public float Fuel;
    public float Distance;
    public float Score;
    public List<EntityDescription> Inventory = new List<EntityDescription>();

    // Reused delta: no allocation per collision
    private DeltaStat _delta = new DeltaStat();

    public float CurrentMaxAngle => _dynamicMaxAngle;
    public float ForwardSpeed => _forwardSpeed;

    private Rigidbody _rb;
    private float _currentCarAngle;
    private float _angularVelocityVelocity;
    private float _wheelRotationX;
    private float _smoothedInput;
    private float _dynamicMaxAngle;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        // The car type on the same prefab seeds the run state
        _carType = GetComponent<CarType>();
        _forwardSpeed = _carType.ForwardSpeed;
        _wheelbase = _carType.Wheelbase;
        _steeringSpeed = _carType.SteeringSpeed;
        _maxWheelAngle = _carType.MaxWheelAngle;
        _maxDiagonalAngle = _carType.MaxDiagonalAngle;
        Health = _carType.MaxHealth;
        Fuel = _carType.MaxFuel;
    }

    private void FixedUpdate()
    {
        // Smooth the input: the steering axis of the 2D move action
        float rawInput = _moveInputActionReference.action.ReadValue<Vector2>().x;

        float inputChangeSpeed = _steeringSpeed / _maxWheelAngle;
        _smoothedInput = Mathf.MoveTowards(_smoothedInput, rawInput, inputChangeSpeed * Time.fixedDeltaTime);

        // Dynamic steering
        float speedFactor = Mathf.Clamp01(Mathf.Abs(_forwardSpeed) / 50f);
        float dynamicMaxAngle = Mathf.Lerp(_maxDiagonalAngle, _maxDiagonalAngle * 0.2f, speedFactor);
        _dynamicMaxAngle = dynamicMaxAngle;
        float targetCarAngle = _smoothedInput * dynamicMaxAngle;

        // Accurate steering
        float maxAngularSpeed = (_forwardSpeed * Mathf.Tan(_maxWheelAngle * Mathf.Deg2Rad)) / _wheelbase * Mathf.Rad2Deg;
        float smoothTime = _maxWheelAngle / _steeringSpeed;

        _currentCarAngle = Mathf.SmoothDampAngle(
            _currentCarAngle,
            targetCarAngle,
            ref _angularVelocityVelocity,
            smoothTime,
            maxAngularSpeed,
            Time.fixedDeltaTime
        );

        // Apply steering
        _rb.angularVelocity = new Vector3(0f, _angularVelocityVelocity * Mathf.Deg2Rad, 0f);

        // Lateral movement only: the world (chunks) carries the forward motion
        float lateralVelocity = _forwardSpeed * Mathf.Tan(_currentCarAngle * Mathf.Deg2Rad);
        _rb.linearVelocity = new Vector3(lateralVelocity, _rb.linearVelocity.y, _rb.linearVelocity.z);
        Distance += _forwardSpeed * Time.fixedDeltaTime;
    }

    private void Update()
    {
        // Wheel steering angle
        float omegaRad = _angularVelocityVelocity * Mathf.Deg2Rad;
        float currentWheelAngle = Mathf.Atan((omegaRad * _wheelbase) / _forwardSpeed) * Mathf.Rad2Deg;

        // Continuous wheel rolling
        float rotationThisFrame = (_forwardSpeed * Time.deltaTime / _wheelRadius) * Mathf.Rad2Deg;
        _wheelRotationX += rotationThisFrame;

        // Apply to wheels
        if (_carType.FrontLeftWheel != null)
            _carType.FrontLeftWheel.localRotation = Quaternion.Euler(_wheelRotationX, currentWheelAngle, 0f);

        if (_carType.FrontRightWheel != null)
            _carType.FrontRightWheel.localRotation = Quaternion.Euler(_wheelRotationX, currentWheelAngle, 0f);

        if (_carType.RearLeftWheel != null)
            _carType.RearLeftWheel.localRotation = Quaternion.Euler(_wheelRotationX, 0f, 0f);

        if (_carType.RearRightWheel != null)
            _carType.RearRightWheel.localRotation = Quaternion.Euler(_wheelRotationX, 0f, 0f);

        // Car body
        if (_carType.CarBody != null)
        {
            float bodyRollZ = currentWheelAngle * _rollMultiplier * _forwardSpeed;

            float currentShakeSpeed = _forwardSpeed * _shakeSpeedMultiplier;
            float noiseRotZ = (Mathf.PerlinNoise(Time.time * currentShakeSpeed, 0f) * 2f - 1f) * _shakeRotAmplitude;
            float noisePosY = (Mathf.PerlinNoise(0f, Time.time * currentShakeSpeed) * 2f - 1f) * _shakePosAmplitude;

            _carType.CarBody.localRotation = Quaternion.Euler(0f, 0f, bodyRollZ + noiseRotZ);
            _carType.CarBody.localPosition = new Vector3(0f, noisePosY, 0f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Entity entity = other.GetComponent<Entity>();
        if (entity != null)
        {
            _delta.Reset();
            _delta.CarManager = this;
            entity.Description.OnCollide(ref _delta);

            // The inventory items adjust the delta before it lands
            for (int i = 0; i < Inventory.Count; i++)
            {
                Inventory[i].InInventory(ref _delta);
            }

            Apply(_delta);
            Debug.Log($"[CarManager] Hit {entity.name} ({entity.Description.name}): dHealth={_delta.Health:+0;-0} dFuel={_delta.Fuel:+0;-0} -> Health={Health:F1} Fuel={Fuel:F1}");

            entity.Description.Release(entity);
        }
    }

    public void Apply(DeltaStat delta)
    {
        Health += delta.Health;
        Fuel += delta.Fuel;
        Score += delta.Score;
        Distance += delta.Distance;
    }
}
