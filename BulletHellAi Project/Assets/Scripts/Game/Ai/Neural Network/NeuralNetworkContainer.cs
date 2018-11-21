using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public struct NNCSaveData
{
    public NNTMSaveData m_trainingData;
    public NNSMSaveData m_sampleData;
    public NNVSaveData m_visuilizationData;
    public NNSaveData m_networkData;

    public int[] m_layerLengths;
    public string m_dataFileName;
    public NeuralNetwork.ActivisionFunctionType m_activisionType;
    public NeuralNetwork.CostFunctionType m_costType;
    public NeuralNetwork.InitializationType m_initializationType;
}

public class NeuralNetworkContainer : MonoBehaviour
{
    public int testVar;
    [Header("------ Settings ------")]
    //[SerializeField] private bool m_setInputLayerLengthDynamicly;
    [SerializeField] private int[] m_layerLengths;
    [Space]
    [SerializeField] private NeuralNetwork.ActivisionFunctionType m_activisionType;
    [SerializeField] private NeuralNetwork.CostFunctionType m_costType;
    [SerializeField] private NeuralNetwork.InitializationType m_initializationType;

    [Header("--- Save / Load ---")]
    [SerializeField] private string m_dataFileName;

    [Header("--- Objects ---")]
    //[SerializeField] private SampleManager m_sampleManager;
    bool placeHolder0;
    public NeuralNetworkVisualization m_visualization { get; private set; }
    public NeuralNetworkTrainingManager m_trainingManager { get; private set; }
    public SampleManager m_sampleManager { get; private set; }

    [Header("------ Debug ------")]
    bool placeHolder1;
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
        if (m_layerLengths[0] <= 0)
            m_layerLengths[0] = m_sampleManager.GetInputLayerLengthDynamicly();
        if (m_layerLengths[m_layerLengths.Length - 1] <= 0)
            m_layerLengths[m_layerLengths.Length - 1] = ScreenshotManager.Instance().GetOutputNumber();
        
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

        SaveContainer(m_dataFileName);
        LoadContainer(m_dataFileName);
    }
    #endregion

    #region Save / Load
    private void LoadData(NNCSaveData data)
    {
        m_layerLengths = data.m_layerLengths;
        m_dataFileName = data.m_dataFileName;
        m_activisionType = data.m_activisionType;
        m_costType = data.m_costType;
        m_initializationType = data.m_initializationType;
    }
    private void ApplyData()
    {

    }

    private void SaveContainer(string fileName)
    {
        NNCSaveData data = new NNCSaveData {
            m_trainingData = m_trainingManager.SaveData(),
            m_sampleData = m_sampleManager.SaveData(),
            m_visuilizationData = m_visualization.SaveData(),
            m_networkData = m_network.SaveData(),

            m_layerLengths = m_layerLengths,
            m_dataFileName = fileName,
            m_activisionType = m_activisionType,
            m_costType = m_costType,
            m_initializationType = m_initializationType
        };


        NeuralNetworkData.Save(data, fileName);
    }
    private void LoadContainer(string fileName)
    {
        NNCSaveData data = NeuralNetworkData.Load(fileName);

        LoadData(data);
        m_trainingManager.LoadData(data.m_trainingData);
        m_sampleManager.LoadData(data.m_sampleData);
        m_visualization.LoadData(data.m_visuilizationData);
        m_network.LoadData(data.m_networkData);

        ApplyData();
        m_trainingManager.ApplyData();
        m_sampleManager.ApplyData();
        m_visualization.ApplyData();
        m_network.ApplyData();
    }
    private void ApplyContainer(NeuralNetworkData network)
    {

    }
    #endregion

    //#region Getter
    //public SampleManager GetSampleManager()
    //{
    //    return m_sampleManager;
    //}
    //public NeuralNetworkTrainingManager GetTrainingManager()
    //{
    //    return m_trainingManager;
    //}
    //public NeuralNetworkVisualization GetVisualization()
    //{
    //    return m_visualization;
    //}
    //#endregion
}
