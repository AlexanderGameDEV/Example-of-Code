using System.Collections.Generic;
using Logic.Core.Enums;
using TMPro;
using UnityEngine;

namespace Logic.UILogic.Viewers
{
  public class PlayerResourcesView : MonoBehaviour, IPlayerResourcesView
  {
    [SerializeField] private TextMeshProUGUI _gems;
    [SerializeField] private TextMeshProUGUI _coins;
    [SerializeField] private TextMeshProUGUI _iron;
    [SerializeField] private TextMeshProUGUI _wood;
    [SerializeField] private TextMeshProUGUI _meat;

    public void UpdateText(IReadOnlyDictionary<ResourceType, int> resources)
    {
      _gems.text = resources.TryGetValue(ResourceType.Gems, out int gems) ? gems.ToString() : "0";
      _coins.text = resources.TryGetValue(ResourceType.Coins, out int coins) ? coins.ToString() : "0";
      _iron.text = resources.TryGetValue(ResourceType.Iron, out int iron) ? iron.ToString() : "0";
      _wood.text = resources.TryGetValue(ResourceType.Wood, out int wood) ? wood.ToString() : "0";
      _meat.text = resources.TryGetValue(ResourceType.Meat, out int meat) ? meat.ToString() : "0";
    }
  }
}