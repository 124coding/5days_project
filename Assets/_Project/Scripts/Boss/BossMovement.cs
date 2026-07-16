using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class BossMovement : MonoBehaviour, IMovementEvents
{
    private BossController controller;
    private Animator anim;

    public float strafeDir { get; private set; } = 1f;

    public void ToggleStrafeDirection() => strafeDir *= -1f; // 방향 전환 함수

    private void Awake()
    {
        controller = GetComponent<BossController>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        controller.Agent.updateRotation = false;
    }

    // 전진 추격
    public void MoveToTarget(Vector3 targetPos, float speed)
    {
        controller.Agent.isStopped = false;
        controller.Agent.speed = speed;
        controller.Agent.SetDestination(targetPos);

        SetAnimParameter();
    }

    public void Strafe(Vector3 playerPos, float range, float speed)
    {
        // 1. 에이전트의 자동 길찾기 및 목적지 이동을 일시 정지
        controller.Agent.isStopped = true;

        Vector3 dirToPlayer = (playerPos - transform.position);
        dirToPlayer.y = 0;

        // 2. 플레이어 중심의 오른쪽/왼쪽 벡터 계산
        Vector3 rightDir = Vector3.Cross(Vector3.up, dirToPlayer.normalized);

        // 3. 거리 유지를 위한 오프셋 (너무 멀면 다가가고, 너무 가까우면 물러남)
        float distance = dirToPlayer.magnitude;
        float distOffset = (distance - range) * 0.5f;

        // 4. 최종 이동 방향 계산 (게걸음 방향 + 거리 유지 방향)
        Vector3 moveDir = (rightDir * strafeDir) + (dirToPlayer.normalized * distOffset);

        // 5. 직접 속도를 적용하여 이동
        controller.Agent.velocity = moveDir.normalized * speed;

        // 애니메이션 업데이트 (애니메이터 파라미터는 로컬 방향 기준이므로 변환 필요)
        Vector3 localVelocity = transform.InverseTransformDirection(controller.Agent.velocity);
        anim.SetFloat("Vertical", localVelocity.z);
        anim.SetFloat("Horizontal", localVelocity.x);
        anim.SetFloat("Speed", controller.Agent.velocity.magnitude);
    }

    public void Stop()
    {
        controller.Agent.isStopped = true;
        anim.SetFloat("Speed", 0f);

        anim.SetFloat("Horizontal", 0f);
        anim.SetFloat("Vertical", 0f);
    }

    public void LookAt(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        dir.y = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);
    }

    public void LookAtInstantly(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        dir.y = 0;

        if (dir != Vector3.zero) // 완전히 겹쳐있을 때의 에러 방지
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    void SetAnimParameter()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(controller.Agent.velocity);

        anim.SetFloat("Vertical", localVelocity.z);
        anim.SetFloat("Horizontal", localVelocity.x);
        anim.SetFloat("Speed", controller.Agent.velocity.magnitude);
    }

    public void ForceDash(float dashDuration, float dashSpeed)
    {
        StartCoroutine(DashRoutine(dashDuration, dashSpeed));
    }

    private IEnumerator DashRoutine(float duration, float speed)
    {
        float timer = 0f;
        Vector3 dashDirection = transform.forward; // 보스가 현재 바라보는 앞방향

        while (timer < duration)
        {
            controller.Agent.Move(dashDirection * speed * Time.deltaTime);

            timer += Time.deltaTime;
            yield return null;
        }
    }

    public void ExecuteBackstep(Vector3 playerPos, float speed)
    {
        // 급 브레이크 방지
        if (controller.Agent.hasPath)
        {
            controller.Agent.ResetPath();
        }

        Vector3 dirFromPlayer = transform.position - playerPos;
        dirFromPlayer.y = 0;
        Vector3 backwardDir = dirFromPlayer.normalized;

        Vector3 moveVelocity = backwardDir * speed;
        controller.Agent.Move(moveVelocity * Time.deltaTime);

        Vector3 localVelocity = transform.InverseTransformDirection(moveVelocity);

        anim.SetFloat("Vertical", localVelocity.z);
        anim.SetFloat("Horizontal", localVelocity.x);
        anim.SetFloat("Speed", moveVelocity.magnitude);
    }

    // 애니메이션 이벤트
    public void OnRollEnd()
    {

    }
    public void FootR()
    {

    }
    public void FootL()
    {

    }
    public void StartRollInvincible()
    {

    }

    public void RollInvincibleEnd()
    {

    }
}
