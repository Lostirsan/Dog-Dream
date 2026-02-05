using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public static class AssignUIActionsToEventSystem
{
    public static void Execute()
    {
        var es = GameObject.Find("EventSystem");
        if (es == null)
        {
            Debug.LogError("EventSystem not found");
            return;
        }

        var module = es.GetComponent<InputSystemUIInputModule>();
        if (module == null)
        {
            Debug.LogError("InputSystemUIInputModule not found on EventSystem");
            return;
        }

        var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/UI.inputactions");
        if (actions == null)
        {
            Debug.LogError("UI InputActionAsset not found at Assets/UI.inputactions (make sure it imported)");
            return;
        }

        module.actionsAsset = actions;
        EditorUtility.SetDirty(module);
        Debug.Log("Assigned Assets/UI.inputactions to EventSystem InputSystemUIInputModule.actionsAsset");
    }
}
