using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [Header("Configura��es")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private bool useRigidbody = true;

    [SerializeField] private GameObject explosionEffect;

    private Vector2 direction;
    private Rigidbody2D rb;

    void Start()
    {
        if (useRigidbody)
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0;
                rb.isKinematic = false;
            }
        }

        // Destroi ap�s o tempo de vida
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (!useRigidbody)
        {
            // Movimento manual
            transform.Translate(direction * speed * Time.deltaTime);
        }
    }

    public void Initialize(Vector2 dir, float spd, int dmg)
    {
        direction = dir.normalized;
        speed = spd;
        damage = dmg;

        if (useRigidbody && rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
    }

    public void OnCollisionEnter2D(Collision2D other)
    {
        // Causa dano ao player
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerController>()?.TakeDamage(damage);
        }

        Instantiate(explosionEffect, transform.position, Quaternion.identity);

        // Destroi o tiro
        Destroy(gameObject);


        // Se quiser destruir ao bater em paredes, use a tag "Wall"
        if (other.gameObject.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}