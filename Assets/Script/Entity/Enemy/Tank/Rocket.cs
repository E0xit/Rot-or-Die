using UnityEngine;

public class Rocket : MonoBehaviour
{
    [Header("Rocket Settings")]
    public float speed = 8f;
    public float damage = 40f;
    public float destroyTime = 6f;

    [Header("Reflect State")]
    public bool isReflected = false;
    public string playerMeleeTag = "Melee";
    public string bossTag = "Enemy";

    [SerializeField]
    private GameObject explosion;

    private Transform bossTarget;

    private void Start()
    {
        Destroy(gameObject, destroyTime);
    }

    public void SetBossTarget(Transform boss)
    {
        bossTarget = boss;
    }

    private void Update()
    {
        if (!isReflected)
        {
            // พุ่งตรงไปตามทิศหัวจรวด (Vector3.up อิงตาม rotation ของ Turret ตอนยิง)
            transform.Translate(Vector3.up * speed * Time.deltaTime);
        }
        else
        {
            // 🔄 โดนสะท้อน: พุ่งย้อนกลับไปหาบอส
            if (bossTarget != null)
            {
                Vector2 direction = (bossTarget.position - transform.position).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.Euler(0, 0, angle);

                transform.position = Vector3.MoveTowards(transform.position, bossTarget.position, speed * 1.5f * Time.deltaTime);
            }
            else
            {
                transform.Translate(Vector3.up * speed * 1.5f * Time.deltaTime);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ⚔️ 1. สัมผัส Melee -> สะท้อนกลับ!
        if (!isReflected && other.CompareTag(playerMeleeTag))
        {
            ReflectRocket();
            return;
        }

        // 💥 2. ชน Player
        if (!isReflected && other.CompareTag("Wall"))
        {
            IDamageable player = other.GetComponent<IDamageable>();
            if (player != null) player.TakeDamage(damage);
            Instantiate(explosion,transform.position,transform.rotation);
            Destroy(gameObject);
        }
        // 💥 3. จรวดสะท้อนกลับมาชนตัวบอส (Hull / Tank Parent)
        else if (isReflected && (other.CompareTag(bossTag) || other.transform == bossTarget || other.transform.IsChildOf(bossTarget)))
        {
            IDamageable boss = other.GetComponentInParent<IDamageable>();
            if (boss != null)
            {
                boss.TakeDamage(damage * 1.5f);
                Debug.Log("Reflected Rocket Hit Boss!");
            }
            Instantiate(explosion,transform.position,transform.rotation);
            Destroy(gameObject);
        }
    }

    private void ReflectRocket()
    {
        isReflected = true;
        Debug.Log("Rocket Parried!");
        transform.Rotate(0, 0, 180f);
    }
}