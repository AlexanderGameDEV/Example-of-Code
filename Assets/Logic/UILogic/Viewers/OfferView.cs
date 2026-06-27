using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Logic.UILogic.Viewers
{
  public class OfferView : MonoBehaviour, IOfferView
  {
    [SerializeField] private Button _buyButton;
    [SerializeField] private TextMeshProUGUI _offerAmountText;
    [SerializeField] private TextMeshProUGUI _priceText;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _offerImage;
    [SerializeField] private TextMeshProUGUI _soldOutText;

    private readonly Color32 _soldOutColor = new(95, 95, 95, 255);

    public event Action OnBuyButtonClicked;

    private void Awake()
    {
      _buyButton.onClick.AddListener(() => OnBuyButtonClicked?.Invoke());
    }

    private void OnDestroy()
    {
      _buyButton.onClick.RemoveAllListeners();
    }

    public void SetPrice(string price)
    {
      _priceText.text = price;
    }

    public void SetAmount(int amount)
    {
      _offerAmountText.text = amount.ToString();
    }

    public void SetOfferImage(Sprite offerSprite)
    {
      _offerImage.sprite = offerSprite;
    }

    public void SetBuyButtonInteractable(bool interactable)
    {
      _buyButton.interactable = interactable;
    }

    public void SetSoldOut()
    {
      SetSoldOutColor();
      _buyButton.interactable = false;
      _soldOutText.enabled = true;
    }

    private void SetSoldOutColor()
    {
      _backgroundImage.color = _soldOutColor;
      _offerImage.color = _soldOutColor;
      _priceText.color = _soldOutColor;
    }
  }
}