using System;
using System.Collections.Generic;
using Logic.Core.StaticData;
using Logic.Events.Base;
using Logic.Services.WindowManager;
using Logic.UILogic.OfferFactory;
using Logic.UILogic.Viewers;
using Zenject;

namespace Logic.UILogic.Presenters
{
  public class ShopWindowPresenter : IShopWindowPresenter, IInitializable, IDisposable
  {
    private readonly IOfferFactory _offerFactory;
    private readonly IShopWindowView _shopWindowView;
    private readonly IEvent _tripleOfferEvent;
    private readonly IWindowManager _windowManager;
    private bool _isEventStarted;

    private List<Offer> _offers = new();

    public ShopWindowPresenter(
      IShopWindowView shopWindowView,
      IEvent tripleOfferEvent,
      IOfferFactory offerFactory,
      IWindowManager windowManager)
    {
      _shopWindowView = shopWindowView;
      _tripleOfferEvent = tripleOfferEvent;
      _offerFactory = offerFactory;
      _windowManager = windowManager;
    }

    public void Initialize()
    {
      GetOffers();
      SpawnOffers();
      SubscribeOnCloseButtonClicked();
      _tripleOfferEvent.OnAllOffersPurchased += CloseWindow;
    }

    public void Dispose()
    {
      _tripleOfferEvent.OnAllOffersPurchased -= CloseWindow;
    }

    private void GetOffers()
    {
      _offers = _tripleOfferEvent.GetOffers();
      if (_offers == null || _offers.Count == 0)
      {
        _isEventStarted = false;
        return;
      }

      _isEventStarted = true;
    }

    private void SpawnOffers()
    {
      if (!_isEventStarted)
        return;

      for (int i = 0; i < _offers.Count; i++) _offerFactory.CreateOfferView(_shopWindowView.GetOfferParent(i));
    }

    private void SubscribeOnCloseButtonClicked()
    {
      _shopWindowView.OnCloseButtonClicked += CloseWindow;
    }

    private void CloseWindow()
    {
      _windowManager.CloseWindow<ShopWindowView>();
    }
  }
}