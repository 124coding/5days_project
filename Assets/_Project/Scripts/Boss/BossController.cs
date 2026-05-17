using UnityEngine;
using UnityEngine.AI;

public class BossController : MonoBehaviour
{
    public enum BossState { Idle, Chase, CombatStance, Attack, Groggy, Dead }
    public BossState currentState = BossState.Idle;

    public BossMovement Movement { get; private set; }
    public BossCombat Combat { get; private set; }
    public BossStat Stat { get; private set; }

    [Header("Settings")]
    public Transform player;

    public NavMeshAgent Agent { get; private set; }
    private Animator anim;

    private float groggyTimer = 0f;

    void Awake()
    {
        Movement = GetComponent<BossMovement>();
        Combat = GetComponent<BossCombat>();
        Stat = GetComponent<BossStat>();

        Agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
 
        // 정지 거리를 공격 사거리보다 살짝 짧게 두어 자연스럽게 멈추게 함
        Agent.stoppingDistance = Stat.attackRange - 0.5f;
        currentState = BossState.Chase;
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (currentState == BossState.Dead) return;
        
        switch (currentState)
        {
            case BossState.Idle:
                break;

            case BossState.Chase:
                UpdateChase();
                break;

            case BossState.CombatStance:
                UpdateCombatStance(distance);
                break;

            case BossState.Attack:
                UpdateAttack();
                break;

            case BossState.Groggy:
                UpdateGroggy();
                break;
        }
    }

    void UpdateChase()
    {
        Movement.MoveToTarget(player.position, Stat.Speed);

        float distance = Vector3.Distance(transform.position, player.position);
        if(distance < Stat.combatRange)
        {
            currentState = BossState.CombatStance;
        }
    }

    void UpdateCombatStance(float distance)
    {
        if (distance > Stat.combatRange + 2.0f)
        {
            ChangeState(BossState.Chase);
            return;
        }

        Movement.LookAt(player.position);

        if (distance <= Stat.attackRange &&
        Combat.currentStanceAction != BossCombat.StanceAction.BackStep &&
        Time.time >= Combat.lastAttackEndTime + 1.5f)
        {
            Combat.nextDecisionTime = Time.time;
        }

        if (Time.time >= Combat.nextDecisionTime)
        {
            Combat.DecideNextAction(distance);
        }

        // 이동 로직 씹힘 방지
        if (currentState != BossState.CombatStance) return;

        // Combat이 결정한 전술에 따라 다르게 이동
        switch (Combat.currentStanceAction)
        {
            case BossCombat.StanceAction.Wait:
                Movement.Stop();
                break;

            case BossCombat.StanceAction.Strafe:
                // 거리를 유지하며 빙빙 돌기
                Movement.Strafe(player.position, Stat.combatRange, Stat.strafeSpeed);
                break;

            case BossCombat.StanceAction.Approach:
                // 거리를 좁히기 위해 안으로 파고들기
                Movement.MoveToTarget(player.position, Stat.Speed);
                break;

            case BossCombat.StanceAction.BackStep:
                Movement.ExecuteBackstep(player.position, Stat.strafeSpeed);
                break;
        }
    }

    void UpdateAttack()
    {
        // 이동 멈춤
        Agent.isStopped = true;
        anim.SetFloat("Speed", 0f);
        anim.SetFloat("Horizontal", 0f);
        anim.SetFloat("Vertical", 0f);
    }

    void UpdateGroggy()
    {
        groggyTimer -= Time.deltaTime;
        if (groggyTimer <= 0)
        {
            anim.SetBool("IsStunned", false);
            Stat.ResetStance(); // 강인도 회복
            ChangeState(BossState.Chase);
        }
    }

    public void ChangeState(BossState state)
    {
        if (currentState == BossState.Dead) return;

        // 그로기 진입 시 설정
        if (state == BossState.Groggy)
        {
            groggyTimer = 10f; // 10초간 지침
            Movement.Stop();
            Combat.CancelAttack();
            anim.SetBool("IsStunned", true);
        }

        currentState = state;
    }
}
