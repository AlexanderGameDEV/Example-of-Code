using System;
using System.Collections.Generic;
using Logic.Core.Enums;

namespace Logic.Services.SaveLoad
{
  [Serializable]
  public class PlayerResources
  {
    public Dictionary<ResourceType, int> Resources = new();

    public event Action OnPlayerResourcesDataChanged;

    public void AddResource(ResourceType type, int amount)
    {
      if (!Resources.ContainsKey(type))
        Resources[type] = 0;

      Resources[type] += amount;

      OnPlayerResourcesDataChanged?.Invoke();
    }
  }
}