using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BossController : EnemyParent
{
    private Rigidbody2D rb;

    [SerializeField] public string currentState = "idle";
    [SerializeField] private string[] states;

    [SerializeField] private float screenLimit = 5.75f;
    [SerializeField] private bool startAction = false;

    private int[] chooseDirStart;
    private int dirStart;

    private int currentDirection = 1;
    private float margin = 0.01f;

    [Header("Skill 1")]
    [SerializeField] private GameObject shoot1;
    [SerializeField] private Transform shoot1Pos1;
    [SerializeField] private Transform shoot1Pos2;
    [SerializeField] private float skill1Duration = 5f;

    [Header("Skill 2")]
    [SerializeField] private GameObject shoot2;
    [SerializeField] private Transform shoot2Pos;
    [SerializeField] private float shoot2Rate = 0.2f;
    [SerializeField] private float moveToCenterSpeed = 3f;
    [SerializeField] private float rotationSpeed = 50f; // Velocidade de rotação do disparo
    [SerializeField] private GameObject shoot2FlashEffect; // Efeito visual para os tiros circulares
    [SerializeField] private bool flashEffectActive = false;
    private bool isAtCenter = false;
    private float currentAngle = 270f; // Começa em 270 graus (para baixo)
    private int rotationDirection = 1; // 1 = horário, -1 = anti-horário
    private bool skill2Initialized = false;
    private Vector2 centerPosition = new Vector2(0f, 0f); // Posição central do boss

    [Header("Skill 3")]
    [SerializeField] private bool startLaser = false;
    [SerializeField] private string changeStateTP = "out";
    [SerializeField] private float alphaTP = 1.0f;
    [SerializeField] private GameObject prefabRaio;
    [SerializeField] private bool beamActive = false;
    [SerializeField] private GameObject prefabDragEnergy;
    [SerializeField] private bool dragEnergyActive = false;
    [SerializeField] private float dragEnergyDuration = 3f;
    [SerializeField] public Transform pontoDisparo;
    [SerializeField] private float beamAddSpeed = 5f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        ChangeState();

        switch (currentState)
        {
            case "idle":
                rb.linearVelocity = Vector2.zero;
                break;

            case "skill1":
                HandleSkill1();

                break;

            case "skill2":
                HandleSkill2();
                break;

            case "skill3":
                HandleSkill3();
                break;

            case "shoot4":
                break;
            case "shoot5":
                break;
        }
    }

    private void HandleSkill1()
    {
        if (!startAction)
        {
            currentDirection = Random.value > 0.5f ? 1 : -1;
            rb.linearVelocity = new Vector2(currentDirection * Mathf.Abs(speed), 0f);
            startAction = true;
        }

        if (skill1Duration > 0)
        {
            CheckAndChangeDirection();
            skill1Duration -= Time.deltaTime;
        }
        else
        {
            skill1Duration = 5f;
            startAction = false;
            currentState = "idle";
        }

        ShootSkill1();
    }

    private void CheckAndChangeDirection(float addSpeed = 1)
    {
        if (transform.position.x >= screenLimit - margin && currentDirection > 0)
        {
            transform.position = new Vector3(screenLimit, transform.position.y, transform.position.z);

            currentDirection = -1;
            rb.linearVelocity = new Vector2(currentDirection * Mathf.Abs(speed * addSpeed), 0f);
        }
        else if (transform.position.x <= -screenLimit + margin && currentDirection < 0)
        {
            transform.position = new Vector3(-screenLimit, transform.position.y, transform.position.z);

            currentDirection = 1;
            rb.linearVelocity = new Vector2(currentDirection * Mathf.Abs(speed * addSpeed), 0f);
        }
    }

    private void ShootSkill1()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            if (shootRateTimer <= 0f)
            {
                var myShootPos1 = Instantiate(shoot1, shoot1Pos1.position, Quaternion.identity);
                myShootPos1.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, -shootSpeed);

                var myShootPos2 = Instantiate(shoot1, shoot1Pos2.position, Quaternion.identity);
                myShootPos2.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, -shootSpeed);

                shootRateTimer = shootRate;
            }

            if (shootRateTimer > 0)
            {
                shootRateTimer -= Time.deltaTime;
            }
        }
    }

    private void HandleSkill2()
    {
        if (!skill2Initialized)
        {
            // Inicializa a skill2: para o movimento e define direção aleatória
            rb.linearVelocity = Vector2.zero;
            isAtCenter = false;
            currentAngle = 270f; // Começa apontando para baixo
            rotationDirection = Random.value > 0.5f ? 1 : -1; // Aleatório: 1 = horário, -1 = anti-horário
            skill2Initialized = true;
        }

        // Primeiro: mover para o centro
        if (!isAtCenter)
        {
            MoveToCenter();
        }
        else
        {
            // Depois de centralizado: começar a atirar em círculo
            PerformCircularShoot();
        }
    }

    private void HandleSkill3()
    {
        if (!startLaser)
        {
            // Fase de teletransporte para o centro
            if (transform.position.x != 0 || transform.position.y != 2.7)
            {
                Teleport();
            }
            else
            {
                startLaser = true;
            }
        }
        else
        {
            // Garante que o movimento está sempre sendo aplicado
            if (!beamActive)
            {
                // Antes do laser começar, só carrega energia
                if (dragEnergyDuration > 0)
                {
                    ParticleEffect(prefabDragEnergy, pontoDisparo, ref dragEnergyActive);
                    dragEnergyDuration -= Time.deltaTime;
                }
                else
                {
                    // Remove efeito de carga e ativa o laser
                    GameObject energyDrag = GameObject.FindWithTag("EnergyDrag");
                    if (energyDrag != null)
                    {
                        Destroy(energyDrag);
                        dragEnergyActive = false;
                    }

                    FireBeam();

                    currentDirection = Random.value > 0.5f ? 1 : -1;
                    rb.linearVelocity = new Vector2(currentDirection * Mathf.Abs(speed * beamAddSpeed), 0f);
                }
            }
            else
            {
                // DURANTE o laser, move continuamente
                CheckAndChangeDirection(beamAddSpeed);
            }
        }
    }

    private void MoveToCenter()
    {
        // Move suavemente para a posição central
        transform.position = Vector2.MoveTowards(transform.position, centerPosition, moveToCenterSpeed * Time.deltaTime);

        // Verifica se chegou ao centro (com uma margem pequena)
        if (Vector2.Distance(transform.position, centerPosition) < 0.01f)
        {
            transform.position = centerPosition;
            isAtCenter = true;
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void PerformCircularShoot()
    {
        // Controla o tempo entre os tiros
        if (shootRateTimer <= 0f)
        {
            // Calcula a direção do tiro baseada no ângulo atual
            float angleInRadians = currentAngle * Mathf.Deg2Rad;
            Vector2 shootDirection = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));

            // Dispara o tiro
            var myShootPos = Instantiate(shoot2, shoot2Pos.position, Quaternion.identity);
            myShootPos.GetComponent<Rigidbody2D>().linearVelocity = shootDirection * shootSpeed;
            ParticleEffect(shoot2FlashEffect, pontoDisparo, ref flashEffectActive, true);

            // Rotaciona o tiro para apontar na direção correta (opcional)
            float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg + 90f;
            myShootPos.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            // Avança para o próximo ângulo baseado na direção
            currentAngle += rotationSpeed * rotationDirection;

            // Verifica se completou 360 graus
            if (rotationDirection == 1 && currentAngle >= 270f + 360f)
            {
                // Completou volta horária
                Debug.Log("Skill 2 completada - Sentido Horário");
                EndSkill2();
            }
            else if (rotationDirection == -1 && currentAngle <= 270f - 360f)
            {
                // Completou volta anti-horária
                Debug.Log("Skill 2 completada - Sentido Anti-horário");
                EndSkill2();
            }

            shootRateTimer = shoot2Rate;
        }

        if (shootRateTimer > 0)
        {
            shootRateTimer -= Time.deltaTime;
        }
    }

    private void EndSkill2()
    {
        // Reseta as variáveis e volta para idle
        skill2Initialized = false;
        isAtCenter = false;
        currentState = "idle";
    }

    private void Teleport()
    {

        rb.linearVelocity = Vector2.zero;
        invencible = true;

        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        Color c = spriteRenderer.color;
        c.a = alphaTP;
        spriteRenderer.color = c;

        switch (changeStateTP)
        { 
            case "out":
                alphaTP -= Time.deltaTime;
                if (alphaTP <= 0)
                {
                    alphaTP = 0;
                    transform.position = new Vector2(0, 2.7f);
                    changeStateTP = "in";
                }
                break;

            case "in":
                alphaTP += Time.deltaTime;
                if (alphaTP >= 1)
                {
                    alphaTP = 1;
                    //transform.position = new Vector2(0, 2.7f);
                    invencible = false;
                    changeStateTP = "out";
                    startLaser = true;
                }
                break;
        }
    }

    void FireBeam()
    {
        if (prefabRaio != null && pontoDisparo != null)
        {
            if (!beamActive)
            {
                // Passa referências para o raio
                GameObject raio = Instantiate(prefabRaio, pontoDisparo.position, Quaternion.identity);
                BeamEnergy beamEnergy = raio.GetComponent<BeamEnergy>();

                if (beamEnergy != null)
                {
                    beamEnergy.boss = this.gameObject;
                    beamEnergy.pontoDisparo = pontoDisparo;
                }

                beamActive = true;
            }
        }
    }

    public void OnBeamDestroyed()
    {
        beamActive = false;
        dragEnergyActive = false;
        dragEnergyDuration = 3f;
        currentState = "idle";
    }

    private GameObject ParticleEffect(GameObject prefabEffect, Transform shootingPoint, ref bool effectControl, bool burst = false)
    {
        if (prefabEffect != null && shootingPoint != null)
        {
            if (!effectControl)
            {
                var effect = Instantiate(prefabEffect, shootingPoint.position, Quaternion.identity);

                if (!burst) {
                    effectControl = true;
                }

                return effect;
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }

    private void ChangeState()
    {
        if (currentState == "idle")
        {
            //currentState = states[Random.Range(0, states.Length)];
        }
    }
}
