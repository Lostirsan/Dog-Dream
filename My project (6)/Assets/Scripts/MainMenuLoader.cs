using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Minimal main menu loader.
/// Attach to any GameObject in MainMenu scene and wire to Button.onClick.
/// </summary>
public class MainMenuLoader : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("Optional Auto-Wire")]
    [Tooltip("If set, this component will find the UI Button at runtime and hook StartGame() so you don't need inspector wiring.")]
    [SerializeField] private bool autoWireStartButton = true;

    [Tooltip("Button path in the scene hierarchy. Default matches our MainMenu scene.")]
    [SerializeField] private string startButtonPath = "Canvas/StartButton";

    private void Awake()
    {
        // Auto-wire is a safe fallback when UI events weren't saved / got duplicated.
        if (!autoWireStartButton)
            return;

        var go = GameObject.Find(startButtonPath);
        if (go == null)
            return;

        var button = go.GetComponent<UnityEngine.UI.Button>();
        if (button == null)
            return;

        // Avoid duplicates.
        button.onClick.RemoveListener(StartGame);
        button.onClick.AddListener(StartGame);
    }

    public void StartGame()
    {
        // Load the game scene by name.
        SceneManager.LoadScene(gameSceneName);
    }

    public void Quit()
    {
        // Works in builds. In editor it won't close the Unity editor.
        Application.Quit();
    }
}
