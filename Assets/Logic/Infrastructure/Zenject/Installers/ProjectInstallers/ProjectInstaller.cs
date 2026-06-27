using Logic.Events.TripleOfferEvent;
using Logic.Infrastructure.GameStateMachine.States;
using Logic.Services.AssetProvider;
using Logic.Services.IAPService;
using Logic.Services.OfferDataService;
using Logic.Services.SaveLoad;
using Logic.Services.SceneLoader;
using Logic.Services.WindowManager;
using Zenject;

namespace Logic.Infrastructure.Zenject.Installers.ProjectInstallers
{
  public class ProjectInstaller : MonoInstaller
  {
    public override void InstallBindings()
    {
      BindGameStateMachine();
      BindStates();
      BindServices();
    }

    private void BindGameStateMachine()
    {
      Container.BindInterfacesAndSelfTo<Infrastructure.GameStateMachine.GameStateMachine>().AsSingle();
    }

    private void BindStates()
    {
      Container.Bind<LoadProgressState>().AsSingle();
      Container.Bind<LoadLevelState>().AsSingle();
      Container.Bind<HubState>().AsSingle();
    }

    private void BindServices()
    {
      Container.Bind<IAddressableAssetProvider>().To<AddressableAssetProvider>().AsSingle();

      Container.Bind<ISceneLoader>().To<SceneLoader>().AsSingle();
      Container.Bind<ISaveLoadService>().To<SaveLoadService>().AsSingle();
      Container.Bind<IWindowManager>().To<WindowManager>().AsSingle();
      Container.Bind<IOfferDataService>().To<OfferDataService>().AsSingle();

      Container.BindInterfacesAndSelfTo<TripleOfferEvent>().AsSingle();
      Container.BindInterfacesAndSelfTo<TripleOfferEventTimer>().AsSingle();

      Container.Bind<IIAPService>().To<IAPService>().AsSingle();
    }
  }
}