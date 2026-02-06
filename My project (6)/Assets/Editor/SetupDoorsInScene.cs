using UnityEditor;
using UnityEngine;
using Game.Doors;

/// <summary>
/// Adds DoorInteractable + a simple collider to all door meshes under FOB_LOD/Doors.
/// Also adds PlayerDoorInteractor to Player and hooks its camera transform.
/// </summary>
public static class SetupDoorsInScene
{
    public static void Execute()
    {
        var doorsRoot = GameObject.Find("FOB_LOD/Doors");
        if (doorsRoot == null)
        {
            Debug.LogError("FOB_LOD/Doors not found in the open scene.");
            return;
        }

        int doorCount = 0;
        int colliderCount = 0;

        var renderers = doorsRoot.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            // We want to attach the component to the renderer's GameObject (usually the mesh).
            var go = r.gameObject;

            if (go.GetComponentInParent<DoorInteractable>() == null)
            {
                go.AddComponent<DoorInteractable>();
                doorCount++;
            }

            if (go.GetComponent<Collider>() == null)
            {
                // Use a BoxCollider sized to renderer bounds (local space approximation).
                var bc = go.AddComponent<BoxCollider>();

                var bounds = r.bounds; // world
                // Convert to local approx.
                var centerLocal = go.transform.InverseTransformPoint(bounds.center);
                var sizeLocal = go.transform.InverseTransformVector(bounds.size);

                bc.center = centerLocal;
                bc.size = new Vector3(Mathf.Abs(sizeLocal.x), Mathf.Abs(sizeLocal.y), Mathf.Abs(sizeLocal.z));

                colliderCount++;
            }
        }

        // Player setup.
        var player = GameObject.Find("Player");
        if (player != null)
        {
            var interactor = player.GetComponent<PlayerDoorInteractor>();
            if (interactor == null)
                interactor = player.AddComponent<PlayerDoorInteractor>();

            // Hook camera.
            var cam = GameObject.Find("Player/Main Camera");
            if (cam != null)
            {
                var so = new SerializedObject(interactor);
                so.FindProperty("cameraTransform").objectReferenceValue = cam.transform;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorUtility.SetDirty(interactor);
        }

        Debug.Log($"SetupDoorsInScene: added DoorInteractable to ~{doorCount} objects, added colliders to ~{colliderCount} objects.");
    }
}
