using UnityEngine;

public class AnimationEventProxy : MonoBehaviour
{
    private Sword sword;

    private void Awake()
    {
        sword = GetComponentInParent<Sword>();
    }

    public void EnableHitbox() => sword?.EnableHitbox();
    public void DisableHitbox() => sword?.DisableHitbox();
    public void OnAttackAnimationEnd() => sword?.OnAttackAnimationEnd();
}
