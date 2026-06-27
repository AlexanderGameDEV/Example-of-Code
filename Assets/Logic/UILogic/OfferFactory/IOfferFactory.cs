using Cysharp.Threading.Tasks;
using Logic.UILogic.Viewers;
using UnityEngine;

namespace Logic.UILogic.OfferFactory
{
  public interface IOfferFactory
  {
    UniTask<OfferView> CreateOfferView(Transform parent);
  }
}