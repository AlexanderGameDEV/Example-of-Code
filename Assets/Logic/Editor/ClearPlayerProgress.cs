using System.IO;
using System.Media;
using UnityEditor;
using UnityEngine;

namespace Logic.Editor
{
  public static class ClearPlayerProgress
  {
    private const string MENU_PATH = "Tools/Clear Player Progress";

    [MenuItem(MENU_PATH)]
    private static void ClearProgress()
    {
      string filePath = Path.Combine(Application.persistentDataPath, "playerProgress.json");

      if (File.Exists(filePath))
      {
        bool isConfirmed = EditorUtility.DisplayDialog(
          "Clear Player Progress",
          "Delete player progress Json file?\n\n" +
          "This action cannot be undone.\n\n" +
          "File is located at the path:\n\n" + filePath,
          "Yes, delete",
          "Cancel");

        if (isConfirmed)
        {
          File.Delete(filePath);
          Debug.Log("<color=green>Player progress file successfully deleted!</color>");

          EditorUtility.DisplayDialog(
            "Success",
            "Player progress file has been deleted successfully!\n\n" +
            "New player progress file will be created after restarting.",
            "OK");
        }
      }
      else
      {
        SystemSounds.Hand.Play();
        EditorUtility.DisplayDialog(
          "File Not Found",
          $"Progress file does not exist:\n{filePath}",
          "OK");
      }
    }

    [MenuItem(MENU_PATH, true)]
    private static bool ValidateClearPlayerProgress()
    {
      string filePath = Path.Combine(Application.persistentDataPath, "playerProgress.json");
      return File.Exists(filePath);
    }
  }
}