using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainSegment : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private float followSpeed = 10f;
    [SerializeField] private float desiredDistance = 1.2f;
    [SerializeField] private float deadZone = 0.2f;
    [SerializeField] private bool ignoreCollisionWithTrain = true;

    [Header("Suavização")]
    [SerializeField] private float positionLerpSpeed = 15f;

    [Header("Rotação")]
    [SerializeField] private bool enableRotation = true;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private bool lookAtTarget = true;
    [SerializeField] private float rotationOffset = 0f;

    private Transform targetTransform;
    private bool isFollowing = false;
    private Collider2D myCollider;
    private Vector2 lastPosition;
    private Rigidbody2D rb;
    private TrainState trainStateComponent;
    private EnemyMover enemyMover;

    void Start()
    {
        myCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        enemyMover = GetComponent<EnemyMover>();
        lastPosition = transform.position;

        trainStateComponent = GetComponent<TrainState>();
        if (trainStateComponent == null)
        {
            trainStateComponent = gameObject.AddComponent<TrainState>();
        }

        if (myCollider != null)
        {
            myCollider.enabled = true;
        }

        if (ignoreCollisionWithTrain)
        {
            StartCoroutine(SetupCollisionIgnore());
        }
    }

    void Update()
    {
        if (!isFollowing || targetTransform == null) return;

        // ATUALIZAR O ESTADO DO LÍDER CONTINUAMENTE
        if (targetTransform != null)
        {
            EnemyMover leaderMover = targetTransform.GetComponent<EnemyMover>();
            if (leaderMover != null && trainStateComponent != null)
            {
                // Atualiza o estado constantemente para ter o progresso mais recente
                trainStateComponent.SaveCurrentState(leaderMover);
            }
        }

        Vector2 directionToTarget = targetTransform.position - transform.position;
        float distance = directionToTarget.magnitude;

        float distanceDifference = distance - desiredDistance;

        if (Mathf.Abs(distanceDifference) > deadZone)
        {
            Vector2 targetPos = (Vector2)targetTransform.position - (directionToTarget.normalized * desiredDistance);
            Vector2 newPos = Vector2.Lerp(transform.position, targetPos, positionLerpSpeed * Time.deltaTime);
            transform.position = newPos;
        }
        else
        {
            Vector2 targetPos = (Vector2)targetTransform.position - (directionToTarget.normalized * desiredDistance);
            Vector2 newPos = Vector2.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime * 0.3f);
            transform.position = newPos;
        }

        if (enableRotation)
        {
            if (directionToTarget.magnitude > 0.1f)
            {
                Vector2 targetPos = (Vector2)targetTransform.position - (directionToTarget.normalized * desiredDistance);
                Vector2 moveDirection = targetPos - (Vector2)transform.position;
                if (moveDirection.magnitude > 0.01f)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation,
                        Quaternion.FromToRotation(Vector3.down, moveDirection.normalized),
                        rotationSpeed * Time.deltaTime);
                }
            }
        }

        lastPosition = transform.position;
    }

    void LateUpdate()
    {
        // Verifica se o alvo foi destruído (líder morreu)
        if (isFollowing && targetTransform == null)
        {
            Debug.Log($"Segmento {gameObject.name} detectou que o líder foi destruído!");
            BecomeNewLeader();
        }
    }

    private void BecomeNewLeader()
    {
        Debug.Log($"TrainState antes de virar líder: waypoint={trainStateComponent?.currentWaypointIndex}, state={trainStateComponent?.currentState}");

        // Adiciona EnemyMover
        EnemyMover mover = gameObject.AddComponent<EnemyMover>();

        if (trainStateComponent != null)
        {
            // Aplica as configurações e restaura o progresso
            trainStateComponent.ApplyToLeader(mover);
            Debug.Log($"Novo líder configurado com waypoint {trainStateComponent.currentWaypointIndex}");
        }
        else
        {
            Debug.LogError($"Segmento {gameObject.name} não tem TrainState!");
        }

        // Configura rotação
        mover.SetRotationSettings(enableRotation, rotationSpeed, rotationOffset);

        // Remove este componente (já não precisa mais seguir)
        Destroy(this);
    }

    public void SetTarget(Transform target)
    {
        targetTransform = target;
        isFollowing = true;

        // Quando recebe o target, salva as configurações do líder imediatamente
        if (targetTransform != null && trainStateComponent != null)
        {
            EnemyMover leaderMover = targetTransform.GetComponent<EnemyMover>();
            TrainState leaderState = targetTransform.GetComponent<TrainState>();

            if (leaderMover != null)
            {
                // Salva o estado atual do líder
                trainStateComponent.SaveCurrentState(leaderMover);
                Debug.Log($"Segmento {gameObject.name} salvou estado inicial do líder: waypoint {trainStateComponent.currentWaypointIndex}");
            }

            if (leaderState != null)
            {
                // Salva as configurações
                trainStateComponent.exitDirection = leaderState.exitDirection;
                trainStateComponent.speed = leaderState.speed;
                trainStateComponent.waypoints = leaderState.waypoints;
                trainStateComponent.loopWaypoints = leaderState.loopWaypoints;
                trainStateComponent.waypointTolerance = leaderState.waypointTolerance;
                trainStateComponent.entryDuration = leaderState.entryDuration;
                trainStateComponent.turnDuration = leaderState.turnDuration;
                trainStateComponent.useWaypoints = leaderState.useWaypoints;
            }
        }

        // Se tiver EnemyMover, remove para não ter movimento independente
        if (enemyMover != null)
        {
            Destroy(enemyMover);
            enemyMover = null;
        }
    }

    public void SetDesiredDistance(float distance)
    {
        desiredDistance = distance;
    }

    public void SetRotationSettings(bool enable, float speed, float offset)
    {
        enableRotation = enable;
        rotationSpeed = speed;
        rotationOffset = offset;

        if (!enable)
        {
            transform.rotation = Quaternion.identity;
        }
    }

    System.Collections.IEnumerator SetupCollisionIgnore()
    {
        yield return new WaitForSeconds(0.2f);

        TrainSegment[] allSegments = FindObjectsOfType<TrainSegment>();
        foreach (TrainSegment segment in allSegments)
        {
            if (segment != this)
            {
                Collider2D otherCollider = segment.GetComponent<Collider2D>();
                if (otherCollider != null && myCollider != null)
                {
                    Physics2D.IgnoreCollision(myCollider, otherCollider);
                }
            }
        }

        EnemyMover leader = FindObjectOfType<EnemyMover>();
        if (leader != null)
        {
            Collider2D leaderCollider = leader.GetComponent<Collider2D>();
            if (leaderCollider != null && myCollider != null)
            {
                Physics2D.IgnoreCollision(myCollider, leaderCollider);
            }
        }
    }
}