using UnityEngine;

[CreateAssetMenu(menuName = "Entities/Instant Heal")]
public class InstantHeal : EntityDescription
{
    [SerializeField, Range(0f, 1f)] private float _heal;

    public override void OnCollide(ref TempState delta)
    {
        delta.Health += _heal * delta.MaxHealth;
    }
}
