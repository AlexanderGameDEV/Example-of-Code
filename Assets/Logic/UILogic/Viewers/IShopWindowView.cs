using Logic.Core.BaseWindow;
using UnityEngine;

namespace Logic.UILogic.Viewers
{
  public interface IShopWindowView : IWindow
  {
    Transform GetOfferParent(int index);
  }
}