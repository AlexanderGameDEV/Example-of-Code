using Logic.Services.SaveLoad;
using Logic.UILogic.Viewers;
using Zenject;

namespace Logic.UILogic.Presenters
{
  public class PlayerResourcesPresenter : IPlayerResourcesPresenter, IInitializable
  {
    private readonly IPlayerResourcesView _playerResourcesView;
    private readonly ISaveLoadService _saveLoadService;

    public PlayerResourcesPresenter(ISaveLoadService saveLoadService, IPlayerResourcesView playerResourcesView)
    {
      _saveLoadService = saveLoadService;
      _playerResourcesView = playerResourcesView;
    }

    public void Initialize()
    {
      _saveLoadService.OnProgressLoaded += OnProgressLoaded;

      if (_saveLoadService.IsProgressLoaded)
        OnProgressLoaded();
    }

    public void UpdateUI()
    {
      _playerResourcesView.UpdateText(_saveLoadService.PlayerProgress.PlayerResources.Resources);
    }

    private void OnProgressLoaded()
    {
      _saveLoadService.OnProgressLoaded -= OnProgressLoaded;
      UpdateUI();
      _saveLoadService.PlayerProgress.PlayerResources.OnPlayerResourcesDataChanged += UpdateUI;
    }
  }
}