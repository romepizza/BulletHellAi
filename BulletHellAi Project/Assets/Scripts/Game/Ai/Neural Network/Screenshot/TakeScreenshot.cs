﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class TakeScreenshot : MonoBehaviour {

    [Header("------- Settings -------")]
    [SerializeField] private Camera m_camera;

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

    [Header("------- Debug -------")]
    private ScreenshotManager m_screenshotManager;
    private RenderTexture m_renderTexture;
    private bool m_isPressingScreenshot;
    private int m_fileNameCounter;
    private Rect m_rect;
    private Texture2D m_screenshotTexture;
    private int m_currentHeight;
    private int m_currentWidth;

    public enum Format { RAW, PNG, JPG, PPM }

    #region Monobehaviour
    // Use this for initialization
    void Start()
    {
        InitializeStuff();
    }
    void InitializeStuff()
    {
        if (m_screenshotManager == null)
            m_screenshotManager = ScreenshotManager.Instance();

        Vector2Int size = m_screenshotManager.GetScreenshotSize();
        m_currentHeight = size.y;
        m_currentWidth = size.x;

        #region CAN_BE_DELETED
        // TODO
        if (m_renderTexture == null)
        {
            //Debug.Log("Info: RenderTexture created automaticly!");
            m_renderTexture = new RenderTexture(m_currentWidth, m_currentHeight, 24);
        }
        m_screenshotTexture = new Texture2D(m_currentWidth, m_currentHeight, TextureFormat.RGB24, false);
        m_camera.targetTexture = m_renderTexture;
        RenderTexture.active = m_renderTexture;
        #endregion

        m_rect = new Rect(0, 0, m_currentWidth, m_currentHeight);
    }
    // Update is called once per frame
    void Update()
    {
        getInput();
    }
    private void LateUpdate()
    {
        PerformTakeScreenshot(false);
    }
    #endregion Monobehaviour

    #region Screenshot Control
    public void PerformTakeScreenshot(bool forceScreenshot)
    {
        if (!m_isPressingScreenshot && !forceScreenshot)
            return;

        PrepareScreenshot();
        ShowScreenshot();
        SaveFile();
    }
    public float[] GetScreenshotComputedData()
    {
        Texture2D texture = PrepareScreenshot();

        int dataLength = m_currentHeight * m_currentWidth;
        float[] data = new float[dataLength];

        for(int height = 0; height < texture.height; height++)
        {
            for (int width = 0; width < texture.width; width++)
            {
                int index = height * texture.width + width;
                float value = 0;

                Color color = texture.GetPixel(width, height);
                // Green = Player / Ai
                // Red = Obstacle
                if (color.r > m_activisionThreshold)
                {
                    value = -color.r;
                    // TODO : index = ...
                }
                else if(color.g > m_activisionThreshold)
                {
                    value = color.g;
                    // TODO : index = ...
                }

                data[index] = value;
            }
        }

        ShowScreenshot();
        SaveFile();

        return data;
    }
    Texture2D PrepareScreenshot()
    {
        Vector2Int size = m_screenshotManager.GetScreenshotSize();
        m_currentHeight = size.y;
        m_currentWidth = size.x;

        m_rect.height = m_currentHeight;
        m_rect.width = m_currentWidth;
        m_renderTexture = new RenderTexture(m_currentWidth, m_currentHeight, 24);
        m_screenshotTexture = new Texture2D(m_currentWidth, m_currentHeight, TextureFormat.RGB24, false);

        m_camera.targetTexture = m_renderTexture;
        m_camera.allowHDR = m_allowHDR;
        m_camera.Render();
        RenderTexture.active = m_renderTexture;
        m_screenshotTexture.ReadPixels(m_rect, 0, 0);
        //m_camera.targetTexture = null;
        RenderTexture.active = null;

        return m_screenshotTexture;
    }
    void ShowScreenshot()
    {
        if (m_rawImage != null)
        {
            if (m_showScreenshot)
            {
                m_rawImage.enabled = true;
                m_screenshotTexture.Apply();
                m_rawImage.texture = m_screenshotTexture;
            }
            else
                m_rawImage.enabled = false;
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

    #region Color Control

    #endregion

    #region misc
    string getFileName()
    {
        string folderName;
        string fileName;

        // set default folder name
        if (m_folderName == null || m_folderName.Length == 0)
            folderName = "screenshots";
        else
            folderName = m_folderName;

        // set default file name
        if (m_fileName == null || m_fileName.Length == 0)
            fileName = "screenshot";
        else
            fileName = m_fileName;

        if (Application.isEditor)
            folderName = Path.GetFullPath(Application.dataPath + "/../" + folderName);
        else
            folderName = Application.dataPath + "/" + folderName;


        System.IO.Directory.CreateDirectory(folderName);

        fileName = string.Format("{0}/{1}_{2}x{3}_{4}.{5}", folderName, fileName, m_currentWidth, m_currentHeight, m_fileNameCounter, m_format.ToString().ToLower());

        m_fileNameCounter++;

        return fileName;
    }
    void getInput()
    {
        m_isPressingScreenshot = Input.GetKeyDown(m_keyCodeScreenshot);
    }
    #endregion
}
