using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Statics : MonoBehaviour
{
    public static int s_mainCameraLayer { get; private set; }
    public static int s_captureCameraLayerPlayer { get; private set; }
    public static int s_captureCameraLayerObstacle { get; private set; }
    public static int s_neuralNetworkLayer { get; private set; }

    [SerializeField] private Camera m_camera;
    private static Camera s_camera;

    #region Mono
    private void Awake()
    {
        s_camera = m_camera;

        s_mainCameraLayer = 15;
        s_captureCameraLayerPlayer = 16;
        s_captureCameraLayerObstacle = 17;
        s_neuralNetworkLayer = 20;
    }
    #endregion

    public static Camera GetMainCamera()
    {
        return s_camera;
    }

    //[SerializeField] private ScreenshotManager m_screenshotManager;
}
