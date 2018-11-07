using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetworkContainer : MonoBehaviour
{
    [Header("------ Settings ------")]
    [SerializeField] private bool m_setInputLayerLengthDynamicly;
    [SerializeField] private int[] m_layerLengths;
    [Space]
    [SerializeField] private NeuralNetwork.ActivisionFunctionType m_activisionType;
    [SerializeField] private NeuralNetwork.CostFunctionType m_costType;
    [SerializeField] private NeuralNetwork.InitializationType m_initializationType;

    [Header("--- Objects ---")]
    //[SerializeField] private SampleManager m_sampleManager;
    private NeuralNetworkVisualization m_visualization;
    private NeuralNetworkTrainingManager m_trainingManager;
    private SampleManager m_sampleManager;

    [Header("------ Debug ------")]
    bool placeHolder;
    public NeuralNetwork m_network { get; private set; }

    #region Mono
    private void Awake()
    {
        if (m_trainingManager == null)
            m_trainingManager = GetComponent<NeuralNetworkTrainingManager>();
        if (m_trainingManager == null)
            Debug.Log("Warning: NeuralNetworkTrainingManager not found!");

        if (m_visualization == null)
            m_visualization = GetComponent<NeuralNetworkVisualization>();
        if (m_visualization == null)
            Debug.Log("Warning: NeuralNetworkVisualization not found!");

        if (m_sampleManager == null)
            m_sampleManager = GetComponent<SampleManager>();
        if (m_sampleManager == null)
            Debug.Log("Warning: SampleManager not found!");
    }
    private void Start()
    {
        if (m_setInputLayerLengthDynamicly)
            m_layerLengths[0] = m_sampleManager.GetInputLayerLengthDynamicly();

        InitializeContainer(new NeuralNetwork(
            m_layerLengths,
            m_trainingManager.GetLearnRate(),
            m_trainingManager.GetBatchSize(),
            m_activisionType,
            m_costType,
            m_initializationType));
    }
    #endregion

    #region Initialization
    public void InitializeContainer(NeuralNetwork network)
    {
        m_network = network;
        m_visualization.CreateVisualization(this);//, transform.position, transform.rotation.eulerAngles, 10f);
        m_trainingManager.SetNetwork(network);
    }
    #endregion

    #region Getter
    public SampleManager GetSampleManager()
    {
        return m_sampleManager;
    }
    public NeuralNetworkVisualization GetVisualization()
    {
        return m_visualization;
    }
    #endregion
}
