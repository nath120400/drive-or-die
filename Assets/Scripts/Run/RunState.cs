using UnityEngine;

// Single owner of the run state: every stat change flows through Apply,
// clamping here so no caller can push Health/Fuel out of range
public class RunState
{
    public float Health { get; private set; }
    public float Fuel { get; private set; }
    public float Score { get; private set; }
    public float Distance { get; private set; }

    public float MaxHealth { get; }
    public float MaxFuel { get; }

    // The run is lost when either gauge runs dry
    public bool IsOver => Health <= 0f || Fuel <= 0f;

    public RunState(float maxHealth, float maxFuel)
    {
        MaxHealth = maxHealth;
        MaxFuel = maxFuel;

        Health = maxHealth;
        Fuel = maxFuel;
    }

    public void Apply(ref DeltaStat delta)
    {
        Health = Mathf.Clamp(Health + delta.Health, 0f, MaxHealth);
        Fuel = Mathf.Clamp(Fuel + delta.Fuel, 0f, MaxFuel);
        
        Score += delta.Score;
        Distance += delta.Distance;
    }
}
