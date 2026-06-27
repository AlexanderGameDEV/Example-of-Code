using Cysharp.Threading.Tasks;
using Logic.Services.AssetProvider;
using Logic.Services.SceneLoader;
using Logic.UILogic.Elements.Logic.UILogic.Elements;
using UnityEngine;

namespace Logic.Infrastructure.GameStateMachine.States
{
  public class LoadLevelState : IState
  {
    private const string SCENE_NAME = "HubScene";
    private readonly IAddressableAssetProvider _addressableAssetProvider;

    private readonly ISceneLoader _sceneLoader;
    private readonly IStateSwitcher _stateSwitcher;
    private LoadingCurtain _loadingCurtain;

    public LoadLevelState(
      ISceneLoader sceneLoader,
      IStateSwitcher stateSwitcher,
      IAddressableAssetProvider addressableAssetProvider)
    {
      _sceneLoader = sceneLoader;
      _stateSwitcher = stateSwitcher;
      _addressableAssetProvider = addressableAssetProvider;
    }

    public async UniTask Enter()
    {
      LoadingCurtain loadingCurtain = await GetLoadingCurtain();
      loadingCurtain.Show();

      await _sceneLoader.LoadSceneAsync(SCENE_NAME);
      await loadingCurtain.Hide();
      ReleaseLoadingCurtain();

      await _stateSwitcher.SwitchState<HubState>();
    }

    private async UniTask<LoadingCurtain> GetLoadingCurtain()
    {
      GameObject curtainGameObject = await _addressableAssetProvider.InstantiateAsync(AssetAddresses.LOADING_CURTAIN);
      return curtainGameObject.GetComponent<LoadingCurtain>();
    }

    private void ReleaseLoadingCurtain()
    {
      if (_loadingCurtain == null)
        return;

      _addressableAssetProvider.ReleaseInstance(_loadingCurtain.gameObject);
      _loadingCurtain = null;
    }
  }
}