using UnityEngine;

[CreateAssetMenu(menuName = "Entities/Instant Fuel")]
// Instant fuel on collision: the fraction of the max tank it refills
public class InstantFuel : EntityDescription
{
    [SerializeField, Range(0f, 1f)] private float _fuel;

    public override void OnCollide(ref TempState delta)
    {
        delta.Fuel += _fuel * delta.MaxFuel;
    }
}
