using UnityEngine;

public class AnimationEventProxy : MonoBehaviour
{
    private Weapon sword;

    private void Awake()
    {
        sword = GetComponentInParent<Weapon>();
    }

    public void EnableHitbox() => sword?.EnableHitbox();
    public void DisableHitbox() => sword?.DisableHitbox();
    public void OnAttackAnimationEnd() => sword?.OnAttackAnimationEnd();
}
