using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.UI;

public static class BuildDogDialogUI
{
    public static void Execute()
    {
        // Ensure textures are sprites.
        EnsureSprite("Assets/Materials/DogDialog.png");
        EnsureSprite("Assets/Materials/Continue.png");

        var canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogError("Canvas not found in scene.");
            return;
        }

        var dialogRoot = GameObject.Find("Canvas/DogDialogUI");
        if (dialogRoot == null)
        {
            dialogRoot = new GameObject("DogDialogUI", typeof(RectTransform));
            dialogRoot.transform.SetParent(canvas.transform, false);
        }

        var rt = (RectTransform)dialogRoot.transform;
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 20f);
        rt.sizeDelta = new Vector2(560f, 180f);

        // Background image
        var bg = dialogRoot.GetComponent<Image>();
        if (bg == null) bg = dialogRoot.AddComponent<Image>();
        bg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Materials/DogDialog.png");
        bg.preserveAspect = true;
        bg.raycastTarget = true;

        // Reuse existing TMP if present.
        GameObject textGO = GameObject.Find("Canvas/Text (TMP)");
        if (textGO == null)
        {
            textGO = new GameObject("DialogText", typeof(RectTransform));
            textGO.transform.SetParent(dialogRoot.transform, false);
            textGO.AddComponent<TextMeshProUGUI>();
        }
        else
        {
            textGO.transform.SetParent(dialogRoot.transform, false);
            textGO.name = "DialogText";
        }

        var tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.text = "Базовый диалог собачки";
        tmp.fontSize = 26;
        tmp.color = new Color(0.1f, 0.1f, 0.1f, 1f);
        tmp.raycastTarget = false;
        tmp.enableWordWrapping = true;

        var textRT = (RectTransform)textGO.transform;
        textRT.anchorMin = new Vector2(0.27f, 0.22f);
        textRT.anchorMax = new Vector2(0.95f, 0.86f);
        textRT.pivot = new Vector2(0f, 1f);
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        // Continue button
        var btnGO = GameObject.Find("Canvas/DogDialogUI/ContinueButton");
        if (btnGO == null)
        {
            btnGO = new GameObject("ContinueButton", typeof(RectTransform));
            btnGO.transform.SetParent(dialogRoot.transform, false);
        }

        var btnImg = btnGO.GetComponent<Image>();
        if (btnImg == null) btnImg = btnGO.AddComponent<Image>();
        btnImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Materials/Continue.png");
        btnImg.preserveAspect = true;
        btnImg.raycastTarget = true;

        var button = btnGO.GetComponent<Button>();
        if (button == null) button = btnGO.AddComponent<Button>();

        var btnRT = (RectTransform)btnGO.transform;
        btnRT.anchorMin = new Vector2(0.78f, 0.06f);
        btnRT.anchorMax = new Vector2(0.98f, 0.28f);
        btnRT.pivot = new Vector2(0.5f, 0.5f);
        btnRT.offsetMin = Vector2.zero;
        btnRT.offsetMax = Vector2.zero;

        // Hook controller
        var controller = dialogRoot.GetComponent<DogDialogUI>();
        if (controller == null) controller = dialogRoot.AddComponent<DogDialogUI>();

        var so = new SerializedObject(controller);
        so.FindProperty("dialogText").objectReferenceValue = tmp;
        so.FindProperty("continueButton").objectReferenceValue = button;
        so.FindProperty("fullText").stringValue = "Базовый диалог собачки";
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(controller);
        EditorUtility.SetDirty(dialogRoot);

        Debug.Log("Built DogDialogUI on Canvas.");
    }

    private static void EnsureSprite(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;

        bool changed = false;

        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            changed = true;
        }

        if (importer.spriteImportMode != SpriteImportMode.Single)
        {
            importer.spriteImportMode = SpriteImportMode.Single;
            changed = true;
        }

        if (importer.alphaIsTransparency == false)
        {
            importer.alphaIsTransparency = true;
            changed = true;
        }

        if (importer.wrapMode != TextureWrapMode.Clamp)
        {
            importer.wrapMode = TextureWrapMode.Clamp;
            changed = true;
        }

        if (changed)
            importer.SaveAndReimport();
    }
}
