using UnityEngine;

public class EnemyPathWaypoint : MonoBehaviour
{
    private float _gizmoSize = 0.1f;
    private bool _isVisible = true;
    private Color _gizmoColor = Color.red;

    public bool IsVisible
    {
        get { return _isVisible; }
        set { _isVisible = value; }
    }

    public Color GizmoColor
    {
        get { return _gizmoColor; }
        set { _gizmoColor = value; }
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = _gizmoColor;
        Gizmos.DrawWireSphere(transform.position, _gizmoSize);
    }
}
