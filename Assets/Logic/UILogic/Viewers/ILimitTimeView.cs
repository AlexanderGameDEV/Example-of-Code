using System;

namespace Logic.UILogic.Viewers
{
  public interface ILimitTimeView
  {
    void SetTime(TimeSpan time);
    void SetTimeOutText();
    void SetOpenSoonText();
  }
}