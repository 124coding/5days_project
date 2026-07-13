using System.Collections;
using System.Security.Cryptography;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using static BossController;

public class BossCombat : MonoBehaviour, ICombatEvents
{
    public enum AttackType { Melee, Dash } // 공격 종류

    public enum StanceAction { Wait, Strafe, Approach, BackStep };
    public StanceAction currentStanceAction = StanceAction.Strafe;

    public MeleeHitbox hitbox;

    private Animator anim;
    private BossController controller;
    private BossStat stat;

    public float nextDecisionTime = 0f;
    public float lastAttackEndTime = 0f;

    public bool IsAttacking { get; private set; }

    private void Awake()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<BossController>();
        stat = GetComponent<BossStat>();
    }

    public void CancelAttack()
    {
        IsAttacking = false;

        // 켜져 있던 보스의 히트박스(검) 끄기
        if (hitbox != null)
        {
            hitbox.DisableHitBox();
        }

        // 혹시 씹혀서 남아있을지 모르는 공격 트리거 찌꺼기 청소 (안전장치)
        anim.ResetTrigger("Attack");
    }

    public void DecideNextAction(float distance)
    {
        nextDecisionTime = Time.time + Random.Range(1.5f, 3f);

        float randomVal = Random.value;

        // 공격 범위 내에 들어왔을 때 우선 처리
        if (distance <= stat.attackRange)
        {
            if(randomVal < 0.3f)
            {
                currentStanceAction = StanceAction.BackStep;
                return;
            }

            ExecuteAttack(AttackType.Melee);
            return;
        }
        else if (distance <= stat.combatRange && Random.value > 0.3f)
        {
            ExecuteAttack(AttackType.Dash);
            return;
        }

        // 대치 중 움직임 결정
        if (randomVal > 0.8f)
        {
            currentStanceAction = StanceAction.Wait; // 멈춰서 노려보기
        }
        else if (randomVal > 0.4f)
        {
            currentStanceAction = StanceAction.Strafe; // 옆으로 돌기
            controller.Movement.ToggleStrafeDirection();
        }
        else
        {
            currentStanceAction = StanceAction.Approach;
        }
    }

    public void ExecuteAttack(AttackType type)
    {
        if (IsAttacking) return;

        IsAttacking = true;
        controller.ChangeState(BossController.BossState.Attack);

        StartCoroutine(AttackWindUpRoutine(type));
    }

    private IEnumerator AttackWindUpRoutine(AttackType type)
    {
        controller.Movement.LookAtInstantly(controller.player.position);

        yield return new WaitForSeconds(0.2f);

        // 애니메이터에 어떤 공격인지 파라미터 전달
        anim.SetInteger("AttackType", (int)type);
        anim.SetTrigger("Attack");

        if (type == AttackType.Melee)
        {
            int randIndex = Random.Range(0, 4);
            anim.SetInteger("AttackIndex", randIndex);
        }
    }

    // 애니메이션 이벤트에서 호출
    public void OnCheckCombo()
    {
    }

    public void OnAttackEnd()
    {
        IsAttacking = false;
        controller.Agent.isStopped = false;

        lastAttackEndTime = Time.time;

        anim.ResetTrigger("Attack");

        controller.ChangeState(BossController.BossState.Chase);
    }

    public void Hit()
    {

    }

    public void OnDashMoveStart()
    {
        controller.Movement.ForceDash(0.2f, 15f);
    }

    public void EnableAttack(string parameters)
    {
        if (string.IsNullOrEmpty(parameters)) return;

        string[] splitData = parameters.Split('/');

        if (splitData.Length >= 2)
        {
            bool isDamageParsed = float.TryParse(splitData[0], out float damageMultiplier);
            bool isKnockbackParsed = float.TryParse(splitData[1], out float knockbackPower);

            if (isDamageParsed && isKnockbackParsed)
            {
                float finalDamage = controller.Stat.damage * damageMultiplier;

                hitbox.knockbackPower = knockbackPower;
                hitbox.SetDamage(finalDamage);
                hitbox.EnableHitBox();
            }
            else
            {
                Debug.LogError($"[BossCombat] 공격 파라미터 변환 실패: {parameters}");
            }
        }
    }

    public void DisableAttack()
    {
        hitbox.knockbackPower = 0;
        hitbox.DisableHitBox();
    }
}
