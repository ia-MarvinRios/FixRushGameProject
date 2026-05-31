using FixRush;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreItem : MonoBehaviour
{
    [Header("Store Item")]
    [SerializeField] private TMP_Text   _itemName;
    [SerializeField] private TMP_Text   _itemPrice;
    [SerializeField] private Image      _itemImage;
    [SerializeField] private GameObject _purchaseButton;
    [SerializeField] private GameObject _purchasedPanel;

    internal int    Price;
    internal string Name;

    internal Store Store;

    public StoreItem Set(Cosmetic cosmetic)
    {
        Price             = cosmetic.BuyPrice;
        Name              = cosmetic.Name;
        _itemImage.sprite = cosmetic.Sprite;
        _itemName.text    = Name;
        _itemPrice.text   = Price.ToString();
        _purchaseButton.SetActive(!cosmetic.Unlocked);
        _purchasedPanel.SetActive(cosmetic.Unlocked);

        return this;
    }

    public void Purchase() { Store.PurchaseItem(this); }

    internal void ShowPurchasedPanel()
    {
        // UI
        _purchaseButton.SetActive(false);
        _purchasedPanel.SetActive(true);
    }
}
