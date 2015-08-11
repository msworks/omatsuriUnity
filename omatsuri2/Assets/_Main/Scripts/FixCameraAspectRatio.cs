using UnityEngine;
using System.Collections;

public class FixCameraAspectRatio : MonoBehaviour {
    public float size = 1f;
    public float targetAspect = 10.0f / 16.0f;
    void Start() {
        Camera camera = GetComponent<Camera>();
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = targetAspect / windowAspect;
        Debug.Log("FixCameraAspectRatio:TGT=" + targetAspect + " CUR:" + windowAspect + " SCL:" + scaleHeight);
        camera.orthographicSize = size * scaleHeight;
    }
}
