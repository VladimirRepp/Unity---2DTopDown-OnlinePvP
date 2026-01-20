using System;
using UnityEngine;

public class Pickuper : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool isViewDebug = false;

    // Эмитция инвентаря
    private int _coins = 0;
    private int _diamonds = 0;

    public int Coins => _coins;
    public int Diamonds => _diamonds;

    public Action<int> OnCoinsChanged;
    public Action<int> OnDiamondsChanged;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Pickup"))
        {
            if (collision.gameObject.TryGetComponent<Coin>(out Coin item_coins))
            {
                _coins++;
                OnCoinsChanged?.Invoke(_coins);

                if (isViewDebug)
                    Debug.Log("Coins: " + _coins);
            }
            else if (collision.gameObject.TryGetComponent<Diamond>(out Diamond item_diamonds))
            {
                _diamonds++;
                OnDiamondsChanged?.Invoke(_diamonds);

                if (isViewDebug)
                    Debug.Log("Diamonds: " + _diamonds);
            }

            Destroy(collision.gameObject);
        }
    }
}
