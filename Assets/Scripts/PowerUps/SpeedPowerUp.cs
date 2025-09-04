using UnityEngine;

public class SpeedPowerUp : PowerUp
{
    public float speadBonus = 5f;
    public int durationBonus = 5;
    PlayerStats _stats;
    public override void UpplyPowerUp(GameObject player)
    {
        _stats = player.GetComponent<PlayerStats>();

        if (_stats != null)
            _stats.ApplySpeedBonus(speadBonus, durationBonus);
    }
}
