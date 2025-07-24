public interface IDamageable
{
    void Damage(PlayerVehicleController playerVehicleController, string playerName);
    ulong GetKillerClientId();
    int GetResapwnTİmer();
    int GetDamageAmount();
    string GetKillerName();
}
