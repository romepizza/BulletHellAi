using System.Collections;
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
    public float playerHeight;
    public AnimationCurve curveWidth;
    public AnimationCurve curveHeight;
    public TakeScreenshot.CaptureType captureType;

    public float[] dataComputed;
    //public float[][] dataRaw;

    public bool Equals(int captureWidth, int captureHeight, float playerHeight, TakeScreenshot.CaptureType captureType, AnimationCurve curveWidth, AnimationCurve curveHeight)
    {
        if (this.captureWidth > captureWidth * 1.0001f || this.captureWidth < captureWidth * 0.9999f)
            return false;
        if (this.captureHeight > captureHeight * 1.0001f || this.captureHeight < captureHeight * 0.9999f)
            return false;
        if (this.playerHeight > playerHeight * 1.0001f || this.playerHeight < playerHeight * 0.9999f)
            return false;
        if(this.captureType != captureType)
            return false;

        for (int i = 0; i < curveWidth.keys.Length; i++)
        {
            if (!this.curveWidth.keys[i].Equals(curveWidth.keys[i]))
                return false;
        }
        for (int i = 0; i < curveHeight.keys.Length; i++)
        {
            if (!this.curveHeight.keys[i].Equals(curveHeight.keys[i]))
                return false;
        }

        return true;
    }
}
public struct CacheScreenshot
{
    public Texture2D screenshot;

    public int width;
    public int height;
    public bool forceHdr;
    public int cullingMask;

    public bool Equals(int width, int height, bool forceHdr, int cullingMask)
    {
        if (this.width != width)
            return false;
        if(this.height != height)
            return false;
        if(this.forceHdr != forceHdr)
            return false;
        if(this.cullingMask != cullingMask)
            return false;

        return true;
    }
}

public class TakeScreenshot : MonoBehaviour
{
    #region Member Variables
    [Header("------- Settings -------")]
    [SerializeField] private Camera m_camera;

    [Header("--- Pixel Size ---")]
    [SerializeField] private int m_captureWidth;
    [SerializeField] private int m_captureHeight;
    [SerializeField] private float m_backgroundHeight;
    [SerializeField] private bool m_changeObstacleScales = true;

    [Header("--- Format ---")]
    [SerializeField] private Format m_format;
    [SerializeField] private bool m_allowHDR;

    [Header("--- Folder ---")]
    [SerializeField] private string m_fileName;
    [SerializeField] private string m_folderName;

    [Header("--- Misc ---")]
    [SerializeField] private float m_showCooldown;
    [SerializeField] private float m_activisionThreshold;
    [SerializeField] private KeyCode m_keyCodeScreenshot;
    [SerializeField] private bool m_saveFile;
    //[SerializeField] private bool m_showScreenshot;

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

    // cache
    private List<CacheData> m_cacheDataComputed = new List<CacheData>();
    //private List<CacheData> m_cacheDataRaw = new List<CacheData>();
    private List<CacheScreenshot> m_cacheScrenshots = new List<CacheScreenshot>();
    private List<int> m_obstacleWidths = new List<int>();
    private List<int> m_obstacleIndices = new List<int>();

    private int m_lastCaptureWidth;
    private int m_lastCaptureHeight;
    private float m_lastBackgroundHeight;
    private float m_pixelWorldScale;
    private Vector3 m_captureAreaSize;
    private Vector3 m_defaultScale;
    private float m_showCooldownRdy;
    #endregion

    #region Enums
    public enum Format { RAW, PNG, JPG, PPM }
    public enum CaptureType { Separate, Raw }
    #endregion

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
        ShowScreenshot(PrepareScreenshot(0, 0, false, 0));
    }
    private void LateUpdate()
    {
        //TakeScreenshotManually(false);
        m_cacheDataComputed.Clear();
        m_cacheScrenshots.Clear();
    }
    #endregion Monobehaviour

    #region Screenshot Control
    //public void TakeScreenshotManually(bool forceScreenshot)
    //{
    //    if (!m_isPressingScreenshot && !forceScreenshot)
    //        return;

    //    ShowScreenshot(PrepareScreenshot(false, 0));
    //    SaveFile();
    //}
    public float[] GetScreenshotDataComputed(int captureWidth, int captureHeight, float playerHeight, CaptureType captureType, AnimationCurve curveWidth, AnimationCurve curveHeight)
    {
        m_currentWidth = captureWidth == 0 ? GetCaptureWidth() : captureWidth;
        m_currentHeight = captureHeight == 0 ? GetCaptureHeight() : captureHeight;
        playerHeight = playerHeight == 0 ? GetPlayerHeight() : playerHeight;

        // check for cache data
        foreach (CacheData cacheData in m_cacheDataComputed)
        {
            if (cacheData.Equals(m_currentWidth, m_currentHeight, playerHeight, captureType, curveWidth, curveHeight))
            {
                return cacheData.dataComputed;
            }
        }

        // take screenshots
        Texture2D textureObstacles = null;
        Texture2D texturePlayer = null;
        if (captureType == CaptureType.Separate)
        {
            texturePlayer = PrepareScreenshot(m_currentWidth, m_currentHeight, false, 1);
            textureObstacles = PrepareScreenshot(m_currentWidth, m_currentHeight, false, 2);
        }
        else if(captureType == CaptureType.Raw)
            textureObstacles = PrepareScreenshot(m_currentWidth, m_currentHeight, false, 0);

        // create data to return
        int enemyLength = m_currentWidth * m_currentHeight;
        int playerLength = GetInputLayerLengthPlayer(m_currentWidth, playerHeight);
        int dataLength = enemyLength + playerLength;
        float[] data = new float[dataLength];

        // if animation curve is used, memorize position of the player
        bool useCurveWidth = curveWidth != null;
        if (useCurveWidth && curveWidth.keys[0].value == 1 && curveWidth.keys[curveWidth.keys.Length - 1].value == 1)
            useCurveWidth = false;
        bool useCurveHeight = curveHeight != null;
        if (useCurveHeight && curveHeight.keys[0].value == 1 && curveHeight.keys[curveHeight.keys.Length - 1].value == 1)
            useCurveHeight = false;
        // these are only in use if the width is in use
        float playerPositionX = 0;
        int playerPositionCounter = 0;
        m_obstacleWidths.Clear();
        m_obstacleIndices.Clear();

        // identify positions of obstacles and player
        for (int height = 0; height < textureObstacles.height; height++)
        {
            for (int width = 0; width < textureObstacles.width; width++)
            {
                int index = height * textureObstacles.width + width;
                float value = 0;

                Color colorObstacle = textureObstacles.GetPixel(width, height);
                Color colorPlayer = colorObstacle;
                if (captureType == CaptureType.Separate)
                    colorPlayer = texturePlayer.GetPixel(width, height);

                if (colorObstacle.r > m_activisionThreshold) // Red = Obstacle
                {
                    float heightFactor = useCurveHeight ? EvaluateCurveHeight(height, textureObstacles.height, curveHeight) : 1;
                    value = 1f * heightFactor;
                    data[index] = value;

                    if (useCurveWidth)
                    {
                        m_obstacleWidths.Add(width);
                        m_obstacleIndices.Add(width);
                    }
                }
                if (index < playerLength && colorPlayer.g > m_activisionThreshold) // Green = Player / Ai
                {
                    value = 1;
                    index += enemyLength;
                    data[index] = value;

                    if (useCurveWidth)
                    {
                        playerPositionX += width;
                        playerPositionCounter++;
                    }
                }
            }
        }

        // alter the values of obstacles by the width curve
        if (useCurveWidth)
        {
            if (playerPositionCounter != 0)
            {
                playerPositionX /= playerPositionCounter;
                for(int i = 0; i < m_obstacleIndices.Count; i++)
                {
                    float widthFactor = EvaluateCurveWidth(m_obstacleWidths[i], playerPositionX, textureObstacles.width, curveWidth);
                    data[m_obstacleIndices[i]] *= widthFactor;
                }
            }
            else
                Debug.Log("Warning: playerPositionCounter is 0!");
        }

        // add cache data
        m_cacheDataComputed.Add(new CacheData {
            captureWidth = m_currentWidth,
            captureHeight = m_currentHeight,
            playerHeight = playerHeight,
            curveWidth = curveWidth,
            curveHeight = curveHeight,
            captureType = captureType,
            dataComputed = data
        });
        return data;
    }
    private float EvaluateCurveWidth(int obstacleIndex, float playerIndex, int maxWidth, AnimationCurve curve)
    {
        if (maxWidth == 0)
            return 1f;

        float difference = Mathf.Abs(obstacleIndex - playerIndex);
        return curve.Evaluate(difference / maxWidth);
    }
    private float EvaluateCurveHeight(float obstacleIndex, float maxIndex, AnimationCurve curve)
    {
        if(maxIndex == 0)
        {
            Debug.Log("Warning: maxIndex was 0!");
            return 1f;
        }

        return curve.Evaluate(obstacleIndex / maxIndex);
    }
    //public float[][] GetScreenshotDataRaw(int captureWidth, int captureHeight, bool forceHdr, bool show)
    //{
    //    m_currentWidth = captureWidth == 0 ? GetCaptureWidth() : captureWidth;
    //    m_currentHeight = captureHeight == 0 ? GetCaptureHeight() : captureHeight;

    //    foreach (CacheData cacheData in m_cacheDataRaw)
    //    {
    //        if (cacheData.Equals(m_currentWidth, m_currentHeight, 0))
    //        {
    //            return cacheData.dataRaw;
    //        }
    //    }

    //    Texture2D textureObstacles = PrepareScreenshot(forceHdr, 0);

    //    float[][] data = new float[m_currentWidth][];
    //    for (int x = 0; x < textureObstacles.width; x++)
    //    {
    //        float[] dataY = new float[m_currentHeight];
    //        for (int y = 0; y < textureObstacles.height; y++)
    //        {
    //            float value = 0;

    //            Color colorObstacle = textureObstacles.GetPixel(x, y);

    //            // Green = Player / Ai
    //            // Red = Obstacle
    //            if (colorObstacle.r > m_activisionThreshold)
    //            {
    //                value = -colorObstacle.r;
    //            }
    //            else if (colorObstacle.g > m_activisionThreshold)
    //            {
    //                value = colorObstacle.g;
    //            }

    //            dataY[y] = value;
    //        }
    //        data[x] = dataY;
    //    }

    //    if (show)
    //        ShowScreenshot(textureObstacles);
    //    SaveFile();

    //    //Destroy(textureObstacles);

    //    m_cacheDataRaw.Add(new CacheData { captureWidth = m_currentWidth, captureHeight = m_currentHeight, playerHeight = 0, dataRaw = data });
    //    return data;
    //}
    #endregion

    #region Screenshot Generation
    // cullingMask == 0: take screenshot raw
    // cullingMask == 1: take screenshot of player only
    // cullingMask == 2: take screenshot of obstacles only
    Texture2D PrepareScreenshot(int width, int height, bool forceHdr, int cullingMask)
    {
        width = width == 0 ? GetCaptureWidth() : width;
        height = height == 0 ? GetCaptureHeight() : height;

        foreach (CacheScreenshot screenshot in m_cacheScrenshots)
        {
            if (screenshot.Equals(width, height, forceHdr, cullingMask))
            {
                return screenshot.screenshot;
            }
        }

        m_rect = new Rect(0, 0, width, height);
        m_renderTexture = new RenderTexture(width, height, 24);
        m_screenshotTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
        m_screenshotTexture.filterMode = FilterMode.Point;

        m_spawner.SetScaleOfActiveObstacles(GetObstacleScale(m_spawner.GetSpawnPrefab().transform.localScale, width));
        SetCaptureSizesPlayer(width);

        int mask = 0;
        if (cullingMask == 0)
            mask = Utility.ExpInt(2, Statics.s_captureCameraLayerPlayer) | Utility.ExpInt(2, Statics.s_captureCameraLayerObstacle);
        else if (cullingMask == 1)
            mask = Utility.ExpInt(2, Statics.s_captureCameraLayerPlayer);
        else if (cullingMask == 2)
            mask = Utility.ExpInt(2, Statics.s_captureCameraLayerObstacle);
        else
            Debug.Log("Oops!");

        m_camera.cullingMask = mask;

        m_camera.targetTexture = m_renderTexture;
        m_camera.allowHDR = forceHdr ? true : m_allowHDR;
        m_camera.Render();
        RenderTexture.active = m_renderTexture;
        m_screenshotTexture.ReadPixels(m_rect, 0, 0);
        m_camera.targetTexture = null;
        RenderTexture.active = null;

        Destroy(m_renderTexture);

        m_spawner.SetScaleOfActiveObstacles(GetObstacleScale(m_spawner.GetSpawnPrefab().transform.localScale, 0));
        SetCaptureSizesPlayer(0);

        m_cacheScrenshots.Add(new CacheScreenshot { width = width, height = height, forceHdr = forceHdr, cullingMask = cullingMask, screenshot = m_screenshotTexture });
        return m_screenshotTexture;
    }
    void ShowScreenshot(Texture2D screenshot)
    {
        if (m_rawImage == null)
            return;

        if (m_showCooldownRdy > Time.time)
            return;
        m_showCooldownRdy = Time.time + m_showCooldown;

        m_rawImage.enabled = true;
        screenshot.Apply();
        m_rawImage.texture = screenshot;
        
    }
    void SaveFile(Texture2D texture)
    {
        if (!m_saveFile)
            return;

        string fileName = getFileName();
        byte[] fileHeader = null;
        byte[] fileData = null;
        if (m_format == Format.RAW)
        {
            fileData = texture.GetRawTextureData();
        }
        else if (m_format == Format.PNG)
        {
            fileData = texture.EncodeToPNG();
        }
        else if (m_format == Format.JPG)
        {
            fileData = texture.EncodeToJPG();
        }
        else if (m_format == Format.PPM)
        {
            // create a file header for ppm formatted file
            string headerStr = string.Format("P6\n{0} {1}\n255\n", m_rect.width, m_rect.height);
            fileHeader = System.Text.Encoding.ASCII.GetBytes(headerStr);
            fileData = texture.GetRawTextureData();
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
        playerHeight = playerHeight == 0 ? GetPlayerHeight() : playerHeight;
        //Debug.Log("0: " + (GetInputLayerLengthEnemy(captureWidth) + GetInputLayerLengthPlayer(captureWidth)));
        return GetInputLayerLengthEnemy(captureWidth, 0) + GetInputLayerLengthPlayer(captureWidth, playerHeight);
    }
    public int GetInputLayerLengthEnemy(int captureWidth, int captureHeight)
    {
        captureWidth = captureWidth == 0 ? GetCaptureWidth() : captureWidth;
        captureHeight = captureHeight == 0 ? GetCaptureHeight() : captureHeight;
        //Debug.Log("1: " + captureWidth * GetCaptureHeight());
        return captureWidth * captureHeight;
    }
    public int GetInputLayerLengthPlayer(int captureWidth, float playerHeight)
    {
        m_currentWidth = captureWidth == 0 ? GetCaptureWidth() : captureWidth;
        playerHeight = playerHeight == 0 ? GetPlayerHeight() : playerHeight;

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
        m_cacheDataComputed.Clear();
        m_cacheScrenshots.Clear();
    }
    #endregion

    #region Setter
    //public void SetSampleManager(SampleManager manager)
    //{
    //    m_sampleManager = manager;
    //}
    public void SetCaptureWidth(int width)
    {
        m_captureWidth = width;
    }
    public void SetCaptureHeight(int height)
    {
        m_captureHeight = height;
    }
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
    public float GetPlayerHeight()
    {
        return (m_playerVisualsCapture != null && m_playerVisualsCapture.Count != 0) ? m_playerVisualsCapture[0].localScale.y : 0;
    }
    #endregion
}
