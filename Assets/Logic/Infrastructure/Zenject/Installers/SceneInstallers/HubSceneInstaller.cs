using Logic.UILogic.Elements;
using Logic.UILogic.Presenters;
using Logic.UILogic.Viewers;
using Zenject;

namespace Logic.Infrastructure.Zenject.Installers.SceneInstallers
{
  public class HubSceneInstaller : MonoInstaller
  {
    public override void InstallBindings()
    {
      BindViewers();
      BindPresenters();
      BindUIElements();
    }

    private void BindViewers()
    {
      Container.Bind<IPlayerResourcesView>().To<PlayerResourcesView>().FromComponentInHierarchy().AsSingle();
      Container.Bind<ILimitTimeView>().To<LimitTimeView>().FromComponentInHierarchy().AsSingle();
    }

    private void BindPresenters()
    {
      Container.BindInterfacesAndSelfTo<PlayerResourcesPresenter>().AsSingle().NonLazy();
      Container.BindInterfacesAndSelfTo<LimitTimePresenter>().AsSingle().NonLazy();
    }

    private void BindUIElements()
    {
      Container.BindInterfacesAndSelfTo<OpenShopWindowButton>().FromComponentInHierarchy().AsSingle();
    }
  }
}