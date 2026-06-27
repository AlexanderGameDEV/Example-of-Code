using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Logic.Services.SceneLoader
{
  public class SceneLoader : ISceneLoader
  {
    public async UniTask LoadSceneAsync(string sceneName)
    {
      if (SceneManager.GetActiveScene().name == sceneName) return;

      AsyncOperation waitNextScene = SceneManager.LoadSceneAsync(sceneName);
      while (!waitNextScene.isDone)
        await UniTask.Yield();
    }
  }
}