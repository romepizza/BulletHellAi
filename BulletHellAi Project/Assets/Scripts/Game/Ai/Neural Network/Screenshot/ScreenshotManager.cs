using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshotManager : MonoBehaviour
{
    private static ScreenshotManager s_instance;

    [Header("------- Settings -------")]
    [SerializeField] private int m_outputNodenNumber;
    [Header("--- Capture Size ---")]
    [SerializeField] private int m_captureWidth;
    [SerializeField] private int m_captureHeight;
    [Header("--- Background ---")]
    //[SerializeField] private bool m_changeBackground;
    [SerializeField] private float m_backgroundHeight;
    [Header("--- Obstacles ---")]
    [SerializeField] private bool m_changeObstacleScales;
    //[SerializeField] private bool m_ceilScaleToPixelQuads;

    [Header("--- Objects ---")]
    [SerializeField] private List<Transform> m_playerVisualsCapture;
    [SerializeField] private List<Transform> m_captureAreas;
    [SerializeField] private List<TakeScreenshot> m_screenshotScripts;


    [Header("------- Debug -------")]
    private float m_ratio;
    private int m_lastCaptureWidth;
    private int m_lastCaptureHeight;
    private float m_lastBackgroundHeight;
    private Vector3 m_captureAreaSize;
    private float m_pixelWorldScale;

    #region Mono
    private void Awake()
    {
        if (s_instance != null)
            Debug.Log("Warning: At least two instances of ScreenshotManager seem to be active!");
        s_instance = this;
    }
    #endregion Mono

    #region Size
    public float GetPixelToWorldScale(int size)
    {
        float pixelHeight = m_captureAreaSize.y / m_captureHeight;

        return pixelHeight * size;
    }
    private void SetCaptureSize(/*int captureWidth, int captureHeight*/)
    {
        if (GetCaptureWidth() == 0 || GetCaptureHeight() == 0)
            return;

        m_ratio = (float)GetCaptureWidth() / (float)GetCaptureHeight();

        // change capture area size
        float finalCaptureAreaWidth = m_backgroundHeight * m_ratio;
        m_captureAreaSize = new Vector3(finalCaptureAreaWidth, m_backgroundHeight, 1);
        m_pixelWorldScale = GetPixelToWorldScale(1);

        for (int i = 0; i < m_screenshotScripts.Count; i++)
        {
            if(m_screenshotScripts[i].IsBase())
                m_screenshotScripts[i].SetCaptureSize(/*captureWidth, captureHeight*/);
        }
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        bool change = m_lastCaptureHeight != m_captureHeight && m_captureHeight > 0 && m_lastBackgroundHeight != 0;
        change |= m_lastCaptureWidth != m_captureWidth && m_captureWidth > 0;
        change |= m_lastBackgroundHeight != m_backgroundHeight && m_backgroundHeight > 0;

        if (!change)
            return;

        SetCaptureSize(/*GetCaptureWidth(), GetCaptureHeight()*/);

        m_lastCaptureWidth = GetCaptureWidth();
        m_lastCaptureHeight = GetCaptureHeight();
    }
    #endregion

    #region Statics
    public static ScreenshotManager Instance()
    {
        return s_instance;
    }
    #endregion

    #region Getter
    public int GetCaptureWidth()
    {
        return m_captureWidth;
    }
    public int GetCaptureHeight()
    {
        return m_captureHeight;
    }
    public float GetBackgroundHeight()
    {
        return m_backgroundHeight;
    }
    public int GetOutputNumber()
    {
        return m_outputNodenNumber;
    }
    #endregion
}
