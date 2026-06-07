using FixRush;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Store : MonoBehaviour
{
    [Header("Store")]
    [SerializeField] private PlayerSettings _playerSettings;
    [SerializeField] private GameContent _gameContent;

    [SerializeField] private TMP_Text _playerMoney;
    [SerializeField] private GameObject _contentContainer;

    [SerializeField] private StoreItem _storeItemPrefab;

    private List<Cosmetic> _cosmetics = new List<Cosmetic>();
    private List<StoreItem> _populated = new List<StoreItem>();

    private void Awake()
    {
        GetCosmetics();
    }

    private void OnEnable()
    {
        _playerMoney.text = _playerSettings.Money.ToString();
        PopulateItems();
    }

    private void OnDisable()
    {
        DestroyItems();
    }

    private void GetCosmetics()
    {
        _cosmetics.Clear();

        foreach (Cosmetic c in _gameContent.Bodies) { _cosmetics.Add(c); }
        foreach (Cosmetic c in _gameContent.Hats) { _cosmetics.Add(c); }
    }

    private void PopulateItems()
    {
        foreach (Cosmetic cosmetic in _cosmetics)
        {
            StoreItem item = Instantiate(
                _storeItemPrefab,
                _contentContainer.transform
            ).Set(cosmetic);

            item.Store = this;

            _populated.Add(item);
        }
    }

    private void DestroyItems()
    {
        foreach (StoreItem item in _populated)
        {
            Destroy(item.gameObject);
        }

        _populated.Clear();
    }

    public void PurchaseItem(StoreItem item)
    {
        foreach (Cosmetic c in _cosmetics)
        {
            if (c.Name != item.Name) { continue; }
            if (_playerSettings.Money < item.Price)
            {
                Debug.LogWarning("<color=red> NO TIENES SUFICIENTE DINERO CHAVAL!! </color>");

                // Audio
                AudioManager.Instance.PlaySoundByName("Error");

                return;
            }

            _playerSettings.Money -= item.Price;
            c.Unlocked = true;
        }

        // Update UI
        _playerMoney.text = _playerSettings.Money.ToString();
        item.ShowPurchasedPanel();

        // Audio
        AudioManager.Instance.PlaySoundByName("BuySuccess");
    }
}
