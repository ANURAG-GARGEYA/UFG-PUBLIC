using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class CrawlingEnemy : MonoBehaviourPun
{
    [SerializeField] protected EnemyPath _path;
    [SerializeField] protected int _health;
    [SerializeField] protected float _killability;
    [SerializeField] protected int _points;
    [SerializeField] protected float _stunDuration;
    [SerializeField] protected GameObject _damageEffect;
    [SerializeField] protected List<AudioClip> _damageClips = new List<AudioClip>();
    [SerializeField] protected GameObject _deathEffect;
    [SerializeField] protected List<AudioClip> _deathClips = new List<AudioClip>();
    [SerializeField] protected GameObject _escapeEffect;

    protected bool _isDead = false;
    protected bool _isIdle = false;
    protected bool _isStunned = false;
    protected bool _isFollowingPath = false;
    protected bool _isCarryingFoodPiece = false;
    protected bool _resolvingDamage = false;
    protected int _currentWaypoint = 0;
    protected int _crawlDirection = 1;

    protected List<Vector3> _foodPositions = new List<Vector3>();
    protected Vector3 _originalScale;
    protected NavMeshAgent _agent;
    protected AudioSource _audioSource;
    protected Animator _animator;

    protected EnemyType type = EnemyType.Crawling;


    public bool IsDead { get { return _health > 0; } }

    public int CurrentWaypoint
    {
        get { return _currentWaypoint; }
        set { _currentWaypoint = value; }
    }

    public EnemyPath Path
    {
        get { return _path; }
        set
        {
            EnemyPath pathToSet = value;
            Transform pathsParent = GameObject.Find("Paths").transform;
            int pathToSetIndex = 0;
            int i = 0;
            foreach (Transform path in pathsParent)
            {
                if (path.GetComponent<EnemyPath>() == pathToSet)
                {
                    pathToSetIndex = i;
                    break;
                }

                i++;
            }

            photonView.RPC("SetPathOnNetwork", RpcTarget.All, pathToSetIndex);
        }
    }


    public Vector3 CurrentWaypointPosition
    {
        get { return _path?.GetNewDestination(_currentWaypoint) ?? transform.position; }
    }


    public Vector3? NearbyFood
    {
        get
        {
            foreach (Vector3 foodPosition in _foodPositions)
            {
                if (Vector3.Distance(transform.position, foodPosition) < 0.5f)
                    return foodPosition;
            }

            return null;
        }
    }


    public bool TraversedWholePath
    {
        get
        {
            return _isFollowingPath &&
                   !_path.isLooped &&
                   _currentWaypoint >= _path.WaypointCount;
        }
    }


    public bool Escaped
    {
        get
        {
            return _isFollowingPath &&
                   _isCarryingFoodPiece &&
                   (_currentWaypoint < 0 ||
                   _currentWaypoint > _path.WaypointCount);

        }
    }



    protected virtual void Start()
    {
        _agent = gameObject.GetComponent<NavMeshAgent>();
        _audioSource = GetComponent<AudioSource>();
        _animator = GetComponent<Animator>();

        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.spatialBlend = 1f;
        }

        _isIdle = _path == null;
        _isFollowingPath = _path != null;

        if (_isFollowingPath)
        {
            _path.AddEnemy();
            _currentWaypoint = _path.GetClosestWaypointIndex(transform.position);
        }

        _originalScale = transform.localScale;

        Transform foodParent = GameObject.Find("Food").transform;
        foreach (Transform food in foodParent)
            _foodPositions.Add(food.position);

        EnemyManager.Instance.AddAlive(type);
    }


    protected virtual void Update()
    {
        if (_isDead)
            return;

        if (_agent.enabled && !_isStunned)
            Crawl();

        ExtraRotationUpdate();
        UpdateAnimation();

        if (Escaped)
            Escape();
    }


    void Crawl()
    {
        if (_isFollowingPath && !_isIdle)
        {
            bool updated = UpdateCurrentWaypoint();
            if (updated)
                CrawlToDestination(CurrentWaypointPosition);
        }

        if (!_isCarryingFoodPiece && NearbyFood != null)
            CrawlToDestination((Vector3)NearbyFood);

        if (!_isFollowingPath && _isIdle)
            StartCoroutine(IdleCrawl());
    }


    protected virtual bool UpdateCurrentWaypoint()
    {
        float distance = _agent.enabled ?
                            _agent.remainingDistance :
                            Vector3.Distance(transform.position, CurrentWaypointPosition);

        if (distance <= 0.1)
        {
            _currentWaypoint += _crawlDirection;
            return true;
        }

        return false;
    }


    void ChangeCrawlDirection()
    {
        if (TraversedWholePath)
            // set it the last + 1 waypoint as it will be deducted later
            _currentWaypoint = _path.WaypointCount;

        _crawlDirection *= -1;
    }


    void CrawlToDestination(Vector3 destination)
    {
        _agent.SetDestination(destination);
    }


    IEnumerator IdleCrawl()
    {
        _isIdle = false;

        Vector3 newDestination = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f));
        _agent.SetDestination(newDestination);

        yield return new WaitForSeconds(Random.Range(4.5f, 6.5f));

        _isIdle = true;
    }


    void ExtraRotationUpdate()
    {
        Vector3 lookRotation;

        if (_agent.enabled)
            lookRotation = _agent.steeringTarget - transform.position;
        else
        {
            Vector3 cwp = _path.GetNewDestination(_currentWaypoint);
            lookRotation = cwp - transform.position;
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookRotation), 5.0f * Time.deltaTime);
    }


    protected virtual void UpdateAnimation()
    {
        if (_animator == null)
            return;

        if (_agent != null && _agent.velocity.magnitude > 0.0f)
            _animator.SetBool("Crawling", true);
        else
            _animator.SetBool("Crawling", false);
    }


    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag != "Weapon" &&
            collision.collider.tag != "FoodPiece")
            return;

        Collider collider = collision.collider;

        if (collider.CompareTag("Weapon") && !_resolvingDamage)
        {
            Rigidbody colliderRb = collider.GetComponentInParent<Rigidbody>();
            if (colliderRb.velocity.magnitude > 0.75f)
            {
                StartCoroutine(ResolveDamage());
                if (_isStunned)
                    photonView.RPC("StunOnNetwork", RpcTarget.All);
            }
        }

        if (collider.CompareTag("FoodPiece") && !_isCarryingFoodPiece)
        {
            PickupFoodPiece(collider.gameObject);
            if (_path != null && !_path.isLooped)
                ChangeCrawlDirection();
        }
    }


    IEnumerator ResolveDamage()
    {
        _resolvingDamage = true;

        // Simulates possibility of the hit missing.
        // The bigger killability of the insect the more likely it is
        // that the left part of the statement will be  smaller than a random 
        // value from range 0-100, thus actaully inflicting damage. 
        // Opposite case also applies.
        bool isDamaged = 100f - _killability < Random.Range(0f, 100f);

        if (isDamaged)
        {
            _health -= 1;

            if (_health <= 0)
            {
                Die();
                yield break;
            }
            else if (!_isStunned)
                _isStunned = true;
        }

        yield return new WaitForSeconds(1f);
        _resolvingDamage = false;
    }


    protected virtual IEnumerator Stun()
    {
        Squash(1.1f, 0.65f, 1.2f);
        photonView.RPC("DamageEffectOnNetwork", RpcTarget.All, _stunDuration);
        yield return new WaitForSeconds(_stunDuration);
        Unsquash();
        _isStunned = false;
    }


    void Squash(float scaleX, float scaleY, float scaleZ)
    {
        _agent.enabled = false;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        transform.localScale = Vector3.Scale(transform.localScale, new Vector3(scaleX, scaleY, scaleZ));
        PlayRandomClip(_damageClips);
    }


    void Unsquash()
    {
        transform.localScale = _originalScale;
        gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        _agent.enabled = true;
    }


    void PickupFoodPiece(GameObject foodPiece)
    {
        _isCarryingFoodPiece = true;
        _agent.speed *= 0.7f;

        Rigidbody foodPieceRb = foodPiece.GetComponent<Rigidbody>();
        foodPieceRb.constraints = RigidbodyConstraints.FreezeAll;
        foodPieceRb.isKinematic = true;
        foodPiece.transform.SetParent(transform);
        foodPiece.transform.SetAsFirstSibling();
    }


    protected float PlayRandomClip(List<AudioClip> clips)
    {
        int randomClipIndex = Random.Range(0, clips.Count);

        _audioSource.clip = clips[randomClipIndex];
        _audioSource.pitch = Time.timeScale;
        _audioSource.volume = 1f;
        _audioSource.spatialBlend = 1f;
        _audioSource.Play();

        return clips[randomClipIndex].length;
    }


    void Die()
    {
        _isDead = true;
        if (_agent.enabled)
            Squash(1.35f, 0.15f, 1.25f);

        float deatchClipLength = PlayRandomClip(_deathClips);
        EnemyManager.Instance.AddKilled(type, _points);
        photonView.RPC("DeathEffectOnNetwork", RpcTarget.All);
        photonView.RPC("DestroyOnNetwork", RpcTarget.All, deatchClipLength);
    }

    void Escape()
    {
        _isDead = true;
        int foodPoints = GetComponentInChildren<FoodPiece>().points;
        FoodManager.Instance.AddStolenFoodPiece(foodPoints);
        EnemyManager.Instance.AddEscaped(type, _points);
        photonView.RPC("EscapeEffectOnNetwork", RpcTarget.All);
        photonView.RPC("DestroyOnNetwork", RpcTarget.All, 0.1f);
    }


    protected GameObject SpawnVFX(GameObject effect, Vector3 scale)
    {
        GetComponentInChildren<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        GameObject vfx = Instantiate(effect, transform.position, Quaternion.Euler(-90f, 0f, 0f));
        vfx.transform.localScale = scale;

        return vfx;
    }


    protected IEnumerator PhotonDestroy(int viewID, float delay)
    {
        yield return new WaitForSeconds(delay);

        PhotonView view = PhotonView.Find(viewID);
        if (view == null || !view.IsMine)
            yield break;

        PhotonNetwork.Destroy(view.gameObject);
    }


    void OnDestroy()
    {
        if (_isFollowingPath)
            _path.RemoveEnemy();
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
        GameObject vfx = SpawnVFX(_deathEffect, new Vector3(0.3f, 0.3f, 0.3f));
        Destroy(vfx, 1.0f);
    }


    [PunRPC]
    void EscapeEffectOnNetwork()
    {
        GameObject vfx = SpawnVFX(_escapeEffect, new Vector3(0.3f, 0.3f, 0.3f));
        Destroy(vfx, 1.0f);
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

