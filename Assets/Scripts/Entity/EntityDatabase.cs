using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Entities/Database")]
public class EntityDatabase : ScriptableObject
{
    public List<EntityDescription> Descriptions = new List<EntityDescription>();
}
