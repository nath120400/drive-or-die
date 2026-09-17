using System.Collections.Generic;
using UnityEngine;

// The single owner of the run: difficulty, speed, the tick and the stats,
// every stat change flows through Apply, clamped so no caller
// can push Health/Fuel out of range
public class RunManager : MonoBehaviour
{
    [Header("Tick")]
    [SerializeField] private float _tickInterval = 1f / 3f;
    [SerializeField] private int _maxDifficultyTicks = 1800; // 10 minutes at 3 ticks per second

    [Header("Speed")]
    [SerializeField] private float _baseSpeed = 20f;
    [SerializeField] private float _targetSpeed = 60f;

    [Header("Stats")]
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _maxFuel = 100f;
    [SerializeField] private float _fuelPerTick = 1f;

    public float Health { get; private set; }
    public float Fuel { get; private set; }
    public float Score { get; private set; }
    public float Distance { get; private set; }

    public float MaxHealth => _maxHealth;
    public float MaxFuel => _maxFuel;

    // The run is lost when either gauge runs dry
    public bool IsOver => Health <= 0f || Fuel <= 0f;

    // Difficulty cursor: ticks since run start over the max difficulty ticks
    public float Difficulty => Mathf.Clamp01((float)_tickCount / _maxDifficultyTicks);

    // The current speed rides the difficulty
    public float ForwardSpeed => Mathf.Lerp(_baseSpeed, _targetSpeed, Difficulty);

    // The held items: they tweak the score, the stat deltas and the spawn weights
    public List<EntityDescription> Inventory = new List<EntityDescription>();

    // Reused delta per tick: no allocation
    private TempState _delta = new TempState();

    private float _tickTimer;
    private int _tickCount;

    // The gauges start full: Awake runs before anyone reads them
    private void Awake()
    {
        Health = _maxHealth;
        Fuel = _maxFuel;
    }

    private void Update()
    {
        // Fixed ticks: one per interval, several in a row after a lag spike
        _tickTimer += Time.deltaTime;
        while (_tickTimer >= _tickInterval)
        {
            _tickTimer -= _tickInterval;
            Tick();
        }
    }

    private void Tick()
    {
        _tickCount++;

        // The score is the distance covered during the tick
        _delta.Reset(_maxHealth, _maxFuel);

        _delta.Fuel = -_fuelPerTick * ForwardSpeed;
        _delta.Score = ForwardSpeed;
        _delta.Distance = ForwardSpeed;

        // The held items adjust the score before it lands
        for (int i = 0; i < Inventory.Count; i++)
        {
            Inventory[i].OnScore(ref _delta);
        }

        Apply(ref _delta);
    }

    // Every stat change lands here, clamped
    public void Apply(ref TempState delta)
    {
        Health = Mathf.Clamp(Health + delta.Health, 0f, _maxHealth);
        Fuel = Mathf.Clamp(Fuel + delta.Fuel, 0f, _maxFuel);

        Score += delta.Score;
        Distance += delta.Distance;
    }
}
