using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct NNSMSaveData
{
    public SampleManager.InputType m_inputType;

    public bool m_tossNoOutputSamples;

    public int m_minSamples;
    public int m_maxSamples;
    public int m_batchSize;

    public List<SampleContainer> m_samples;

    public NNTSSaveData m_screenshotData;
}

public class SampleManager : MonoBehaviour
{
    [Header("------ Settings ------")]
    [SerializeField] private InputType m_inputType;
    //[SerializeField] private bool m_saveSamples;

    [Header("--- Filter ---")]
    [SerializeField] private bool m_tossNoOutputSamples;

    [Header("--- Offline ---")]
    [SerializeField] private int m_minSamples;
    [SerializeField] private int m_maxSamples;
    [SerializeField] private int m_batchSize;

    [Header("--- Save / Load ---")]
    [SerializeField] private bool m_useGatheredSamples;
    [SerializeField] private bool m_usePreDefinedSamples;
    [SerializeField] private TextAsset m_sampleDataFile;
    private bool m_keepSamples;

    [Header("--- Objects ---")]
    [SerializeField] private TakeScreenshot m_screenshotScriptThis;
    [SerializeField] private TakeScreenshot m_screenshotScriptSource;
    private NeuralNetworkTrainingManager m_trainingManager;

    [Header("------ Debug ------")]
    private List<SampleContainer> m_samplesGathered;
    private List<SampleContainer> m_samplesPredefined;
    private SampleContainer m_cacheSampleSource;
    private SampleContainer m_cacheSampleThis;

    #region Enums
    public enum InputType { Screenshots, Raycasts, WorldInformation }
    #endregion

    #region Mono
    private void Awake()
    {
        if (m_trainingManager == null)
            m_trainingManager = GetComponent<NeuralNetworkTrainingManager>();

        m_samplesGathered = new List<SampleContainer>();

        SampleData data = SampleSaveManager.LoadSampleData(m_sampleDataFile);
        if (data.m_isCorrupted == true)
            m_samplesPredefined = null;
        else
        {
            m_samplesPredefined = data.ToSampleContainers();
            FilterSamples();
        }
    }
    private void LateUpdate()
    {
        m_cacheSampleSource = null;
        m_cacheSampleThis = null;
    }
    #endregion

    #region Sample Control
    public SampleContainer GenerateSampleSource(bool save)
    {
        float[] desiredOutput = GenerateDesiredOutput();
        bool isOkay = CheckIsOkayDesiredOutput(desiredOutput);
        if (!isOkay)
            return new SampleContainer(false);

        float[] input = GenerateInputSource();
        isOkay = CheckIsOkayInput(input);

        if (!isOkay)
            return new SampleContainer(false);


        SampleContainer sampleContainer = new SampleContainer(input, desiredOutput, CheckFilterDesiredOutput(desiredOutput));

        if(save)
            SaveSample(sampleContainer);

        m_cacheSampleSource = sampleContainer;
        return sampleContainer;
    }
    public SampleContainer GenerateSampleThis()
    {
        //if (m_cacheSampleThis != null)
        //    return m_cacheSampleThis;

        float[] input = GenerateInputThis();
        float[] desiredOutput = null;

        SampleContainer sampleContainer = new SampleContainer(input, desiredOutput, null);
        //SaveSample(sampleContainer);

        m_cacheSampleThis = sampleContainer;
        return sampleContainer;
    }
    public SampleContainer GenerateSampleOffline()
    {
        if(m_usePreDefinedSamples && m_useGatheredSamples)
        {
            Debug.Log("Warning: m_usePreDefinedSamples && m_useGatheredSamples not implented yet! Using only m_usePreDefinedSamples instead!");
        }
        if (m_usePreDefinedSamples)
        {
            if (m_samplesPredefined == null || m_samplesPredefined.Count == 0)
                return new SampleContainer(false);

            int index = Random.Range(0, m_samplesPredefined.Count - 1);
            SampleContainer sampleContainer = m_samplesPredefined[index];

            return sampleContainer;
        }
        else if (m_useGatheredSamples)
        {
            if (m_samplesGathered.Count < m_minSamples || m_samplesGathered.Count == 0)
                return new SampleContainer(false);

            int index = Random.Range(0, m_samplesGathered.Count - 1);
            SampleContainer sampleContainer = m_samplesGathered[index];

            return sampleContainer;
        }

        return new SampleContainer(false);
    }

    private float[] GenerateDesiredOutput()
    {
        return PlayerMovementManager.Instance().GenerateInputData();
    }
    private void SaveSample(SampleContainer sampleContainer)
    {
        while (m_maxSamples >= 0 && m_samplesGathered.Count > m_maxSamples)
        {
            m_samplesGathered.RemoveAt(Random.Range(0, m_samplesGathered.Count - 1));
        }
        m_samplesGathered.Add(sampleContainer);
    }
    #endregion

    #region Generate Input
    // source
    private float[] GenerateInputSource()
    {
        float[] input = new float[0];

        if (m_inputType == InputType.Screenshots)
            input = GenerateInputSourceScreenshot();
        else if (m_inputType == InputType.Raycasts)
            input = GenerateInputSourceRaycast();
        else if (m_inputType == InputType.WorldInformation)
            input = GenerateInputSourceWorldInformation();

        return input;
    }
    private float[] GenerateInputSourceScreenshot()
    {
        float[] input = m_screenshotScriptSource.GetScreenshotDataComputed(m_screenshotScriptThis.GetCaptureWidth(), m_screenshotScriptThis.GetCaptureHeight(), m_screenshotScriptThis.GetPlayerHeight(), TakeScreenshot.CaptureType.Separate);

        return input;
    }
    private float[] GenerateInputSourceRaycast()
    {
        float[] input = new float[0];

        // TODO

        return input;
    }
    private float[] GenerateInputSourceWorldInformation()
    {
        float[] input = new float[0];

        // TODO

        return input;
    }

    // this
    private float[] GenerateInputThis()
    {
        float[] input = new float[0];

        if (m_inputType == InputType.Screenshots)
            input = GenerateInputScreenshotThis();
        else if (m_inputType == InputType.Raycasts)
            input = GenerateInputRaycastThis();
        else if (m_inputType == InputType.WorldInformation)
            input = GenerateInputWorldInformationThis();

        return input;
    }
    private float[] GenerateInputScreenshotThis()
    {
        float[] input = m_screenshotScriptThis.GetScreenshotDataComputed(0, 0, 0, TakeScreenshot.CaptureType.Separate);

        return input;
    }
    private float[] GenerateInputRaycastThis()
    {
        float[] input = new float[0];

        // TODO

        return input;
    }
    private float[] GenerateInputWorldInformationThis()
    {
        float[] input = new float[0];

        // TODO

        return input;
    }
    #endregion

    #region Layer Length
    public int GetInputLayerLengthDynamicly()
    {
        if (m_inputType == InputType.Screenshots)
            return GetInputLayerLengthDynamiclyScreenshot();
        else if (m_inputType == InputType.Raycasts)
            return GetInputLayerLengthDynamiclyRaycast();
        else if (m_inputType == InputType.WorldInformation)
            return GetInputLayerLengthDynamiclyWorldInfo();

        return -1;
    }
    public int GetInputLayerLengthDynamiclyScreenshot()
    {
        return m_screenshotScriptThis.GetInputLayerLengthTotal(0, 0);
    }
    public int GetInputLayerLengthDynamiclyRaycast()
    {
        return -1;
    }
    public int GetInputLayerLengthDynamiclyWorldInfo()
    {
        return -1;
    }
    #endregion

    #region Filter
    private void FilterSamples()
    {
        if (m_samplesPredefined == null || m_samplesPredefined.Count == 0)
            return;

        for(int i = m_samplesPredefined.Count - 1; i >= 0; i--)
        {
            SampleContainer sample = m_samplesPredefined[i];

            if (!CheckIsOkayDesiredOutput(sample.m_desiredOutput) || !CheckIsOkayInput(sample.m_input))
                m_samplesPredefined.RemoveAt(i);
        }
    }
    public bool CheckIsOkayDesiredOutput(float[] desiredOutput)
    {
        bool isOkay = true;

        bool[] filters = CheckFilterDesiredOutput(desiredOutput);
        foreach(bool b in filters)
        {
            if (b == true)
                isOkay = false;
        }

        return isOkay;
    }
    public bool[] CheckFilterDesiredOutput(float[] desiredOutput)
    {
        bool[] filters = new bool[1];

        if (m_tossNoOutputSamples)
        {
            if (desiredOutput[0] == 0 && desiredOutput[1] == 0)
                filters[0] = true;
        }

        return filters;
    }
    public bool CheckIsOkayInput(float[] input)
    {
        bool isOkay = true;

        

        return isOkay;
    }
    #endregion

    #region Save / Load
    // sample data control


    // object data control
    public NNSMSaveData SaveData()
    {
        NNSMSaveData data = new NNSMSaveData
        {
            m_inputType = m_inputType,
            m_tossNoOutputSamples = m_tossNoOutputSamples,

            m_minSamples = m_minSamples,
            m_maxSamples = m_maxSamples,
            m_batchSize = m_batchSize,

            m_screenshotData = m_screenshotScriptThis.SaveData()
        };

        if (m_keepSamples)
        {
            data.m_samples = new List<SampleContainer>();
            foreach(SampleContainer sample in m_samplesGathered)
                data.m_samples.Add(sample);
        }

        return data;
    }
    public void LoadData(NNSMSaveData data)
    {
        m_screenshotScriptThis.LoadData(data.m_screenshotData);

        m_inputType = data.m_inputType;
        m_tossNoOutputSamples = data.m_tossNoOutputSamples;

        m_minSamples = data.m_minSamples;
        m_maxSamples = data.m_maxSamples;
        m_batchSize = data.m_batchSize;

        if(m_keepSamples)
        {
            m_samplesGathered.Clear();
            foreach (SampleContainer sample in data.m_samples)
                m_samplesGathered.Add(sample);
        }
    }
    public void ApplyData()
    {
        m_samplesGathered.Clear();
        m_cacheSampleSource = null;
        m_cacheSampleThis = null;
        m_screenshotScriptThis.ApplyData();
    }
    #endregion

    #region Getter
    public InputType GetInputType()
    {
        return m_inputType;
    }
    public TakeScreenshot GetScreenshotScript()
    {
        return m_screenshotScriptThis;
    }
    #endregion
}
