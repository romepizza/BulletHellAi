using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SampleGeneration : MonoBehaviour
{
    #region Member Variables
    [Header("----- Editor -----")]
    [SerializeField] private bool m_create;
    [SerializeField] private bool m_concatToExistingFile;

    [Header("----- Settings -----")]
    [SerializeField] private int m_width;
    [SerializeField] private int m_height;

    [Header("--- Distribution ---")]
    [SerializeField] private float m_selfDistribution;

    [Header("--- Seed ---")]
    [SerializeField] private int m_initSeed = -1;

    [Header("--- Objects ---")]
    [SerializeField] private List<SampleGenerationBase> m_generateSimpleData;
    [SerializeField] private TextAsset m_targetAsset;
    [SerializeField] private ScreenshotManager m_sampleManager;

    [Header("----- Debug -----")]
    private int m_currentSeed;
    #endregion

    #region Mono
    public void Awake()
    {
        m_currentSeed = m_initSeed;
    }
    private void Start()
    {
        GenerateSamples();
    }
    private void OnDrawGizmosSelected()
    {
        if (m_create)
        {
            m_create = false;

            List<SampleContainer> data = WriteSamplesToAsset();
            if (data == null || data.Count == 0)
            {
                Debug.Log("No Data has been writen! " + (data == null ? "Data was null." : "Data length was 0. (Pre)"));
                return;
            }

            if (m_concatToExistingFile)
                Concat(data, m_targetAsset);
            else
                WriteSampleData(data, m_targetAsset);
        }
    }
    #endregion

    #region Creation
    private List<SampleContainer> WriteSamplesToAsset()
    {
        if (m_generateSimpleData == null || m_generateSimpleData.Count == 0)
            return null;

        List<SampleContainer> data = new List<SampleContainer>();

        int width = m_width == 0 ? m_sampleManager.GetCaptureWidth() : m_width;
        int height = m_height == 0 ? m_sampleManager.GetCaptureHeight() : m_height;
        float playerHeight = m_sampleManager.GetPlayerHeight();

        int obstacleLength = width * height;
        int playerLength = m_sampleManager.GetInputLayerLengthPlayer(width, playerHeight);
        int dataLength = obstacleLength + playerLength;

        foreach (SampleGenerationBase sampleGeneration in m_generateSimpleData)
        {
            List<SampleContainer> simpleData = sampleGeneration.GenerateSamples(width, height, obstacleLength, playerLength);
            for (int i = 0; i < simpleData.Count; i++)
                data.Add(simpleData[i]);
        }

        if (data.Count == 0)
        {
            Debug.Log("No data has been writen! Data length was 0. (Post)");
            return null;
        }

        return data;
    }
    private void GenerateSamples()
    {
        if (m_generateSimpleData == null || m_generateSimpleData.Count == 0)
            return;

        int width = m_width == 0 ? m_sampleManager.GetCaptureWidth() : m_width;
        int height = m_height == 0 ? m_sampleManager.GetCaptureHeight() : m_height;
        float playerHeight = m_sampleManager.GetPlayerHeight();

        int obstacleLength = width * height;
        int playerLength = m_sampleManager.GetInputLayerLengthPlayer(width, playerHeight);
        int dataLength = obstacleLength + playerLength;

        foreach (SampleGenerationBase sampleGeneration in m_generateSimpleData)
        {
            sampleGeneration.GenerateSamples(width, height, obstacleLength, playerLength);
        }
    }

    //private List<SampleContainer> GenerateSimpleData(int width, int height, float playerHeight)
    //{
    //    List<SampleContainer> data = new List<SampleContainer>();

    //    int enemyLength = width * height;
    //    int playerLength = ScreenshotManager.Instance().GetInputLayerLengthPlayer(width, playerHeight);
    //    int dataLength = enemyLength + playerLength;

    //    //for (int i = 0; i < 5; i++)
    //    {
    //        for(int h = 0; h < height; h++)
    //        {
    //            float[] input = new float[dataLength];
    //            float[] desiredOutput = new float[ScreenshotManager.Instance().GetOutputNumber()];

    //            for (int w = 0; w < width; w++)
    //            {
    //                int obstacleIndex = height * width + width;
    //                int playerIndex = obstacleIndex + enemyLength;

    //                // obstacle

    //                // player
    //                if(obstacleIndex < playerLength)
    //                {

    //                }
    //            }

    //            SampleContainer container = new SampleContainer(input, desiredOutput, null);
    //            data.Add(container);
    //        }

    //    }

    //    return data;
    //}
    #endregion

    #region Write Data
    private void WriteSampleData(List<SampleContainer> samples, TextAsset target)
    {
        if (samples.Count == 0)
            return;

        string path = Utility.GetAssetPath(target);
        SampleData data = new SampleData(samples);

        using (StreamWriter sw = new StreamWriter(path))
        {
            sw.WriteLine(JsonUtility.ToJson(data, true));
            sw.Close();
        }

        Debug.Log("Wrote Data! Size: " + samples.Count + ", Name: " + path);
    }
    private void Concat(List<SampleContainer> samples, TextAsset target)
    {
        List<SampleContainer> targetData = SampleSaveManager.LoadSampleData(target).ToSampleContainers();

        List<SampleContainer> newData = new List<SampleContainer>();
        foreach (SampleContainer sample in targetData)
            newData.Add(sample);
        foreach (SampleContainer sample in samples)
            newData.Add(sample);

        WriteSampleData(newData, target);
    }
    #endregion

    #region Getter
    public float GetDistribution()
    {
        return m_selfDistribution;
    }
    public SampleContainer GetRandomSample()
    {
        float total = 0;
        foreach (SampleGenerationBase bundle in m_generateSimpleData)
        {
            total += bundle.GetDistribution();
        }
        float counter = 0;
        float random = Utility.GetRandomWithSeed(0, total, m_currentSeed);
        m_currentSeed += m_initSeed >= 0 ? 1 : 0;
        foreach (SampleGenerationBase bundle in m_generateSimpleData)
        {
            counter += bundle.GetDistribution();
            if (random <= counter)
            {
                return bundle.GetRandomSample();
            }
        }
        Debug.Log("Warning!");
        return new SampleContainer(false);
    }
    #endregion
}
