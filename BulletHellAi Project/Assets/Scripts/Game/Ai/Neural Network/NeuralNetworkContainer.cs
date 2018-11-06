using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetworkContainer : MonoBehaviour
{
    [Header("------ Settings ------")]
    [SerializeField] private int[] m_layerLengths;


    [Header("--- Objects ---")]
    //[SerializeField] private SampleManager m_sampleManager;
    private NeuralNetworkVisualization m_visualization;
    public NeuralNetworkTrainingManager m_trainingManager { get; private set; }

    [Header("------ Debug ------")]
    bool placeHolder;
    public NeuralNetwork m_network { get; private set; }

    #region Mono
    private void Start()
    {
        if (m_trainingManager == null)
            m_trainingManager = GetComponent<NeuralNetworkTrainingManager>();
        if (m_trainingManager == null)
            Debug.Log("Warning: NeuralNetworkTrainingManager not found!");

        if (m_visualization == null)
            m_visualization = GetComponent<NeuralNetworkVisualization>();
        if (m_visualization == null)
            Debug.Log("Warning: NeuralNetworkVisualization not found!");

        InitializeContainer(new NeuralNetwork(
            m_layerLengths,
            m_trainingManager.GetLearnRate(),
            m_trainingManager.GetBatchSize(),
            NeuralNetwork.ActivisionFunctionType.Sigmoid,
            NeuralNetwork.CostFunctionType.Quadratic,
            NeuralNetwork.InitializationType.Random));
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
    public int[] GetLayerLengths()
    {
        return m_layerLengths;
    }
    #endregion
}
