using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float xRange = 8.3f;
    [SerializeField] private float yRange = 4.3f;
    private Rigidbody2D rb;

    [SerializeField] private GameObject shoot;
    [SerializeField] private GameObject shoot2;
    [SerializeField] private float shoot2Spacing = 0.5f;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private float fireRateTimer = 0f;
    [SerializeField] private float fireRateHoming = 2f;
    [SerializeField] private float fireRateHomingTimer = 0f;
    [SerializeField] private float shootSpeed = 10f;

    [SerializeField] private Transform instantiateShoot;

    [SerializeField] private GameObject explosion;

    [SerializeField] private int shootLevel = 1;

    [SerializeField] private AudioSource shootSound;

    [Header("Homing Missile Settings")]
    [SerializeField] private GameObject homingMissilePrefab;
    [SerializeField] private float homingMissileSpacing = 0.8f;
    [SerializeField] private float homingMissileStartDelay = 0.5f;

    [SerializeField] private GameObject shield;
    [SerializeField] private float shieldDuration = 5f;
    [SerializeField] private float shieldTimer = 0f;
    [SerializeField] private int currentShieldNumber = 3;
    [SerializeField] private int shieldMax = 2;
    private GameObject activeShield;
    private int dropScoreValue = 50;

    public event Action<int> OnShieldActivation;
    public event Action<int> OnLoseHelath;
    public event Action<int> OnDropScore;

    public int hp = 3;

    [SerializeField] private Animator animator;

    [SerializeField] private AudioSource playerHitSound;
    [SerializeField] private AudioSource shieldActivateSound;

    [Header("Invincibility Settings")]
    [SerializeField] private float invincibilityDuration = 2f;  // Duração da invulnerabilidade após levar dano
    [SerializeField] private float blinkInterval = 0.1f;        // Intervalo entre piscadas
    [SerializeField] private int blinkCount = 5;                 // Número de piscadas (opcional, calculado automaticamente se for 0)

    private bool isInvincible = false;
    private SpriteRenderer[] spriteRenderers;  // Para suportar múltiplos sprites (nave + escudo)
    private Coroutine invincibilityCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Pega todos os SpriteRenderers do player (incluindo filhos como escudo, asas, etc)
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();

        Shooting();

        ActivateShield();
    }

    private void Shooting()
    {
        if (InputManager.instance.ShootInput && fireRateTimer <= 0)
        {
            switch (shootLevel)
            {
                case 1:
                    ShootFirst();
                    break;
                case 2:
                    ShootSecond();
                    break;
                case 3:
                    if (fireRateHomingTimer <= 0)
                    {
                        ShootThird();
                        fireRateHomingTimer = fireRateHoming;
                    }
                    else
                    {
                        ShootSecond();
                    }
                    break;

                default:
                    ShootFirst();
                    break;
            }

            shootSound.Play();
        }

        if (fireRateTimer > 0)
        {
            fireRateTimer -= Time.deltaTime;
        }

        if (fireRateHomingTimer > 0)
        {
            fireRateHomingTimer -= Time.deltaTime;
        }
    }

    public void ShootFirst()
    {
        var shootLeft = Instantiate(shoot, new Vector3(instantiateShoot.position.x - shoot2Spacing, instantiateShoot.position.y, instantiateShoot.position.z), transform.rotation);
        var shootRight = Instantiate(shoot, new Vector3(instantiateShoot.position.x + shoot2Spacing, instantiateShoot.position.y, instantiateShoot.position.z), transform.rotation);
        shootLeft.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, shootSpeed);
        shootRight.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, shootSpeed);

        fireRateTimer = fireRate;
    }

    public void ShootSecond()
    {

        var shootLeft2 = Instantiate(shoot, new Vector3(instantiateShoot.position.x - shoot2Spacing - 0.1f, instantiateShoot.position.y - 0.1f, instantiateShoot.position.z), transform.rotation);
        var shootRight2 = Instantiate(shoot, new Vector3(instantiateShoot.position.x + shoot2Spacing + 0.1f, instantiateShoot.position.y - 0.1f, instantiateShoot.position.z), transform.rotation);
        shootLeft2.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, shootSpeed);

        shootRight2.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, shootSpeed);

        ShootFirst();

        fireRateTimer = fireRate;
    }

    public void ShootThird()
    {
        ShootFirst();
        ShootSecond();
        ShootHomingMissiles();

        fireRateTimer = fireRate;
        fireRateHomingTimer = fireRateHoming;
    }

    public void ShootHomingMissiles()
    {
        // Instancia dois m�sseis teleguiados, um de cada lado
        var missileLeft = Instantiate(homingMissilePrefab,
            new Vector3(instantiateShoot.position.x - homingMissileSpacing,
                        instantiateShoot.position.y - 0.4f,
                        instantiateShoot.position.z),
            Quaternion.identity);

        var missileRight = Instantiate(homingMissilePrefab,
            new Vector3(instantiateShoot.position.x + homingMissileSpacing,
                        instantiateShoot.position.y - 0.4f,
                        instantiateShoot.position.z),
            Quaternion.identity);

        // Configurar os m�sseis
        ConfigureMissile(missileLeft, 1f, 90f);
        ConfigureMissile(missileRight, -1f, -90f);
    }

    private void ConfigureMissile(GameObject missile, float startHomingSpeed, float startHominRotation)
    {
        HomingMissile homingScript = missile.GetComponent<HomingMissile>();
        if (homingScript != null)
        {
            homingScript.startSpeed = startHomingSpeed;
            homingScript.startRotation = startHominRotation;
        }
    }

    private void Move()
    {
        Vector2 movementInput = InputManager.instance.MoveInput;

        if (movementInput != null)
        {
            var horizontalInput = movementInput.x;
            var verticalInput = movementInput.y;
            rb.linearVelocity = new Vector2(horizontalInput, verticalInput).normalized * speed;
            float clampedX = Mathf.Clamp(transform.position.x, -xRange, xRange);
            float clampedY = Mathf.Clamp(transform.position.y, -yRange, yRange);
            transform.position = new Vector3(clampedX, clampedY, transform.position.z);
            if (horizontalInput != 0)
            {
                if (horizontalInput < 0)
                {
                    animator.SetBool("sideLeft", true);
                    animator.SetBool("sideRight", false);
                    animator.SetBool("idleBack", false);
                }
                else if (horizontalInput > 0)
                {
                    animator.SetBool("sideLeft", false);
                    animator.SetBool("sideRight", true);
                    animator.SetBool("idleBack", false);
                }
            }
            else
            {
                animator.SetBool("sideLeft", false);
                animator.SetBool("sideRight", false);
                animator.SetBool("idleBack", true);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        // Se já está invencível, não toma dano
        if (isInvincible) return;

        if (!activeShield)
        {
            hp -= damage;
            playerHitSound.Play();
            OnLoseHelath?.Invoke(hp);

            // Ativa o efeito de invencibilidade
            StartInvincibility();

            if (hp <= 0)
            {
                Instantiate(explosion, transform.position, transform.rotation);
                Destroy(gameObject);

                var inputNameUI = FindFirstObjectByType<UIManager>();
                if (inputNameUI != null)
                {
                    inputNameUI.OnGameOver();
                }
            }
        }
    }

    private void StartInvincibility()
    {
        if (invincibilityCoroutine != null)
            StopCoroutine(invincibilityCoroutine);

        invincibilityCoroutine = StartCoroutine(InvincibilityRoutine());
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;

        // Calcula quantas piscadas serão necessárias baseado na duração total
        int totalBlinks = blinkCount > 0 ? blinkCount : Mathf.CeilToInt(invincibilityDuration / (blinkInterval * 2));
        float blinkDuration = blinkInterval * 2; // Cada ciclo de piscada (desaparece e aparece)

        for (int i = 0; i < totalBlinks; i++)
        {
            // Torna o player semitransparente (ou invisível)
            SetPlayerAlpha(0.3f);
            yield return new WaitForSeconds(blinkInterval);

            // Restaura a opacidade
            SetPlayerAlpha(1f);
            yield return new WaitForSeconds(blinkInterval);
        }

        // Garante que o player está totalmente visível no final
        SetPlayerAlpha(1f);

        // Aguarda o resto do tempo de invencibilidade se o piscar terminou antes
        float elapsedTime = blinkDuration * totalBlinks;
        if (elapsedTime < invincibilityDuration)
        {
            yield return new WaitForSeconds(invincibilityDuration - elapsedTime);
        }

        isInvincible = false;
        invincibilityCoroutine = null;
    }

    private void SetPlayerAlpha(float alpha)
    {
        if (spriteRenderers == null) return;

        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            if (renderer != null)
            {
                Color color = renderer.color;
                color.a = alpha;
                renderer.color = color;
            }
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PowerUp"))
        {
            shootLevel = Mathf.Min(shootLevel + 1, 3);

            if (shootLevel >= 3)
            {
                OnDropScore?.Invoke(dropScoreValue);
            }

            Destroy(collision.gameObject);
        }
    }

    private void ActivateShield()
    {
        if (InputManager.instance.ShieldInput)
        {
            if (currentShieldNumber > 0)
            {
                if (!activeShield)
                {
                    activeShield = Instantiate(shield, transform.position, transform.rotation, transform);
                    currentShieldNumber--;
                    OnShieldActivation?.Invoke(currentShieldNumber);
                    shieldActivateSound.Play();
                }
            }
            else
            {
                currentShieldNumber = 0;
            }
        }

        if (activeShield)
        {
            shieldTimer += Time.deltaTime;

            if (shieldTimer > shieldDuration)
            {
                Destroy(activeShield);
                activeShield = null;
                shieldTimer = 0f;
            }
        }
    }
}