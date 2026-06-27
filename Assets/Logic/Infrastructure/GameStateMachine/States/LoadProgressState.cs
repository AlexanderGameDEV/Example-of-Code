using Cysharp.Threading.Tasks;
using Logic.Events.Base;
using Logic.Services.AssetProvider;
using Logic.Services.SaveLoad;
using UnityEngine;

namespace Logic.Infrastructure.GameStateMachine.States
{
  public class LoadProgressState : IState
  {
    private readonly IAddressableAssetProvider _addressableAssetProvider;
    private readonly ISaveLoadService _saveLoadService;
    private readonly IStateSwitcher _stateSwitcher;
    private readonly IEvent _tripleOfferEvent;

    public LoadProgressState(
      ISaveLoadService saveLoadService,
      IStateSwitcher stateSwitcher,
      IAddressableAssetProvider addressableAssetProvider,
      IEvent tripleOfferEvent)
    {
      _saveLoadService = saveLoadService;
      _stateSwitcher = stateSwitcher;
      _addressableAssetProvider = addressableAssetProvider;
      _tripleOfferEvent = tripleOfferEvent;
    }

    public async UniTask Enter()
    {
      await _saveLoadService.LoadProgress();

      await PreloadAssets();
      await _tripleOfferEvent.PreloadOffers();

      await _stateSwitcher.SwitchState<LoadLevelState>();
    }

    private async UniTask PreloadAssets()
    {
      await _addressableAssetProvider.Initialize();

      await _addressableAssetProvider.LoadAssetAsync<GameObject>(AssetAddresses.LOADING_CURTAIN);
      await _addressableAssetProvider.LoadAssetAsync<GameObject>(AssetAddresses.OFFER_VIEW);
      await _addressableAssetProvider.LoadAssetAsync<GameObject>(AssetAddresses.SHOP_WINDOW);
    }
  }
}