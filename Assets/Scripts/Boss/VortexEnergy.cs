using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VortexEnergy : MonoBehaviour
{
    [Header("Configurações da Espiral")]
    [SerializeField] private float maxRadius = 5f;
    [SerializeField] private float minRadius = 0.5f;
    [SerializeField] private float spiralSpeed = 2f;
    [SerializeField] private int spiralLines = 3;
    [SerializeField] private int pointsPerLine = 20;

    private List<LineRenderer> lines = new List<LineRenderer>();
    private Transform bossTransform;

    void Start()
    {
        bossTransform = transform;
        StartCoroutine(SpiralCoroutine());
    }

    IEnumerator SpiralCoroutine()
    {
        // Cria as linhas da espiral
        for (int i = 0; i < spiralLines; i++)
        {
            CreateSpiralLine(i);
        }

        float time = 0f;
        float duration = 2f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = time / duration;

            for (int i = 0; i < lines.Count; i++)
            {
                LineRenderer line = lines[i];
                Vector3[] positions = new Vector3[pointsPerLine];

                for (int j = 0; j < pointsPerLine; j++)
                {
                    float t = (float)j / (pointsPerLine - 1);

                    // Ângulo base + offset da linha + progresso
                    float baseAngle = (t * 720f) + (i * 120f) + (time * 360f * spiralSpeed);
                    float angle = baseAngle * Mathf.Deg2Rad;

                    // Raio diminui com o tempo
                    float currentRadius = Mathf.Lerp(maxRadius, minRadius, t + progress * 0.5f);
                    currentRadius = Mathf.Max(0.1f, currentRadius);

                    // Posição em espiral
                    float x = Mathf.Cos(angle) * currentRadius;
                    float y = Mathf.Sin(angle) * currentRadius * 0.3f; // Achatado verticalmente

                    positions[j] = bossTransform.position + new Vector3(x, y, 0);
                }

                line.SetPositions(positions);

                // Cor muda com o progresso
                Color color = Color.Lerp(Color.cyan, Color.blue, progress);
                color.a = Mathf.Lerp(1f, 0.2f, progress);
                line.startColor = color;
                line.endColor = new Color(color.r, color.g, color.b, 0);
            }

            yield return null;
        }

        // Remove as linhas
        foreach (LineRenderer line in lines)
        {
            Destroy(line.gameObject);
        }
        lines.Clear();
    }

    void CreateSpiralLine(int index)
    {
        GameObject lineObj = new GameObject($"Spiral_{index}");
        lineObj.transform.SetParent(transform);

        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        line.positionCount = pointsPerLine;
        line.startWidth = 0.1f;
        line.endWidth = 0.5f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = Color.cyan;
        line.endColor = Color.blue;
        line.useWorldSpace = true;

        lines.Add(line);
    }
}