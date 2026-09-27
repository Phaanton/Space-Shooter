using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamEnergy : MonoBehaviour
{
    [SerializeField] public GameObject boss;
    public Transform pontoDisparo; // Isso será atribuído pelo código!

    [Header("Configurações")]
    [SerializeField] private float largura = 1.5f;
    [SerializeField] private float duracao = 3f;
    [SerializeField] private AnimationCurve intensidadeCurve;

    private LineRenderer raioPrincipal;
    private LineRenderer raioInterno;
    private Light luzDoRaio;
    private Transform bossTransform;

    void Awake()
    {
        // Não precisa mais encontrar o boss aqui, pois já será atribuído
        if (boss != null)
        {
            bossTransform = boss.transform;
        }

        // Cria curva padrão se não definida
        if (intensidadeCurve == null || intensidadeCurve.keys.Length == 0)
        {
            intensidadeCurve = new AnimationCurve();
            intensidadeCurve.AddKey(0, 0);
            intensidadeCurve.AddKey(0.2f, 1);
            intensidadeCurve.AddKey(0.8f, 1);
            intensidadeCurve.AddKey(1, 0);
        }

        CriarRaio();
    }

    void CriarRaio()
    {
        // Raio principal (mais largo, semi-transparente)
        raioPrincipal = gameObject.AddComponent<LineRenderer>();
        raioPrincipal.positionCount = 2;
        raioPrincipal.startWidth = largura;
        raioPrincipal.endWidth = largura * 2;
        raioPrincipal.material = new Material(Shader.Find("Sprites/Default"));
        raioPrincipal.startColor = new Color(0, 0.5f, 1, 0.3f);
        raioPrincipal.endColor = new Color(0, 0, 1, 0.1f);

        // Raio interno (mais brilhante)
        GameObject raioGO = new GameObject("RaioInterno");
        raioGO.transform.parent = transform;
        raioGO.transform.localPosition = Vector3.zero;

        raioInterno = raioGO.AddComponent<LineRenderer>();
        raioInterno.positionCount = 2;
        raioInterno.startWidth = largura * 0.3f;
        raioInterno.endWidth = largura * 0.6f;
        raioInterno.material = new Material(Shader.Find("Sprites/Default"));
        raioInterno.startColor = Color.white;
        raioInterno.endColor = Color.cyan;

        // Luz para efeito
        luzDoRaio = gameObject.AddComponent<Light>();
        luzDoRaio.type = LightType.Point;
        luzDoRaio.range = 5;
        luzDoRaio.color = Color.cyan;
        luzDoRaio.intensity = 0;
    }

    void Start()
    {
        StartCoroutine(EfeitoRaio());
    }

    IEnumerator EfeitoRaio()
    {
        float tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float t = tempo / duracao;
            float intensidade = intensidadeCurve.Evaluate(t);

            // POSIÇÃO DE ORIGEM: USA DIRETAMENTE O pontoDisparo!
            Vector3 inicio;

            if (pontoDisparo != null)
            {
                inicio = pontoDisparo.position; // USANDO DIRETO!
            }
            else if (bossTransform != null)
            {
                inicio = bossTransform.position;
            }
            else
            {
                inicio = transform.position;
            }

            // POSIÇÃO DE DESTINO: No chão (Y = -10)
            Vector3 fim = new Vector3(inicio.x, -10f, 0);

            // Atualiza as posições do raio
            raioPrincipal.SetPosition(0, inicio);
            raioPrincipal.SetPosition(1, fim);
            raioInterno.SetPosition(0, inicio);
            raioInterno.SetPosition(1, fim);

            // Efeito de "pulso"
            float pulso = 1 + Mathf.Sin(tempo * 20) * 0.1f;

            // Ajusta largura baseado na intensidade
            raioPrincipal.startWidth = largura * intensidade * pulso;
            raioPrincipal.endWidth = (largura * 2) * intensidade * pulso;

            raioInterno.startWidth = (largura * 0.3f) * intensidade;
            raioInterno.endWidth = (largura * 0.6f) * intensidade;

            // Luz pulsante
            luzDoRaio.intensity = intensidade * 3;

            // Opcional: A posição da luz também segue o boss
            if (luzDoRaio != null)
            {
                luzDoRaio.transform.position = inicio;
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        var boss = Object.FindFirstObjectByType<BossController>();
        if (boss != null)
        {
            boss.OnBeamDestroyed();
        }
    }
}