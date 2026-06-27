using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Logic.UILogic.Elements
{
  namespace Logic.UILogic.Elements
  {
    public class LoadingCurtain : MonoBehaviour
    {
      [SerializeField] private CanvasGroup _curtain;
      [SerializeField] private float _durationSec = 1.75f;
      [SerializeField] private float _endAlpha;

      private void Awake()
      {
        DontDestroyOnLoad(gameObject);
      }

      public void Show()
      {
        gameObject.SetActive(true);
        _curtain.alpha = 1;
      }

      public async UniTask Hide() => await FadeOutAsync();

      private async UniTask FadeOutAsync()
      {
        float currentAlpha = 1f;

        while (currentAlpha >= _endAlpha)
        {
          currentAlpha -= Time.deltaTime / _durationSec;
          _curtain.alpha = currentAlpha;

          await UniTask.Yield(PlayerLoopTiming.Update);
        }
      }
    }
  }
}