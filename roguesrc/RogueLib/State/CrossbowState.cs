public class CrossbowState
{
    public bool HasCrossbow { get; set; }
    public float CrossbowCooldown { get; set; } = 2.0f;
    public float CrossbowCooldownTimer { get; set; }
    public bool CrossbowOnCooldown { get; set; }
}