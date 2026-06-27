using Logic.Infrastructure.Bootstrap;
using Zenject;

namespace Logic.Infrastructure.Zenject.Installers.SceneInstallers
{
  public class BootstrapSceneInstaller : MonoInstaller
  {
    public override void InstallBindings()
    {
      BindGameBootstrapper();
    }

    private void BindGameBootstrapper()
    {
      Container.BindInterfacesAndSelfTo<GameBootstrapper>().AsSingle().NonLazy();
    }
  }
}