using Logic.Events.Base;
using Logic.Services.WindowManager;

namespace Logic.UILogic.Elements
{
  public interface IOpenWindowButton
  {
    void Construct(IWindowManager windowManage, IEvent tripleOfferEvent);

    void OpenShopWindow();
  }
}