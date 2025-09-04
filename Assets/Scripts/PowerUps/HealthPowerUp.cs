using UnityEngine;

public class HealthPowerUp : PowerUp
{
    public int healthBonus = 30;
    Health _health;
    public override void UpplyPowerUp(GameObject player)
    {
        _health = player.GetComponent<Health>();

        if (_health != null)
            _health.Heal(healthBonus);
    }
}
