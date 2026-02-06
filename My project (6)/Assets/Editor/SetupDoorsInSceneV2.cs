using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Game.Doors;

/// <summary>
/// Rebuilds door interaction setup:
/// - Treat each direct child under FOB_LOD/Doors as a door root
/// - Put DoorInteractable on the root (rotateTarget = root)
/// - Ensure root has a BoxCollider sized to combined renderer bounds
/// - Mark doors as non-static (root + children) so they can move
/// - Ensure Player has PlayerDoorInteractor with camera set
/// </summary>
public static class SetupDoorsInSceneV2
{
    public static void Execute()
    {
        var doorsRoot = GameObject.Find("FOB_LOD/Doors");
        if (doorsRoot == null)
        {
            Debug.LogError("FOB_LOD/Doors not found in the open scene.");
            return;
        }

        int interactablesAdded = 0;
        int collidersAdded = 0;
        int staticsCleared = 0;
        int interactablesRemovedFromChildren = 0;

        var rootTransform = doorsRoot.transform;
        var doorRoots = new List<Transform>();
        for (int i = 0; i < rootTransform.childCount; i++)
            doorRoots.Add(rootTransform.GetChild(i));

        foreach (var doorRoot in doorRoots)
        {
            if (doorRoot == null) continue;

            // Only consider objects that actually contain renderers (skip empty groups).
            if (doorRoot.GetComponentInChildren<Renderer>(true) == null)
                continue;

            // Remove DoorInteractable from children to avoid double-updates.
            var childInteractables = doorRoot.GetComponentsInChildren<DoorInteractable>(true);
            foreach (var di in childInteractables)
            {
                if (di == null) continue;
                if (di.transform == doorRoot) continue;
                Object.DestroyImmediate(di);
                interactablesRemovedFromChildren++;
            }

            // Add/ensure DoorInteractable on root.
            var interactable = doorRoot.GetComponent<DoorInteractable>();
            if (interactable == null)
            {
                interactable = doorRoot.gameObject.AddComponent<DoorInteractable>();
                interactablesAdded++;
            }
            interactable.SetRotateTarget(doorRoot);
            EditorUtility.SetDirty(interactable);

            // Clear static flags so it can move.
            foreach (var t in doorRoot.GetComponentsInChildren<Transform>(true))
            {
                if (t.gameObject.isStatic)
                {
                    t.gameObject.isStatic = false;
                    staticsCleared++;
                }
            }

            // Ensure a collider on the root for reliable raycasts.
            if (doorRoot.GetComponent<Collider>() == null)
            {
                var bc = doorRoot.gameObject.AddComponent<BoxCollider>();
                FitBoxColliderToRenderers(doorRoot, bc);
                collidersAdded++;
            }
        }

        // Player setup.
        var player = GameObject.Find("Player");
        if (player != null)
        {
            var interactor = player.GetComponent<PlayerDoorInteractor>();
            if (interactor == null)
                interactor = player.AddComponent<PlayerDoorInteractor>();

            var cam = GameObject.Find("Player/Main Camera");
            if (cam != null)
            {
                var so = new SerializedObject(interactor);
                so.FindProperty("cameraTransform").objectReferenceValue = cam.transform;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorUtility.SetDirty(interactor);
        }

        Debug.Log($"SetupDoorsInSceneV2: roots={doorRoots.Count}, interactablesAdded={interactablesAdded}, childInteractablesRemoved={interactablesRemovedFromChildren}, collidersAdded={collidersAdded}, staticsCleared={staticsCleared}");
    }

    private static void FitBoxColliderToRenderers(Transform root, BoxCollider bc)
    {
        var renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers == null || renderers.Length == 0)
            return;

        var bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        // Convert world bounds into root local space.
        var centerLocal = root.InverseTransformPoint(bounds.center);
        var sizeLocal = root.InverseTransformVector(bounds.size);

        bc.center = centerLocal;
        bc.size = new Vector3(Mathf.Abs(sizeLocal.x), Mathf.Abs(sizeLocal.y), Mathf.Abs(sizeLocal.z));
    }
}
