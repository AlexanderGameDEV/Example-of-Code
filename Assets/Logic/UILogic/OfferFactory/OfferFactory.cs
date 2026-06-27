using Cysharp.Threading.Tasks;
using Logic.Services.AssetProvider;
using Logic.UILogic.Viewers;
using UnityEngine;
using Zenject;

namespace Logic.UILogic.OfferFactory
{
  public class OfferFactory : IOfferFactory
  {
    private readonly IAddressableAssetProvider _addressableAssetProvider;
    private readonly DiContainer _container;

    public OfferFactory(
      IAddressableAssetProvider addressableAssetProvider,
      DiContainer container)
    {
      _addressableAssetProvider = addressableAssetProvider;
      _container = container;
    }

    public async UniTask<OfferView> CreateOfferView(Transform parent)
    {
      GameObject offerViewPrefab = await _addressableAssetProvider.LoadAssetAsync<GameObject>(AssetAddresses.OFFER_VIEW);

      GameObject offerView = _container.InstantiatePrefab(offerViewPrefab, parent);

      return offerView.GetComponent<OfferView>();
    }
  }
}