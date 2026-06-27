using System;
using Cysharp.Threading.Tasks;
using Logic.Core.StaticData;
using Logic.Events.Base;
using Logic.Services.IAPService;
using Logic.Services.SaveLoad;
using Logic.UILogic.Viewers;
using Zenject;

namespace Logic.UILogic.Presenters
{
  public class OfferPresenter : IOfferPresenter, IInitializable, IDisposable
  {
    private readonly IIAPService _iapService;
    private readonly IOfferView _offerView;
    private readonly ISaveLoadService _saveLoadService;
    private readonly IEvent _tripleOfferEvent;

    private Offer _offer;
    private bool _isSubscribed;

    public OfferPresenter(
      IEvent tripleOfferEvent,
      IIAPService iapService,
      IOfferView offerView,
      ISaveLoadService saveLoadService)
    {
      _tripleOfferEvent = tripleOfferEvent;
      _iapService = iapService;
      _offerView = offerView;
      _saveLoadService = saveLoadService;
    }

    public void Initialize()
    {
      SubscribeOnEventEnded();

      if (IsOfferConfigGotten())
      {
        SubscribeOnPurchaseDataChanged();

        UpdateUIBasedOnPurchaseStatus();
      }
    }

    public void Dispose()
    {
      _tripleOfferEvent.OnEventEnded -= EndTripleOfferEvent;
      _offerView.OnBuyButtonClicked -= OnBuyButtonClicked;

      UnsubscribeFromPurchaseDataChanged();
    }

    public void RefreshUI()
    {
      _offerView.SetPrice(_offer.Price);
      _offerView.SetAmount(_offer.Amount);
      _offerView.SetOfferImage(_offer.Image);
    }

    public void OnBuyButtonClicked() => BuyOffer().Forget();

    private void SubscribeOnEventEnded() => _tripleOfferEvent.OnEventEnded += EndTripleOfferEvent;

    private void EndTripleOfferEvent()
    {
      SetSoldOut();
      UnsubscribeFromPurchaseDataChanged();
    }

    private void SetSoldOut()
    {
      _offerView.SetSoldOut();
      _offerView.OnBuyButtonClicked -= OnBuyButtonClicked;
    }

    private void UnsubscribeFromPurchaseDataChanged()
    {
      if (!_isSubscribed || _saveLoadService?.PlayerProgress == null)
        return;

      _saveLoadService.PlayerProgress.PurchaseData.OnPurchaseDataChanged -= OfferUnavailable;
      _isSubscribed = false;
    }

    private bool IsOfferConfigGotten()
    {
      _offer = _tripleOfferEvent.GetOffer();
      if (_offer == null)
        return false;

      return true;
    }

    private void SubscribeOnPurchaseDataChanged()
    {
      if (_isSubscribed || _saveLoadService?.PlayerProgress == null)
        return;

      _saveLoadService.PlayerProgress.PurchaseData.OnPurchaseDataChanged += OfferUnavailable;
      _isSubscribed = true;
    }

    private void UpdateUIBasedOnPurchaseStatus()
    {
      if (IsPurchased())
      {
        SetSoldOut();
      }
      else
      {
        RefreshUI();
        SubscribeOnBuyButtonClicked();
      }
    }

    private void OfferUnavailable()
    {
      if (_offer == null)
        return;

      if (IsPurchased())
      {
        SetSoldOut();
        UnsubscribeFromPurchaseDataChanged();
      }
    }

    private bool IsPurchased() => _saveLoadService.PlayerProgress.PurchaseData.IsPurchased(_offer.Id);

    private void SubscribeOnBuyButtonClicked()
    {
      _offerView.OnBuyButtonClicked += OnBuyButtonClicked;
    }

    private async UniTask BuyOffer()
    {
      _offerView.SetBuyButtonInteractable(false);

      bool isPurchased = await _iapService.MakePurchase(_offer);

      if (!isPurchased)
      {
        _offerView.SetBuyButtonInteractable(true);
      }
      else
      {
        UnsubscribeFromPurchaseDataChanged();
        SetSoldOut();
      }
    }
  }
}