    using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Pool;
using static UnityEngine.Rendering.DebugUI;

public class PlayerCombat : MonoBehaviour, ICombatEvents
{
    private Animator anim;
    private Rigidbody rb;
    private PlayerStats stats;

    public MeleeHitbox[] hitboxes;

    private float lastAttackTime;
    private int comboIndex = 0;
    private float comboResetTime = 1.5f;

    private bool isAttacking = false;
    private bool inputReserved = false; // 선입력 예약

    public bool IsAttacking { get { return isAttacking; } }

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        stats = GetComponent<PlayerStats>();
    }

    private void ExecuteAttack()
    {
        if (Time.time - lastAttackTime > comboResetTime)
        {
            comboIndex = 0;
        }

        isAttacking = true;
        inputReserved = false; // 예약 초기화

        anim.SetInteger("ComboIndex", comboIndex);
        anim.SetTrigger("StandAttack");

        // 물리 관성을 제거하여 공격 시 미끄러짐 방지
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }

        lastAttackTime = Time.time;
    }

    public void HandleAttackInput(bool isMoving, float attackSpeed)
    {
        float attackInterval = 1f / attackSpeed;
        float timeSinceLastAttack = Time.time - lastAttackTime;

        anim.speed = attackSpeed;

        // 이동 중 공격 (RunAttack)
        if (isMoving)
        {
            // 이동 공격은 콤보가 아니므로 최소 Interval이 지나야만 재발동 허용 (끊김 방지)
            if (timeSinceLastAttack >= attackInterval)
            {
                isAttacking = false;
                inputReserved = false;
                comboIndex = 0;

                int randomIndex = Random.Range(0, 2);
                anim.SetInteger("RunAttackIndex", randomIndex);
                anim.SetTrigger("RunAttack");
                lastAttackTime = Time.time;
            }
            return;
        }

        // 제자리 콤보 공격 (StandAttack)
        if (isAttacking)
        {
            // 공격 도중 마우스를 누르면 예약만 함
            // 간격의 30% 정도 지났을 때부터 예약 허용
            if (timeSinceLastAttack >= attackInterval * 0.3f)
            {
                inputReserved = true;
            }
        }
        else
        {
            // 첫 공격 시작
            if (timeSinceLastAttack >= attackInterval)
            {
                ExecuteAttack();
            }
        }
    }

    // 애니메이션 이벤트에서 호출
    public void OnCheckCombo()
    {
        if (inputReserved)
        {
            // 다음 콤보 인덱스로 업데이트 후 실행
            comboIndex = (comboIndex + 1) % 3;
            // anim.ResetTrigger("StandAttack");
            ExecuteAttack();
        }
    }

    public void OnAttackEnd()
    {
        if (!inputReserved)
        {
            isAttacking = false;
            // 예약이 없었다면 콤보 인덱스 초기화 로직 등 실행

            anim.ResetTrigger("StandAttack");
            comboIndex = 0;
        }
    }

    public void Hit()
    {

    }

    public void OnDashMoveStart()
    {

    }

    public void EnableAttack(string parameters)
    {
        string[] splitData = parameters.Split('/');

        if (splitData.Length >= 2)
        {
            float damageMultiplier = float.Parse(splitData[0]);
            float knockbackPower = float.Parse(splitData[1]);

            float finalDamage = stats.damage * damageMultiplier;

            foreach (var hitbox in hitboxes)
            {
                hitbox.knockbackPower = knockbackPower;
                hitbox.SetDamage(finalDamage);
                hitbox.EnableHitBox();
            }
        }
    }

    public void DisableAttack()
    {
        foreach (var hitbox in hitboxes)
        {
            hitbox.knockbackPower = 0;
            hitbox.DisableHitBox();
        }
    }

    public void CancelAttack()
    {
        isAttacking = false;
        inputReserved = false; // 예약 초기화
        comboIndex = 0;        // 콤보 인덱스 초기화

        // 애니메이터 초기화 (모든 트리거 제거)
        if (anim != null)
        {
            anim.ResetTrigger("StandAttack");
            anim.ResetTrigger("RunAttack");
            anim.SetInteger("ComboIndex", 0);
        }

        // 히트박스 끄기
        if (hitboxes != null)
        {
            foreach (var hitbox in hitboxes)
            {
                hitbox.DisableHitBox();
            }
        }
    }
}
