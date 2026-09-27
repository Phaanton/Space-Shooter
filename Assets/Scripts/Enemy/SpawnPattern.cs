using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSpawnPattern", menuName = "Game/Spawn Pattern")]
public class SpawnPattern : ScriptableObject
{
    public string patternName;
    public FormationType formation;

    [Header("Posição de Spawn")]
    public SpawnPosition baseSpawnPosition = SpawnPosition.Top;
    public Vector2 customPosition;

    [Header("Spawn Point Específico")]
    public bool useSpecificSpawnPoint = false;           // Ativar modo específico?
    public int specificSpawnPointIndex = 0;              // Índice do spawn point (0, 1, 2...)

    [Header("Configurações da Formação")]
    public int numberOfEnemies = 5;
    public float spacing = 1.5f;
    public float entryDelay = 0.2f;
    public float preDelay = 0f;
    public float postDelay = 1f;

    [Header("Inimigos")]
    public GameObject[] enemiesToSpawn;

    [Header("TIPO DE MOVIMENTO")]
    public MovementType movementType = MovementType.Simple;

    [Header("Configurações Gerais de Movimento")]
    public Vector2 entryDirection = Vector2.down;
    public float entrySpeed = 3f;

    [Header("Padrão 1 - Curva de Saída")]
    public bool useCurveExit = false;
    public float curveExitY = -2f;
    public float curveDuration = 1.5f;
    public AnimationCurve curvePath;
    public Vector2 curveExitDirection = Vector2.left;

    [Header("Padrão 2 - Movimento em Arco")]
    public float arcHeight = 3f;
    public float arcWidth = 5f;
    public float arcDuration = 3f;

    [Header("Padrão 3 - Movimento em L")]
    public float lShapeTurnPoint = 0f;
    public Vector2 lShapeExitDirection = Vector2.left;
    public bool turnOnX = true;

    [Header("Padrão 4 - Zig-Zag")]
    public float zigZagAmplitude = 2f;
    public float zigZagFrequency = 3f;
    public float zigZagDuration = 0f;

    [Header("Padrão 5 - Desaceleração")]
    public float decelerationRate = 2f;
    public float stopDuration = 0f;
    public bool exitAfterStop = false;
    public Vector2 exitDirection = Vector2.down;
    public float exitSpeed = 2f;

    [Header("Padrão 6 - Movimento Trem/Centopeia com Waypoints")]
    public bool useTrainMovement = false;
    public Vector2 trainExitDirection = Vector2.left;  // Usado apenas se não houver waypoints ou loop=false
    public float trainEntryDuration = 2f;
    public float trainTurnDuration = 1.5f;
    public float trainSpacing = 1.2f;

    [Header("Sistema de Waypoints (para movimento do trem)")]
    public bool useWaypoints = false;                    // Usar waypoints?
    public Waypoint[] waypoints;                         // Lista de pontos
    public bool loopWaypoints = false;                    // Voltar ao primeiro após o último?
    public float waypointTolerance = 0.5f;                 // Distância para considerar que chegou

    [Header("Configurações de Rotação")]
    public bool enableRotation = true;
    public float leaderRotationSpeed = 5f;
    public float followerRotationSpeed = 8f;
    public float spriteRotationOffset = 0f;  // -90 se sprite aponta pra cima, etc.
}
