using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct NNTMSaveData
{
    public NeuralNetworkTrainingManager.UpdateType m_updateType;

    public float m_learnRate;
    public int m_batchSize;
    public int m_curveRange;
    public AnimationCurve m_curve;

    public float m_regularizationKeepRate;

    public float m_weightDecayRate;
    public bool m_trainNetworkOnline;
    public bool m_saveSamplesOnline;
    public float m_trainingCooldownOnlineMin;
    public float m_trainingCooldownOnlineMax;
    
    public bool m_trainNetworkOffline;
    public int m_trainingUnits;
    public float m_trainingCooldownOfflineMin;
    public float m_trainingCooldownOfflineMax;
    public float m_trainingCooldownOfflineGatherMin;
    public float m_trainingCooldownOfflineGatherMax;
     
    public int m_stopAtMaximumUnitCount;
     
    public float m_trainingCooldownRdyOnline;
    public float m_trainingCooldownRdyOffline;
    public float m_trainingCooldownRdyOfflineGather;
     
    public int m_trainingUnitssCompleted;
}

public class NeuralNetworkTrainingManager : MonoBehaviour
{
    [Header("------ Settings -------")]
    [Header("--- Learn Type ---")]
    [SerializeField] private UpdateType m_updateType;

    [Header("--- Learn Rate ---")]
    [SerializeField] private float m_learnRate;
    [SerializeField] private int m_batchSize;
    [SerializeField] private int m_curveRange;
    [SerializeField] private AnimationCurve m_curve;

    [Header("--- Regularization ---")]
    [SerializeField] private float m_dropoutKeepRate = 1;

    [Header("--- WeightDecay ---")]
    [SerializeField] private float m_weightDecayRate;

    [Header("--- Online Learning ---")]
    [SerializeField] private bool m_trainNetworkOnline;
    [SerializeField] private bool m_saveSamplesOnline;
    [SerializeField] private float m_trainingCooldownOnlineMin;
    [SerializeField] private float m_trainingCooldownOnlineMax;

    [Header("--- Offline Learning ---")]
    [SerializeField] private bool m_trainNetworkOffline;
    [SerializeField] private int m_trainingUnits;
    [SerializeField] private float m_trainingCooldownOfflineMin;
    [SerializeField] private float m_trainingCooldownOfflineMax;
    [Space]
    [SerializeField] private float m_trainingCooldownOfflineGatherMin;
    [SerializeField] private float m_trainingCooldownOfflineGatherMax;
    
    [Header("--- Stop Learning ---")]
    [SerializeField] private int m_stopAtMaximumUnitCount;

    [Header("--- Objects ---")]
    [SerializeField] private Text m_unitCountText;

    [Header("------ Debug -------")]
    private SampleManager m_sampleManager;
    private NeuralNetworkVisualization m_visualization;
    private NeuralNetwork m_network;

    private float m_trainingCooldownRdyOnline;
    private float m_trainingCooldownRdyOffline;
    private float m_trainingCooldownRdyOfflineGather;

    private int m_trainingUnitssCompleted;


    #region Enums
    public enum UpdateType { SGD, Momentum, NAG, Adam }
    #endregion

    #region Mono
    private void Awake()
    {
        if (m_sampleManager == null)
            m_sampleManager = GetComponent<SampleManager>();
        if (m_sampleManager == null)
            Debug.Log("Warning: SampleManager not found!");

        if (m_visualization == null)
            m_visualization = GetComponent<NeuralNetworkVisualization>();
        if (m_visualization == null)
            Debug.Log("Warning: NeuralNetworkVisualization not found!");
    }
    private void Start()
    {
        m_trainingCooldownRdyOnline = Time.time + Random.Range(m_trainingCooldownOnlineMin, m_trainingCooldownOnlineMax);
        m_trainingCooldownRdyOffline = Time.time + Random.Range(m_trainingCooldownOfflineMin, m_trainingCooldownOfflineMax);
        m_trainingCooldownRdyOfflineGather = Time.time + Random.Range(m_trainingCooldownOfflineGatherMin, m_trainingCooldownOfflineGatherMax);
    }
    private void Update()
    {
        ManageTraining();
    }
    #endregion

    #region Training
    private void ManageTraining()
    {
        if (m_stopAtMaximumUnitCount > 0 && m_trainingUnitssCompleted >= m_stopAtMaximumUnitCount)
            m_trainNetworkOnline = m_trainNetworkOffline = false;

        if (m_trainNetworkOnline && m_trainingCooldownRdyOnline < Time.time)
        {
            TrainNetworkOnline();
            m_trainingCooldownRdyOnline = Time.time + Random.Range(m_trainingCooldownOnlineMin, m_trainingCooldownOnlineMax);
        }

        if (m_trainNetworkOffline)
        {
            if (m_trainingCooldownRdyOfflineGather < Time.time)
            {
                GatherNetworkOffline();
                m_trainingCooldownRdyOfflineGather = Time.time + Random.Range(m_trainingCooldownOfflineGatherMin, m_trainingCooldownOfflineGatherMax);
            }

            if (m_trainingCooldownRdyOffline < Time.time)
            {
                for (int i = 0; i < m_trainingUnits; i++)
                {
                    TrainNetworkOffline();
                }
                m_trainingCooldownRdyOffline = Time.time + Random.Range(m_trainingCooldownOfflineMin, m_trainingCooldownOfflineMax);
            }
        }
    }
    public void TrainNetworkOnline()
    {
        SampleContainer sampleSource = m_sampleManager.GenerateSampleSource(m_saveSamplesOnline);
        if (!sampleSource.m_isOkay)
            return;

        bool update = m_network.AddTrainingData(sampleSource.m_input, sampleSource.m_desiredOutput, GetLearnRate(m_trainingUnitssCompleted));


        SampleContainer sampleThis = m_sampleManager.GenerateSampleThis();
        m_visualization.UpdateActivisions(sampleThis.m_input);
        if (update)
        {
            UpdateTraningCount();
            m_visualization.UpdateVisualization();
        }
    }
    public void TrainNetworkOffline()
    {
        SampleContainer sampleSource = m_sampleManager.GenerateSampleOffline();
        if (!sampleSource.m_isOkay)
            return;

        bool update = m_network.AddTrainingData(sampleSource.m_input, sampleSource.m_desiredOutput, GetLearnRate(m_trainingUnitssCompleted));

        SampleContainer sampleThis = m_sampleManager.GenerateSampleThis();
        m_visualization.UpdateActivisions(sampleThis.m_input);
        if (update)
        {
            UpdateTraningCount();
            m_visualization.UpdateVisualization();
        }
    }
    private void GatherNetworkOffline()
    {
        /*SampleContainer sampleSource =*/ m_sampleManager.GenerateSampleSource(true);
    }
    #endregion
    
    #region Misc
    private float GetLearnRate(int unitNumber)
    {
        float learnRate = m_learnRate * m_curve.Evaluate(unitNumber / (m_curveRange == 0 ? 1 : m_curveRange));
        return learnRate;
    }
    public void SetNetwork(NeuralNetwork network)
    {
        m_network = network;
    }
    private void UpdateTraningCount()
    {
        m_trainingUnitssCompleted++;
        if(m_unitCountText != null)
            m_unitCountText.text = "" + m_trainingUnitssCompleted;
    }
    #endregion

    #region Save / Load
    public NNTMSaveData SaveData()
    {
        NNTMSaveData data = new NNTMSaveData
        {
            m_learnRate                                = m_learnRate,
            m_batchSize                                = m_batchSize,
            m_curveRange                               = m_curveRange,
            m_curve                                    = m_curve,

            m_regularizationKeepRate                   = m_dropoutKeepRate,

            m_weightDecayRate                          = m_weightDecayRate,

            m_trainNetworkOnline                       = m_trainNetworkOnline,
            m_saveSamplesOnline                        = m_saveSamplesOnline,
            m_trainingCooldownOnlineMin                = m_trainingCooldownOnlineMin,
            m_trainingCooldownOnlineMax                = m_trainingCooldownOnlineMax,
                                                       
            m_trainNetworkOffline                      = m_trainNetworkOffline,
            m_trainingUnits                            = m_trainingUnits,
            m_trainingCooldownOfflineMin               = m_trainingCooldownOfflineMin,
            m_trainingCooldownOfflineMax               = m_trainingCooldownOfflineMax,
            m_trainingCooldownOfflineGatherMin         = m_trainingCooldownOfflineGatherMin,
            m_trainingCooldownOfflineGatherMax         = m_trainingCooldownOfflineGatherMax,
                                                       
            m_stopAtMaximumUnitCount                   = m_stopAtMaximumUnitCount,
                                                       
            m_trainingCooldownRdyOnline                = m_trainingCooldownRdyOnline,
            m_trainingCooldownRdyOffline               = m_trainingCooldownRdyOffline,
            m_trainingCooldownRdyOfflineGather         = m_trainingCooldownRdyOfflineGather,
                                                       
            m_trainingUnitssCompleted                  = m_trainingUnitssCompleted,
        };

        return data;
    }
    public void LoadData(NNTMSaveData data)
    {
        m_learnRate = data.m_learnRate;
        m_batchSize = data.m_batchSize;
        m_curveRange = data.m_curveRange;
        m_curve = data.m_curve;

        m_dropoutKeepRate = data.m_regularizationKeepRate;

        m_weightDecayRate = data.m_weightDecayRate;

        m_trainNetworkOnline = data.m_trainNetworkOnline;
        m_saveSamplesOnline = data.m_saveSamplesOnline;
        m_trainingCooldownOnlineMin = data.m_trainingCooldownOnlineMin;
        m_trainingCooldownOnlineMax = data.m_trainingCooldownOnlineMax;

        m_trainNetworkOffline = data.m_trainNetworkOffline;
        m_trainingUnits = data.m_trainingUnits;
        m_trainingCooldownOfflineMin = data.m_trainingCooldownOfflineMin;
        m_trainingCooldownOfflineMax = data.m_trainingCooldownOfflineMax;
        m_trainingCooldownOfflineGatherMin = data.m_trainingCooldownOfflineGatherMin;
        m_trainingCooldownOfflineGatherMax = data.m_trainingCooldownOfflineGatherMax;

        m_stopAtMaximumUnitCount = data.m_stopAtMaximumUnitCount;

        m_trainingCooldownRdyOnline = data.m_trainingCooldownRdyOnline;
        m_trainingCooldownRdyOffline = data.m_trainingCooldownRdyOffline;
        m_trainingCooldownRdyOfflineGather = data.m_trainingCooldownRdyOfflineGather;

        m_trainingUnitssCompleted = data.m_trainingUnitssCompleted;
    }
    public void ApplyData()
    {

    }
    #endregion

    #region Getter
    public float GetLearnRate()
    {
        if(m_learnRate < 0)
        {
            Debug.Log("Warning: learn rate was set positive! Using positive instead!");
            return -m_learnRate;
        }
        return m_learnRate;
    }
    public int GetBatchSize()
    {
        if (m_batchSize < 1)
        {
            Debug.Log("Warning: batch size was set smaller than 1! Using 1 instead!");
            return 1;
        }
        return m_batchSize;
    }
    public bool GetTrainOffline()
    {
        return m_trainNetworkOffline;
    }
    public float GetDropoutRate()
    {
        return m_dropoutKeepRate;
    }
    public float GetWeightDecayRate()
    {
        return m_weightDecayRate;
    }
    #endregion
}
