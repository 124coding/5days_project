using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    public float damageAmount = 15f;

    public float knockbackPower = 0f;

    public string targetTag;

    [Header("VFX (시각 효과)")]
    public GameObject hitEffectPrefab;

    private Collider hitboxCollider;

    public void SetDamage(float damage)
    {
        damageAmount = damage;
    }

    private void Awake()
    {
        hitboxCollider = GetComponent<Collider>();
        // 기본적으로 타격 박스는 끄기
        hitboxCollider.enabled = false;
    }

    // 애니메이션 이벤트를 활용하여 Collider 껏다 켰다 하기
    public void EnableHitBox() => hitboxCollider.enabled = true;
    public void DisableHitBox() => hitboxCollider.enabled = false;

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag(targetTag))
        {

            IDamageable target = other.GetComponent<IDamageable>();
            if(target != null)
            {

                Vector3 hitPoint = other.ClosestPoint(transform.position);
                
                Vector3 hitDirection = (other.transform.position - transform.position).normalized;
                hitDirection.y = 0;

                target.TakeDamage(damageAmount, hitPoint, hitDirection, knockbackPower);

                // 이펙트 소환
                SpawnHitEffect(hitPoint, hitDirection);

                DisableHitBox();
            }
        }
    }

    private void SpawnHitEffect(Vector3 hitPoint, Vector3 hitDirection)
    {
        if (hitEffectPrefab == null) return;

        // 이펙트 생성
        GameObject effect = Instantiate(hitEffectPrefab, hitPoint, Quaternion.LookRotation(-hitDirection));

        // 1초 뒤에 메모리에서 삭제 (나중에 최적화가 필요 시 오브젝트 풀링)
        Destroy(effect, 1f);
    }
}
