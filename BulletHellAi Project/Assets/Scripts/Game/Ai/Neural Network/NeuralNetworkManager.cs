using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetworkManager : MonoBehaviour
{
    private static NeuralNetworkManager s_instance;

    [Header("------- Settings -------")]

    [Header("--- KeyCodes ---")]
    [SerializeField] private KeyCode m_keyCodeCreateNeuralNetwork;

    [Header("--- Objects ---")]
    [SerializeField] private Transform m_parent;
    [SerializeField] private GameObject m_networkPrefab;

    [Header("--- Editor ---")]
    [SerializeField] private List<GameObject> m_initialNetworks;
    [SerializeField] private int[] m_onClickLayerLengths;
    [SerializeField] private int m_onClickVisualizationIndex;

    [Header("------- Debug -------")]
    private bool m_isPressingCreate;
    //private bool m_isPressingVisualize;
    private List<NeuralNetworkContainer> m_neuralNetworks = new List<NeuralNetworkContainer>();

    #region Mono
    private void Awake()
    {
        if (s_instance != null)
            Debug.Log("Warning: More than two instances of NeuralNetworkManager have been detected!");
        s_instance = this;


    }
    private void Start()
    {
        //CreateInitialNetworks();
    }
    private void Update()
    {
        //GetPlayerInput();
        //ManagePlayerInput();
    }
    #endregion

    #region Creation and Destruction of Neural Networks
    //private NeuralNetworkContainer CreateNetwork(int[] layerLengths)
    //{
    //    if (layerLengths.Length < 2)
    //    {
    //        Debug.Log("Aborted: Tried to create a neural network with less than two layers! (" + layerLengths.Length + ")");
    //        return null;
    //    }
    //    if (layerLengths[0] < 1)
    //    {
    //        Debug.Log("Aborted: Tried to create a neural network with no nodes in the input layer! (" + layerLengths[0] + ")");
    //        return null;
    //    }
    //    if (layerLengths[layerLengths.Length - 1] < 1)
    //    {
    //        Debug.Log("Aborted: Tried to create a neural network with no nodes in the output layer! (" + layerLengths[layerLengths.Length - 1] + ")");
    //        return null;
    //    }

    //    GameObject networkObject = Instantiate(m_networkPrefab, m_parent);
    //    NeuralNetworkContainer networkContainer = networkObject.GetComponent<NeuralNetworkContainer>();
    //    if (networkContainer == null)
    //    {
    //        Debug.Log("Aborted: network prefab did not contain container script!");
    //        return null;
    //    }
    //    NeuralNetwork network = new NeuralNetwork(layerLengths);
    //    networkContainer.InitializeContainer(network);
    //    m_neuralNetworks.Add(networkContainer);

    //    return networkContainer;
    //}
    //private NeuralNetworkContainer CreateNetwork(GameObject networkContainerPrefab)
    //{
    //    GameObject networkContainerObject = Instantiate(networkContainerPrefab, m_parent);
    //    NeuralNetworkContainer networkContainer = networkContainerObject.GetComponent<NeuralNetworkContainer>();
    //    if (networkContainer == null)
    //    {
    //        Debug.Log("Aborted: network prefab did not contain container script!");
    //        return null;
    //    }
    //    //NeuralNetwork network = new NeuralNetwork(networkContainer.GetLayerLengths(), networkContainer.m_trainingManager.GetLearnRate(), networkContainer.m_trainingManager.GetBatchSize());
    //    //networkContainer.InitializeContainer(network);
    //    m_neuralNetworks.Add(networkContainer);

    //    return networkContainer;
    //}
    private void DestroyNetwork(NeuralNetworkContainer network)
    {
        m_neuralNetworks.Remove(network);
    }

    //private void CreateInitialNetworks()
    //{
    //    if (m_initialNetworks == null || m_initialNetworks.Count == 0)
    //        return;

    //    for (int i = 0; i < m_initialNetworks.Count; i++)
    //    {
    //        NeuralNetworkContainer networkContainer = CreateNetwork(m_initialNetworks[i]);
    //        VisualizeNetwork(networkContainer);
    //    }
    //}
    #endregion

    #region Visualization
    private void VisualizeNetwork(NeuralNetworkContainer network)
    {
        NeuralNetworkVisualizationManager.Instance().CreateVisualization(network);
    }
    #endregion

    #region Manage Input
    //private void ManagePlayerInput()
    //{
    //    if (m_isPressingCreate)
    //    {
    //        NeuralNetworkContainer network = CreateNetwork(m_onClickLayerLengths);
    //        VisualizeNetwork(network);
    //    }
    //}
    //private void GetPlayerInput()
    //{
    //    m_isPressingCreate = false;
    //    if (Input.GetKeyDown(m_keyCodeCreateNeuralNetwork))
    //    {
    //        m_isPressingCreate = true;
    //    }

    //    //m_isPressingVisualize = false;
    //    //if(Input.GetKeyDown(m_keyCodeVisualizeNetwork))
    //    //{
    //    //    m_isPressingVisualize = true;
    //    //}
    //}
    #endregion
    
    #region Statics
    public static NeuralNetworkManager Instance()
    {
        return s_instance;
    }
    #endregion
}
