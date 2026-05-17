using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour, IMovementEvents
{
    private PlayerController controller;
    private PlayerCombat combat;
    private PlayerStats stats;
    private Animator anim;
    private Rigidbody rb;

    public LayerMask groundLayer;

    private bool isRolling = false;

    public bool IsRolling { get { return isRolling; } }

    void Awake()
    {
        controller = GetComponent<PlayerController>();
        combat = GetComponent<PlayerCombat>();
        stats = GetComponent<PlayerStats>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    public void HandleMovement(float h, float v)
    {
        // 이동 계산
        Vector3 worldMoveDir = (Vector3.forward * v + Vector3.right * h).normalized;

        Vector3 localMove = transform.InverseTransformDirection(worldMoveDir);

        // 애니메이션 파라미터 갱신
        anim.SetFloat("InputX", localMove.x);
        anim.SetFloat("InputZ", localMove.z);
        // sanim.SetFloat("Speed", new Vector2(h, v).magnitude);

        // 물리 회전
        LookAtMouse(stats.rotationSpeed);

        // 물리 이동
        if (worldMoveDir.magnitude >= 0.1f)
        {
            rb.MovePosition(rb.position + worldMoveDir * stats.moveSpeed * Time.fixedDeltaTime);
        }
    }

    private void LookAtMouse(float rotationSpeed)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            Vector3 targetPoint = new Vector3(hit.point.x, transform.position.y, hit.point.z);
            Vector3 lookDir = (targetPoint - transform.position).normalized;
            if (lookDir != Vector3.zero)
            {
                // 마우스 바라보기
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
            }
        }
    }

    public void StartRoll(float h, float v)
    {
        isRolling = true;

        combat.CancelAttack();

        Vector3 inputDir = new Vector3(h, 0, v).normalized;

        if(inputDir.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(inputDir);
        }

        controller.stats.isStunned = true;
        controller.stats.canRollCancel = false;

        anim.SetTrigger("Roll");

        // 구르는 동안 물리적 충돌이나 관성 방지
        rb.linearVelocity = Vector3.zero;
    }

    // 애니메이션 이벤트
    public void OnRollEnd()
    {
        isRolling = false;

        // 각속도 Zero
        rb.angularVelocity = Vector3.zero;
        controller.stats.EndStun();
    }
    public void FootR()
    {

    }
    public void FootL()
    {

    }
    public void StartRollInvincible()
    {
        if (stats != null)
        {
            stats.isInvincible = true;
        }
    }

    public void RollInvincibleEnd()
    {
        if (stats != null)
        {
            stats.isInvincible = false;
        }
    }
}
