using Unity.VisualScripting.Antlr3.Runtime.Misc;
using System.Collections;
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

    private Coroutine rollRoutine;

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
        // 이동 계산 (y축은 중력을 위해 기존 속도 유지)
        Vector3 worldMoveDir = (Vector3.forward * v + Vector3.right * h).normalized;
        Vector3 targetVelocity = worldMoveDir * stats.moveSpeed;

        // y축 속도는 물리 엔진이 처리하도록 현재 y속도를 그대로 보존
        targetVelocity.y = rb.linearVelocity.y;

        // velocity 직접 할당
        rb.linearVelocity = targetVelocity;

        // 애니메이션 파라미터 갱신
        Vector3 localMove = transform.InverseTransformDirection(worldMoveDir);
        anim.SetFloat("InputX", localMove.x);
        anim.SetFloat("InputZ", localMove.z);

        // 물리 회전
        LookAtMouse(stats.rotationSpeed);
    }

    private void LookAtMouse(float rotationSpeed)
    {
        if (combat.IsAttacking || isRolling) return;

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

        rb.angularVelocity = Vector3.zero;

        Vector3 inputDir = new Vector3(h, 0, v).normalized;
        if (inputDir.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(inputDir);
        }

        controller.stats.isStunned = true;
        controller.stats.canRollCancel = false;

        anim.SetTrigger("Roll");
        rb.linearVelocity = Vector3.zero;

        // 기존 코루틴이 있다면 정지 (중복 실행 방지)
        if (rollRoutine != null) StopCoroutine(rollRoutine);

        // 안전 장치용 코루틴 시작
        rollRoutine = StartCoroutine(ForceEndRollRoutine(1f));
    }

    private IEnumerator ForceEndRollRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        // 구르기가 아직 끝나지 않았다면 강제로 종료
        if (isRolling)
        {
            Debug.Log("구르기 시간 초과로 강제 종료");
            OnRollEnd(); // 기존의 종료 로직을 재사용
        }
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
