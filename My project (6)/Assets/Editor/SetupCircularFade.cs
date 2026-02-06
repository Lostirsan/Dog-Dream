using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class SetupCircularFade : MonoBehaviour
{
    [MenuItem("Tools/Setup Circular Fade Panel")]
    public static void Setup()
    {
        // Find the CircularFadePanel
        GameObject panel = GameObject.Find("Canvas/CircularFadePanel");
        if (panel == null)
        {
            Debug.LogError("CircularFadePanel not found!");
            return;
        }
        
        Image image = panel.GetComponent<Image>();
        if (image == null)
        {
            Debug.LogError("Image component not found on CircularFadePanel!");
            return;
        }
        
        // Find the shader
        Shader shader = Shader.Find("UI/CircularFade");
        if (shader == null)
        {
            Debug.LogError("UI/CircularFade shader not found!");
            return;
        }
        
        // Create material
        Material mat = new Material(shader);
        mat.SetFloat("_Radius", 1.5f);
        mat.SetVector("_Center", new Vector4(0.5f, 0.5f, 0, 0));
        mat.SetColor("_Color", Color.black);
        
        // Save material as asset
        string matPath = "Assets/Materials/CircularFadeMaterial.mat";
        AssetDatabase.CreateAsset(mat, matPath);
        AssetDatabase.SaveAssets();
        
        // Assign to image
        image.material = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        
        // Deactivate panel
        panel.SetActive(false);
        
        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log("CircularFadePanel setup complete!");
    }
}
