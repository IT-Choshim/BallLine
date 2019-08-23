using UnityEngine;
using System.Collections;

namespace BallLine
{
    [RequireComponent(typeof(Camera))]
    public class CameraWidthControl : MonoBehaviour
    {
        public float screenWidth = 1800;

        private Camera cam;

        private float size;
        private float ratio;
        private float screenHeight;

        void Awake()
        {
            cam = GetComponent<Camera>();
            ratio = (float)Screen.height / (float)Screen.width;
            screenHeight = screenWidth * ratio;
            size = screenHeight / 200;
            cam.orthographicSize = size;
        }

        void Update()
        {
            cam = GetComponent<Camera>();
            ratio = (float)Screen.height / (float)Screen.width;
            screenHeight = screenWidth * ratio;
            size = screenHeight / 200;
            cam.orthographicSize = size;
        }
    }
}