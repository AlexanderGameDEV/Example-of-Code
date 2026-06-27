using Cysharp.Threading.Tasks;
using Logic.Core.BaseWindow;

namespace Logic.Services.WindowManager
{
  public interface IWindowManager
  {
    UniTask OpenWindow<T>() where T : class, IWindow;

    void CloseWindow<T>() where T : class, IWindow;
  }
}