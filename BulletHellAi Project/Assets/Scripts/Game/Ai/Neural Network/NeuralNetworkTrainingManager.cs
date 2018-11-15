using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NeuralNetworkTrainingManager : MonoBehaviour
{
    [Header("------ Settings -------")]
    [SerializeField] private float m_learnRate;
    [SerializeField] private int m_batchSize;
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
        if(m_trainNetworkOnline && m_trainingCooldownRdyOnline < Time.time)
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

        bool update = m_network.AddTrainingData(sampleSource.m_input, sampleSource.m_desiredOutput);
        UpdateTraningCount();

        SampleContainer sampleThis = m_sampleManager.GenerateSampleThis();
        m_visualization.UpdateActivisions(sampleThis.m_input);
        if (update)
            m_visualization.UpdateVisualization();
    }
    public void TrainNetworkOffline()
    {
        SampleContainer sampleSource = m_sampleManager.GenerateSampleOffline();
        if (!sampleSource.m_isOkay)
            return;

        bool update = m_network.AddTrainingData(sampleSource.m_input, sampleSource.m_desiredOutput);
        UpdateTraningCount();

        SampleContainer sampleThis = m_sampleManager.GenerateSampleThis();
        m_visualization.UpdateActivisions(sampleThis.m_input);
        if (update)
            m_visualization.UpdateVisualization();
    }
    private void GatherNetworkOffline()
    {
        SampleContainer sampleSource = m_sampleManager.GenerateSampleSource(true);
    }
    #endregion

    #region Misc
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
    #endregion
}
