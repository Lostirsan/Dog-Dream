using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor-only helper to ensure the correct scenes are in Build Settings and in the right order.
/// (MainMenu first, then GameScene)
/// </summary>
public static class SetupMenuAndBuildScenes
{
    public static void Execute()
    {
        var mainMenuPath = "Assets/Scenes/MainMenu.unity";
        var gameScenePath = "Assets/Scenes/GameScene.unity";

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(mainMenuPath, true),
            new EditorBuildSettingsScene(gameScenePath, true)
        };

        Debug.Log("Build Settings updated: MainMenu -> GameScene");
    }
}
