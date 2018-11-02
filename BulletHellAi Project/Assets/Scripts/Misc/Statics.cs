using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Statics : MonoBehaviour
{
    public static int s_mainCameraLayer = 15;
    public static int s_captureCameraLayer = 16;
    public static int s_neuralNetworkLayer = 20;

    [SerializeField] private Camera m_camera;
    private static Camera s_camera;

    #region Mono
    private void Awake()
    {
        s_camera = m_camera;
    }
    #endregion

    public static Camera GetMainCamera()
    {
        return s_camera;
    }

    //[SerializeField] private ScreenshotManager m_screenshotManager;
}
