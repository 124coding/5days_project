using UnityEngine;

public class AnimationEventReceiver : MonoBehaviour
{
    private IMovementEvents movement;
    private ICombatEvents combat;

    private void Awake()
    {
        movement = GetComponentInParent<IMovementEvents>();
        combat = GetComponentInParent<ICombatEvents>();
    }

    // Combat
    public void OnCheckCombo() => combat.OnCheckCombo();
    public void OnAttackEnd() => combat.OnAttackEnd();
    public void Hit() => combat.Hit(); // »èÁ¦ ¿ä¸Á
    public void OnDashMoveStart() => combat.OnDashMoveStart();

    public void EnableAttack(string parameters) => combat.EnableAttack(parameters);
    public void DisableAttack() => combat.DisableAttack();

    // Movement
    public void OnRollEnd() => movement.OnRollEnd();
    public void FootR() => movement.FootR();
    public void FootL() => movement.FootL();
    public void StartRollInvincible() => movement.StartRollInvincible();
    public void RollInvincibleEnd() => movement.RollInvincibleEnd();
}
