using System.Collections.Generic;
using UnityEngine;

// The single owner of the run: difficulty, speed, stats and the tick
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

    // Difficulty cursor: ticks since run start over the max difficulty ticks
    public float Difficulty => Mathf.Clamp01((float)_tickCount / _maxDifficultyTicks);

    // The current speed rides the difficulty
    public float ForwardSpeed => Mathf.Lerp(_baseSpeed, _targetSpeed, Difficulty);

    // The run state lives here: every stat change lands in Apply
    private RunState _state;
    public RunState State => _state;

    // The held entities: they adjust the score, the deltas and the spawns
    public List<EntityDescription> Inventory = new List<EntityDescription>();

    // Reused delta per tick: no allocation
    private DeltaStat _delta = new DeltaStat();

    private float _tickTimer;
    private int _tickCount;

    private void Start()
    {
        _state = new RunState(_maxHealth, _maxFuel);
    }

    private void Update()
    {
        // Run ticks: the run state advances once per interval
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
        _delta.Reset();

        _delta.Fuel = -_fuelPerTick * ForwardSpeed;
        _delta.Score = ForwardSpeed;
        _delta.Distance = ForwardSpeed;

        // The held items adjust the score before it lands
        for (int i = 0; i < Inventory.Count; i++)
        {
            Inventory[i].OnScore(ref _delta);
        }

        _state.Apply(ref _delta);
    }
}
