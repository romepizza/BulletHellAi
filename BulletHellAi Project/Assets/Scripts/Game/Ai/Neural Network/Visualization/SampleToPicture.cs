using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SampleToPicture : MonoBehaviour
{
    #region Member Variables
    [Header("----- Settings -----")]

    [Header("--- Key Bindings ---")]
    [SerializeField] private KeyCode m_keyCodeNextSample;
    [SerializeField] private KeyCode m_keyCodePrevSample;
    [SerializeField] private KeyCode m_keyCodeReset;
    [SerializeField] private KeyCode m_keyCodeToggle;
    [SerializeField] private KeyCode m_keyCodeRandom;
    [SerializeField] private float m_pressTime;
    [SerializeField] private float m_pressCooldown;

    [Header("--- Objects ---")]
    [SerializeField] private TextAsset m_sourceSamples;
    [SerializeField] private RawImage m_image;
    [SerializeField] private ScreenshotManager m_screenshotManager;
    [SerializeField] private List<GameObject> m_toggleObjects;

    [Header("----- Debug -----")]
    private bool m_isActive;

    private bool m_isPressingNext;
    private bool m_isPressingPrev;
    private bool m_isPressingReset;
    private bool m_isPressingToggle;
    private bool m_isPressingRandom;

    private List<SampleContainer> m_samples;
    private int m_currentSampleIndex;

    private Rect m_rect;
    private RenderTexture m_renderTexture;
    private Texture2D m_screenshotTexture;

    private float m_pressCooldownRdy;
    private float m_holdingTimeNext;
    private float m_holdingTimePrev;
    private float m_holdingTimeRandom;
    #endregion


    #region Mono
    private void Awake()
    {
        if (m_toggleObjects == null)
            m_toggleObjects = new List<GameObject>();
    }
    void Start ()
    {
        if(m_sourceSamples != null)
            m_samples = (SampleSaveManager.LoadSampleData(m_sourceSamples)).ToSampleContainers();
    }
	void Update ()
    {
        ManageInput();
        ManagePressing();
    }
    #endregion

    #region Control
    void ManagePressing()
    {
        if (m_isPressingToggle)
        {
            Toggle();
        }

        if (!m_isActive)
            return;

        if (m_isPressingNext || m_holdingTimeNext > m_pressTime && m_pressCooldownRdy < Time.time)
        {
            m_currentSampleIndex = m_currentSampleIndex + 1 >= m_samples.Count ? 0 : m_currentSampleIndex + 1;
            m_pressCooldownRdy = Time.time + m_pressCooldown;
            Render();
        }
        if (m_isPressingPrev || m_holdingTimePrev > m_pressTime && m_pressCooldownRdy < Time.time)
        {
            m_currentSampleIndex = m_currentSampleIndex - 1 < 0 ? m_samples.Count - 1 : m_currentSampleIndex - 1;
            m_pressCooldownRdy = Time.time + m_pressCooldown;
            Render();
        }
        if (m_isPressingReset)
        {
            m_currentSampleIndex = 0;
            Render();
        }
        if(m_isPressingRandom || m_holdingTimeRandom > m_pressTime && m_pressCooldownRdy < Time.time)
        {
            m_currentSampleIndex = Random.Range(0, m_samples.Count);
            m_pressCooldownRdy = Time.time + m_pressCooldown;
            Render();
        }
    }
    void Toggle()
    {
        if (m_isActive)
        {
            m_image.enabled = false;
            foreach (GameObject o in m_toggleObjects)
                o.SetActive(true);
            m_isActive = false;
        }
        else
        {
            m_image.enabled = true;
            foreach (GameObject o in m_toggleObjects)
                o.SetActive(false);
            m_isActive = true;
            Render();
        }
    }
    void Render()
    {
        SampleContainer sample = m_samples[m_currentSampleIndex];
        int renderWidth = sample.m_width;
        int renderHeight = sample.m_height + 1;
        

        float pixelSize = m_screenshotManager.GetBackgroundHeight() * m_screenshotManager.GetCaptureWidth() / m_screenshotManager.GetCaptureHeight() / renderWidth;
        int height = (int)(m_screenshotManager.GetPlayerHeight() / pixelSize);
        if (m_screenshotManager.GetPlayerHeight() != pixelSize)
            height += 1;
        int playerLength = renderWidth * height;


        //m_rect = new Rect(0, 0, renderWidth, renderHeight);
        //m_renderTexture = new RenderTexture(renderWidth, renderHeight, 24);
        //m_screenshotTexture = m_image.texture as Texture2D;// new Texture2D(renderWidth, renderHeight, TextureFormat.RGB24, false);
        Destroy(m_image.texture);
        Destroy(m_screenshotTexture);
        m_screenshotTexture = new Texture2D(renderWidth, renderHeight, TextureFormat.RGB24, false);
        m_screenshotTexture.filterMode = FilterMode.Point;

        // input
        for(int h = 0; h < sample.m_height; h++)
        {
            for (int w = 0; w < sample.m_width; w++)
            {
                int index = w + h * sample.m_width;

                float obstacleInput = Mathf.Clamp01(sample.m_input[index]);
                int playerIndex = index + sample.m_width * sample.m_height;
                float playerInput = playerIndex >= sample.m_input.Length ? 0f : Mathf.Clamp01(sample.m_input[playerIndex]);
                if (index < playerLength && playerInput > 0)
                {
                    m_screenshotTexture.SetPixel(w, h, new Color(0, playerInput, 0));
                }
                else if (obstacleInput > 0)
                {
                    m_screenshotTexture.SetPixel(w, h, new Color(obstacleInput, 0, 0));
                }
                else
                    m_screenshotTexture.SetPixel(w, h, new Color(0, 0, 0));
            }
        }

        // desired output
        for(int i = 0; i < sample.m_desiredOutput.Length; i++)
        {
            int startIndex =(int)((float)sample.m_width * i / (sample.m_desiredOutput.Length));
            int endIndex = (int)Mathf.Min(startIndex + (float)sample.m_width / sample.m_desiredOutput.Length, sample.m_width - 1);


            for (int j = startIndex; j <= endIndex; j++)
            {
                Color c = new Color();
                if (i == 0)
                    c.g = sample.m_desiredOutput[0];
                if (i == 1)
                    c.r = sample.m_desiredOutput[2];
                if (i == 2)
                    c.b = sample.m_desiredOutput[1];
                m_screenshotTexture.SetPixel(j, renderHeight - 1, c);// new Color(sample.m_desiredOutput[0] * 255, sample.m_desiredOutput[1] * 255, sample.m_desiredOutput[2] * 255));
            }
        }
        

        // apply
        m_screenshotTexture.Apply();
        m_image.texture = m_screenshotTexture;
    }
    #endregion

    #region Input
    void ManageInput()
    {
        m_isPressingNext = Input.GetKeyDown(m_keyCodeNextSample);
        m_isPressingPrev = Input.GetKeyDown(m_keyCodePrevSample);
        m_isPressingReset = Input.GetKeyDown(m_keyCodeReset);
        m_isPressingToggle = Input.GetKeyDown(m_keyCodeToggle);
        m_isPressingRandom = Input.GetKeyDown(m_keyCodeRandom);

        if (Input.GetKey(m_keyCodeNextSample))
            m_holdingTimeNext += Time.deltaTime;
        else
            m_holdingTimeNext = 0;

        if (Input.GetKey(m_keyCodePrevSample))
            m_holdingTimePrev += Time.deltaTime;
        else
            m_holdingTimePrev = 0;

        if (Input.GetKey(m_keyCodeRandom))
            m_holdingTimeRandom += Time.deltaTime;
        else
            m_holdingTimeRandom = 0;
    }
    #endregion
}
