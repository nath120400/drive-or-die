// One stat change in flight: a tick or a collision builds its deltas here,
// the held items can amend them, then the run applies them once
public struct TempState
{
    public Car Car;
    public float MaxHealth;
    public float MaxFuel;
    public float Health;
    public float Fuel;
    public float Distance;
    public float Score;

    public void Reset(float maxHealth, float maxFuel)
    {
        Car = null;
        MaxHealth = maxHealth;
        MaxFuel = maxFuel;
        Health = 0f;
        Fuel = 0f;
        Distance = 0f;
        Score = 0f;
    }
}
