using System;
using TMPro;
using UnityEngine;

namespace Logic.UILogic.Viewers
{
  public class LimitTimeView : MonoBehaviour, ILimitTimeView
  {
    private const string TIME_OUT_TEXT = "0h 0m!";
    private const string OPEN_SOON_TEXT = "Open soon...";

    private readonly Color _redColor = Color.red;

    [SerializeField] private TextMeshProUGUI _timeText;

    public void SetTime(TimeSpan time)
    {
      _timeText.text = $"{time.Hours}h {time.Minutes}m";
    }

    public void SetTimeOutText()
    {
      _timeText.text = TIME_OUT_TEXT;
      _timeText.color = _redColor;
    }

    public void SetOpenSoonText()
    {
      _timeText.text = OPEN_SOON_TEXT;
    }
  }
}