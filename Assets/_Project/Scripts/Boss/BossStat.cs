using UnityEngine;
using System.Collections;

public class BossStat : MonoBehaviour, IDamageable
{
    [Header("UI 연결")]
    public UIManager uiManager;

    [Header("Stats")]
    public float maxHp = 500f;
    public float currentHp;
    public float maxStance = 100f; // 강인도 (슈퍼 아머)
    public float currentStance;

    public bool isStunned = false;

    public float Speed = 3f;
    public float damage = 10f;
    public float combatRange = 5f;    // 대치 거리를 유지할 범위
    public float attackRange = 3f;
    public float strafeSpeed = 0.5f;    // 측면 이동 속도

    private Animator anim;
    private BossController controller;

    void Awake()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<BossController>();
        currentHp = maxHp;
        currentStance = maxStance;

        if (uiManager != null)
        {
            uiManager.UpdateBossHP(currentHp, maxHp);
            uiManager.UpdateBossStance(currentStance, maxStance);
        }
    }

    // 데미지를 받는 함수 (외부에서 호출)
    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitDirection, float knockbackPower)
    {
        if (controller.currentState == BossController.BossState.Dead) return;

        Debug.Log("Hit");

        currentHp -= amount;

        if (uiManager != null)
        {
            uiManager.UpdateBossHP(currentHp, maxHp);
        }

        if (isStunned)
        {
            anim.SetTrigger("Hit");
        }

        // 강인도가 다 깎였을 때 피격(Hit) 상태로 전환 요청
        if (controller.currentState != BossController.BossState.Groggy)
        {
            currentStance -= amount;

            if (uiManager != null)
            {
                uiManager.UpdateBossStance(currentStance, maxStance);
            }

            if (currentStance <= 0)
            {
                controller.Combat.DisableAttack();
                controller.ChangeState(BossController.BossState.Groggy);
            }
        }
        else
        {
            // 그로기 상태일 때 맞으면 피격 모션 재생
            anim.SetTrigger("Hit");
        }

        if (currentHp <= 0)
        {
            Die();
            return;
        }
    }

    private void Die()
    {
        isStunned = false;
        controller.ChangeState(BossController.BossState.Dead);

        controller.Combat.CancelAttack();

        controller.enabled = false;

        anim.SetBool("IsStunned", false);

        anim.ResetTrigger("Hit");

        anim.SetTrigger("Die");

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        if (uiManager != null)
        {
            uiManager.ShowWinUIDelayed(3f);
        }
    }

    public void ResetStance()
    {
        currentStance = maxStance;
    }

    public void EndStun()
    {

    }
}
