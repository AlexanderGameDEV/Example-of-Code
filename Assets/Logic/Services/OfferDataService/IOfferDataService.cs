using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Logic.Core.StaticData;

namespace Logic.Services.OfferDataService
{
  public interface IOfferDataService
  {
    UniTask<List<Offer>> GetOffers();

    TripleOfferConfig GetEventConfig();
  }
}