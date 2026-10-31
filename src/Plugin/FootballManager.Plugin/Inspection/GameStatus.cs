using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FootballManager.Plugin.Inspection;

internal static class GameStatus
{
    public static string Describe()
    {
        var output = new StringBuilder();
        output.AppendLine($"bridge: {Plugin.Name} {Plugin.Version}");
        output.AppendLine($"product: {Application.productName} {Application.version}");
        output.AppendLine($"unity: {Application.unityVersion}");
        output.AppendLine($"scene: {SceneManager.GetActiveScene().name}");
        output.AppendLine($"focused: {Application.isFocused}, runInBackground: {Application.runInBackground}");
        output.AppendLine($"frame: {Time.frameCount}");
        output.AppendLine($"uiDocuments: {UiDocuments.All().Count}");
        return output.ToString();
    }
}
