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

    [Header("--- Key Bindings ---")]
    [SerializeField] private KeyCode m_keyCodeIncreaseSize;

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
        ManageSize();
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

    #region Increase Size
    private void ManageSize()
    {
        if (!Input.GetKeyDown(m_keyCodeIncreaseSize))
            return;

        NNSaveData data = m_network.SaveData();

        int oldEnemyWidth = m_sampleManager.GetScreenshotScript().GetInputLayerLengthEnemy(0);
        int oldPlayerHeightPixel = m_sampleManager.GetScreenshotScript().GetInputLayerLengthPlayer(0, 0);

        m_sampleManager.GetScreenshotScript().SetCaptureHeight(m_sampleManager.GetScreenshotScript().GetCaptureHeight() * 2);
        m_sampleManager.GetScreenshotScript().SetCaptureWidth(m_sampleManager.GetScreenshotScript().GetCaptureWidth() * 2);
        m_sampleManager.GetScreenshotScript().SetCaptureSize();
        m_sampleManager.GetScreenshotScript().SetCaptureSizesPlayer(m_sampleManager.GetScreenshotScript().GetCaptureWidth());
        m_layerLengths[0] = m_sampleManager.GetScreenshotScript().GetInputLayerLengthTotal(0, 0);

        int newPlayerHeightPixel = m_sampleManager.GetScreenshotScript().GetInputLayerLengthPlayer(0, 0);
        int width = m_sampleManager.GetScreenshotScript().GetCaptureWidth();
        JaggedArrayContainer[] newWeights = new JaggedArrayContainer[data.m_biases[0].data.Length];

        //Debug.Log("old: " + oldPlayerHeightPixel + ", new: " + newPlayerHeightPixel);

        for (int nodeIndex = 0; nodeIndex < data.m_biases[0].data.Length; nodeIndex++)
        {
            bool addIndex = true;
            JaggedArrayContainer weights2 = new JaggedArrayContainer(m_layerLengths[0], 0);
            int index = 0;
            for (int weightIndex = 0; weightIndex < data.m_weights[0].array[0].data.Length; weightIndex++)
            {
                if (weightIndex < oldEnemyWidth)
                {
                    if (weightIndex % (width / 2) == 0 && weightIndex != 0)
                        index += width + 2;
                    else if (weightIndex != 0)
                        index += 2;
                    int[] indices = { index, index + 1, index + width, index + width + 1 };

                    //Debug.Log(weightIndex + ": (" + indices[0] + ",  " + indices[1] + ",  " + indices[2] + ", " + indices[3] + "),  " + ((nodeIndex) % (width / 2)));
                    foreach (int i in indices)
                    {
                        weights2.data[i] = data.m_weights[0].array[nodeIndex].data[weightIndex] * 0.25f;
                    }
                }
                else
                {
                    if (2 * oldPlayerHeightPixel >  newPlayerHeightPixel)
                    {
                        if (weightIndex % (width / 2) == 0 && weightIndex != 0)
                            index += width + 2;
                        else if (weightIndex != 0)
                            index = index + 2;
                        int[] indices = { index, index + 1, index + width, index + width + 1 };
                        //Debug.Log(weightIndex + ": (" + indices[0] + ",  " + indices[1] + ",  " + indices[2] + ", " + indices[3] + ")");
                        foreach (int i in indices)
                        {
                            weights2.data[i] = data.m_weights[0].array[nodeIndex].data[weightIndex] * 0.25f;
                        }
                    }
                    else// if (2 * oldPlayerHeightPixel == newPlayerHeightPixel)
                    {
                        if (addIndex)
                        {
                            index += width + 2;
                            addIndex = false;
                        }

                        //Debug.Log(weightIndex + ": (" + index + ")");
                        weights2.data[index] = data.m_weights[0].array[nodeIndex].data[weightIndex] * 0.5f;
                        weights2.data[index + 1] = data.m_weights[0].array[nodeIndex].data[weightIndex] * 0.5f;
                        index += 2;
                    }
                    //else
                        //Debug.Log("Warning!");

                    newWeights[nodeIndex] = weights2;
                }
            }
        }

        data.m_weights[0].array = newWeights;

        //Debug.Log(data.m_weights[0].array[0].data.Length);

        NNCSaveData containerData = new NNCSaveData
        {
            m_trainingData = m_trainingManager.SaveData(),
            m_sampleData = m_sampleManager.SaveData(),
            m_visuilizationData = m_visualization.SaveData(),
            m_networkData = data,


            m_layerLengths = m_layerLengths,
            m_dataFileName = m_dataFileName,
            m_activisionType = m_activisionType,
            m_activisionTypeOutput = m_activisionTypeOutput,
            m_costType = m_costType,
            m_initializationType = m_initializationType,
            m_activisionConstant = m_activisionConstant
        };

        LoadContainer(containerData);

        //m_network.LoadData(data);
        //m_visualization.ApplyData();
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
