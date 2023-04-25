using UnityEngine;
using System.Linq;
using Photon.Pun;

public class PropManager : MonoBehaviourPun
{
    [SerializeField] private int _propsDestroyed;
    [SerializeField] private int _originalPropsValue;
    [SerializeField] private int _currentPropsValue;

    public int PropsDestroyed { get { return _propsDestroyed; } }
    public int OriginalPropsValue { get { return _originalPropsValue; } }
    public int CurrentPropsValue { get { return _currentPropsValue; } }
    public int DestroyedPropsValue { get { return _originalPropsValue - _currentPropsValue; } }

    public static PropManager Instance;

    void Awake()
    {
        Instance = this;
        _currentPropsValue = _originalPropsValue = FindObjectsOfType<Prop>()
                                                    .Select(prop => prop.points)
                                                    .ToArray().Sum();
    }


    public void AddDestroyedProp(int points)
    {
        photonView.RPC("AddDestroyedPropRPC", RpcTarget.All, points);
    }


    [PunRPC]
    private void AddDestroyedPropRPC(int points)
    {
        _propsDestroyed++;
        _currentPropsValue -= points;
    }
}
