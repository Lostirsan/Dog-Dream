using UnityEngine;
using UnityEditor;

public class CreateClimbablePipe : MonoBehaviour
{
    [MenuItem("Tools/Create Climbable Pipe")]
    public static void Execute()
    {
        // Delete existing ClimbablePipe if it exists
        GameObject existing = GameObject.Find("ClimbablePipe");
        if (existing != null)
        {
            Object.DestroyImmediate(existing);
        }
        
        // Create parent object
        GameObject pipeParent = new GameObject("ClimbablePipe");
        pipeParent.transform.position = new Vector3(5f, 0f, 5f);
        
        // Pipe parameters
        float pipeLength = 15f;
        float outerRadius = 3f;
        float wallThickness = 0.2f;
        int segments = 12; // Number of wall segments around the pipe
        
        // Create the pipe walls as individual curved segments
        for (int i = 0; i < segments; i++)
        {
            float angle = (360f / segments) * i;
            float nextAngle = (360f / segments) * (i + 1);
            float midAngle = (angle + nextAngle) / 2f;
            
            // Create wall segment
            GameObject wallSegment = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallSegment.name = $"PipeWall_{i}";
            wallSegment.transform.parent = pipeParent.transform;
            
            // Calculate position on the circle
            float radians = midAngle * Mathf.Deg2Rad;
            float x = Mathf.Cos(radians) * (outerRadius - wallThickness / 2f);
            float z = Mathf.Sin(radians) * (outerRadius - wallThickness / 2f);
            
            wallSegment.transform.localPosition = new Vector3(x, pipeLength / 2f, z);
            wallSegment.transform.localRotation = Quaternion.Euler(0f, -midAngle, 0f);
            
            // Calculate segment width based on arc length
            float segmentWidth = 2f * Mathf.PI * outerRadius / segments * 1.1f; // Slight overlap
            wallSegment.transform.localScale = new Vector3(segmentWidth, pipeLength, wallThickness);
            
            // Assign material
            MeshRenderer renderer = wallSegment.GetComponent<MeshRenderer>();
            renderer.material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/PipeMaterial.mat");
            if (renderer.material == null)
            {
                renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                renderer.material.color = new Color(0.4f, 0.4f, 0.5f);
            }
        }
        
        // Create floor inside the pipe (walkable surface)
        GameObject pipeFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pipeFloor.name = "PipeFloor";
        pipeFloor.transform.parent = pipeParent.transform;
        pipeFloor.transform.localPosition = new Vector3(0f, 0.1f, 0f);
        pipeFloor.transform.localScale = new Vector3(outerRadius * 1.8f, 0.2f, outerRadius * 1.8f);
        
        MeshRenderer floorRenderer = pipeFloor.GetComponent<MeshRenderer>();
        floorRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        floorRenderer.material.color = new Color(0.3f, 0.3f, 0.35f);
        
        // Create entrance ramp
        GameObject entranceRamp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        entranceRamp.name = "EntranceRamp";
        entranceRamp.transform.parent = pipeParent.transform;
        entranceRamp.transform.localPosition = new Vector3(outerRadius + 1.5f, 0.5f, 0f);
        entranceRamp.transform.localRotation = Quaternion.Euler(0f, 0f, 15f);
        entranceRamp.transform.localScale = new Vector3(4f, 0.2f, 3f);
        
        MeshRenderer rampRenderer = entranceRamp.GetComponent<MeshRenderer>();
        rampRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        rampRenderer.material.color = new Color(0.5f, 0.5f, 0.4f);
        
        // Create a gap in the pipe wall for entrance (remove one segment's collider or make it a trigger)
        Transform entranceWall = pipeParent.transform.Find("PipeWall_0");
        if (entranceWall != null)
        {
            // Make entrance wall shorter to create opening
            entranceWall.localScale = new Vector3(entranceWall.localScale.x, pipeLength - 4f, entranceWall.localScale.z);
            entranceWall.localPosition = new Vector3(entranceWall.localPosition.x, (pipeLength - 4f) / 2f + 4f, entranceWall.localPosition.z);
        }
        
        // Create entrance opening floor extension
        GameObject entranceFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        entranceFloor.name = "EntranceFloor";
        entranceFloor.transform.parent = pipeParent.transform;
        entranceFloor.transform.localPosition = new Vector3(outerRadius - 0.5f, 0.1f, 0f);
        entranceFloor.transform.localScale = new Vector3(2f, 0.2f, 3f);
        
        MeshRenderer entranceFloorRenderer = entranceFloor.GetComponent<MeshRenderer>();
        entranceFloorRenderer.material = floorRenderer.material;
        
        // Create spiral ramp inside for climbing
        int spiralSegments = 20;
        float spiralHeight = pipeLength - 2f;
        float spiralRadius = outerRadius * 0.6f;
        
        for (int i = 0; i < spiralSegments; i++)
        {
            GameObject spiralStep = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spiralStep.name = $"SpiralStep_{i}";
            spiralStep.transform.parent = pipeParent.transform;
            
            float t = (float)i / spiralSegments;
            float spiralAngle = t * 720f; // Two full rotations
            float height = 0.5f + t * spiralHeight;
            
            float radians = spiralAngle * Mathf.Deg2Rad;
            float x = Mathf.Cos(radians) * spiralRadius;
            float z = Mathf.Sin(radians) * spiralRadius;
            
            spiralStep.transform.localPosition = new Vector3(x, height, z);
            spiralStep.transform.localRotation = Quaternion.Euler(0f, -spiralAngle, 0f);
            spiralStep.transform.localScale = new Vector3(2.5f, 0.15f, 1.2f);
            
            MeshRenderer stepRenderer = spiralStep.GetComponent<MeshRenderer>();
            stepRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            stepRenderer.material.color = new Color(0.6f, 0.55f, 0.4f);
        }
        
        // Create top platform
        GameObject topPlatform = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        topPlatform.name = "TopPlatform";
        topPlatform.transform.parent = pipeParent.transform;
        topPlatform.transform.localPosition = new Vector3(0f, pipeLength - 0.1f, 0f);
        topPlatform.transform.localScale = new Vector3(outerRadius * 1.6f, 0.1f, outerRadius * 1.6f);
        
        MeshRenderer topRenderer = topPlatform.GetComponent<MeshRenderer>();
        topRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        topRenderer.material.color = new Color(0.4f, 0.5f, 0.4f);
        
        // Select the created pipe
        Selection.activeGameObject = pipeParent;
        
        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log("Climbable Pipe created! Walk up the entrance ramp and use the spiral stairs inside to climb up.");
    }
}
