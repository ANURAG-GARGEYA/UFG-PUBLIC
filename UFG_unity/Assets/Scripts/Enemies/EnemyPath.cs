using System.Collections.Generic;
using UnityEngine;

public enum PathType
{
    Crawling,
    Flying
}

[ExecuteInEditMode]
public class EnemyPath : MonoBehaviour
{

    public bool isLooped;
    public PathType type = PathType.Crawling;

    [SerializeField] private Color _waypointColor = Color.red;
    [SerializeField] private List<EnemyPathWaypoint> _waypoints = new List<EnemyPathWaypoint>();

    private int _enemiesOnPath = 0;

    public int WaypointCount { get { return _waypoints.Count; } }
    public int EnemiesOnPath { get { return _enemiesOnPath; } }

    public void AddEnemy() { _enemiesOnPath++; }
    public void RemoveEnemy() { _enemiesOnPath--; }

    public Vector3 GetNewDestination(int index)
    {
        Vector3? newDestination = GetWaypointPosition(index);

        return newDestination ?? Vector3.zero;
    }


    public Vector3? GetWaypointPosition(int index)
    {
        if (!isLooped)
        {
            if (index < 0)
                index = 0;
            else if (index >= _waypoints.Count)
                index = _waypoints.Count - 1;
        }

        if (isLooped)
        {
            index = index % _waypoints.Count;
            if (index < 0)
                index = _waypoints.Count + index;
        }

        return _waypoints[index].transform.position;
    }


    public int GetClosestWaypointIndex(Vector3 position)
    {
        float smallestDistance = float.MaxValue;
        int closestIndex = 0;

        for (int i = 0; i < _waypoints.Count; i++)
        {
            float currentDistance = Vector3.Distance(position, _waypoints[i].transform.position);
            if (currentDistance < smallestDistance)
            {
                smallestDistance = currentDistance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }


    void Update()
    {
        CheckForNewWaypoints();
        CheckForDeletedWaypoints();
    }


    void OnValidate()
    {
        CheckForWaypointColorChange();
    }


    void CheckForNewWaypoints()
    {
        //                  Waypoints object always at 0 -------v
        Transform waypointsParentTransfrom = transform.GetChild(0);

        foreach (Transform wpTransform in waypointsParentTransfrom)
        {
            GameObject wpGameObject = wpTransform.gameObject;
            EnemyPathWaypoint wp = wpGameObject.GetComponent<EnemyPathWaypoint>();

            if (!_waypoints.Contains(wp))
            {
                if (wp == null)
                {
                    wp = wpGameObject.AddComponent<EnemyPathWaypoint>();
                    wp.GizmoColor = _waypointColor;
                }

                _waypoints.Insert(0, wp);
            }
        }
    }


    void CheckForDeletedWaypoints()
    {
        if (_waypoints.Count > 0)
            _waypoints.RemoveAll(wp => wp == null);
    }


    void CheckForWaypointColorChange()
    {
        if (_waypoints.Count > 0 && _waypoints[0].GizmoColor != _waypointColor)
            foreach (EnemyPathWaypoint wp in _waypoints)
                wp.GizmoColor = _waypointColor;
    }


    public void OnDrawGizmos()
    {
        Gizmos.color = _waypointColor;
        for (int i = 0; i < _waypoints.Count - 1; i++)
            Gizmos.DrawLine(_waypoints[i].transform.position, _waypoints[i + 1].transform.position);

        if (isLooped)
            Gizmos.DrawLine(_waypoints[0].transform.position, _waypoints[_waypoints.Count - 1].transform.position);
    }
}
