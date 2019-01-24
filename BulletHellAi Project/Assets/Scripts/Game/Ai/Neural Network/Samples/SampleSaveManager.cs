using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public struct SampleData
{
    public bool m_isCorrupted;

    public JaggedArrayContainer[] m_input;
    public JaggedArrayContainer[] m_desiredOutput;
    public JaggedArrayContainer[] m_filters;
    public int[] m_widths;
    public int[] m_heights;

    // Constructor
    public SampleData(List<SampleContainer> samples)
    {
        int sampleCount = samples.Count;

        m_isCorrupted = false;

        m_input = new JaggedArrayContainer[sampleCount];
        m_desiredOutput = new JaggedArrayContainer[sampleCount];
        m_filters = new JaggedArrayContainer[sampleCount];
        m_widths = new int[sampleCount];
        m_heights = new int[sampleCount];


        for (int sampleIndex = 0; sampleIndex < sampleCount; sampleIndex++)
        {
            JaggedArrayContainer dataInput = new JaggedArrayContainer(samples[sampleIndex].m_input);
            JaggedArrayContainer dataDesiredOutput = new JaggedArrayContainer(samples[sampleIndex].m_desiredOutput);
            JaggedArrayContainer dataFilters = new JaggedArrayContainer(samples[sampleIndex].m_filters);

            m_input[sampleIndex] = dataInput;
            m_desiredOutput[sampleIndex] = dataDesiredOutput;
            m_filters[sampleIndex] = dataFilters;
            m_widths[sampleIndex] = samples[sampleIndex].m_width;
            m_heights[sampleIndex] = samples[sampleIndex].m_height;
        }
    }

    // Operations
    //public SampleData AddData(List<SampleContainer> samples)
    //{
    //    int sampleCount = samples.Count;

    //    JaggedArrayContainer[] newInput = new JaggedArrayContainer[m_input != null ? m_input.Length + sampleCount : sampleCount];
    //    JaggedArrayContainer[] newDesiredOutput = new JaggedArrayContainer[newInput.Length];
    //    JaggedArrayContainer[] newFilters = new JaggedArrayContainer[newInput.Length];


    //    for (int sampleIndex = 0; sampleIndex < newInput.Length; sampleIndex++)
    //    {
    //        if(sampleIndex < m_input.Length)
    //        {
    //            newInput[sampleIndex] = m_input[sampleIndex];
    //            newDesiredOutput[sampleIndex] = m_desiredOutput[sampleIndex];
    //            newFilters[sampleIndex] = m_filters[sampleIndex];
    //        }
    //        else
    //        {
    //            int index = sampleIndex - m_input.Length;

    //            JaggedArrayContainer dataInput = new JaggedArrayContainer(samples[index].m_input);
    //            JaggedArrayContainer dataDesiredOutput = new JaggedArrayContainer(samples[index].m_desiredOutput);
    //            JaggedArrayContainer dataFilters = new JaggedArrayContainer(samples[index].m_filters);

    //            newInput[sampleIndex] = dataInput;
    //            newDesiredOutput[sampleIndex] = dataDesiredOutput;
    //            newFilters[sampleIndex] = dataFilters;
    //        }
    //    }

    //    m_input = newInput;
    //    m_desiredOutput = newDesiredOutput;
    //    m_filters = newFilters;
    //}

    // Misc
    public List<SampleContainer> ToSampleContainers()
    {
        if (m_input == null || m_desiredOutput == null || m_filters == null)
            return null;

        List<SampleContainer> samples = new List<SampleContainer>();

        for (int sampleIndex = 0; sampleIndex < m_input.Length; sampleIndex++)
        {
            samples.Add(new SampleContainer(m_input[sampleIndex].dataFloat, m_desiredOutput[sampleIndex].dataFloat, m_filters[sampleIndex].dataBool, m_widths[sampleIndex], m_heights[sampleIndex]));
        }

        return samples;
    }
}

public class SampleSaveManager : MonoBehaviour
{
    [Header("----- Settings -----")]

    [Header("--- Save / Load ---")]

    [SerializeField] private bool m_savePlayerSamples;
    [SerializeField] private bool m_concatToExistingSamples;
    
    [Header("--- Path ---")]
    [SerializeField] private string m_directoryPath = "Sample Saves";
    [SerializeField] private string m_dataName = "sample";

    [Header("--- Operations ---")]

    [Header("- Concat / Add Data -")]

    [SerializeField] private bool m_concat;
    [SerializeField] private TextAsset m_concatTarget;
    [SerializeField] private TextAsset m_concatSource;
   
    [Header("--- Objects ---")]
    [SerializeField] private PlayerMovementManager m_movementManager;

    private SampleManager m_sampleManager;
    private List<SampleContainer> m_samples;


    #region Mono
    private void Awake()
    {
        m_sampleManager = GetComponent<SampleManager>();
        m_samples = new List<SampleContainer>();
    }
    void Update ()
    {
        SavePlayerSample();
    }
    private void OnDestroy()
    {
        if (m_concatToExistingSamples)
            Concat("", m_samples);
        else
            WriteSampleData(m_samples, "");
    }
    private void OnDrawGizmosSelected()
    {
        if(m_concat)
        {
            Concat(m_concatTarget, m_concatSource);
            m_concat = false;
        }

    }
    #endregion

    #region Save / Load
    private void SavePlayerSample()
    {
        if (!m_savePlayerSamples || m_movementManager.GetControllerType() != PlayerMovementManager.ControllerType.Player)
            return;

        float[] desiredOutput = PlayerMovementManager.Instance().GenerateInputData();
        bool isOkay = m_sampleManager.CheckIsOkayDesiredOutput(desiredOutput);
        if (!isOkay)
            return;

        float[] input = m_sampleManager.GetScreenshotScript().GetScreenshotDataComputed(0, 0, 0, TakeScreenshot.CaptureType.Separate, m_sampleManager.GetCurveWidthSource(), m_sampleManager.GetCurveHeightSource());
        isOkay = m_sampleManager.CheckIsOkayInput(input);
        if (!isOkay)
            return;

        SampleContainer sampleContainer = new SampleContainer(input, desiredOutput, m_sampleManager.CheckFilterDesiredOutput(desiredOutput), m_sampleManager.GetScreenshotScript().GetCaptureWidth(), m_sampleManager.GetScreenshotScript().GetCaptureHeight());
        m_samples.Add(sampleContainer);
    }
    private void WriteSampleData(List<SampleContainer> samples, string filePath)
    {
        if (samples.Count == 0)
            return;

        string path = filePath == "" ? GetDirectoryPath(filePath) : filePath;

        SampleData data = new SampleData(samples);

        using (StreamWriter sw = new StreamWriter(path))
        {
            sw.WriteLine(JsonUtility.ToJson(data, true));
            sw.Close();
        }

        Debug.Log("Wrote Data! Name: " + path + ", Size: " + samples.Count);
    }

    public static SampleData LoadSampleData(TextAsset dataFile)
    {
        if (dataFile == null)
            return new SampleData();

        return JsonUtility.FromJson<SampleData>(dataFile.text);
    }
    private string GetDirectoryPath(string fileName)
    {
        string directoryName = m_directoryPath != "" ? m_directoryPath : "Sample Saves";

        if (Application.isEditor)
            directoryName = Path.GetFullPath(Application.dataPath + "\\" + directoryName);
        else
            directoryName = Application.dataPath + "\\" + directoryName;
        Directory.CreateDirectory(directoryName);

        if (fileName == "")
        {
            directoryName += string.Format("\\{0}x{1}", m_sampleManager.GetScreenshotScript().GetCaptureHeight(), m_sampleManager.GetScreenshotScript().GetCaptureWidth());
            Directory.CreateDirectory(directoryName);
            fileName = m_dataName;
        }
        directoryName += "\\" + fileName + ".json";

        return directoryName;
    }
    
    #endregion

    #region Operations
    private void Concat(TextAsset target, TextAsset source)
    {
        Concat(target, LoadSampleData(source).ToSampleContainers());

        //if (!Application.isEditor)
        //    return;

        //List<SampleContainer> targetData = LoadSampleData(target).ToSampleContainers();
        //List<SampleContainer> sourceData = LoadSampleData(source).ToSampleContainers();

        //List<SampleContainer> newData = new List<SampleContainer>();
        //foreach (SampleContainer sample in targetData)
        //    newData.Add(sample);
        //foreach (SampleContainer sample in sourceData)
        //    newData.Add(sample);

        ////SampleData data = new SampleData(newData);

        //string path = Path.GetFullPath(Application.dataPath);
        //path = path.Substring(0, path.Length - 6) + UnityEditor.AssetDatabase.GetAssetPath(target);

        //WriteSampleData(newData, path);
    }
    private void Concat(TextAsset target, List<SampleContainer> source)
    {
        if (!Application.isEditor)
            return;

        List<SampleContainer> targetData = LoadSampleData(target).ToSampleContainers();
        //List<SampleContainer> sourceData = LoadSampleData(source).ToSampleContainers();

        List<SampleContainer> newData = new List<SampleContainer>();
        foreach (SampleContainer sample in targetData)
            newData.Add(sample);
        foreach (SampleContainer sample in source)
            newData.Add(sample);

        //SampleData data = new SampleData(newData);

        //string path = Path.GetFullPath(Application.dataPath);
        //path = path.Substring(0, path.Length - 6) + UnityEditor.AssetDatabase.GetAssetPath(target);

        WriteSampleData(newData, Utility.GetAssetPath(target));
    }
    private void Concat(string targetPath, List<SampleContainer> source)
    {
        if (!Application.isEditor)
            return;

        if (source.Count == 0)
            return;

        string path = targetPath == "" ? GetDirectoryPath(targetPath) : targetPath;
        StreamReader reader = new StreamReader(path);
        string jsonText = reader.ReadToEnd();
        reader.Close();
        List<SampleContainer> oldData = JsonUtility.FromJson<SampleData>(jsonText).ToSampleContainers();

        List<SampleContainer> newData = new List<SampleContainer>();
        foreach (SampleContainer sample in oldData)
            newData.Add(sample);
        foreach (SampleContainer sample in source)
            newData.Add(sample);

        WriteSampleData(newData, path);
    }

    #endregion
}
