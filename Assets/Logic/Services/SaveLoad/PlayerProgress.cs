using System;
using Newtonsoft.Json;

namespace Logic.Services.SaveLoad
{
  [Serializable]
  public class PlayerProgress
  {
    public PurchaseData PurchaseData;
    public PlayerResources PlayerResources;

    [JsonConstructor]
    public PlayerProgress()
    {
      PurchaseData = new PurchaseData();
      PlayerResources = new PlayerResources();
    }
  }
}