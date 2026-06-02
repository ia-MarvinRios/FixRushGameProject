using TMPro;
using UnityEngine;

public class GameOverPanel : MonoBehaviour
{
    [Header("Game Over Panel")]
    [Space(10)]
    [SerializeField] private TMP_Text _levelMoney;
    [SerializeField] private TMP_Text _repairedCars;
    [SerializeField] private TMP_Text _lostCars;

    private void OnEnable()
    {
        _levelMoney.text = GameManager.Instance.GetCurrentCash().ToString();
        _repairedCars.text = GameManager.Instance.RepairedCars.ToString();
        _lostCars.text = GameManager.Instance.LostCars.ToString();
    }
}
