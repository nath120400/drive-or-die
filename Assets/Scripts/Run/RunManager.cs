using UnityEngine;

public class RunManager : MonoBehaviour
{
    [SerializeField] private CarManager _car;

    [Header("Tick")]
    [SerializeField] private float _tickInterval = 1f / 3f;
    [SerializeField] private float _scorePerTick = 10f;

    // Reused delta per tick: no allocation
    private DeltaStat _delta = new DeltaStat();
    private float _tickTimer;

    private void Update()
    {
        // Run ticks: the run state advances once per interval
        _tickTimer += Time.deltaTime;
        while (_tickTimer >= _tickInterval)
        {
            _tickTimer -= _tickInterval;

            _delta.Reset();
            _delta.Fuel -= _car.FuelPerTick;
            _delta.Score += _scorePerTick;
            _car.Apply(_delta);
        }
    }
}
