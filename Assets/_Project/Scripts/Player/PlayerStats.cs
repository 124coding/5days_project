using UnityEngine;
using UnityEngine.InputSystem.XR;
using System.Collections;

[System.Serializable]
public class PlayerStats : MonoBehaviour, IDamageable
{
    private PlayerCombat combat;
    private Animator anim;

    [Header("UI 연결")]
    public UIManager uiManager;

    [Header("--- Health Stats ---")]
    public float maxHp = 100f; // 최대 체력
    public float currentHp = 100f; // 현재 체력
    public float defense = 5f; // 방어력

    [Header("--- Combat Stats ---")]
    public float damage = 10f; // 공격력
    public float attackSpeed = 1f; // 초당 공격 횟수
    public float attackRange = 2f; // 공격 범위

    [Header("--- Movement Stats ---")]
    public float moveSpeed = 5f; // 속도
    public float rotationSpeed = 10f; // 회전 속도

    public bool isKnockedBack = false;
    public bool isStunned = false;
    public bool canRollCancel = false;
    public bool isInvincible = false;
    public bool isDead = false;

    void Awake()
    {
        combat = GetComponent<PlayerCombat>();
        anim = GetComponent<Animator>();
        
        currentHp = maxHp;

        if (uiManager != null)
        {
            uiManager.UpdatePlayerHP(currentHp, maxHp);
        }
    }

    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitDirection, float knockbackPower)
    {
        if (isInvincible) return;
        if (currentHp <= 0.0f) return;

        isStunned = false;
        isInvincible = false;
        isKnockedBack = false;

        currentHp -= amount;

        if (uiManager != null)
        {
            uiManager.UpdatePlayerHP(currentHp, maxHp);
        }

        combat.CancelAttack();

        Vector3 localHitPoint = transform.InverseTransformPoint(hitPoint);

        // 앞뒤, 좌우만 남김
        Vector3 hitDir2D = new Vector3(localHitPoint.x, 0, localHitPoint.z).normalized;

        if (knockbackPower > 0f)
        {
            isStunned = true;
            anim.SetTrigger("Knockdown");
            StartCoroutine(KnockbackRoutine(hitDirection, knockbackPower, 0.5f));
        }
        else
        {
            isStunned = true;
            anim.SetFloat("HitX", hitDir2D.x);
            anim.SetFloat("HitZ", hitDir2D.y);

            anim.SetTrigger("Hit");
        }

        if (currentHp <= 0.0f)
        {
            Die();
            return;
        }
    }

    private IEnumerator KnockbackRoutine(Vector3 direction, float force, float duration)
    {
        isKnockedBack = true;
        float timer = 0f;
        Rigidbody rb = GetComponent<Rigidbody>();

        // 날아가기 직전 기존 물리, 회전 관성 0
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        while (timer < duration)
        {
            float speed = Mathf.Lerp(force, 0f, timer / duration);

            rb.MovePosition(rb.position + direction * speed * Time.deltaTime);

            timer += Time.deltaTime;
            yield return null;
        }

        isKnockedBack = false;
    }

    private void Die()
    {
        isDead = true;
        isStunned = false;

        anim.SetTrigger("Die");

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (uiManager != null)
        {
            uiManager.ShowLoseUIDelayed(3f);
        }
    }

    // 애니메이션 이벤트
    public void AllowRollCancel()
    {
        canRollCancel = true;
    }

    public void EndStun()
    {
        isStunned = false;
        canRollCancel = false;
    }
}
