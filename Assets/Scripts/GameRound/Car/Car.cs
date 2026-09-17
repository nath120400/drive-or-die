using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Car : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputActionReference _moveInputActionReference;
    [SerializeField] private RunManager _run;
    [SerializeField] private Transform _frontLeftWheel;
    [SerializeField] private Transform _frontRightWheel;
    [SerializeField] private Transform _rearLeftWheel;
    [SerializeField] private Transform _rearRightWheel;
    [SerializeField] private Transform _carBody;

    [Header("Physical")]
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

    // The run state lives in the run manager: the car only reads its speed
    public float ForwardSpeed => _run.ForwardSpeed;

    // Reused delta: no allocation per collision
    private TempState _delta = new TempState();

    public float CurrentMaxAngle => _dynamicMaxAngle;

    private Rigidbody _rb;
    private float _baseYaw;
    private float _currentCarAngle;
    private float _angularVelocityVelocity;
    private float _wheelRotationX;
    private float _smoothedInput;
    private float _dynamicMaxAngle;

    // How fast a collision jolt's yaw error decays back to the steering cap
    private const float RealignSpeed = 8f;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _baseYaw = _rb.rotation.eulerAngles.y;
    }

    private void FixedUpdate()
    {
        // Smooth the input: the steering axis of the 2D move action
        float rawInput = _moveInputActionReference.action.ReadValue<Vector2>().x;

        float inputChangeSpeed = _steeringSpeed / _maxWheelAngle;
        _smoothedInput = Mathf.MoveTowards(_smoothedInput, rawInput, inputChangeSpeed * Time.fixedDeltaTime);

        // The yaw cap tightens with speed: 25° at rest, half of it at full speed
        float speedFactor = Mathf.Clamp01(Mathf.Abs(ForwardSpeed) / 50f);
        float dynamicMaxAngle = Mathf.Lerp(_maxDiagonalAngle, _maxDiagonalAngle * 0.5f, speedFactor);
        _dynamicMaxAngle = dynamicMaxAngle;
        float targetCarAngle = _smoothedInput * dynamicMaxAngle;

        // The yaw rate cap from the wheel geometry: speed * tan(wheel angle) / wheelbase
        float maxAngularSpeed = (ForwardSpeed * Mathf.Tan(_maxWheelAngle * Mathf.Deg2Rad)) / _wheelbase * Mathf.Rad2Deg;
        float smoothTime = _maxWheelAngle / _steeringSpeed;

        _currentCarAngle = Mathf.SmoothDampAngle(
            _currentCarAngle,
            targetCarAngle,
            ref _angularVelocityVelocity,
            smoothTime,
            maxAngularSpeed,
            Time.fixedDeltaTime
        );

        // Apply steering, plus the realign: a collision jolt bends the yaw,
        // it decays back to the steering cap every tick
        float yawError = Mathf.DeltaAngle(_rb.rotation.eulerAngles.y, _baseYaw + _currentCarAngle);
        float realignRate = yawError * RealignSpeed;
        _rb.angularVelocity = new Vector3(0f, (_angularVelocityVelocity + realignRate) * Mathf.Deg2Rad, 0f);

        // Lateral movement only: the world (chunks) carries the forward motion
        float lateralVelocity = ForwardSpeed * Mathf.Tan(_currentCarAngle * Mathf.Deg2Rad);
        _rb.linearVelocity = new Vector3(lateralVelocity, _rb.linearVelocity.y, _rb.linearVelocity.z);
    }

    private void Update()
    {
        // The front wheels match the current yaw rate
        float omegaRad = _angularVelocityVelocity * Mathf.Deg2Rad;
        float currentWheelAngle = Mathf.Atan((omegaRad * _wheelbase) / ForwardSpeed) * Mathf.Rad2Deg;

        // Rolling: the distance covered over the wheel radius, in degrees
        float rotationThisFrame = (ForwardSpeed * Time.deltaTime / _wheelRadius) * Mathf.Rad2Deg;
        _wheelRotationX += rotationThisFrame;

        // Apply to wheels
        if (_frontLeftWheel != null)
            _frontLeftWheel.localRotation = Quaternion.Euler(_wheelRotationX, currentWheelAngle, 0f);

        if (_frontRightWheel != null)
            _frontRightWheel.localRotation = Quaternion.Euler(_wheelRotationX, currentWheelAngle, 0f);

        if (_rearLeftWheel != null)
            _rearLeftWheel.localRotation = Quaternion.Euler(_wheelRotationX, 0f, 0f);

        if (_rearRightWheel != null)
            _rearRightWheel.localRotation = Quaternion.Euler(_wheelRotationX, 0f, 0f);

        // Car body
        if (_carBody != null)
        {
            float bodyRollZ = currentWheelAngle * _rollMultiplier * ForwardSpeed;

            float currentShakeSpeed = ForwardSpeed * _shakeSpeedMultiplier;
            float noiseRotZ = (Mathf.PerlinNoise(Time.time * currentShakeSpeed, 0f) * 2f - 1f) * _shakeRotAmplitude;
            float noisePosY = (Mathf.PerlinNoise(0f, Time.time * currentShakeSpeed) * 2f - 1f) * _shakePosAmplitude;

            _carBody.localRotation = Quaternion.Euler(0f, 0f, bodyRollZ + noiseRotZ);
            _carBody.localPosition = new Vector3(0f, noisePosY, 0f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Entity entity = other.GetComponent<Entity>();
        if (entity != null)
        {
            _delta.Reset(_run.MaxHealth, _run.MaxFuel);
            _delta.Car = this;
            entity.Description.OnCollide(ref _delta);

            // The held items adjust the delta before it lands
            for (int i = 0; i < _run.Inventory.Count; i++)
            {
                _run.Inventory[i].OnInventory(ref _delta);
            }

            _run.Apply(ref _delta);

            // The effects outlive the entity: they slide on with its chunk
            for (int i = 0; i < entity.Description.Effects.Count; i++)
            {
                entity.Description.Effects[i].Spawn(entity.transform.position, entity.Chunk.transform);
            }

            entity.Description.Release(entity);
        }
    }
}
