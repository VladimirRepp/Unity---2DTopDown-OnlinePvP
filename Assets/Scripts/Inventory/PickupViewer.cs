using UnityEngine;
using TMPro;

public class PickupViewer : MonoBehaviour
{
    [Header("Reference Settings")]
    [SerializeField] private Pickuper pickuper;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text diamondsText;

    private void OnEnable()
    {
        pickuper.OnCoinsChanged += UpdateCoinsText;
        pickuper.OnDiamondsChanged += UpdateDiamondsText;
    }

    private void OnDisable()
    {
        pickuper.OnCoinsChanged -= UpdateCoinsText;
        pickuper.OnDiamondsChanged -= UpdateDiamondsText;
    }

    private void UpdateCoinsText(int coins)
    {
        coinsText.text = coins.ToString();
    }

    private void UpdateDiamondsText(int diamonds)
    {
        diamondsText.text = diamonds.ToString();
    }
}
