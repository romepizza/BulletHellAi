using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshotManager : MonoBehaviour
{
    private static ScreenshotManager s_instance;

    [Header("------- Settings -------")]
    [Header("--- Size ---")]
    [SerializeField] private int m_captureWidth;
    [SerializeField] private int m_captureHeight;
    [Header("--- Background ---")]
    //[SerializeField] private bool m_changeBackground;
    [SerializeField] private float m_backgroundHeight;
    [Header("--- Obstacles ---")]
    [SerializeField] private bool m_changeObstacleScales;
    //[SerializeField] private bool m_ceilScaleToPixelQuads;

    [Header("--- Objects ---")]
    //public Camera m_playerCaptureCamera;
    [SerializeField] private TakeScreenshot m_playerScreenshotScript;
    [SerializeField] private Transform m_playerCaptureArea;
    [Space]
    //public Camera m_aiCaptureCamera;
    [SerializeField] private TakeScreenshot m_aiScreenshotScript;
    [SerializeField] private Transform m_aiCaptureArea;
    [Space]
    //[SerializeField] private Camera m_mainCamera;


    [Header("------- Debug -------")]
    private float m_ratio;
    private int m_lastCaptureWidth;
    private int m_lastCaptureHeight;
    [SerializeField] private Vector3 m_captureAreaSize;
    [SerializeField] private float m_pixelWorldScale;

    #region Mono
    private void Awake()
    {
        if (s_instance != null)
            Debug.Log("Warning: At least two instances of ScreenshotManager seem to be active!");
        s_instance = this;
    }
    #endregion Mono

    #region Alter Size
    public Vector2Int GetScreenshotSize()
    {
        Vector2Int size = new Vector2Int();
        size.x = m_captureWidth;
        size.y = m_captureHeight;

        return size;
    }
    public float GetPixelToWorldScale(int size)
    {
        float pixelWidth = m_captureAreaSize.x / m_captureWidth;
        float pixelHeight = m_captureAreaSize.y / m_captureHeight;


        return pixelHeight * size;
    }
    private void SetCaptureSize(int captureWidth, int captureHeight)
    {
        m_captureWidth = captureWidth;
        m_captureHeight = captureHeight;

        m_ratio = (float)captureWidth / (float)captureHeight;

        // change capture area size
        float finalCaptureAreaWidth = m_backgroundHeight * m_ratio;
        m_captureAreaSize = new Vector3(finalCaptureAreaWidth, m_backgroundHeight, 1);
        m_playerCaptureArea.localScale = m_captureAreaSize;
        m_aiCaptureArea.localScale = m_captureAreaSize;
        m_pixelWorldScale = GetPixelToWorldScale(1);
    }
    public Vector3 GetObstacleScale(Vector3 originalScale)
    {
        if (!m_changeObstacleScales)
        {
            return originalScale;
        }

        if (originalScale.x != originalScale.y)
            Debug.Log("Warning: localScale components weren't the same (" + originalScale.x + "/" + originalScale.y + "). Taking the x-component.");

        float finalCaptureSize = originalScale.x;
        if (originalScale.x < m_pixelWorldScale)
            finalCaptureSize = m_pixelWorldScale;

        return new Vector3(finalCaptureSize, finalCaptureSize, finalCaptureSize);
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        bool change = m_lastCaptureHeight != m_captureHeight && m_captureHeight != 0;
        change |= m_lastCaptureWidth != m_captureWidth && m_captureWidth != 0;

        if (!change)
            return;

        SetCaptureSize(m_captureWidth, m_captureHeight);

        m_lastCaptureWidth = m_captureWidth;
        m_lastCaptureHeight = m_captureHeight;
    }
    #endregion

    #region Statics
    public static ScreenshotManager Instance()
    {
        return s_instance;
    }
    #endregion
}
