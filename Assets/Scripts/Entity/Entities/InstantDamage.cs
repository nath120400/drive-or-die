using UnityEngine;

[CreateAssetMenu(menuName = "Entities/Instant Damage")]
public class InstantDamage : EntityDescription
{
    [SerializeField] private int _damage;

    public override void OnCollide(ref DeltaStat delta)
    {
        delta.Health -= _damage;
    }
}
