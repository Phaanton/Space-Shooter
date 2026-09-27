using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;
using UnityEngine.Audio;

public class Enemy : MonoBehaviour
{
    public event Action<int> OnDeath;

    [SerializeField] private int scoreValue = 10;
    [SerializeField] private int health = 1;
    [SerializeField] protected GameObject explosion;

    [SerializeField] protected GameObject dropShootUpgrade;
    [SerializeField] protected int dropChance = 90;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color hitFlashColor = new Color(1f, 0.5f, 0f, 1f); // Laranja avermelhado
    [SerializeField] private float flashDuration = 0.1f;

    private Color originalColor;
    private Coroutine flashCoroutine;

    public event Action OnDieByPlayer;

    private bool isDead = false;

    void Awake()
    {
        // Se não atribuiu no Inspector, tenta pegar automaticamente
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // Salva a cor original
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    void Start()
    {
        // Se cadastra no GameManager quando o inimigo � criado
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterEnemy(this);
        }
        else
        {
            Debug.LogWarning("GameManager n�o encontrado na cena! Certifique-se de que existe um GameManager.");
        }
    }

    void OnDestroy()
    {
        // Se desinscreve quando o inimigo for destru�do
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterEnemy(this);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        if (transform.position.y < 4.8f)
        {
            health -= damage;

            // Ativa o efeito de flash
            FlashHit();

            if (health <= 0)
            {
                Die();
            }
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // Dispara o evento para o GameManager que vai chamar o UIManager
        OnDeath?.Invoke(scoreValue);
        OnDieByPlayer?.Invoke();

        // Cria efeito de explos�o
        Instantiate(explosion, transform.position, transform.rotation);

        if (UnityEngine.Random.Range(0, 100) <= dropChance)
        {
            Instantiate(dropShootUpgrade, transform.position, transform.rotation);
        }

        // Destroi o objeto
        Destroy(gameObject);
    }

    private void FlashHit()
    {
        if (spriteRenderer == null) return;

        // Se já está piscando, interrompe a corrotina atual
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        // Muda para a cor de flash
        spriteRenderer.color = hitFlashColor;

        // Aguarda o tempo do flash
        yield return new WaitForSeconds(flashDuration);

        // Volta para a cor original
        spriteRenderer.color = originalColor;

        flashCoroutine = null;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerController>()?.TakeDamage(1);
            Die();
        }
    }
}