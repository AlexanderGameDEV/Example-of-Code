using System;
using Logic.Core.Enums;

namespace Logic.Core.StaticData
{
  [Serializable]
  public class OfferConfig
  {
    public int OfferId;
    public int Amount;
    public string Price;
    public string ImageAddress;
    public ResourceType ResourceType;
  }
}