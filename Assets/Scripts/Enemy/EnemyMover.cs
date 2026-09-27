using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyMover : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [SerializeField] private MovementType movementType = MovementType.Simple;

    [Header("Configurações Gerais")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private Vector2 entryDirection = Vector2.down;

    [Header("Padrão 1 - Curva de Saída")]
    [SerializeField] private bool useCurveExit = false;
    [SerializeField] private float curveExitY = -2f;        // Ponto Y para iniciar curva
    [SerializeField] private float curveDuration = 1.5f;    // Duração da curva
    [SerializeField] private AnimationCurve curvePath;       // Curva personalizada
    [SerializeField] private Vector2 curveExitDirection = Vector2.left; // Direção da saída

    [Header("Padrão 2 - Movimento em Arco")]
    [SerializeField] private float arcHeight = 3f;           // Altura do arco
    [SerializeField] private float arcWidth = 5f;            // Largura do arco
    [SerializeField] private float arcDuration = 3f;         // Duração do arco

    [Header("Padrão 3 - Movimento em L")]
    [SerializeField] private float lShapeTurnPoint = 0f;     // Ponto de virada (X ou Y)
    [SerializeField] private Vector2 lShapeExitDirection = Vector2.left;
    [SerializeField] private bool turnOnX = true;            // true = vira no X, false = vira no Y

    [Header("Padrão 4 - Zig-Zag")]
    [SerializeField] private float zigZagAmplitude = 2f;     // Amplitude do zig-zag
    [SerializeField] private float zigZagFrequency = 3f;     // Frequência do zig-zag
    [SerializeField] private float zigZagDuration = 0f;      // 0 = infinito

    [Header("Padrão 5 - Desaceleração")]
    [SerializeField] private float decelerationRate = 2f;    // Taxa de desaceleração
    [SerializeField] private float stopDuration = 0f;        // 0 = para permanentemente
    [SerializeField] private bool exitAfterStop = false;     // Sai após parar?
    [SerializeField] private Vector2 exitDirection = Vector2.down;
    [SerializeField] private float exitSpeed = 2f;

    [Header("Configurações Train (Líder)")]
    [SerializeField] private bool isTrainLeader = false;
    [SerializeField] private Vector2 trainExitDirection;
    [SerializeField] private float trainEntryDuration = 2f;
    [SerializeField] private float trainTurnDuration = 1.5f;

    [Header("Sistema de Waypoints")]
    [SerializeField] private Waypoint[] waypoints;
    [SerializeField] private bool loopWaypoints = false;
    [SerializeField] private float waypointTolerance = 0.5f;
    [SerializeField] private int currentWaypointIndex = 0;
    [SerializeField] private bool isWaiting = false;
    [SerializeField] private float waitTimer = 0f;
    [SerializeField] private float currentSpeed;

    [Header("Rotação")]
    [SerializeField] private bool enableRotation = true;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float rotationOffset = 0f;  // Offset para sprite

    public int GetCurrentWaypointIndex() { return currentWaypointIndex; }
    public bool IsWaiting() { return isWaiting; }
    public float GetWaitTimer() { return waitTimer; }
    public TrainLeaderState GetCurrentState() { return trainState; }
    public float GetStateStartTime() { return trainLeaderStartTime; }

    private Vector2 lastPosition;
    private bool isMovingToWaypoint = false;

    private bool trainLeaderMode = false;
    private bool usingWaypoints = false;
    private Vector2 targetPosition;
    private Vector2 moveDirection;

    private float trainLeaderStartTime;
    private Vector2 trainLeaderStartPos;
    
    private TrainLeaderState trainState = TrainLeaderState.Descending;

    // Controles internos
    private Vector2 startPosition;
    private float startTime;
    private bool isMoving = true;
    private bool hasExited = false;
    private bool isDecelerating = false;
    private bool isStopped = false;
    private float stopTimer = 0f;
    private Vector2 movementDirection;
    private bool hasEnteredScreen = false;
    private float screenEnterTime = 0f;
    private float exitDelay = 2f; // Tempo após sair da tela antes de destruir

    void Start()
    {
        startPosition = transform.position;
        startTime = Time.time;
        currentSpeed = speed;
        movementDirection = entryDirection.normalized;
        lastPosition = transform.position;
        enableRotation = false;
    }

    void Update()
    {
        if (!isMoving || hasExited) return;

        if (trainLeaderMode)
        {
            if (usingWaypoints)
            {
                MoveTrainWithWaypoints();
            }
            else
            {
                MoveTrainLeader();
            }

            if (enableRotation)
            {
                UpdateRotationForTrain();
            }
        }
        else
        {
            isMovingToWaypoint = false;

            switch (movementType)
            {
                case MovementType.Simple:
                    MoveSimple();
                    break;
                case MovementType.Diagonal:
                    MoveDiagonal();
                    break;
                case MovementType.CurveExit:
                    MoveCurveExit();
                    break;
                case MovementType.Arc:
                    MoveArc();
                    break;
                case MovementType.LShape:
                    MoveLShape();
                    break;
                case MovementType.ZigZag:
                    MoveZigZag();
                    break;
                case MovementType.Decelerate:
                    MoveDecelerate();
                    break;
            }
        }

        if (enableRotation && !isMovingToWaypoint) // Não rotacionar durante waypoints?
        {
            UpdateRotation();
        }

        lastPosition = transform.position;

        // Verifica se saiu da tela
        CheckBounds();
    }

    #region Movimentos Básicos

    private void MoveSimple()
    {
        transform.Translate(movementDirection * currentSpeed * Time.deltaTime);
    }

    private void MoveDiagonal()
    {
        transform.Translate(movementDirection * currentSpeed * Time.deltaTime);
    }

    #endregion

    #region Padrão 1 - Curva de Saída

    private void MoveCurveExit()
    {
        if (!useCurveExit)
        {
            MoveSimple();
            return;
        }

        // Verifica se chegou no ponto de curva
        if (!hasExited && transform.position.y <= curveExitY)
        {
            StartCoroutine(ExecuteCurveExit());
            hasExited = true;
        }

        if (!hasExited)
        {
            transform.Translate(Vector2.down * currentSpeed * Time.deltaTime);
        }
    }

    private System.Collections.IEnumerator ExecuteCurveExit()
    {
        float elapsed = 0f;
        Vector2 startPos = transform.position;
        Vector2 endPos = new Vector2(
            startPos.x + (curveExitDirection.x * 5f),
            startPos.y - 2f
        );

        while (elapsed < curveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / curveDuration;

            // Usa AnimationCurve para controle preciso da trajetória
            float curveT = curvePath != null ? curvePath.Evaluate(t) : t;

            // Movimento parabólico
            float x = Mathf.Lerp(startPos.x, endPos.x, curveT);
            float y = Mathf.Lerp(startPos.y, endPos.y, curveT) +
                      Mathf.Sin(t * Mathf.PI) * 1.5f; // Efeito de curva

            transform.position = new Vector2(x, y);

            yield return null;
        }
    }

    #endregion

    #region Padrão 2 - Movimento em Arco

    private void MoveArc()
    {
        float elapsed = Time.time - startTime;
        float t = Mathf.Clamp01(elapsed / arcDuration);

        // Calcula posição no arco
        float x = startPosition.x + (arcWidth * Mathf.Sin(t * Mathf.PI));
        float y = startPosition.y - (arcHeight * t) +
                  (arcHeight * Mathf.Sin(t * Mathf.PI));

        transform.position = new Vector2(x, y);

        // Se terminou o arco, continua reto
        if (t >= 1f)
        {
            movementType = MovementType.Simple;
            movementDirection = Vector2.down;
        }
    }

    #endregion

    #region Padrão 3 - Movimento em L

    private void MoveLShape()
    {
        if (!hasExited)
        {
            if (turnOnX)
            {
                // Move até atingir o X alvo
                if (Mathf.Abs(transform.position.x - lShapeTurnPoint) > 0.5f)
                {
                    transform.Translate(Vector2.right * Mathf.Sign(lShapeTurnPoint - transform.position.x)
                                       * currentSpeed * Time.deltaTime);
                }
                else
                {
                    hasExited = true;
                    movementDirection = lShapeExitDirection;
                }
            }
            else
            {
                // Move até atingir o Y alvo
                if (Mathf.Abs(transform.position.y - lShapeTurnPoint) > 0.5f)
                {
                    transform.Translate(Vector2.down * currentSpeed * Time.deltaTime);
                }
                else
                {
                    hasExited = true;
                    movementDirection = lShapeExitDirection;
                }
            }
        }
        else
        {
            transform.Translate(movementDirection * currentSpeed * Time.deltaTime);
        }
    }

    #endregion

    #region Padrão 4 - Zig-Zag

    private void MoveZigZag()
    {
        float elapsed = Time.time - startTime;

        // Verifica duração
        if (zigZagDuration > 0 && elapsed >= zigZagDuration)
        {
            movementType = MovementType.Simple;
            MoveSimple();
            return;
        }

        // Movimento base
        Vector2 baseMovement = entryDirection * currentSpeed * Time.deltaTime;

        // Oscilação lateral
        float offsetX = Mathf.Sin(elapsed * zigZagFrequency) * zigZagAmplitude * Time.deltaTime;

        transform.Translate(new Vector2(baseMovement.x + offsetX, baseMovement.y));
    }

    #endregion

    #region Padrão 5 - Desaceleração

    private void MoveDecelerate()
    {
        if (!isDecelerating && !isStopped)
        {
            // Desacelera
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, decelerationRate * Time.deltaTime);

            if (currentSpeed <= 0.1f)
            {
                currentSpeed = 0f;
                isDecelerating = true;
                isStopped = true;
                stopTimer = stopDuration;
            }

            transform.Translate(movementDirection * currentSpeed * Time.deltaTime);
        }
        else if (isStopped)
        {
            if (stopDuration > 0)
            {
                stopTimer -= Time.deltaTime;

                if (stopTimer <= 0)
                {
                    if (exitAfterStop)
                    {
                        // Sai da tela
                        movementType = MovementType.Simple;
                        movementDirection = exitDirection;
                        currentSpeed = exitSpeed;
                        isStopped = false;
                        isDecelerating = false;
                    }
                }
            }
        }
    }

    #endregion

    // Movimento do líder do trem
    private void MoveTrainLeader()
    {
        switch (trainState)
        {
            case TrainLeaderState.Descending:
                // Desce reto
                transform.Translate(Vector2.down * currentSpeed * Time.deltaTime);

                // Verifica se já desceu o suficiente
                if (Time.time - trainLeaderStartTime >= trainEntryDuration)
                {
                    trainState = TrainLeaderState.Turning;
                    trainLeaderStartTime = Time.time;
                }
                break;

            case TrainLeaderState.Turning:
                // Faz a curva suave
                float turnProgress = (Time.time - trainLeaderStartTime) / trainTurnDuration;
                turnProgress = Mathf.Clamp01(turnProgress);

                // Interpola a direção (usa AnimationCurve se disponível)
                Vector2 currentDir = Vector2.Lerp(Vector2.down, trainExitDirection, turnProgress).normalized;
                transform.Translate(currentDir * currentSpeed * Time.deltaTime);

                if (turnProgress >= 1f)
                {
                    trainState = TrainLeaderState.Exiting;
                }
                break;

            case TrainLeaderState.Exiting:
                // Move na direção final
                transform.Translate(trainExitDirection.normalized * currentSpeed * Time.deltaTime);
                break;
        }
    }

    // NOVO: Configurar movimento Train
    public void SetTrainLeader(Vector2 exitDirection, float speed)
    {
        trainLeaderMode = true;
        isTrainLeader = true;
        trainExitDirection = exitDirection;
        currentSpeed = speed;
        trainLeaderStartTime = Time.time;
        trainLeaderStartPos = transform.position;
        trainState = TrainLeaderState.Descending;

        // Configurações opcionais
        trainEntryDuration = 2f;  // Desce por 2 segundos
        trainTurnDuration = 1.5f;  // Curva de 1.5 segundos
    }

    public void SetTrainTiming(float entryDuration, float turnDuration)
    {
        trainEntryDuration = entryDuration;
        trainTurnDuration = turnDuration;
    }

    private void MoveTrainWithWaypoints()
    {
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                isWaiting = false;
            }
            return;
        }

        if (waypoints == null || waypoints.Length == 0) return;

        // Pega o waypoint atual
        Waypoint targetWaypoint = waypoints[currentWaypointIndex];
        targetPosition = targetWaypoint.position;

        // Calcula direção
        Vector2 directionToTarget = targetPosition - (Vector2)transform.position;
        float distanceToTarget = directionToTarget.magnitude;

        // Se muito perto, considera que chegou
        if (distanceToTarget <= waypointTolerance)
        {
            // Chegou no waypoint
            Debug.Log($"Líder chegou no waypoint {currentWaypointIndex}: {targetWaypoint.pointName}");

            // Se tem tempo de espera
            if (targetWaypoint.waitTime > 0)
            {
                isWaiting = true;
                waitTimer = targetWaypoint.waitTime;
            }

            // Avança para o próximo waypoint
            currentWaypointIndex++;

            // Verifica se acabaram os waypoints
            if (currentWaypointIndex >= waypoints.Length)
            {
                if (loopWaypoints)
                {
                    // Volta para o primeiro
                    currentWaypointIndex = 0;
                    Debug.Log("Loop: voltando ao primeiro waypoint");
                }
                else
                {
                    // Acabou, agora faz a saída
                    Debug.Log("Waypoints concluídos, iniciando saída");
                    usingWaypoints = false;
                    trainState = TrainLeaderState.Exiting;
                    trainLeaderStartTime = Time.time;

                    // IMPORTANTE: Define uma direção de saída padrão se não tiver
                    if (trainExitDirection == Vector2.zero)
                    {
                        trainExitDirection = Vector2.down; // Sai para baixo por padrão
                    }
                }
            }
            return;
        }

        // Define velocidade (usa a do waypoint ou a padrão)
        float waypointSpeed = targetWaypoint.speed > 0 ? targetWaypoint.speed : this.currentSpeed;

        // Move em direção ao waypoint
        Vector2 movement = directionToTarget.normalized * waypointSpeed * Time.deltaTime;
        transform.Translate(movement, Space.World);

        // DEBUG: Desenhar linha para o waypoint
        Debug.DrawLine(transform.position, targetPosition, Color.red);
    }

    public void SetTrainLeaderWithWaypoints(Waypoint[] points, bool loop, float tolerance, float speed)
    {
        trainLeaderMode = true;
        isTrainLeader = true;
        usingWaypoints = true;
        waypoints = points;
        loopWaypoints = loop;
        waypointTolerance = tolerance;
        currentSpeed = speed;
        currentWaypointIndex = 0;
        isWaiting = false;

        // Mantém a direção de saída já configurada (do pattern)
        // Se não tiver, define um padrão
        if (trainExitDirection == Vector2.zero)
        {
            trainExitDirection = Vector2.down;
        }

        // IMPORTANTE: Resetar rotação para zero ao começar waypoints
        if (enableRotation)
        {
            transform.rotation = Quaternion.identity;
        }

        Debug.Log($"Líder configurado com {points.Length} waypoints. Loop: {loop}. Saída: {trainExitDirection}");
    }

    public void RestoreProgress(int waypointIndex, bool waiting, float timer, TrainLeaderState state, float startTime)
    {
        if (!usingWaypoints) return;

        currentWaypointIndex = waypointIndex;
        isWaiting = waiting;
        waitTimer = timer;
        trainState = state;
        trainLeaderStartTime = startTime;

        // Log para debug
        Debug.Log($"Restaurando progresso: waypoint={waypointIndex}, waiting={waiting}, state={state}, startTime={startTime}");
    }

    public void RestoreSimpleState(TrainLeaderState state, float startTime)
    {
        if (usingWaypoints) return;

        trainState = state;
        trainLeaderStartTime = startTime;

        Debug.Log($"Restaurado estado simples: {state}");
    }

    private void UpdateRotationForTrain()
    {
        // Calcula direção do movimento baseado na posição atual vs anterior
        Vector2 moveDirection = (Vector2)transform.position - lastPosition;

        // Se não está se movendo significativamente, não rotaciona
        if (moveDirection.magnitude < 0.01f)
            return;

        // Aplica rotação baseada na direção real do movimento
        Quaternion targetRotation = Quaternion.FromToRotation(Vector3.down, moveDirection.normalized);

        // Adiciona offset se necessário
        if (rotationOffset != 0)
        {
            targetRotation *= Quaternion.Euler(0, 0, rotationOffset);
        }

        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    public void SetRotationSettings(bool enable, float speed, float offset)
    {
        enableRotation = enable;
        rotationSpeed = speed;
        rotationOffset = offset;

        // Se está ativando a rotação, garante que a rotação inicial seja resetada
        if (enable)
        {
            // Não resetamos aqui para não perder a direção atual
            Debug.Log($"Rotação configurada para {gameObject.name}: enable={enable}, speed={speed}, offset={offset}");
        }
    }

    private void UpdateRotation()
    {
        Vector2 moveDirection = targetPosition - (Vector2)transform.position;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.FromToRotation(Vector3.down, moveDirection.normalized), rotationSpeed * Time.deltaTime);
    }

    private void CheckBounds()
    {
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        bool isOnScreen = viewportPos.x >= -0.1f && viewportPos.x <= 1.1f &&
                          viewportPos.y >= -0.1f && viewportPos.y <= 1.1f;

        // Se entrou na tela pela primeira vez
        if (isOnScreen && !hasEnteredScreen)
        {
            hasEnteredScreen = true;
            screenEnterTime = Time.time;
        }

        // Se está fora da tela
        if (viewportPos.x < -0.3f || viewportPos.x > 1.3f ||
            viewportPos.y < -0.3f || viewportPos.y > 1.3f)
        {
            // Se nunca entrou na tela (acabou de spawnar), não destrói ainda
            if (!hasEnteredScreen)
                return;

            // Se já entrou na tela, dá um tempo antes de destruir
            if (Time.time - screenEnterTime > 1f)
            {
                Destroy(gameObject);
            }
        }
    }

    #region Métodos Públicos para Configuração

    public void SetMovementType(MovementType type)
    {
        movementType = type;
        startPosition = transform.position;
        startTime = Time.time;
    }

    public void SetSimpleMovement(Vector2 direction, float moveSpeed)
    {
        movementType = MovementType.Simple;
        entryDirection = direction;
        speed = moveSpeed;
        movementDirection = direction.normalized;
    }

    public void SetCurveExit(float exitY, Vector2 exitDir, float curveTime, AnimationCurve curve = null)
    {
        movementType = MovementType.CurveExit;
        useCurveExit = true;
        curveExitY = exitY;
        curveExitDirection = exitDir;
        curveDuration = curveTime;
        if (curve != null) curvePath = curve;
    }

    public void SetArcMovement(float height, float width, float duration)
    {
        movementType = MovementType.Arc;
        arcHeight = height;
        arcWidth = width;
        arcDuration = duration;
        startPosition = transform.position;
    }

    public void SetLShape(float turnPoint, Vector2 exitDir, bool turnOnXAxis)
    {
        movementType = MovementType.LShape;
        lShapeTurnPoint = turnPoint;
        lShapeExitDirection = exitDir;
        turnOnX = turnOnXAxis;
    }

    public void SetZigZag(float amplitude, float frequency, float duration = 0f)
    {
        movementType = MovementType.ZigZag;
        zigZagAmplitude = amplitude;
        zigZagFrequency = frequency;
        zigZagDuration = duration;
    }

    public void SetDecelerate(float decelRate, float stopTime, bool exitAfter, Vector2 exitDir, float exitSpd)
    {
        movementType = MovementType.Decelerate;
        decelerationRate = decelRate;
        stopDuration = stopTime;
        exitAfterStop = exitAfter;
        exitDirection = exitDir;
        exitSpeed = exitSpd;
    }

    // Métodos existentes
    public void SetEntryDirection(Vector2 direction)
    {
        entryDirection = direction;
        movementDirection = direction.normalized;
    }

    public void SetEntrySpeed(float moveSpeed)
    {
        speed = moveSpeed;
        currentSpeed = moveSpeed;
    }

    #endregion
}
