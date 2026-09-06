public struct DeltaStat
{
    public CarManager CarManager;
    public float Health;
    public float Fuel;
    public float Distance;
    public float Score;

    public void Reset()
    {
        CarManager = null;
        Health = 0f;
        Fuel = 0f;
        Distance = 0f;
        Score = 0f;
    }
}
