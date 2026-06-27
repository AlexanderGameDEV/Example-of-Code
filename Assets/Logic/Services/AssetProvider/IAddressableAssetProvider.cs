using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Logic.Services.AssetProvider
{
  public interface IAddressableAssetProvider
  {
    UniTask Initialize();

    UniTask<T> LoadAssetAsync<T>(string address) where T : class;

    UniTask<GameObject> InstantiateAsync(string address);

    UniTask<GameObject> InstantiateAsync(string address, Transform parent);

    void Release(string address);

    void ReleaseInstance(GameObject instance);

    void CleanUp();
  }
}