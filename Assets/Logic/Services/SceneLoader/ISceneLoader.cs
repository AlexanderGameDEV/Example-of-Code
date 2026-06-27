using Cysharp.Threading.Tasks;

namespace Logic.Services.SceneLoader
{
  public interface ISceneLoader
  {
    UniTask LoadSceneAsync(string sceneName);
  }
}