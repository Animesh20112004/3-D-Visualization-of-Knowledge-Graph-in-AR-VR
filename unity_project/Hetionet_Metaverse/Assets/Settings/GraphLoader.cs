using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

public class GraphLoader : MonoBehaviour {
    [Header("Setup")]
    public GameObject nodePrefab;   // Drag your Sphere Prefab here
    public Material lineMaterial;    // Drag your GraphLine Material here
    
    [Header("Settings")]
    public string centerNodeId = "Gene::9021"; // Starting node
    private string baseUrl = "http://127.0.0.1:5000/get_neighbors/";

    void Start() {
        LoadNewNode(centerNodeId);
    }

    public void LoadNewNode(string id) {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
        StartCoroutine(GetGraphData(id));
    }

    IEnumerator GetGraphData(string id) {
        string encodedId = UnityWebRequest.EscapeURL(id);
        string fullUrl = baseUrl + encodedId;
    
        Debug.Log("🌐 Fixed URL: " + fullUrl);
        
        // FIX: Use the 'fullUrl' (encoded) for the actual request
        using (UnityWebRequest webRequest = UnityWebRequest.Get(baseUrl+id)){
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success) {
                string jsonResponse = "{\"nodes\":" + webRequest.downloadHandler.text + "}";
                NodeListWrapper wrapper = JsonUtility.FromJson<NodeListWrapper>(jsonResponse);

                Debug.Log("Found " + wrapper.nodes.Length + " neighbors for this node!");

                // 3. Create the Center Node using the new SpawnNode function
                GameObject centerObj = SpawnNode(id, "ID: " + id, Vector3.zero, Color.red, true);

                // 4. Create Neighbors
                float angle = 0;
                if (wrapper.nodes.Length > 0) {
                    foreach (NodeData node in wrapper.nodes) {
                        float radius = 4f + Random.value * 2f; 
                        Vector3 pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
                        
                        // Spawn neighbor using the helper function
                        GameObject neighbor = SpawnNode(node.id, node.name, pos, GetTypeColor(node.type), false);

                        // 5. Draw the Edge
                        DrawLine(centerObj.transform.position, neighbor.transform.position);

                        angle += 2 * Mathf.PI / wrapper.nodes.Length;
                    }
                }
            } else {
                Debug.LogError("Error fetching data: " + webRequest.error);
            }
        }
    }

    // THE REPAIR: Added SpawnNode helper function
    GameObject SpawnNode(string id, string label, Vector3 localPos, Color color, bool isCenter) {
        // Instantiate as a child of this GraphManager (transform)
        GameObject node = Instantiate(nodePrefab, transform); 
        node.transform.localPosition = localPos;
        node.name = (isCenter ? "CENTER_" : "NODE_") + id;

        // Apply Color
        Renderer rend = node.GetComponent<Renderer>();
        if (rend != null) rend.material.color = color;

        // Apply Text Label
        TextMeshProUGUI textComp = node.GetComponentInChildren<TextMeshProUGUI>(true);
        if (textComp != null) {
            textComp.text = label;
            // Force visible scale for the text transform
            textComp.transform.localScale = new Vector3(20, 20, 20); 
        }

        // Attach Interaction
        NodeInteraction script = node.GetComponent<NodeInteraction>();
        if (script == null) script = node.AddComponent<NodeInteraction>();
        script.nodeId = id;
        script.loader = this;

        return node;
    }

    // Helper for coloring based on biological type
    Color GetTypeColor(string type) {
        if (type == "Gene") return Color.green;
        if (type == "Disease") return Color.magenta;
        if (type == "Compound") return Color.cyan;
        return Color.white;
    }

    void DrawLine(Vector3 start, Vector3 end) {
        GameObject lineObj = new GameObject("Edge");
        lineObj.transform.SetParent(this.transform);
        
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.material = lineMaterial;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.positionCount = 2;
        lr.useWorldSpace = true; // Essential for correct 3D connections
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }
}

[System.Serializable]
public class NodeData {
    public string id;
    public string name;
    public string type;
}

[System.Serializable]
public class NodeListWrapper {
    public NodeData[] nodes;
}