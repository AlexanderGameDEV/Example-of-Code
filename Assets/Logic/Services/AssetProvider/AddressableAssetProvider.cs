using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Logic.Services.AssetProvider
{
  public class AddressableAssetProvider : IAddressableAssetProvider
  {
    private readonly Dictionary<string, AsyncOperationHandle> _cachedHandles = new();

    public async UniTask Initialize()
    {
      await Addressables.InitializeAsync().ToUniTask();
    }

    public async UniTask<T> LoadAssetAsync<T>(string address) where T : class
    {
      if (_cachedHandles.TryGetValue(address, out AsyncOperationHandle cachedHandle))
        return cachedHandle.Result as T;

      AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);

      await handle.ToUniTask();

      if (handle.Status != AsyncOperationStatus.Succeeded)
      {
        Debug.LogError($"[AddressableAssetProvider]: Failed to load asset at address '{address}'. " +
                       $"Status: {handle.Status}. Exception: {handle.OperationException}");
        Addressables.Release(handle);
        return null;
      }

      _cachedHandles[address] = handle;

      return handle.Result;
    }

    public async UniTask<GameObject> InstantiateAsync(string address)
    {
      AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(address);

      await handle.ToUniTask();

      return handle.Result;
    }

    public async UniTask<GameObject> InstantiateAsync(string address, Transform parent)
    {
      AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(address, parent);

      await handle.ToUniTask();

      return handle.Result;
    }

    public void Release(string address)
    {
      if (!_cachedHandles.TryGetValue(address, out AsyncOperationHandle handle))
        return;

      Addressables.Release(handle);

      _cachedHandles.Remove(address);
    }

    public void ReleaseInstance(GameObject instance)
    {
      if (instance == null)
      {
        Debug.LogWarning("[AddressableAssetProvider]: ReleaseInstance called with null instance.");
        return;
      }

      Addressables.ReleaseInstance(instance);
    }

    public void CleanUp()
    {
      foreach (AsyncOperationHandle handle in _cachedHandles.Values) Addressables.Release(handle);

      _cachedHandles.Clear();
    }
  }
}