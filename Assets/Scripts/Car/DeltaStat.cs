public struct DeltaStat
{
    public Car Car;
    public float Health;
    public float Fuel;
    public float Distance;
    public float Score;

    public void Reset()
    {
        Car = null;
        Health = 0f;
        Fuel = 0f;
        Distance = 0f;
        Score = 0f;
    }
}
