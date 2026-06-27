using Logic.UILogic.Presenters;
using Logic.UILogic.Viewers;
using Zenject;

namespace Logic.Infrastructure.Zenject.Installers.GameObjectInstallers
{
  public class OfferInstaller : MonoInstaller
  {
    public override void InstallBindings()
    {
      BindView();
      BindPresenter();
    }

    private void BindView()
    {
      Container.Bind<OfferView>().FromComponentOnRoot().AsSingle();
      Container.Bind<IOfferView>().To<OfferView>().FromResolve();
    }

    private void BindPresenter()
    {
      Container.BindInterfacesAndSelfTo<OfferPresenter>().AsSingle().NonLazy();
    }
  }
}