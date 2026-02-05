using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

/// <summary>
/// One-click config for InputSystemUIInputModule so UI buttons receive pointer clicks.
/// Unity's InputSystemUIInputModule needs action references (Point/Click/etc) to be assigned.
/// </summary>
public static class ConfigureInputSystemUIForMenu
{
    public static void Execute()
    {
        var es = GameObject.Find("EventSystem");
        if (es == null)
        {
            Debug.LogError("EventSystem not found in the open scene.");
            return;
        }

        var module = es.GetComponent<InputSystemUIInputModule>();
        if (module == null)
        {
            Debug.LogError("InputSystemUIInputModule not found on EventSystem.");
            return;
        }

        var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/UI.inputactions");
        if (actions == null)
        {
            Debug.LogError("Assets/UI.inputactions not found / not imported as InputActionAsset.");
            return;
        }

        module.actionsAsset = actions;

        // Hook standard UI actions by name: UI/Point, UI/LeftClick, UI/RightClick, UI/MiddleClick, UI/ScrollWheel, UI/Move, UI/Submit, UI/Cancel
        module.point = InputActionReference.Create(actions.FindAction("UI/Point", true));
        module.leftClick = InputActionReference.Create(actions.FindAction("UI/LeftClick", true));
        module.rightClick = InputActionReference.Create(actions.FindAction("UI/RightClick", true));
        module.middleClick = InputActionReference.Create(actions.FindAction("UI/MiddleClick", true));
        module.scrollWheel = InputActionReference.Create(actions.FindAction("UI/ScrollWheel", true));
        module.move = InputActionReference.Create(actions.FindAction("UI/Move", true));
        module.submit = InputActionReference.Create(actions.FindAction("UI/Submit", true));
        module.cancel = InputActionReference.Create(actions.FindAction("UI/Cancel", true));

        EditorUtility.SetDirty(module);
        Debug.Log("Configured InputSystemUIInputModule action references for UI clicks.");
    }
}
