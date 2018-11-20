using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("--- Objects ---")]
    [SerializeField] private TakeScreenshot m_screenshotScriptThis;
    [SerializeField] private TakeScreenshot m_screenshotScriptSource;
    [SerializeField] private NeuralNetworkTrainingManager m_trainingManager;

    [Header("------ Debug ------")]
    bool b;
    private List<SampleContainer> m_samples;
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
        if (m_trainingManager == null)
            Debug.Log("Warning: Training Manager not found!");

        m_samples = new List<SampleContainer>();

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
        //if (m_cacheSampleSource != null)
        //    return m_cacheSampleSource;

        float[] desiredOutput = GenerateDesiredOutput();
        bool isOkay = CheckFilterDesiredOutput(desiredOutput);
        if (!isOkay)
            return new SampleContainer(null, null, false);

        float[] input = GenerateInputSource();
        isOkay = CheckFilterInput(input);

        if (!isOkay)
            return new SampleContainer(null, null, false);

        SampleContainer sampleContainer = new SampleContainer(input, desiredOutput, isOkay);

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

        SampleContainer sampleContainer = new SampleContainer(input, desiredOutput, true);
        //SaveSample(sampleContainer);

        m_cacheSampleThis = sampleContainer;
        return sampleContainer;
    }
    public SampleContainer GenerateSampleOffline()
    {
        if (m_samples.Count < m_minSamples || m_samples.Count == 0)
            return new SampleContainer(null, null, false);

        //SampleContainer container = new SampleContainer(null, null, true);

        int index = Random.Range(0, m_samples.Count - 1);
        SampleContainer sampleContainer = m_samples[index];

        return sampleContainer;
    }

    private float[] GenerateDesiredOutput()
    {
        return PlayerMovementManager.Instance().GenerateInputData();
    }
    private void SaveSample(SampleContainer sampleContainer)
    {
        if (m_samples.Count >= m_maxSamples)
            m_samples.RemoveAt(Random.Range(0, m_samples.Count - 1));
        m_samples.Add(sampleContainer);
        
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
        float[] input = m_screenshotScriptSource.GetScreenshotDataComputed(m_screenshotScriptThis.GetCaptureWidth(), m_screenshotScriptThis.GetCaptureHeight(), m_screenshotScriptThis.GetPlayerHight(), false);

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
        float[] input = m_screenshotScriptThis.GetScreenshotDataComputed(0, 0, 0, true);

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
    private bool CheckFilterDesiredOutput(float[] desiredOutput)
    {
        bool isOkay = true;

        if(m_tossNoOutputSamples)
        {
            if(desiredOutput[0] == 0 && desiredOutput[1] == 0)
            isOkay = false;
            
        }

        return isOkay;
    }
    private bool CheckFilterInput(float[] input)
    {
        bool isOkay = true;

        

        return isOkay;
    }
    #endregion

    #region Getter
    public InputType GetInputType()
    {
        return m_inputType;
    }
    #endregion
}
