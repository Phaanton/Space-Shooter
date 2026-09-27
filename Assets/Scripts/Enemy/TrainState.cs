using UnityEngine;

public class TrainState : MonoBehaviour
{
    [Header("Configurações do Trem")]
    public Vector2 exitDirection;
    public float speed;
    public Waypoint[] waypoints;
    public bool loopWaypoints;
    public float waypointTolerance;
    public float entryDuration;
    public float turnDuration;
    public bool useWaypoints;

    [Header("Estado Atual (Progresso)")]
    public int currentWaypointIndex = 0;
    public bool isWaiting = false;
    public float waitTimer = 0f;
    public TrainLeaderState currentState = TrainLeaderState.Descending;
    public float stateStartTime = 0f;

    public void SaveConfig(SpawnPattern pattern)
    {
        exitDirection = pattern.trainExitDirection;
        speed = pattern.entrySpeed;
        waypoints = pattern.waypoints;
        loopWaypoints = pattern.loopWaypoints;
        waypointTolerance = pattern.waypointTolerance;
        entryDuration = pattern.trainEntryDuration;
        turnDuration = pattern.trainTurnDuration;
        useWaypoints = pattern.useWaypoints;
    }

    public void SaveCurrentState(EnemyMover mover)
    {
        if (mover == null) return;

        currentWaypointIndex = mover.GetCurrentWaypointIndex();
        isWaiting = mover.IsWaiting();
        waitTimer = mover.GetWaitTimer();
        currentState = mover.GetCurrentState();
        stateStartTime = mover.GetStateStartTime();

        Debug.Log($"Estado salvo: waypoint={currentWaypointIndex}, waiting={isWaiting}, state={currentState}, startTime={stateStartTime}");
    }

    public void ApplyToLeader(EnemyMover mover)
    {
        if (mover == null) return;

        Debug.Log($"Aplicando estado ao novo líder: waypoint={currentWaypointIndex}, waiting={isWaiting}, state={currentState}");

        if (useWaypoints && waypoints != null && waypoints.Length > 0)
        {
            mover.SetTrainLeaderWithWaypoints(waypoints, loopWaypoints, waypointTolerance, speed);
            mover.RestoreProgress(currentWaypointIndex, isWaiting, waitTimer, currentState, stateStartTime);
        }
        else
        {
            mover.SetTrainLeader(exitDirection, speed);
            mover.SetTrainTiming(entryDuration, turnDuration);
            mover.RestoreSimpleState(currentState, stateStartTime);
        }
    }
}