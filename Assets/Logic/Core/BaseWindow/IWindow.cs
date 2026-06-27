using System;

namespace Logic.Core.BaseWindow
{
  public interface IWindow
  {
    event Action OnCloseButtonClicked;
  }
}