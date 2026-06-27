using System;
using UnityEngine;
using UnityEngine.UI;

namespace Logic.UILogic.Viewers
{
  public class ShopWindowView : MonoBehaviour, IShopWindowView
  {
    [SerializeField] private Transform[] _offersParentsContainers;
    [SerializeField] private Button _closeWindowButton;

    public event Action OnCloseButtonClicked;

    private void Awake()
    {
      _closeWindowButton.onClick.AddListener(OnCloseClicked);
    }

    public Transform GetOfferParent(int index)
    {
      return _offersParentsContainers[index];
    }

    private void OnCloseClicked()
    {
      OnCloseButtonClicked?.Invoke();
    }
  }
}