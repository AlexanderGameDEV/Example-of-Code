using System;
using System.Collections.Generic;

namespace Logic.Core.StaticData
{
  [Serializable]
  public class TripleOfferConfig : IEventConfig
  {
    public string EventId;
    public List<OfferConfig> OffersConfigs;
    public DateTime EndTime;
    public DateTime StartTime;
  }
}