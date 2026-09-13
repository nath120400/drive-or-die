using UnityEngine;

public class RunManager : MonoBehaviour
{
    [SerializeField] private CarManager _car;

    [Header("Tick")]
    [SerializeField] private float _tickInterval = 1f / 3f;
    [SerializeField] private int _maxDifficultyTicks = 1800; // 10 minutes at 3 ticks per second

    // Difficulty cursor: ticks since run start over the max difficulty ticks

    // Reused delta per tick: no allocation
    private DeltaStat _delta = new DeltaStat();

    private float _tickTimer;
    private int _tickCount;

    public float Difficulty => Mathf.Clamp01((float)_tickCount / _maxDifficultyTicks);

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

        // The car lerps toward its own target as the difficulty rises
        _car.SetProgress(Difficulty);

        // The score is the distance covered during the tick
        _delta.Reset();

        _delta.Fuel = -_car.FuelPerTick * _car.ForwardSpeed;
        _delta.Score = _car.ForwardSpeed;
        _delta.Distance = _car.ForwardSpeed;

        // The inventory items adjust the score before it lands
        for (int i = 0; i < _car.Inventory.Count; i++)
        {
            _car.Inventory[i].OnScore(ref _delta);
        }

        _car.State.Apply(ref _delta);

    }
}
