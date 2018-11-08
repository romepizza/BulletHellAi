using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetworkTrainingManager : MonoBehaviour
{
    [Header("------ Settings -------")]
    [SerializeField] private bool m_trainNetwork;
    [SerializeField] private float m_trainingCooldownMin;
    [SerializeField] private float m_trainingCooldownMax;
    [Space]
    [SerializeField] private float m_learnRate;
    [SerializeField] private int m_batchSize;
    [Header("--- Objects ---")]
    //[SerializeField] private NeuralNetworkContainer m_container;

    [Header("------ Debug -------")]
    private SampleManager m_sampleManager;
    private NeuralNetworkVisualization m_visualization;
    private NeuralNetwork m_network;

    private float m_trainingCooldownRdy;

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
        m_trainingCooldownRdy = Time.time + Random.Range(m_trainingCooldownMin, m_trainingCooldownMax);
    }
    private void Update()
    {
        ManageTraining();
    }
    #endregion

    #region Training
    private void ManageTraining()
    {
        if (!m_trainNetwork)
            return;

        if (m_trainingCooldownRdy > Time.time)
            return;
        m_trainingCooldownRdy = Time.time + Random.Range(m_trainingCooldownMin, m_trainingCooldownMax);

        TrainNetwork();
    }
    public void TrainNetwork()
    {
        SampleContainer sampleSource = m_sampleManager.GenerateSampleSource();
        if (!sampleSource.m_isOkay)
            return;

        bool update = m_network.AddTrainingData(sampleSource.m_input, sampleSource.m_desiredOutput);

        SampleContainer sampleThis = m_sampleManager.GenerateSampleThis();
        m_visualization.UpdateActivisions(sampleThis.m_input);
        if (update)
            m_visualization.UpdateVisualization();
    }
    #endregion

    #region Misc
    public void SetNetwork(NeuralNetwork network)
    {
        m_network = network;
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
    #endregion
}
