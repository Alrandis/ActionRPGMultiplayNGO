using UnityEngine;
using Unity.Netcode;
using System.Collections;
using Unity.Collections;

/// <summary>
/// Хранит и синхронизирует характеристики игрока
/// </summary>
public class PlayerStats : NetworkBehaviour
{
    private float _normalSpeed = 5f;
    public NetworkVariable<float> moveSpeed = new NetworkVariable<float>(5f);

    public void ApplySpeedBonus(float bonus, int duration)
    {
        moveSpeed.Value += bonus;
        StartCoroutine(BonusDuration(duration));
    }

    private IEnumerator BonusDuration(int duration)
    {
        yield return new WaitForSeconds(duration);
        moveSpeed.Value = _normalSpeed;
    }
}
