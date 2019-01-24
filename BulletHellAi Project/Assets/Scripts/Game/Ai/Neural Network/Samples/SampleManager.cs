using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct NNSMSaveData
{
    public float m_sampleDistributionStay;
    public float m_sampleDistributionPre;
    public AnimationCurve m_widthModifierSource;
    public AnimationCurve m_heightModifierSource;
    public AnimationCurve m_widthModifierThis;
    public AnimationCurve m_heightModifierThis;

    public bool m_tossNoOutputSamples;

    public int m_minSamplesNoStay;
    public int m_maxSamplesNoStay;
    public int m_maxSamplesStay;
    public int m_maxSamplesStayPercent;

    public float m_initSeed;
    public float m_currentSeed;

    public int m_batchSize;

    public List<SampleContainer> m_samples;

    public NNTSSaveData m_screenshotData;
}

public class SampleManager : MonoBehaviour
{
    #region Member Variables
    [Header("------ Settings ------")]
    [Header("--- Sample Distribution ---")]
    [SerializeField][Range(0f, 1f)] private float m_sampleDistributionStay;
    [SerializeField][Range(0f, 1f)] private float m_sampleDistributionPre;

    [Header("--- Sample Size ---")]
    [SerializeField] private int m_minSamplesNoStay;
    [SerializeField] private int m_maxSamplesNoStay;

    [SerializeField] private int m_maxSamplesStay;
    [SerializeField] private int m_maxSamplesStayPercent;


    [Header("--- Magnitude Modifier ---")]
    [SerializeField] private AnimationCurve m_widthModifierSource = AnimationCurve.Constant(0, 1, 1);
    [SerializeField] private AnimationCurve m_heightModifierSource = AnimationCurve.Constant(0, 1, 1);
    [SerializeField] private AnimationCurve m_widthModifierThis = AnimationCurve.Constant(0, 1, 1);
    [SerializeField] private AnimationCurve m_heightModifierThis = AnimationCurve.Constant(0, 1, 1);

    [Header("--- Filter ---")]
    [SerializeField] private bool m_tossNoOutputSamples;

    [Header("--- Seed ---")]
    private int m_initSeed = -1;

    [Header("--- Predefined ---")]
    //[SerializeField] private bool m_useGatheredSamples;
    //[SerializeField] private bool m_usePreDefinedSamples;
    //[SerializeField] private List<TextAsset> m_sampleDataFiles;
    [SerializeField] private List<SampleGeneration> m_sampleBundles;

    [Header("--- Save / Load ---")]
    //[SerializeField] private bool m_keepSamplesGathered;
    //[SerializeField] private bool m_keepSampleBundles;

    [Header("--- Objects ---")]
    [SerializeField] private TakeScreenshot m_screenshotScriptThis;
    [SerializeField] private TakeScreenshot m_screenshotScriptSource;
    private NeuralNetworkTrainingManager m_trainingManager;

    [Header("------ Debug ------")]
    private int m_batchSize;
    private List<SampleContainer> m_samplesGatheredNoStay;
    private List<SampleContainer> m_samplesGatheredStay;
    private List<SampleContainer> m_samplesPredefined;
    private SampleContainer m_cacheSampleSource;
    private SampleContainer m_cacheSampleThis;
    private int m_currentSeed;
    #endregion

    #region Mono
    private void Awake()
    {
        if (m_trainingManager == null)
            m_trainingManager = GetComponent<NeuralNetworkTrainingManager>();

        m_samplesGatheredNoStay = new List<SampleContainer>();
        m_samplesGatheredStay = new List<SampleContainer>();

        m_samplesPredefined = new List<SampleContainer>();
        m_currentSeed = m_initSeed;
        //if(m_sampleBundles != null)
        //{
        //    foreach(SampleGeneration sampleBundle in m_sampleBundles)
        //    {
        //        sampleBundle.
        //    }
        //}

        //for (int i = 0; i < m_sampleDataFiles.Count; i++)
        //{
        //    SampleData data = SampleSaveManager.LoadSampleData(m_sampleDataFiles[i]);
        //    if (data.m_isCorrupted == true)
        //        continue;// m_samplesPredefined = null;
        //    else
        //    {
        //        List<SampleContainer> samples = data.ToSampleContainers();
        //        for (int j = 0; j < samples.Count; j++)
        //        {
        //            m_samplesPredefined.Add(samples[j]);
        //        }
        //    }
        //}
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

        SampleContainer sampleContainer = new SampleContainer(input, desiredOutput, CheckFilterDesiredOutput(desiredOutput), m_screenshotScriptThis.GetCaptureWidth(), m_screenshotScriptThis.GetCaptureHeight());

        if(save)
            SaveSample(sampleContainer);

        m_cacheSampleSource = sampleContainer;
        return sampleContainer;
    }
    public SampleContainer GenerateSampleThis()
    {
        float[] input = GenerateInputThis();
        float[] desiredOutput = null;

        SampleContainer sampleContainer = new SampleContainer(input, desiredOutput, null, m_screenshotScriptThis.GetCaptureWidth(), m_screenshotScriptThis.GetCaptureHeight());

        m_cacheSampleThis = sampleContainer;
        return sampleContainer;
    }
    public SampleContainer GenerateSampleOffline()
    {
        //if(m_usePreDefinedSamples && m_useGatheredSamples)
        //{
        //    Debug.Log("Warning: m_usePreDefinedSamples && m_useGatheredSamples not implented yet! Using only m_usePreDefinedSamples instead!");
        //}
        /*else*/
        if (m_minSamplesNoStay > 0 && m_samplesGatheredNoStay.Count < m_minSamplesNoStay)
            return new SampleContainer(false);

        
        if (GetRandom(0f, 1f) < m_sampleDistributionPre)
        {
            SampleContainer sampleContainer = null;

            float total = 0;
            foreach(SampleGeneration bundle in m_sampleBundles)
            {
                total += bundle.GetDistribution();
            }
            float counter = 0;
            float random = GetRandom(0f, total);
            foreach (SampleGeneration bundle in m_sampleBundles)
            {
                counter += bundle.GetDistribution();
                if (random <= counter)
                {
                    sampleContainer = bundle.GetRandomSample();
                    break;
                }
            }

            return sampleContainer;
        }
        else
        {
            SampleContainer sampleContainer = new SampleContainer(false);
            bool isStay = GetRandom(0f, 1f) < m_sampleDistributionStay;

            if (isStay && m_samplesGatheredStay.Count > 0)
            {
                int index = GetRandom(0, m_samplesGatheredStay.Count);
                sampleContainer = m_samplesGatheredStay[index];
            }
            else if(m_samplesGatheredNoStay.Count > 0)
            {
                int index = GetRandom(0, m_samplesGatheredNoStay.Count);
                sampleContainer = m_samplesGatheredNoStay[index];
            }

            return sampleContainer;
        }

        //return new SampleContainer(false);
    }

    private float[] GenerateDesiredOutput()
    {
        return PlayerMovementManager.Instance().GenerateInputData();
    }
    private void SaveSample(SampleContainer sampleContainer)
    {
        bool isStay = IsStay(sampleContainer.m_desiredOutput);

        if (!isStay)
        {
            while (m_maxSamplesNoStay >= 0 && m_samplesGatheredNoStay.Count > m_maxSamplesNoStay)
            {
                m_samplesGatheredNoStay.RemoveAt(GetRandom(0, m_samplesGatheredNoStay.Count));
            }
            m_samplesGatheredNoStay.Add(sampleContainer);
        }
        else if(m_maxSamplesStay > 0 || m_maxSamplesStayPercent > 0)
        {
            int maxSamplesStay = m_maxSamplesStay;// (int)(m_maxSamplesNoStay * m_sampleStayDistribution);
            while (maxSamplesStay >= 0 && m_samplesGatheredStay.Count > maxSamplesStay)
            {
                m_samplesGatheredStay.RemoveAt(GetRandom(0, m_samplesGatheredStay.Count));
            }
            m_samplesGatheredStay.Add(sampleContainer);
        }
    }
    private bool IsStay(float[] output)
    {
        bool isStay = false;
        isStay = output[0] == 0 && output[1] == 0;

        return isStay;
    }
    #endregion

    #region Generate Input
    private float[] GenerateInputSource()
    {
        float[] input = m_screenshotScriptSource.GetScreenshotDataComputed(m_screenshotScriptThis.GetCaptureWidth(), m_screenshotScriptThis.GetCaptureHeight(), m_screenshotScriptThis.GetPlayerHeight(), TakeScreenshot.CaptureType.Separate, m_widthModifierSource, m_heightModifierSource);

        return input;
    }
    private float[] GenerateInputThis()
    {
        float[] input = m_screenshotScriptThis.GetScreenshotDataComputed(0, 0, 0, TakeScreenshot.CaptureType.Separate, m_widthModifierThis, m_heightModifierThis);

        return input;
    }
    #endregion

    #region Layer Length
    public int GetInputLayerLengthDynamicly()
    {
        return GetInputLayerLengthDynamiclyScreenshot();
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

    #region Misc
    private float GetRandom(float min, float max)
    {
        float random = 0;
        if (m_initSeed >= 0)
            random = Utility.GetRandomWithSeed(min, max, m_currentSeed++);
        else
            random = Random.Range(min, max);
        return random;
    }
    private int GetRandom(int min, int max)
    {
        int random = 0;
        if (m_initSeed >= 0)
            random = Utility.GetRandomWithSeed(min, max, m_currentSeed++);
        else
            random = Random.Range(min, max);
        return random;
    }
    #endregion

    #region Save / Load
    // object data control
    public NNSMSaveData SaveData()
    {
        NNSMSaveData data = new NNSMSaveData
        {
            m_sampleDistributionStay = m_sampleDistributionStay,
            m_sampleDistributionPre = m_sampleDistributionPre,

            m_widthModifierSource = m_widthModifierSource,
            m_heightModifierSource = m_heightModifierSource,
            m_widthModifierThis = m_widthModifierThis,
            m_heightModifierThis = m_heightModifierThis,
            m_tossNoOutputSamples = m_tossNoOutputSamples,

            m_minSamplesNoStay = m_minSamplesNoStay,
            m_maxSamplesNoStay = m_maxSamplesNoStay,
            m_maxSamplesStay = m_maxSamplesStay,
            m_maxSamplesStayPercent = m_maxSamplesStayPercent,

            m_batchSize = m_batchSize,

            m_screenshotData = m_screenshotScriptThis.SaveData()
        };

        //if (m_keepSamples)
        //{
        //    data.m_samples = new List<SampleContainer>();
        //    foreach(SampleContainer sample in m_samplesGatheredNoStay)
        //        data.m_samples.Add(sample);
        //}

        return data;
    }
    public void LoadData(NNSMSaveData data)
    {
        m_screenshotScriptThis.LoadData(data.m_screenshotData);

        m_sampleDistributionStay = data.m_sampleDistributionStay;
        m_sampleDistributionPre = data.m_sampleDistributionPre;

        m_widthModifierSource = data.m_widthModifierSource;
        m_heightModifierSource = data.m_heightModifierSource;
        m_widthModifierThis = data.m_widthModifierThis;
        m_heightModifierThis = data.m_heightModifierThis;

        m_tossNoOutputSamples = data.m_tossNoOutputSamples;

        m_minSamplesNoStay = data.m_minSamplesNoStay;
        m_maxSamplesNoStay = data.m_maxSamplesNoStay;
        m_maxSamplesStay = data.m_maxSamplesStay;
        m_maxSamplesStayPercent = data.m_maxSamplesStayPercent;


        m_batchSize = data.m_batchSize;

        //if(m_keepSamples)
        //{
        //    m_samplesGatheredNoStay.Clear();
        //    foreach (SampleContainer sample in data.m_samples)
        //        m_samplesGatheredNoStay.Add(sample);
        //}
    }
    public void ApplyData()
    {
        m_samplesGatheredNoStay.Clear();
        m_cacheSampleSource = null;
        m_cacheSampleThis = null;
        m_screenshotScriptThis.ApplyData();
    }
    #endregion

    #region Getter
    public TakeScreenshot GetScreenshotScript()
    {
        return m_screenshotScriptThis;
    }
    public AnimationCurve GetCurveWidthSource()
    {
        return m_widthModifierSource;
    }
    public AnimationCurve GetCurveHeightSource()
    {
        return m_heightModifierSource;
    }
    #endregion
}
