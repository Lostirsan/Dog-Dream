using UnityEditor;
using UnityEngine;

public static class FixPlayerCameraSetup
{
    public static void Execute()
    {
        var player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogError("Player not found");
            return;
        }

        var cam = GameObject.Find("Player/Main Camera");
        if (cam == null)
        {
            // Fallback: find camera component under player
            var cameraComp = player.GetComponentInChildren<Camera>(true);
            cam = cameraComp != null ? cameraComp.gameObject : null;
        }

        if (cam == null)
        {
            Debug.LogError("Main Camera under Player not found");
            return;
        }

        // Ensure camera is a child of Player
        cam.transform.SetParent(player.transform, false);

        // Standard FPS eye height relative to player pivot (grounded at y=0)
        cam.transform.localPosition = new Vector3(0f, 1.6f, 0f);
        cam.transform.localRotation = Quaternion.identity;

        // Ensure CharacterController dimensions are sane and centered
        var cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.height = 1.8f;
            cc.center = new Vector3(0f, 0.9f, 0f);
        }

        // Mark scene dirty so changes persist
        EditorUtility.SetDirty(player);
        EditorUtility.SetDirty(cam);
        if (cc != null) EditorUtility.SetDirty(cc);

        Debug.Log("Fixed Player/Main Camera local position to (0,1.6,0) and CharacterController height/center.");
    }
}
