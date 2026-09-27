using UnityEngine;

public class TrainConfig : MonoBehaviour
{
    public Vector2 exitDirection;
    public float speed;
    public Waypoint[] waypoints;
    public bool loopWaypoints;
    public float waypointTolerance;
    public float entryDuration;
    public float turnDuration;
    public bool useWaypoints;

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

    public void ApplyToLeader(EnemyMover mover)
    {
        if (useWaypoints && waypoints != null && waypoints.Length > 0)
        {
            mover.SetTrainLeaderWithWaypoints(waypoints, loopWaypoints, waypointTolerance, speed);
        }
        else
        {
            mover.SetTrainLeader(exitDirection, speed);
            mover.SetTrainTiming(entryDuration, turnDuration);
        }
    }
}