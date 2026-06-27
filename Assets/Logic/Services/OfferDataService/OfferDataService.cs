using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Logic.Core.StaticData;
using Logic.Services.AssetProvider;
using Newtonsoft.Json;
using UnityEngine;

namespace Logic.Services.OfferDataService
{
  public class OfferDataService : IOfferDataService
  {
    private readonly IAddressableAssetProvider _addressableAssetProvider;

    private readonly string _configPath = Path.Combine(Application.streamingAssetsPath, "tripleOffer.json");
    private readonly List<Offer> _offers = new();
    private TripleOfferConfig _tripleOfferConfig;

    public OfferDataService(IAddressableAssetProvider addressableAssetProvider)
    {
      _addressableAssetProvider = addressableAssetProvider;
    }

    public async UniTask<List<Offer>> GetOffers()
    {
      _offers.Clear();

      _tripleOfferConfig = TryGetTripleOfferConfig();

      if (!IsConfigValid())
      {
        return _offers;
      }

      await ConvertOfferConfigsToOffers();

      return _offers;
    }

    public TripleOfferConfig GetEventConfig()
    {
      return TryGetTripleOfferConfig();
    }

    private TripleOfferConfig TryGetTripleOfferConfig()
    {
      if (!IsConfigFileFound())
        return null;

      try
      {
        if (!IsConfigFileRead(out string jsonString))
          return null;

        if (!IsConfigFileDeserialized(jsonString, out TripleOfferConfig tripleOfferConfig))
          return null;

        return tripleOfferConfig;
      }
      catch (Exception e)
      {
        Debug.LogError($"[OfferDataService]: Json parse error: {e.Message}");
        return null;
      }
    }

    private bool IsConfigFileFound()
    {
      if (!File.Exists(_configPath))
      {
        Debug.LogError($"[OfferDataService]: Config file not found. File Path: {_configPath}");
        return false;
      }

      return true;
    }

    private bool IsConfigFileRead(out string jsonString)
    {
      jsonString = File.ReadAllText(_configPath);

      if (string.IsNullOrWhiteSpace(jsonString))
      {
        Debug.LogError("[OfferDataService]: Config file is empty.");
        return false;
      }

      return true;
    }

    private static bool IsConfigFileDeserialized(string jsonString, out TripleOfferConfig tripleOfferConfig)
    {
      tripleOfferConfig = JsonConvert.DeserializeObject<TripleOfferConfig>(jsonString);

      if (tripleOfferConfig == null)
      {
        Debug.LogError("[OfferDataService]: Failed to deserialize config (null result).");
        return false;
      }

      return true;
    }

    private bool IsConfigValid()
    {
      if (_tripleOfferConfig == null)
      {
        Debug.LogError("[OfferDataService]: TripleOfferConfig == null. Returning empty offers.");
        return false;
      }

      if (_tripleOfferConfig.OffersConfigs == null)
      {
        Debug.LogError("[OfferDataService]: List OfferConfigs null. Returning empty offers.");
        return false;
      }

      if (_tripleOfferConfig.OffersConfigs.Count == 0)
      {
        Debug.LogError("[OfferDataService]: List OfferConfigs empty. Returning empty offers.");
        return false;
      }

      return true;
    }

    private async UniTask ConvertOfferConfigsToOffers()
    {
      foreach (OfferConfig offerConfig in _tripleOfferConfig.OffersConfigs)
      {
        if (!IsValidOfferConfig(offerConfig))
        {
          Debug.LogWarning($"[OfferDataService]: Invalid OfferConfig skipped. ID: {offerConfig?.OfferId}");
          continue;
        }

        Sprite sprite = await LoadSpriteAsync(offerConfig.ImageAddress);
        _offers.Add(new Offer
        {
          Id = offerConfig.OfferId,
          Price = offerConfig.Price,
          Amount = offerConfig.Amount,
          ResourceType = offerConfig.ResourceType,
          Image = sprite
        });
      }
    }

    private async UniTask<Sprite> LoadSpriteAsync(string address) =>
      await _addressableAssetProvider.LoadAssetAsync<Sprite>(address);

    private bool IsValidOfferConfig(OfferConfig offerConfig)
    {
      if (string.IsNullOrWhiteSpace(offerConfig.Price))
        return false;

      if (string.IsNullOrWhiteSpace(offerConfig.Amount.ToString()))
        return false;

      if (string.IsNullOrWhiteSpace(offerConfig.ImageAddress))
        return false;

      return true;
    }
  }
}