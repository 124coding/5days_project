using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerStats stats; // 데이터

    // 컴포넌트들
    private MyPlayerInput input;
    private PlayerMovement movement;
    private PlayerCombat combat;
    private Rigidbody rb;
    private Animator anim;

    void Awake()
    {
        input = GetComponent<MyPlayerInput>();
        movement = GetComponent<PlayerMovement>();
        combat = GetComponent<PlayerCombat>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (stats.isDead) return;
        if (stats.isKnockedBack) return;
        if (movement.IsRolling) return;

        if (input != null && input.IsRoll)
        {
            if (!stats.isStunned || stats.canRollCancel)
            {
                movement.StartRoll(input.H, input.V);
                return;
            }
        }

        if (stats.isStunned) return;

        // 공격 입력 시 실행
        if (input != null && input.IsAttack)
        {
            bool isMoving = new Vector2(input.H, input.V).magnitude > 0.1f;
            combat.HandleAttackInput(isMoving, stats.attackSpeed);
        }
    }

    void FixedUpdate()
    {
        if (stats.isDead) return;
        if (stats.isKnockedBack) return;
        if (movement.IsRolling) return;
        if (stats.isStunned) return;

        // 이동 로직 실행 (입력 데이터와 물리 컴포넌트들을 전달)
        if (movement != null && input != null && !combat.IsAttacking)  movement.HandleMovement(input.H, input.V);
    }
}
