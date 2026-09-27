using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyParent : MonoBehaviour
{
    [SerializeField] protected float speed;
    [SerializeField] protected int hp;
    [SerializeField] protected GameObject explosion;

    [SerializeField] protected float shootRate = 1f;
    [SerializeField] protected float shootRateTimer = 0f;
    [SerializeField] protected GameObject shoot;
    [SerializeField] protected float shootSpeed = 10f;

    [SerializeField] protected Transform instantiateShoot;

    [SerializeField] protected int scoreValue = 10;

    [SerializeField] protected bool canDrop = true;
    [SerializeField] protected GameObject powerUp;
    [SerializeField] protected int dropChance = 20;

    [SerializeField] protected bool invencible = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DestroySelf()
    {
        if (transform.position.y < -6)
        {
            DeathEffect();
        }
    }

    public void TakeDamage(int damage)
    {
        if (transform.position.y < 5f && !invencible)
        {
            hp -= damage;
            if (hp <= 0)
            {
                DeathEffect(true, canDrop);
            }
        }
    }

    public void Shoot(bool playerDirection = false)
    {
        bool vis = GetComponentInChildren<SpriteRenderer>().isVisible;
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (vis)
        {
            if (player != null)
            {
                if (shootRateTimer <= 0)
                {
                    var my_shoot = Instantiate(shoot, instantiateShoot.position, transform.rotation);

                    if (!playerDirection)
                    {
                        my_shoot.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, -shootSpeed);

                        shootRateTimer = shootRate;
                    }
                    else
                    {
                        Vector2 direction = (player.transform.position - transform.position).normalized;
                        my_shoot.GetComponent<Rigidbody2D>().linearVelocity = direction * shootSpeed;
                        my_shoot.transform.rotation = Quaternion.FromToRotation(Vector3.down, direction);

                        shootRateTimer = shootRate;
                    }
                }

                if (shootRateTimer > 0)
                {
                    shootRateTimer -= Time.deltaTime;
                }
            }
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerController>()?.TakeDamage(1);

            DeathEffect();
        }

        if (collision.gameObject.CompareTag("Shield"))
        {
            TakeDamage(1);
            DeathEffect();
        }
    }

    public void DeathEffect(bool gainScore = false, bool hasDrop = false)
    {
        Instantiate(explosion, transform.position, transform.rotation);

        var scoreUI = Object.FindFirstObjectByType<UIManager>();

        if (scoreUI != null)
        {
            if (gainScore)
            {
                scoreUI.UpdateScore(scoreValue);
            }
        }

        if (canDrop)
        {
            if (hasDrop)
            {
                if (Random.Range(0, 100) < dropChance)
                {
                    var drop = Instantiate(powerUp, transform.position, transform.rotation);
                }
            }
        }
        

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        var generator = Object.FindFirstObjectByType<EnemyGenerator>();

        if (generator != null)
        {
            generator.DecreaseEnemies();
        }
    }
}
