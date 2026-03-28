using UnityEngine;

public class NodeInteraction : MonoBehaviour {
    public string nodeId;
    public GraphLoader loader;

    private void OnMouseDown() {
        Debug.Log("Attempting to bloom node: " + nodeId);
        
        // If the loader is missing, find the one in the scene
        if (loader == null) {
            loader = Object.FindFirstObjectByType<GraphLoader>();
        }

        if (loader != null) {
            loader.LoadNewNode(nodeId);
        } else {
            Debug.LogError("GraphLoader not found in scene!");
        }
    }
}