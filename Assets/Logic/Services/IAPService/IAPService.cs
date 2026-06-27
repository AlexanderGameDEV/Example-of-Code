using System;
using Cysharp.Threading.Tasks;
using Logic.Core.StaticData;
using Logic.Services.SaveLoad;
using UnityEngine;

namespace Logic.Services.IAPService
{
  public class IAPService : IIAPService
  {
    private readonly ISaveLoadService _saveLoadService;

    public event Action<int> OnOfferPurchased;

    public IAPService(ISaveLoadService saveLoadService)
    {
      _saveLoadService = saveLoadService;
    }

    public UniTask<bool> MakePurchase(Offer offer)
    {
      bool isPurchaseAdded = _saveLoadService.PlayerProgress.PurchaseData.AddPurchase(offer.Id);

      if (!isPurchaseAdded)
      {
        Debug.Log($"[IAPService]: Purchased {offer.Id} failed]");
        return UniTask.FromResult(false);
      }

      _saveLoadService.PlayerProgress.PlayerResources.AddResource(offer.ResourceType, offer.Amount);
      OnOfferPurchased?.Invoke(offer.Id);

      return UniTask.FromResult(true);
    }
  }
}