using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossDrone : MonoBehaviour
{
    [Header("Configurações Gerais")]
    public float health = 500f;
    public float maxHealth = 500f;
    public float moveSpeed = 3f;
    public float introDuration = 3f;

    [Header("Limites de Movimento")]
    public float minX = -2f;
    public float maxX = 2f;
    public float minY = -4f;
    public float maxY = 4f;

    [Header("Referências")]
    public Transform[] leftCannons;      // 3 canhões do lado esquerdo
    public Transform[] rightCannons;     // 3 canhões do lado direito
    public Transform centerCannon;       // Canhão central
    public Transform[] frontCannons;     // 2 canhões frontais (skill especial)

    [Header("Projétil")]
    public GameObject circularBulletPrefab;  // Único sprite de tiro circular

    [Header("Configuração de Tiro")]
    public float baseBulletSpeed = 8f;
    public float bulletScale = 0.5f;

    [Header("Cores por Fase")]
    public Color phase1Color = new Color(0.5f, 0.8f, 1f); // Azul claro
    public Color phase2Color = new Color(1f, 0.6f, 0.2f); // Laranja
    public Color phase3Color = new Color(1f, 0.3f, 0.6f); // Rosa

    [SerializeField] private float currentHealth;
    private int currentPhase = 1;
    private bool isAttacking = false;
    private bool isIntro = true;
    private Vector3 startPosition;
    private Transform player;
    private SpriteRenderer spriteRenderer;
    private Vector3 targetPosition;

    private bool gamePaused = false;

    [SerializeField] private UIManager uiManager;

    [Header("Debug")]
    public bool showSkillDebug = true;  // Ativa/desativa mensagens de debug

    void Start()
    {
        uiManager = FindObjectOfType<UIManager>();

        currentHealth = health;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        targetPosition = transform.position;

        StartCoroutine(IntroSequence());
        StartCoroutine(BossBehavior());
    }

    void Update()
    {
        if (gamePaused) return;

        // Aplica restrição de movimento em tempo real
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, minX, maxX),
            Mathf.Clamp(transform.position.y, minY, maxY),
            transform.position.z
        );
    }

    IEnumerator IntroSequence()
    {
        if (showSkillDebug) Debug.Log("<color=cyan>🎮 BOSS ENTROU EM CENA! Preparando para batalha...</color>");

        Vector3 targetPos = new Vector3(0, maxY - 1f, 0);
        float elapsed = 0;

        while (elapsed < introDuration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPos, elapsed / introDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        isIntro = false;

        if (showSkillDebug) Debug.Log("<color=cyan>⚔️ BOSS PRONTO! Iniciando padrões de ataque - FASE 1 ⚔️</color>");
    }

    IEnumerator BossBehavior()
    {
        while (currentHealth > 0)
        {
            // Movimentação padrão
            yield return StartCoroutine(MovePattern());
            yield return new WaitForSeconds(Random.Range(0.8f, 1.5f));

            // Escolhe e executa skill
            if (!isAttacking && !isIntro)
            {
                yield return StartCoroutine(ExecuteRandomSkill());
            }
        }

        Die();
    }

    IEnumerator MovePattern()
    {
        float elapsed = 0;
        float patternDuration = Random.Range(2.5f, 4f);
        Vector3 startPos = transform.position;

        // Escolhe padrão de movimento adequado aos limites
        int moveType = Random.Range(0, 5);

        while (elapsed < patternDuration)
        {
            float t = elapsed / patternDuration;
            Vector3 newPosition = transform.position;

            switch (moveType)
            {
                case 0: // Movimento senoidal horizontal
                    float xOffset = Mathf.Sin(elapsed * 2f) * 1.5f;
                    newPosition.x = Mathf.Clamp(startPos.x + xOffset, minX, maxX);
                    newPosition.y = Mathf.Lerp(startPos.y, startPos.y + Mathf.Sin(elapsed * 1.5f) * 1f, t);
                    break;

                case 1: // Movimento de figura 8 (infinito)
                    float angle = elapsed * 1.8f;
                    float xFig8 = Mathf.Sin(angle) * 1.2f;
                    float yFig8 = Mathf.Sin(angle * 2f) * 1.2f;
                    newPosition.x = Mathf.Clamp(startPos.x + xFig8, minX, maxX);
                    newPosition.y = Mathf.Clamp(startPos.y + yFig8, minY + 1f, maxY - 1f);
                    break;

                case 2: // Movimento de quadrado suave
                    float squareT = elapsed % 4f;
                    if (squareT < 1f)
                    {
                        newPosition.x = Mathf.Lerp(startPos.x, maxX, squareT);
                        newPosition.y = startPos.y;
                    }
                    else if (squareT < 2f)
                    {
                        newPosition.x = maxX;
                        newPosition.y = Mathf.Lerp(startPos.y, maxY - 1f, squareT - 1f);
                    }
                    else if (squareT < 3f)
                    {
                        newPosition.x = Mathf.Lerp(maxX, minX, squareT - 2f);
                        newPosition.y = maxY - 1f;
                    }
                    else
                    {
                        newPosition.x = minX;
                        newPosition.y = Mathf.Lerp(maxY - 1f, startPos.y, squareT - 3f);
                    }
                    break;

                case 3: // Movimento de perseguição suave ao player (limitado)
                    Vector3 targetPos = player.position;
                    targetPos.x = Mathf.Clamp(targetPos.x, minX + 0.5f, maxX - 0.5f);
                    targetPos.y = Mathf.Clamp(targetPos.y, minY + 0.5f, maxY - 0.5f);
                    newPosition = Vector3.Lerp(startPos, targetPos, t * 0.4f);
                    break;

                case 4: // Movimento circular limitado
                    float circleRadius = 1.2f;
                    float circleAngle = elapsed * 1.5f;
                    float centerX = Mathf.Clamp(startPos.x, minX + circleRadius, maxX - circleRadius);
                    float centerY = Mathf.Clamp(startPos.y, minY + circleRadius, maxY - circleRadius);
                    newPosition.x = centerX + Mathf.Cos(circleAngle) * circleRadius;
                    newPosition.y = centerY + Mathf.Sin(circleAngle) * circleRadius;
                    break;
            }

            // Aplica clamping para garantir que não sai dos limites
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

            transform.position = Vector3.MoveTowards(transform.position, newPosition, moveSpeed * Time.deltaTime);

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator ExecuteRandomSkill()
    {
        isAttacking = true;

        // Escolhe skill baseada na fase
        int skillIndex;

        if (currentPhase == 1)
            skillIndex = Random.Range(0, 2);
        else if (currentPhase == 2)
            skillIndex = Random.Range(0, 3);
        else
            skillIndex = Random.Range(1, 4);

        // DEBUG: Mostra qual skill vai ser executada
        if (showSkillDebug)
        {
            string skillName = "";
            switch (skillIndex)
            {
                case 0: skillName = "Dispersão em Espiral"; break;
                case 1: skillName = "Trio de Projéteis Direcionados"; break;
                case 2: skillName = "Barragem em Anel"; break;
                case 3: skillName = "Ondas Alternadas"; break;
            }
            Debug.Log("<color=cyan>[BOSS]</color> <color=yellow>Fase " + currentPhase + "</color> - Executando skill: <color=green>" + skillName + "</color>");
        }

        switch (skillIndex)
        {
            case 0:
                if (showSkillDebug) Debug.Log("  ↳ Iniciando: Dispersão em Espiral (usando canhões laterais alternados)");
                yield return StartCoroutine(Skill_SpiralSpread());
                break;
            case 1:
                if (showSkillDebug) Debug.Log("  ↳ Iniciando: Trio de Projéteis Direcionados (todos os canhões laterais)");
                yield return StartCoroutine(Skill_TripleShot());
                break;
            case 2:
                if (showSkillDebug) Debug.Log("  ↳ Iniciando: Barragem em Anel (canhão central)");
                yield return StartCoroutine(Skill_RingBarrage());
                break;
            case 3:
                if (showSkillDebug) Debug.Log("  ↳ Iniciando: Ondas Alternadas (alternando lados)");
                yield return StartCoroutine(Skill_AlternatingWaves());
                break;
        }

        if (showSkillDebug) Debug.Log("<color=cyan>[BOSS]</color> Skill finalizada! Próximo ataque em " + Random.Range(1.2f, 2f) + " segundos");

        isAttacking = false;

        // Pausa entre ataques
        yield return new WaitForSeconds(Random.Range(1.2f, 2f));

        // Verifica mudança de fase
        CheckPhase();
    }

    void CheckPhase()
    {
        float healthPercent = currentHealth / maxHealth;

        if (healthPercent <= 0.33f && currentPhase == 2)
        {
            currentPhase = 3;
            StartCoroutine(PhaseTransition());
        }
        else if (healthPercent <= 0.66f && currentPhase == 1)
        {
            currentPhase = 2;
            StartCoroutine(PhaseTransition());
        }
    }

    IEnumerator PhaseTransition()
    {
        if (showSkillDebug) Debug.Log("<color=magenta>⚡ MUDANÇA DE FASE! Agora na FASE " + currentPhase + " ⚡</color>");

        Color targetColor = currentPhase == 2 ? phase2Color : phase3Color;

        for (int i = 0; i < 8; i++)
        {
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, targetColor, 0.15f);

            // Dispara um anel de energia na transição
            SpiralRing(centerCannon.position, 16, 360, 6f, targetColor);
            yield return new WaitForSeconds(0.1f);
        }

        if (showSkillDebug) Debug.Log("<color=magenta>✓ Transição de fase concluída! Novos padrões de ataque disponíveis</color>");
    }

    // ============ SKILL 1: Dispersão em Espiral (APENAS CANHÃO CENTRAL) ============
    IEnumerator Skill_SpiralSpread()
    {
        // Usa apenas o canhão central para criar um padrão espiral
        int waves = currentPhase == 3 ? 8 : 5;
        int bulletsPerWave = currentPhase == 3 ? 12 : 8;

        if (showSkillDebug)
        {
            string faseInfo = currentPhase == 3 ? "fase 3 (rápido)" : "fase normal";
            Debug.Log("    • Dispersão em Espiral (Canhão Central): " + waves + " ondas, " + bulletsPerWave + " projéteis por onda, " + faseInfo);
        }

        for (int wave = 0; wave < waves; wave++)
        {
            if (showSkillDebug && wave % 2 == 0)
                Debug.Log("      Onda " + (wave + 1) + "/" + waves + " - Ângulo base: " + (wave * 15f) + "°");

            // Ângulo inicial rotaciona a cada onda para criar espiral
            float startAngle = wave * 15f;
            float speed = baseBulletSpeed;

            // Na fase 3, aumenta a velocidade gradualmente
            if (currentPhase == 3)
                speed = baseBulletSpeed + (wave * 0.5f);

            for (int i = 0; i < bulletsPerWave; i++)
            {
                // Calcula o ângulo de cada projétil no círculo
                float angle = (360f / bulletsPerWave) * i + startAngle;
                SpawnBullet(centerCannon.position, angle, speed);
            }

            // Delay entre ondas (menor na fase 3)
            float delay = currentPhase == 3 ? 0.12f : 0.2f;
            yield return new WaitForSeconds(delay);
        }

        if (showSkillDebug) Debug.Log("    ✓ Dispersão em Espiral concluída");
    }

    // ============ SKILL 2: Barragem em Anel (APENAS CANHÃO CENTRAL) ============
    IEnumerator Skill_RingBarrage()
    {
        // Usa apenas o canhão central para criar anéis concêntricos
        int rings = currentPhase == 3 ? 6 : 4;
        int bulletsPerRing = currentPhase == 3 ? 24 : 16;

        if (showSkillDebug) Debug.Log("    • Barragem em Anel (Canhão Central): " + rings + " anéis, " + bulletsPerRing + " projéteis por anel");

        for (int ring = 0; ring < rings; ring++)
        {
            if (showSkillDebug)
                Debug.Log("      Anel " + (ring + 1) + "/" + rings + " - Velocidade: " + (baseBulletSpeed + (ring * 1.5f)) + ", Projéteis: " + bulletsPerRing);

            // Velocidade aumenta com cada anel
            float speed = baseBulletSpeed + (ring * 1.5f);

            // Ângulo inicial rotaciona levemente para dar efeito visual
            float startAngle = ring * 10f;

            for (int i = 0; i < bulletsPerRing; i++)
            {
                float angle = (360f / bulletsPerRing) * i + startAngle;
                SpawnBullet(centerCannon.position, angle, speed);
            }

            // Aumenta o número de projéteis no próximo anel (opcional)
            if (currentPhase == 3 && ring < rings - 1)
                bulletsPerRing += 4;

            yield return new WaitForSeconds(0.2f);
        }

        if (showSkillDebug) Debug.Log("    ✓ Barragem em Anel concluída");
    }

    // ============ SKILL 3: Trio de Projéteis Direcionados (APENAS CANHÕES LATERAIS) ============
    IEnumerator Skill_TripleShot()
    {
        // Usa apenas os canhões laterais (esquerda e direita)
        int shotsPerCannon = currentPhase == 3 ? 4 : 3;
        float spreadAngle = 15f;

        // Coleta todos os canhões laterais
        List<Transform> sideCannons = new List<Transform>();
        if (leftCannons != null) sideCannons.AddRange(leftCannons);
        if (rightCannons != null) sideCannons.AddRange(rightCannons);

        int totalCannons = sideCannons.Count;

        if (showSkillDebug) Debug.Log("    • Trio de Projéteis Direcionados (Canhões Laterais): " + shotsPerCannon + " salvas, " + totalCannons + " canhões, spread de " + spreadAngle + "°");

        for (int shot = 0; shot < shotsPerCannon; shot++)
        {
            if (showSkillDebug && shot % 2 == 0)
                Debug.Log("      Salva " + (shot + 1) + "/" + shotsPerCannon);

            foreach (Transform cannon in sideCannons)
            {
                if (cannon == null) continue;

                // Calcula direção para o player
                Vector2 directionToPlayer = (player.position - cannon.position).normalized;
                float baseAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

                // Dispara 3 projéteis: central, esquerda e direita
                float[] angles = {
                baseAngle - spreadAngle,  // Projétil da esquerda
                baseAngle,                // Projétil central (direto no player)
                baseAngle + spreadAngle   // Projétil da direita
            };

                foreach (float angle in angles)
                {
                    SpawnBullet(cannon.position, angle, baseBulletSpeed);
                }
            }

            // Delay entre salvas
            float delay = currentPhase == 3 ? 0.25f : 0.4f;
            yield return new WaitForSeconds(delay);
        }

        if (showSkillDebug) Debug.Log("    ✓ Trio de Projéteis concluído");
    }

    // ============ SKILL 4: Ondas Alternadas (APENAS CANHÕES LATERAIS) ============
    // ============ SKILL 4: Ondas Alternadas (Leque Móvel - Efeito Varredura) ============
    IEnumerator Skill_AlternatingWaves()
    {
        int waves = currentPhase == 3 ? 16 : 12;
        int bulletsPerWave = 5;

        if (showSkillDebug) Debug.Log("    • Ondas Alternadas (Leque Móvel): " + waves + " ondas, varredura vertical");

        for (int wave = 0; wave < waves; wave++)
        {
            // Alterna entre canhões esquerdos e direitos
            bool useLeft = wave % 2 == 0;
            Transform[] cannonsToUse = useLeft ? leftCannons : rightCannons;

            if (cannonsToUse == null || cannonsToUse.Length == 0)
                continue;

            // Calcula o offset vertical baseado na onda (cria efeito de varredura)
            float verticalSweep = Mathf.Sin(wave * 0.5f) * 25f; // Varre entre -25 e 25 graus

            foreach (Transform cannon in cannonsToUse)
            {
                if (cannon == null) continue;

                // Direção base para o player
                Vector2 directionToPlayer = (player.position - cannon.position).normalized;
                float baseAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

                // Dispara um leque que se move verticalmente
                for (int i = 0; i < bulletsPerWave; i++)
                {
                    float spread = (i - 2) * 8f; // -16, -8, 0, 8, 16
                    float finalAngle = baseAngle + spread + verticalSweep;

                    SpawnBullet(cannon.position, finalAngle, baseBulletSpeed);
                }
            }

            if (showSkillDebug && wave % 4 == 0)
            {
                Debug.Log("      Onda " + (wave + 1) + "/" + waves + " - Sweep: " + Mathf.RoundToInt(verticalSweep) + "°");
            }

            float delay = currentPhase == 3 ? 0.1f : 0.15f;
            yield return new WaitForSeconds(delay);
        }

        if (showSkillDebug) Debug.Log("    ✓ Ondas Alternadas concluída");
    }

    // ============ SKILL ESPECIAL: Jato de Fogo (Frontal) ============
    public IEnumerator Special_FireStream()
    {
        float duration = 3f;
        float fireRate = 0.08f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            foreach (Transform frontCannon in frontCannons)
            {
                // Dispara para frente (ângulo 0 graus = direita)
                SpawnBullet(frontCannon.position, 0f, baseBulletSpeed * 1.5f);

                // Pequena variação vertical para criar efeito de chamas
                SpawnBullet(frontCannon.position, 5f, baseBulletSpeed * 1.4f);
                SpawnBullet(frontCannon.position, -5f, baseBulletSpeed * 1.4f);
            }

            elapsed += fireRate;
            yield return new WaitForSeconds(fireRate);
        }
    }

    // ============ FUNÇÕES UTILITÁRIAS ============

    void SpawnBullet(Vector3 position, float angleDegrees, float speed)
    {
        GameObject bullet = Instantiate(circularBulletPrefab, position, Quaternion.identity);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = bullet.AddComponent<Rigidbody2D>();

        Vector2 direction = new Vector2(
            Mathf.Cos(angleDegrees * Mathf.Deg2Rad),
            Mathf.Sin(angleDegrees * Mathf.Deg2Rad)
        );

        rb.linearVelocity = direction * speed;

        SpriteRenderer bulletRenderer = bullet.GetComponent<SpriteRenderer>();
        if (bulletRenderer != null)
        {
            bulletRenderer.color = GetBulletColor();
        }

        bullet.transform.localScale = Vector3.one * bulletScale;
        Destroy(bullet, 5f);
    }

    Color GetBulletColor()
    {
        switch (currentPhase)
        {
            case 1: return phase1Color;
            case 2: return phase2Color;
            case 3: return phase3Color;
            default: return Color.white;
        }
    }

    void SpiralRing(Vector3 origin, int bullets, float totalAngle, float speed, Color color)
    {
        for (int i = 0; i < bullets; i++)
        {
            float angle = (totalAngle / bullets) * i;
            GameObject bullet = Instantiate(circularBulletPrefab, origin, Quaternion.identity);

            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            rb.linearVelocity = direction * speed;

            bullet.GetComponent<SpriteRenderer>().color = color;
            Destroy(bullet, 3f);
        }
    }

    float GetAngleToPlayer(Vector3 fromPosition)
    {
        Vector2 direction = (player.position - fromPosition).normalized;
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
            Die();
    }

    IEnumerator DamageFlash()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);

        Color phaseColor = Color.white;
        switch (currentPhase)
        {
            case 1: phaseColor = phase1Color; break;
            case 2: phaseColor = phase2Color; break;
            case 3: phaseColor = phase3Color; break;
        }
        spriteRenderer.color = phaseColor;
    }

    void Die()
    {
        if (showSkillDebug) Debug.Log("<color=red>💀 BOSS DERROTADO! 💀</color>");

        //StopAllCoroutines();
        // Efeito de explosão com vários projéteis
        //for (int i = 0; i < 32; i++)
        //{
        //    float angle = i * 11.25f;
        //    SpawnBullet(transform.position, angle, 5f);
        //}

        if (showSkillDebug) Debug.Log("<color=yellow>🏆 Parabéns! Boss eliminado! 🏆</color>");

        uiManager.OnGameOver();

        Destroy(gameObject);
    }

    // Função para debug visual (opcional)
    void OnDrawGizmosSelected()
    {
        // Desenha os limites no editor
        Gizmos.color = Color.red;
        Vector3 bottomLeft = new Vector3(minX, minY, 0);
        Vector3 topRight = new Vector3(maxX, maxY, 0);
        Vector3 bottomRight = new Vector3(maxX, minY, 0);
        Vector3 topLeft = new Vector3(minX, maxY, 0);

        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
    }
}