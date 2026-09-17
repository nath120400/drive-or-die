using UnityEngine;

[CreateAssetMenu(menuName = "Entities/Instant Heal")]
// Instant heal on collision: the fraction of the max health it restores
public class InstantHeal : EntityDescription
{
    [SerializeField, Range(0f, 1f)] private float _heal;

    public override void OnCollide(ref TempState delta)
    {
        delta.Health += _heal * delta.MaxHealth;
    }
}
