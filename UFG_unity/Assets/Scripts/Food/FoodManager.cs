using UnityEngine;
using System.Linq;
using Photon.Pun;


public class FoodManager : MonoBehaviourPun
{
    [SerializeField] private int _untouchedFood;
    [SerializeField] private int _stolenFoodPieces;
    [SerializeField] private int _originalFoodValue;
    [SerializeField] private int _currentFoodValue;

    public int UntouchedFood { get { return _untouchedFood; } }
    public int StolenFoodPieces { get { return _stolenFoodPieces; } }
    public int OriginalFoodValue { get { return _originalFoodValue; } }
    public int CurrentFoodValue { get { return _currentFoodValue; } }
    public int StolenFoodValue { get { return _originalFoodValue - _currentFoodValue; } }


    public static FoodManager Instance;


    void Awake()
    {
        Instance = this;
        _currentFoodValue = _originalFoodValue = FindObjectsOfType<Food>()
                                                   .Select(food => food.TotalPoints)
                                                   .ToArray().Sum();
    }


    public void AddUntouchedFood()
    {
        photonView.RPC("AddUntouchedFoodRPC", RpcTarget.All);
    }


    [PunRPC]
    private void AddUntouchedFoodRPC()
    {
        _untouchedFood++;
    }


    public void RemoveUntouchedFood()
    {
        photonView.RPC("RemoveUntouchedFoodRPC", RpcTarget.All);
    }


    [PunRPC]
    private void RemoveUntouchedFoodRPC()
    {
        _untouchedFood--;
    }


    public void AddStolenFoodPiece(int points)
    {
        photonView.RPC("AddStolenFoodPieceRPC", RpcTarget.All, points);
    }


    [PunRPC]
    private void AddStolenFoodPieceRPC(int points)
    {
        _stolenFoodPieces++;
        _currentFoodValue -= points;
    }
}
