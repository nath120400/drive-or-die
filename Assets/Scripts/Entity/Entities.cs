using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Entities/List")]
public class Entities : ScriptableObject
{
    public List<EntityDescription> Descriptions = new List<EntityDescription>();
}
