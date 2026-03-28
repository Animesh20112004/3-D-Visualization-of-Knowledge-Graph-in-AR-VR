using UnityEngine;

public class Billboard : MonoBehaviour {
    private Canvas myCanvas;

    void Start() {
        // Find the Canvas this script is attached to
        myCanvas = GetComponentInParent<Canvas>();
        
        // AUTOMATICALLY find the Main Camera and assign it
        if (myCanvas != null && myCanvas.worldCamera == null) {
            myCanvas.worldCamera = Camera.main;
        }
    }

    void LateUpdate() {
        // Keep the text facing the camera
        if (Camera.main != null) {
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                             Camera.main.transform.rotation * Vector3.up);
        }
    }
}