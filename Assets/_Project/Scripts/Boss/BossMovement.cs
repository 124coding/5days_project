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
        controller.Agent.isStopped = false;

        // 플레이어를 향하는 방향 (땅바닥 기준 2D 평면화)
        Vector3 dirToPlayer = (playerPos - transform.position);
        dirToPlayer.y = 0;

        // 완벽한 게걸음 방향 (외적 활용)
        // 플레이어를 바라보는 방향의 완벽한 오른쪽 직각 벡터
        Vector3 rightDir = Vector3.Cross(Vector3.up, dirToPlayer.normalized);

        // strafeDir이 1이면 오른쪽, -1이면 왼쪽 게걸음
        Vector3 strafeVector = rightDir * strafeDir;

        // SetDestination을 쓰지 않고 매 프레임 Agent의 속도를 강제로 조종!
        Vector3 moveVelocity = strafeVector.normalized * speed;
        controller.Agent.velocity = moveVelocity;

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
