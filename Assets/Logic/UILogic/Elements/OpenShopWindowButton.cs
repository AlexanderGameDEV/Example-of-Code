using Logic.Events.Base;
using Logic.Services.WindowManager;
using Logic.UILogic.Viewers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Logic.UILogic.Elements
{
  public class OpenShopWindowButton : MonoBehaviour, IOpenWindowButton
  {
    [SerializeField] private Button _openWindowButton;
    private IEvent _tripleOfferEvent;
    private IWindowManager _windowManager;

    public void Awake()
    {
      DisableButton();

      _openWindowButton.onClick.AddListener(OpenShopWindow);

      SubscribeOnEventStates();

      if (_tripleOfferEvent.IsEventStarted)
        EnableButton();
    }

    private void OnDestroy()
    {
      _openWindowButton.onClick.RemoveAllListeners();
      UnsubscribeFromEventState();
    }

    [Inject]
    public void Construct(
      IWindowManager windowManager,
      IEvent tripleOfferEvent)
    {
      _windowManager = windowManager;
      _tripleOfferEvent = tripleOfferEvent;
    }

    public void OpenShopWindow()
    {
      _windowManager.OpenWindow<ShopWindowView>();
    }

    private void EnableButton() => _openWindowButton.interactable = true;

    private void DisableButton()
    {

      _openWindowButton.interactable = false;
    }

    private void SubscribeOnEventStates()
    {
      _tripleOfferEvent.OnEventStarted += EnableButton;
      _tripleOfferEvent.OnEventEnded += DisableButton;
      _tripleOfferEvent.OnAllOffersPurchased += DisableButton;
    }

    private void UnsubscribeFromEventState()
    {
      _tripleOfferEvent.OnEventStarted -= EnableButton;
      _tripleOfferEvent.OnEventEnded -= DisableButton;
      _tripleOfferEvent.OnAllOffersPurchased -= DisableButton;
    }
  }
}