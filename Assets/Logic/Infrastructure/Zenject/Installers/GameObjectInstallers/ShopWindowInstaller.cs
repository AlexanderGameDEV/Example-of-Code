using Logic.UILogic.OfferFactory;
using Logic.UILogic.Presenters;
using Logic.UILogic.Viewers;
using Zenject;

namespace Logic.Infrastructure.Zenject.Installers.GameObjectInstallers
{
  public class ShopWindowInstaller : MonoInstaller
  {
    public override void InstallBindings()
    {
      BindView();
      BindFactory();
      BindPresenter();
    }

    private void BindView()
    {
      Container.Bind<IShopWindowView>().To<ShopWindowView>().FromComponentOnRoot().AsSingle();
    }

    private void BindFactory()
    {
      Container.Bind<IOfferFactory>().To<OfferFactory>().AsSingle();
    }

    private void BindPresenter()
    {
      Container.BindInterfacesAndSelfTo<ShopWindowPresenter>().AsSingle();
    }
  }
}