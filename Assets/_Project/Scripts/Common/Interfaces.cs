using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitDirection, float knockbackPower);
}

public interface ICombatEvents
{
    void OnCheckCombo();
    void OnAttackEnd();
    void Hit();
    void OnDashMoveStart();

    void EnableAttack(string parameters);
    void DisableAttack();
}

public interface IMovementEvents
{
    void OnRollEnd();
    void FootR();
    void FootL();
    void StartRollInvincible();
    void RollInvincibleEnd();
}