using System;
using System.IO;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Logic.Services.SaveLoad
{
  public class SaveLoadService : ISaveLoadService
  {
    private readonly string _filePath = Path.Combine(Application.persistentDataPath, "playerProgress.json");
    public PlayerProgress PlayerProgress { get; private set; }

    public bool IsProgressLoaded { get; private set; }
    public event Action OnProgressLoaded;

    public async UniTask LoadProgress()
    {
      UnsubscribeFromDataChanged();

      if (File.Exists(_filePath))
      {
        string json = File.ReadAllText(_filePath);
        PlayerProgress = JsonConvert.DeserializeObject<PlayerProgress>(json);
        Debug.Log("[SaveLoadService]: Progress file loaded successfully!");
      }
      else
      {
        Debug.Log("[SaveLoadService]: Save file not found. Create new Player Progress.");
        PlayerProgress = new PlayerProgress();
        await SaveProgress();
      }

      SubscribeOnDataChanged();
      IsProgressLoaded = true;
      OnProgressLoaded?.Invoke();
    }

    public async UniTask SaveProgress()
    {
      if (PlayerProgress == null)
      {
        Debug.Log("[SaveLoadService]: Save file not found. Create new Player Progress.");
        PlayerProgress = new PlayerProgress();
      }

      string json = JsonConvert.SerializeObject(PlayerProgress, Formatting.Indented);
      await File.WriteAllTextAsync(_filePath, json);
      Debug.Log("[SaveLoadService]: Progress saved successfully !");
    }

    private void UnsubscribeFromDataChanged()
    {
      if (PlayerProgress == null)
        return;

      if (PlayerProgress.PurchaseData != null)
        PlayerProgress.PurchaseData.OnPurchaseDataChanged -= OnDataChanged;

      if (PlayerProgress.PlayerResources != null)
        PlayerProgress.PlayerResources.OnPlayerResourcesDataChanged -= OnDataChanged;
    }

    private void SubscribeOnDataChanged()
    {
      PlayerProgress.PurchaseData.OnPurchaseDataChanged += OnDataChanged;
      PlayerProgress.PlayerResources.OnPlayerResourcesDataChanged += OnDataChanged;
    }

    private void OnDataChanged() => SaveProgress().Forget();
  }
}