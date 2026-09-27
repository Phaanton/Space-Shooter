using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingMissile : MonoBehaviour
{
    [SerializeField] public float startSpeed = 1f;
    [SerializeField] public float startRotation = 0f;

    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 200f;
    [SerializeField] private float startDelay = 0.5f;
    [SerializeField] private float maxLifetime = 5f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int damage = 1;

    [Header("Visual Effects")]
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private bool useTrail = true;
    [SerializeField] private GameObject explosionEffect;

    private Transform target;
    private Rigidbody2D rb;
    private float startTimer;
    private bool isHoming = false;
    private float lifeTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startTimer = startDelay;
        lifeTimer = maxLifetime;

        // Encontrar o inimigo mais próximo ao instanciar
        FindClosestEnemy();
    }

    void Update()
    {
        // Timer de vida máxima
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0)
        {
            Destroy(gameObject);
            return;
        }

        // Timer de delay inicial
        if (startTimer > 0)
        {
            startTimer -= Time.deltaTime;
            if (startTimer <= 0)
            {
                isHoming = true;
                // Atualizar o alvo quando começar a perseguir
                FindClosestEnemy();
            }
        }
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            if (!isHoming)
            {
                // Ficar inerte durante o delay
                rb.linearVelocity = Vector2.left * startSpeed;
                rb.rotation = startRotation;
            }
            else
            {
                // Perseguir o alvo
                if (target != null)
                {
                    Homing();
                }
                else
                {
                    // Se não tem alvo, procurar novamente
                    FindClosestEnemy();
                    if (target == null)
                    {
                        // Se ainda não tem alvo, seguir em frente
                        rb.linearVelocity = transform.up * speed;
                    }
                }

                if (trailRenderer != null && useTrail)
                {
                    trailRenderer.enabled = true;
                }
            }
        }
    }

    void Homing()
    {
        if (target == null) return;

        Vector2 direction = (Vector2)target.position - rb.position;
        direction.Normalize();

        float rotateAmount = Vector3.Cross(direction, transform.up).z;
        rb.angularVelocity = -rotateAmount * rotationSpeed;
        rb.linearVelocity = transform.up * speed;
    }

    void FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closestDistance = Mathf.Infinity;
        GameObject closestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        if (closestEnemy != null)
        {
            target = closestEnemy.transform;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<Enemy>()?.TakeDamage(damage);
        }

        if (other.CompareTag("Boss"))
        {
            other.GetComponent<BossDrone>()?.TakeDamage(damage);
        }

        Instantiate(explosionEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
