using System;
using UnityEngine;

namespace Logic.UILogic.Viewers
{
  public interface IOfferView
  {
    event Action OnBuyButtonClicked;

    void SetPrice(string price);

    void SetAmount(int amount);

    void SetOfferImage(Sprite offerSprite);

    void SetSoldOut();

    void SetBuyButtonInteractable(bool interactable);
  }
}