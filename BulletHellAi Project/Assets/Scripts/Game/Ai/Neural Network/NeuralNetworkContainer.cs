using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public struct NNCSaveData
{
    public bool m_isCorrupted;

    public NNTMSaveData m_trainingData;
    public NNSMSaveData m_sampleData;
    public NNVSaveData m_visuilizationData;
    public NNSaveData m_networkData;

    public int[] m_layerLengths;
    public string m_dataFileName;
    public NeuralNetwork.ActivisionFunctionType m_activisionType;
    public NeuralNetwork.ActivisionFunctionType m_activisionTypeOutput;
    public NeuralNetwork.CostFunctionType m_costType;
    public NeuralNetwork.InitializationType m_initializationType;

    public float m_activisionConstant;
}

public class NeuralNetworkContainer : MonoBehaviour
{
    [Header("------ Settings ------")]
    //[SerializeField] private bool m_setInputLayerLengthDynamicly;
    [SerializeField] private int[] m_layerLengths;
    [Space]
    [SerializeField] private NeuralNetwork.ActivisionFunctionType m_activisionType;
    [SerializeField] private NeuralNetwork.ActivisionFunctionType m_activisionTypeOutput;
    [SerializeField] private NeuralNetwork.CostFunctionType m_costType;
    [SerializeField] private NeuralNetwork.InitializationType m_initializationType;

    [Header("--- Other Hyperparameter ---")]
    [SerializeField] private float m_activisionConstant;

    [Header("--- Save / Load ---")]
    [SerializeField] private bool m_save;
    [SerializeField] private bool m_loadFile;
    [SerializeField] private TextAsset m_dataFile;
    [SerializeField] private bool m_loadPath;
    [SerializeField] private string m_dataFileName;

    [Header("--- Objects ---")]
    [SerializeField] private AiMovement m_movementManager;

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
            m_trainingManager.GetDropoutRate(),
            m_trainingManager.GetWeightDecayRate(),
            m_activisionType,
            m_activisionTypeOutput,
            m_costType,
            m_initializationType,
            m_activisionConstant));

        //SaveContainer(m_dataFileName);
    }
    private void Update()
    {
        CheckLoadOrSave();
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

    #region Save / Load
    private void LoadData(NNCSaveData data)
    {
        m_layerLengths = data.m_layerLengths;
        //m_dataFileName = data.m_dataFileName;
        m_activisionType = data.m_activisionType;
        m_activisionTypeOutput = data.m_activisionTypeOutput;
        m_costType = data.m_costType;
        m_initializationType = data.m_initializationType;

        m_activisionConstant = data.m_activisionConstant;
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
            m_activisionTypeOutput = m_activisionTypeOutput,
            m_costType = m_costType,
            m_initializationType = m_initializationType,
            m_activisionConstant = m_activisionConstant
        };


        NeuralNetworkData.Save(data, fileName);
    }
    private void LoadContainer(NNCSaveData data)
    {
        if (data.m_isCorrupted)
        {
            Debug.Log("Aborted: loading data is corrupted!");
            return;
        }

        LoadData(data);
        m_trainingManager.LoadData(data.m_trainingData);
        m_sampleManager.LoadData(data.m_sampleData);
        m_visualization.LoadData(data.m_visuilizationData);
        m_network.LoadData(data.m_networkData);

        InitializeContainer(new NeuralNetwork(
            data.m_networkData,
            m_layerLengths,
            m_trainingManager.GetLearnRate(),
            m_trainingManager.GetBatchSize(),
            m_trainingManager.GetDropoutRate(),
            m_trainingManager.GetWeightDecayRate(),
            m_activisionType,
            m_activisionTypeOutput,
            m_costType,
            m_initializationType,
            m_activisionConstant
            ));

        ApplyData();
        m_trainingManager.ApplyData();
        m_sampleManager.ApplyData();
        m_visualization.ApplyData();
        m_network.ApplyData();
    }

    private void CheckLoadOrSave()
    {
        if(m_save)
        {
            SaveContainer(m_dataFileName);
            m_save = false;
        }
        if (m_loadPath)
        {
            LoadContainer(NeuralNetworkData.Load(m_dataFileName));
            m_loadPath = false;
        }
        if(m_loadFile)
        {
            LoadContainer(NeuralNetworkData.Load(m_dataFile));
            m_loadFile = false;
        }
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
