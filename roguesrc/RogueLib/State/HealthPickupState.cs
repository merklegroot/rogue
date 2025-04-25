namespace RogueLib.State;

public class HealthPickupState
{
    public readonly List<HealthPickup> HealthPickups = [];
    public float _timeSinceLastHealthSpawn = 0f;
}