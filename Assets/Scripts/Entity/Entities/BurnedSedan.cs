using UnityEngine;

[CreateAssetMenu(menuName = "Entities/Burned Sedan")]
public class BurnedSedan : EntityDescription
{
    [SerializeField] private int _damage;

    public override void OnCollide(ref DeltaStat delta)
    {
        delta.Health -= _damage;
    }
}
