using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

[InitializeOnLoad]
public class RunSetupCircularFade
{
    static RunSetupCircularFade()
    {
        EditorApplication.delayCall += RunSetup;
    }
    
    static void RunSetup()
    {
        // Find the CircularFadePanel
        GameObject panel = GameObject.Find("Canvas/CircularFadePanel");
        if (panel == null)
        {
            return;
        }
        
        Image image = panel.GetComponent<Image>();
        if (image == null || image.material != null && image.material.shader.name == "UI/CircularFade")
        {
            return;
        }
        
        // Find the shader
        Shader shader = Shader.Find("UI/CircularFade");
        if (shader == null)
        {
            Debug.LogError("UI/CircularFade shader not found!");
            return;
        }
        
        // Check if material already exists
        string matPath = "Assets/Materials/CircularFadeMaterial.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        
        if (mat == null)
        {
            // Create material
            mat = new Material(shader);
            mat.SetFloat("_Radius", 1.5f);
            mat.SetVector("_Center", new Vector4(0.5f, 0.5f, 0, 0));
            mat.SetColor("_Color", Color.black);
            
            // Save material as asset
            AssetDatabase.CreateAsset(mat, matPath);
            AssetDatabase.SaveAssets();
        }
        
        // Assign to image
        image.material = mat;
        
        // Deactivate panel
        panel.SetActive(false);
        
        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log("CircularFadePanel setup complete!");
    }
}
