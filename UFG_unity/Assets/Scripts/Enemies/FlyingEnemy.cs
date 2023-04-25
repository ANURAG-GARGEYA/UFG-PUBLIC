using System.Collections;
using UnityEngine;
using Photon.Pun;

// ! PHOTON ISSUE: RPC ARE NOT INHERITED PROPERLY AND IT BREAKS THE DESTROY AND STUN CALLS
public class FlyingEnemy : CrawlingEnemy
{
    private bool _isOnCrawlingSufrace = false;
    new private EnemyType type = EnemyType.Flying;
    private Vector3? _obstacleAvoidanceWaypoint;
    private ObstacleAvoidanceBarrier _obstacleBarrier;

    private bool IsFlying
    {
        get { return !_agent.enabled && !_isOnCrawlingSufrace; }
    }

    private bool IsNextWaypointFar
    {
        get
        {
            float distance = Vector3.Distance(transform.position, CurrentWaypointPosition);

            return distance > _obstacleBarrier.Radius * 1.1;
        }
    }

    private float FlyingSpeed
    {
        get { return _agent.speed * 0.85f; }
    }


    protected override void Start()
    {
        base.Start();

        _obstacleBarrier = transform.GetChild(1).GetComponent<ObstacleAvoidanceBarrier>();
        _obstacleBarrier.OnCollisionEnterAction += OnObstacleBarrierEnter;
        EnableFlight();
    }


    protected override void Update()
    {
        base.Update();

        if (!IsFlying && IsNextWaypointFar && !_isStunned)
            EnableFlight();

        if (IsFlying && !_isStunned)
            Fly();
    }


    protected override void UpdateAnimation()
    {
        base.UpdateAnimation();

        if (IsFlying)
            _animator.SetBool("Flying", true);
        else
            _animator.SetBool("Flying", false);
    }



    protected override void OnCollisionEnter(Collision collision)
    {
        if (IsFlying && collision.collider.tag == "CrawlingSurface")
        {
            _isOnCrawlingSufrace = true;
            DisableFlight();
        }

        base.OnCollisionEnter(collision);
    }


    void OnObstacleBarrierEnter(Collision collision)
    {
        if (collision.collider.tag != "CrawlingSurface")
            return;

        if (IsFlying && IsNextWaypointFar && !_isStunned)
        {
            Vector3 cwp = _path.GetNewDestination(_currentWaypoint);
            Vector3 movingDirection = Vector3.Normalize(cwp - transform.position);
            ContactPoint cp = collision.GetContact(0);
            _obstacleAvoidanceWaypoint = transform.position + Vector3.Reflect(cp.normal, movingDirection) * 0.5f;
        }
    }


    void OnCollisionExit(Collision collision)
    {
        if (collision.collider.tag == "CrawlingSurface")
            _isOnCrawlingSufrace = false;
    }


    void EnableFlight()
    {
        _agent.enabled = false;
    }

    void DisableFlight()
    {
        _agent.enabled = true;
    }


    void Fly()
    {
        if (_isFollowingPath)
        {
            UpdateCurrentWaypoint();

            Vector3 destination = _obstacleAvoidanceWaypoint ?? _path.GetNewDestination(_currentWaypoint);
            if (!_isCarryingFoodPiece && NearbyFood != null)
                destination = (Vector3)NearbyFood;

            FlyToDestination(destination);
        }
    }


    protected override bool UpdateCurrentWaypoint()
    {
        if (Vector3.Distance(transform.position, _obstacleAvoidanceWaypoint ?? Vector3.positiveInfinity) < 0.1f)
            _obstacleAvoidanceWaypoint = null;

        return base.UpdateCurrentWaypoint();
    }

    void FlyToDestination(Vector3 destination)
    {
        transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * FlyingSpeed);
    }

    protected override IEnumerator Stun()
    {
        if (IsFlying)
        {
            GetComponent<Rigidbody>().useGravity = true;
            PlayRandomClip(_damageClips);
            yield return new WaitForSeconds(_stunDuration);
            GetComponent<Rigidbody>().useGravity = false;
            _currentWaypoint = _path.GetClosestWaypointIndex(transform.position);
            _isStunned = false;
        }
        else
            yield return StartCoroutine(base.Stun());
    }


    [PunRPC]
    void DamageEffectOnNetwork(float delay)
    {
        GameObject vfx = SpawnVFX(_damageEffect, new Vector3(0.3f, 0.3f, 0.3f));
        Destroy(vfx, delay);
    }


    [PunRPC]
    void DeathEffectOnNetwork()
    {
        SpawnVFX(_deathEffect, new Vector3(0.3f, 0.3f, 0.3f));
    }


    [PunRPC]
    void EscapeEffectOnNetwork()
    {
        SpawnVFX(_escapeEffect, new Vector3(0.3f, 0.3f, 0.3f));
    }


    [PunRPC]
    void SetPathOnNetwork(int pathIndex)
    {
        Transform pathsParent = GameObject.Find("Paths").transform;
        EnemyPath pathToSet = pathsParent.GetChild(pathIndex).GetComponent<EnemyPath>();
        _path = pathToSet;
    }


    [PunRPC]
    void DestroyOnNetwork(float delay)
    {
        StartCoroutine(PhotonDestroy(photonView.ViewID, delay));
    }


    [PunRPC]
    void StunOnNetwork()
    {
        StartCoroutine(Stun());
    }
}
