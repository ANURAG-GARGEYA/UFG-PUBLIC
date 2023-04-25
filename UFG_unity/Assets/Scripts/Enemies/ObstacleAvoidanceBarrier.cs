using System;
using UnityEngine;

public class ObstacleAvoidanceBarrier : MonoBehaviour
{
    public Action<Collision> OnCollisionEnterAction;

    private float _radius;

    public float Radius { get { return _radius; } }


    void Start()
    {
        _radius = GetComponent<SphereCollider>().radius;
    }


    void OnCollisionEnter(Collision collision)
    {
        OnCollisionEnterAction?.Invoke(collision);
    }
}
