using UnityEngine;

[CreateAssetMenu(menuName = "Entities/Instant Damage")]
// Instant damage on collision: flat health points, not a gauge fraction
public class InstantDamage : EntityDescription
{
    [SerializeField] private int _damage;

    public override void OnCollide(ref TempState delta)
    {
        delta.Health -= _damage;
    }
}
