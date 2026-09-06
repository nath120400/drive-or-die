using System.Collections.Generic;
using UnityEngine;

public class CarType : MonoBehaviour
{
    [Header("References")]
    public Transform FrontLeftWheel;
    public Transform FrontRightWheel;
    public Transform RearLeftWheel;
    public Transform RearRightWheel;
    public Transform CarBody;

    [Header("Physical")]
    public float ForwardSpeed;
    public float Wheelbase;
    public float SteeringSpeed;
    public float MaxWheelAngle;
    public float MaxDiagonalAngle;

    [Header("Stats")]
    public float MaxHealth;
    public float MaxFuel;

    [Header("Inventory")]
    public List<InventorySlotCount> Slots = new List<InventorySlotCount>();
}
