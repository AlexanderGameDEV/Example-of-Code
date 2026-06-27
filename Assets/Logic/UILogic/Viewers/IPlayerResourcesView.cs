using System.Collections.Generic;
using Logic.Core.Enums;

namespace Logic.UILogic.Viewers
{
  public interface IPlayerResourcesView
  {
    void UpdateText(IReadOnlyDictionary<ResourceType, int> resources);
  }
}