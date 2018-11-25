﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

[System.Serializable]
public struct NNTSSaveData
{
    public int m_captureWidth;
    public int m_captureHeight;
    public float m_backgroundHeight;
    public bool m_changeObstacleScales;

    public TakeScreenshot.Format m_format;
    public bool m_allowHDR;

    public float m_activisionThreshold;
}

public struct CacheData
{
    public int captureWidth;
    public int captureHeight;
    public float playerHight;

    public bool Equals(int captureWidth, int captureHeight, float playerHight)
    {
        if (this.captureWidth > captureWidth * 1.0001f || this.captureWidth < captureWidth * 0.9999f)
            return false;
        if (this.captureHeight > captureHeight * 1.0001f || this.captureHeight < captureHeight * 0.9999f)
            return false;
        if (this.playerHight > playerHight * 1.0001f || this.playerHight < playerHight * 0.9999f)
            return false;

        return true;
    }
}

public class TakeScreenshot : MonoBehaviour {

    [Header("------- Settings -------")]
    [SerializeField] private Camera m_camera;

    [Header("--- Pixel Size ---")]
    [SerializeField] private int m_captureWidth;
    [SerializeField] private int m_captureHeight;
    [SerializeField] private float m_backgroundHeight;
    [SerializeField] private bool m_changeObstacleScales;

    [Header("--- Format ---")]
    [SerializeField] private Format m_format;
    [SerializeField] private bool m_allowHDR;

    [Header("--- Folder ---")]
    [SerializeField] private string m_fileName;
    [SerializeField] private string m_folderName;

    [Header("--- Misc ---")]
    [SerializeField] private float m_activisionThreshold;
    [SerializeField] private KeyCode m_keyCodeScreenshot;
    [SerializeField] private bool m_saveFile;
    [SerializeField] private bool m_showScreenshot;

    [Header("--- Objects ---")]
    [SerializeField] private RawImage m_rawImage;
    [SerializeField] private List<Transform> m_playerVisualsCapture;
    [SerializeField] private List<Transform> m_captureAreas;
    [SerializeField] private ScreenshotManager m_screenshotManager;
    [SerializeField] private SpawnObstacles m_spawner;

    [Header("------- Debug -------")]
    //private SampleManager m_sampleManager;
    private RenderTexture m_renderTexture;
    private bool m_isPressingScreenshot;
    private int m_fileNameCounter;
    private Rect m_rect;
    private Texture2D m_screenshotTexture;
    private int m_currentHeight;
    private int m_currentWidth;

    private Dictionary<CacheData, float[]> m_cacheDataComputed = new Dictionary<CacheData, float[]>();
    private Dictionary<CacheData, float[][]> m_cacheDataRaw = new Dictionary<CacheData, float[][]>();

    private int m_lastCaptureWidth;
    private int m_lastCaptureHeight;
    private float m_lastBackgroundHeight;
    private float m_pixelWorldScale;
    private Vector3 m_captureAreaSize;
    private Vector3 m_defaultScale;

    public enum Format { RAW, PNG, JPG, PPM }

    #region Mono
    // Use this for initialization
    private void Awake()
    {
        m_defaultScale = m_playerVisualsCapture[0].localScale;
    }
    void Start()
    {
        SetCaptureSize();
        SetCaptureSizesPlayer(0);
    }
    // Update is called once per frame
    void Update()
    {
        getInput();
    }
    private void LateUpdate()
    {
        TakeScreenshotManually(false);
        m_cacheDataComputed.Clear();
        m_cacheDataRaw.Clear();
    }
    #endregion Monobehaviour

    #region Screenshot Control
    public void TakeScreenshotManually(bool forceScreenshot)
    {
        if (!m_isPressingScreenshot && !forceScreenshot)
            return;

        PrepareScreenshot(false);
        ShowScreenshot();
        SaveFile();
    }
    public float[] GetScreenshotDataComputed(int captureWidth, int captureHeight, float playerHight, bool show)
    {
        //CacheData cacheData = new CacheData { captureWidth = captureWidth, captureHeight = captureHeight, playerHight = playerHight };
        

        m_currentWidth = captureWidth == 0 ? GetCaptureWidth() : captureWidth;
        m_currentHeight = captureHeight == 0 ? GetCaptureHeight() : captureHeight;
        playerHight = playerHight == 0 ? GetPlayerHight() : playerHight;

        foreach (CacheData cacheData in m_cacheDataComputed.Keys)
        {
            
            if (cacheData.Equals(m_currentWidth, m_currentHeight, playerHight))
                return m_cacheDataComputed[cacheData];
        }
        Debug.Log("0: " + m_cacheDataComputed.Count);

        Texture2D texture = PrepareScreenshot(false);

        int enemyLength = m_currentWidth * m_currentHeight;
        int playerLength = GetInputLayerLengthPlayer(m_currentWidth, playerHight);

        int dataLength = enemyLength + playerLength;
        //Debug.Log("E: " + enemyLength);
        //Debug.Log("P: " + playerLenth);
        //Debug.Log("Tw: " + texture.width + ", Th: " + texture.height);
        float[] data = new float[dataLength];

        for (int height = 0; height < texture.height; height++)
        {
            for (int width = 0; width < texture.width; width++)
            {
                int index = height * texture.width + width;
                float value = 0;

                Color color = texture.GetPixel(width, height);
                
                if (color.r > m_activisionThreshold) // Red = Obstacle
                {
                    value = color.r;
                }
                else if (index < playerLength && color.g > m_activisionThreshold) // Green = Player / Ai
                {
                    value = color.g;
                    index += enemyLength;
                }

                //try
                //{
                    data[index] = value;
                //}
                //catch (System.Exception e)
                //{

                //    Debug.Log("i: " + index);
                //}
            }
        }

        if(show)
            ShowScreenshot();
        SaveFile();

        m_cacheDataComputed.Add(new CacheData { captureWidth = m_captureWidth, captureHeight = m_captureHeight, playerHight = playerHight }, data);// new CacheData { captureWidth = captureWidth, captureHeight = captureHeight, playerHight = playerHight }; ;
        return data;
    }
    public float[][] GetScreenshotDataRaw(int captureWidth, int captureHeight, bool forceHdr, bool show)
    {
        //CacheData cacheData = new CacheData { captureWidth = captureWidth, captureHeight = captureHeight };
        

        m_currentWidth = captureWidth == 0 ? GetCaptureWidth() : captureWidth;
        m_currentHeight = captureHeight == 0 ? GetCaptureHeight() : captureHeight;

        Debug.Log("1: " + m_cacheDataRaw.Count);
        foreach (CacheData cacheData in m_cacheDataRaw.Keys)
        {

            if (cacheData.Equals(m_currentWidth, m_currentHeight, 0))
                return m_cacheDataRaw[cacheData];
        }

        Texture2D texture = PrepareScreenshot(forceHdr);
        
        float[][] data = new float[m_currentWidth][];
        for (int x = 0; x < texture.width; x++)
        {
            float[] dataY = new float[m_currentHeight];
            for (int y = 0; y < texture.height; y++)
            {
                float value = 0;

                Color color = texture.GetPixel(x, y);
                // Green = Player / Ai
                // Red = Obstacle
                if (color.r > m_activisionThreshold)
                {
                    value = -color.r;
                }
                else if (color.g > m_activisionThreshold)
                {
                    value = color.g;
                }

                dataY[y] = value;
            }
            data[x] = dataY;
        }

        if (show)
            ShowScreenshot();
        SaveFile();

        m_cacheDataRaw.Add(new CacheData { captureWidth = m_captureWidth, captureHeight = m_captureHeight, playerHight = 0 }, data);
        return data;
    }
    #endregion

    #region Screenshot Generation
    Texture2D PrepareScreenshot(bool forceHdr)
    {
        m_rect = new Rect(0, 0, m_currentWidth, m_currentHeight);
        m_renderTexture = new RenderTexture(m_currentWidth, m_currentHeight, 24);
        m_screenshotTexture = new Texture2D(m_currentWidth, m_currentHeight, TextureFormat.RGB24, false);
        m_screenshotTexture.filterMode = FilterMode.Point;

        //Debug.Log(GetObstacleScale(m_spawner.GetSpawnPrefab().transform.localScale, m_currentWidth));
        m_spawner.setScaleOfActiveObstacles(GetObstacleScale(m_spawner.GetSpawnPrefab().transform.localScale, m_currentWidth));
        SetCaptureSizesPlayer(m_currentWidth);


        m_camera.targetTexture = m_renderTexture;
        m_camera.allowHDR = forceHdr ? true : m_allowHDR;
        m_camera.Render();
        RenderTexture.active = m_renderTexture;
        m_screenshotTexture.ReadPixels(m_rect, 0, 0);
        m_camera.targetTexture = null;
        RenderTexture.active = null;

        Destroy(m_renderTexture);

        m_spawner.setScaleOfActiveObstacles(GetObstacleScale(m_spawner.GetSpawnPrefab().transform.localScale, 0));
        SetCaptureSizesPlayer(0);

        return m_screenshotTexture;
    }
    void ShowScreenshot()
    {
        if (m_rawImage != null)
        {
            //if (m_showScreenshot)
            //{
                m_rawImage.enabled = true;
                m_screenshotTexture.Apply();
                m_rawImage.texture = m_screenshotTexture;
            //}
            //else
            //    m_rawImage.enabled = false;
        }
    }
    void SaveFile()
    {
        if (!m_saveFile)
            return;

        string fileName = getFileName();
        byte[] fileHeader = null;
        byte[] fileData = null;
        if (m_format == Format.RAW)
        {
            fileData = m_screenshotTexture.GetRawTextureData();
        }
        else if (m_format == Format.PNG)
        {
            fileData = m_screenshotTexture.EncodeToPNG();
        }
        else if (m_format == Format.JPG)
        {
            fileData = m_screenshotTexture.EncodeToJPG();
        }
        else if (m_format == Format.PPM)
        {
            // create a file header for ppm formatted file
            string headerStr = string.Format("P6\n{0} {1}\n255\n", m_rect.width, m_rect.height);
            fileHeader = System.Text.Encoding.ASCII.GetBytes(headerStr);
            fileData = m_screenshotTexture.GetRawTextureData();
        }

        new System.Threading.Thread(() =>
        {
            // create file and write optional header with image bytes
            FileStream f = System.IO.File.Create(fileName);
            if (fileHeader != null) f.Write(fileHeader, 0, fileHeader.Length);
            f.Write(fileData, 0, fileData.Length);
            f.Close();
            Debug.Log(string.Format("Wrote screenshot {0} of size {1}", fileName, fileData.Length));
        }).Start();
    }
    #endregion

    #region Screenshot Size
    public float GetPixelToWorldScale(int captureWidth)
    {
        return m_captureAreaSize.x / (captureWidth == 0 ? GetCaptureWidth() : captureWidth);
    }

    public void SetCaptureSize(/*int captureWidth, int captureHeight*/)
    {
        float ratio = (float)GetCaptureWidth() / (float)GetCaptureHeight();

        // change capture area size
        float finalCaptureAreaWidth = -1;
        finalCaptureAreaWidth = GetBackgroundHeight() * ratio;
        m_captureAreaSize = new Vector3(finalCaptureAreaWidth, GetBackgroundHeight(), 1);
        m_pixelWorldScale = GetPixelToWorldScale(0);

        if (m_captureAreas == null)
            return;

        for (int i = 0; i < m_captureAreas.Count; i++)
        {
            m_captureAreas[i].localScale = m_captureAreaSize;
        }
    }
    public Vector3 GetObstacleScale(Vector3 originalScale, int captureWidth)
    {
        if (!m_changeObstacleScales)
        {
            return originalScale;
        }

        if (originalScale == Vector3.zero)
            return originalScale;

        if (originalScale.x != originalScale.y)
            Debug.Log("Warning: localScale components weren't the same (" + originalScale.x + "/" + originalScale.y + "). Taking the x-component.");

        float finalCaptureSize = originalScale.x;
        float pixelWorldScale = GetPixelToWorldScale(captureWidth);
        if (originalScale.x < pixelWorldScale)
            finalCaptureSize = pixelWorldScale;

        return new Vector3(finalCaptureSize, finalCaptureSize, finalCaptureSize);
    }
    public void SetCaptureSizesPlayer(int captureWidth)
    {
        for (int i = 0; i < m_playerVisualsCapture.Count; i++)
        {
            Vector3 scale = GetObstacleScale(m_defaultScale, captureWidth);
            if (scale == Vector3.zero)
                continue;

            m_playerVisualsCapture[i].localScale = scale;
        }
    }
    #endregion

    #region Input Length
    public int GetInputLayerLengthTotal(int captureWidth, float playerHeight)
    {
        captureWidth = captureWidth == 0 ? GetCaptureWidth() : captureWidth;
        playerHeight = playerHeight == 0 ? GetPlayerHight() : playerHeight;
        //Debug.Log("0: " + (GetInputLayerLengthEnemy(captureWidth) + GetInputLayerLengthPlayer(captureWidth)));
        return GetInputLayerLengthEnemy(captureWidth) + GetInputLayerLengthPlayer(captureWidth, playerHeight);
    }
    public int GetInputLayerLengthEnemy(int captureWidth)
    {
        captureWidth = captureWidth == 0 ? GetCaptureWidth() : captureWidth;
        //Debug.Log("1: " + captureWidth * GetCaptureHeight());
        return captureWidth * GetCaptureHeight();
    }
    public int GetInputLayerLengthPlayer(int captureWidth, float playerhight)
    {
        float playerHeight = -1;
        //if (m_playerVisualsCapture != null && m_playerVisualsCapture.Count != 0)
            playerHeight = playerhight;// m_playerVisualsCapture[0].localScale.y;
        //else
        //    Debug.Log("Warning: No player visual capture transform found!");
        
        float pixelSize = GetPixelToWorldScale(captureWidth);

        int height = (int)(playerHeight / pixelSize);
        if (playerHeight != pixelSize)
            height += 1;

        //Debug.Log("2: " + (captureWidth == 0 ? GetCaptureWidth() : captureWidth) + " * " + height + " = " + (captureWidth == 0 ? GetCaptureWidth() : captureWidth) * height);
        return (captureWidth == 0 ? GetCaptureWidth() : captureWidth) * height;
    }
    #endregion

    #region Misc
    string getFileName()
    {
        string directory;
        string fileName;

        // set default folder name
        if (m_folderName == null || m_folderName.Length == 0)
            directory = "screenshots";
        else
            directory = m_folderName;

        // set default file name
        if (m_fileName == null || m_fileName.Length == 0)
            fileName = "screenshot";
        else
            fileName = m_fileName;

        if (Application.isEditor)
            directory = Path.GetFullPath(Application.dataPath + "/../" + directory);
        else
            directory = Application.dataPath + "/" + directory;


        System.IO.Directory.CreateDirectory(directory);

        fileName = string.Format("{0}/{1}_{2}x{3}_{4}.{5}", directory, fileName, m_currentWidth, m_currentHeight, m_fileNameCounter, m_format.ToString().ToLower());

        m_fileNameCounter++;

        return fileName;
    }
    void getInput()
    {
        m_isPressingScreenshot = Input.GetKeyDown(m_keyCodeScreenshot);
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        bool change = m_lastCaptureHeight != GetCaptureHeight() && GetCaptureHeight() > 0;
        change |= m_lastCaptureWidth != GetCaptureWidth() && GetCaptureWidth() > 0;
        change |= m_lastBackgroundHeight != m_backgroundHeight && m_backgroundHeight > 0;

        if (!change)
            return;

        SetCaptureSize(/*GetCaptureWidth(), GetCaptureHeight()*/);

        m_lastCaptureWidth = GetCaptureWidth();
        m_lastCaptureHeight = GetCaptureHeight();
    }

    #endregion

    #region Save / Load
    public NNTSSaveData SaveData()
    {
        NNTSSaveData data = new NNTSSaveData
        {
            m_captureWidth         = m_captureWidth,
            m_captureHeight        = m_captureHeight,
            m_backgroundHeight     = m_backgroundHeight,
            m_changeObstacleScales = m_changeObstacleScales,
                                   
            m_format               = m_format,
            m_allowHDR             = m_allowHDR,
                                  
            m_activisionThreshold  = m_activisionThreshold
        };

        return data;
    }
    public void LoadData(NNTSSaveData data)
    {
        m_captureWidth = data.m_captureWidth;
        m_captureHeight = data.m_captureHeight;
        m_backgroundHeight = data.m_backgroundHeight;
        m_changeObstacleScales = data.m_changeObstacleScales;

        m_format = data.m_format;
        m_allowHDR = data.m_allowHDR;

        m_activisionThreshold = data.m_activisionThreshold;

    }
    public void ApplyData()
    {
        SetCaptureSize();
        SetCaptureSizesPlayer(0);
    }
    #endregion


    #region Setter
    //public void SetSampleManager(SampleManager manager)
    //{
    //    m_sampleManager = manager;
    //}
    #endregion

    #region Getter
    public int GetCaptureWidth()
    {
        if (m_captureWidth <= 0)
            return m_screenshotManager.GetCaptureWidth();
        return m_captureWidth;
    }
    public int GetCaptureHeight()
    {
        if (m_captureHeight <= 0)
            return m_screenshotManager.GetCaptureHeight();
        return m_captureHeight;
    }
    public float GetBackgroundHeight()
    {
        if (m_backgroundHeight <= 0)
            return m_screenshotManager.GetBackgroundHeight();
        return m_backgroundHeight;
    }
    public bool IsBase()
    {
        bool isBase = m_backgroundHeight <= 0;
        isBase |= m_captureWidth <= 0;
        isBase |= m_captureHeight <= 0;

        return isBase;
    }
    public float GetPlayerHight()
    {
        return (m_playerVisualsCapture != null && m_playerVisualsCapture.Count != 0) ? m_playerVisualsCapture[0].localScale.y : 0;
    }
    #endregion
}
