using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;      // 추적할 대상 (Player)

    [Header("Offset Settings")]
    [SerializeField] private float height = 10f;    // 카메라 높이
    [SerializeField] private float distance = 10f;  // 캐릭터와의 수평 거리
    [SerializeField] private float angle = 45f;     // 내려다보는 각도 (X축 회전)

    [Header("Smoothing")]
    [SerializeField] private float smoothSpeed = 5f; // 추적 부드러움 정도

    private void LateUpdate()
    {
        if (target == null) return;

        // 캐릭터 뒤쪽(distance)과 위쪽(height)으로 떨어진 좌표 구하기
        Vector3 worldOffset = new Vector3(0, height, -distance);
        Vector3 targetPosition = target.position + worldOffset;

        // FixedUpdate 이후에 실행되는 LateUpdate에서 카메라를 옮겨 떨림을 방지
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);

        // 쿼터뷰 특유의 고정 각도를 유지
        transform.rotation = Quaternion.Euler(angle, 0, 0);
    }
}
