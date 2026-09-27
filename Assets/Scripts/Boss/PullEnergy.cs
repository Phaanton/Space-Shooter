using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PullEnergy : MonoBehaviour
{
    [Header("Configurações das Partículas")]
    [SerializeField] private float absorptionRadius = 3f;
    [SerializeField] private float absorptionSpeed = 2f;
    [SerializeField] private int particleCount = 20;
    [SerializeField] private float absorptionDuration = 1.5f;
    [SerializeField] private GameObject particlePrefab; // Opcional: um sprite pequeno

    private List<Transform> particles = new List<Transform>();
    private Transform bossTransform;

    void Start()
    {
        bossTransform = transform;
        StartCoroutine(AbsorbEnergy());
    }

    IEnumerator AbsorbEnergy()
    {
        // Cria partículas ao redor do boss
        for (int i = 0; i < particleCount; i++)
        {
            CreateParticle();
            yield return new WaitForSeconds(absorptionDuration / particleCount * 0.3f);
        }

        // Aguarda um pouco e começa a absorver
        yield return new WaitForSeconds(0.3f);

        float time = 0f;
        List<Transform> particlesToRemove = new List<Transform>();

        // Fase de absorção
        while (time < absorptionDuration)
        {
            time += Time.deltaTime;

            foreach (Transform particle in particles)
            {
                if (particle != null)
                {
                    // Move a partícula em direção ao boss
                    Vector3 direction = (bossTransform.position - particle.position).normalized;
                    float distance = Vector3.Distance(particle.position, bossTransform.position);

                    // Acelera quando mais perto
                    float speed = absorptionSpeed * (1 + (1 - distance / absorptionRadius));

                    particle.position += direction * speed * Time.deltaTime;

                    // Se chegou muito perto, marca para destruir
                    if (distance < 0.3f)
                    {
                        particlesToRemove.Add(particle);
                    }
                }
            }

            // Remove partículas que chegaram ao boss
            foreach (Transform p in particlesToRemove)
            {
                particles.Remove(p);
                if (p != null) Destroy(p.gameObject);
            }
            particlesToRemove.Clear();

            yield return null;
        }

        // Destroi partículas restantes
        foreach (Transform p in particles)
        {
            if (p != null) Destroy(p.gameObject);
        }
        particles.Clear();
    }

    void CreateParticle()
    {
        // Cria uma partícula em posição aleatória ao redor do boss
        Vector2 randomPosition = Random.insideUnitCircle * absorptionRadius;
        Vector3 position = bossTransform.position + new Vector3(randomPosition.x, randomPosition.y, 0);

        GameObject particle;

        if (particlePrefab != null)
        {
            particle = Instantiate(particlePrefab, position, Quaternion.identity, transform);
        }
        else
        {
            // Cria uma primitiva se não tiver prefab
            particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            particle.transform.position = position;
            particle.transform.localScale = Vector3.one * 0.2f;
            particle.transform.SetParent(transform);

            // Remove o collider (opcional)
            Destroy(particle.GetComponent<Collider>());

            // Adiciona brilho
            SpriteRenderer sr = particle.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.Load<Sprite>("Sprites/Circle"); // Você precisaria ter um sprite de círculo
        }

        particles.Add(particle.transform);
    }
}