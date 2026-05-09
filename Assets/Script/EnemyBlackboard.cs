using UnityEngine;

// Pizarra compartida (Blackboard). No es MonoBehaviour — todos los enemigos la leen/escriben directamente.
public static class EnemyBlackboard
{
    // Información del jugador
    public static Vector2 PlayerPosition    { get; set; }
    public static float   PlayerHealthRatio { get; set; } = 1f;
    public static bool    PlayerIsStunned   { get; private set; }
    private static float  _stunEnd;

    public static void SetPlayerStunned(float duration)
    {
        PlayerIsStunned = true;
        _stunEnd = Time.time + duration;
    }

    // Llama esto desde un Manager o desde Player.Update() para limpiar el stun
    public static void Tick()
    {
        if (PlayerIsStunned && Time.time >= _stunEnd)
            PlayerIsStunned = false;
    }

    // Coordinación de embestidas (solo 1 Charger a la vez)
    public static int MaxSimultaneousCharges = 1;
    private static int _activeCharges;

    public static bool RequestCharge()
    {
        if (_activeCharges < MaxSimultaneousCharges) { _activeCharges++; return true; }
        return false;
    }

    public static void ReleaseCharge() => _activeCharges = Mathf.Max(0, _activeCharges - 1);
}
