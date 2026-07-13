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
        // Agent가 경로를 강제로 이동하도록 설정
        controller.Agent.isStopped = false;
        controller.Agent.speed = speed;

        Vector3 dirToPlayer = (playerPos - transform.position);
        dirToPlayer.y = 0;

        // 플레이어와의 거리가 range보다 가까우면 뒤로, 멀면 앞으로 조정하여 거리를 유지
        float distance = dirToPlayer.magnitude;
        float distOffset = (distance - range) * 0.5f;

        // 플레이어를 바라보는 오른쪽 벡터 계산
        Vector3 rightDir = Vector3.Cross(Vector3.up, dirToPlayer.normalized);

        // 옆으로 이동할 위치 계산 (플레이어로부터의 거리 유지 + 게걸음 방향)
        Vector3 targetPos = transform.position + (rightDir * strafeDir) + (dirToPlayer.normalized * distOffset);

        // velocity 직접 수정 대신, 목적지를 계속 갱신
        controller.Agent.SetDestination(targetPos);

        SetAnimParameter();
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
