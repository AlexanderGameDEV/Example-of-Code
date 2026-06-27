using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Logic.Core.BaseWindow;
using Logic.Services.AssetProvider;
using UnityEngine;
using Zenject;

namespace Logic.Services.WindowManager
{
  public class WindowManager : IWindowManager
  {
    private readonly IAddressableAssetProvider _addressableAssetProvider;
    private readonly DiContainer _container;

    private readonly Dictionary<Type, IWindow> _openedWindows = new();

    public WindowManager(
      DiContainer container,
      IAddressableAssetProvider addressableAssetProvider)
    {
      _container = container;
      _addressableAssetProvider = addressableAssetProvider;
    }

    public async UniTask OpenWindow<T>() where T : class, IWindow
    {
      Type windowType = typeof(T);

      if (_openedWindows.ContainsKey(windowType))
        return;

      GameObject windowInstance = await CreateWindowAsync(windowType);

      if (windowInstance == null)
      {
        Debug.LogError($"[WindowManager]:  Failed to instantiate window <{windowType.Name}>");
        return;
      }

      ResolveDependencies(windowInstance);

      IWindow window = windowInstance.GetComponent<IWindow>();

      if (window == null)
      {
        Debug.LogError($"[WindowManager]: Component <{windowType.Name}> not found on instantiated GameObject <{windowInstance.name}>");
        _addressableAssetProvider.ReleaseInstance(windowInstance);
        return;
      }

      _openedWindows[windowType] = window;
    }

    public void CloseWindow<T>() where T : class, IWindow
    {
      Type windowType = typeof(T);

      if (!_openedWindows.TryGetValue(windowType, out IWindow window))
        return;

      if (window is Component component)
        _addressableAssetProvider.ReleaseInstance(component.gameObject);

      _openedWindows.Remove(windowType);
    }

    private async UniTask<GameObject> CreateWindowAsync(Type windowType) =>
      await _addressableAssetProvider.InstantiateAsync(windowType.Name);

    private void ResolveDependencies(GameObject windowInstance) =>
      _container.InjectGameObject(windowInstance);
  }
}