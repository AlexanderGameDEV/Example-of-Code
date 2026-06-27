using System;

namespace Logic.UILogic.Presenters
{
  public interface ILimitTimePresenter
  {
    void UpdateTime(TimeSpan time);

    void EndTripleOfferEvent();
  }
}