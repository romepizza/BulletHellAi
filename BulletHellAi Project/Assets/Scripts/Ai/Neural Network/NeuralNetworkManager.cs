using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetworkManager : MonoBehaviour
{
    private static NeuralNetworkManager s_instance;

    [Header("------- Settings -------")]

    [Header("--- KeyCodes ---")]
    [SerializeField] private KeyCode m_keyCodeCreateNeuralNetwork;
    [SerializeField] private KeyCode m_keyCodeVisualizeNetwork;

    [Header("--- Editor ---")]
    [SerializeField] private int[] m_onClickLayerLengths;
    [SerializeField] private int m_onClickVisualizationIndex;

    [Header("------- Debug -------")]
    private bool m_isPressingCreate;
    private bool m_isPressingVisualize;
    private List<NeuralNetwork> m_neuralNetworks = new List<NeuralNetwork>();

    #region Mono
    private void Awake()
    {
        if (s_instance != null)
            Debug.Log("Warning: More than two instances of NeuralNetworkManager have been detected!");
        s_instance = this;
    }
 //   private void Start ()
 //   {
 //       initializeStuff();
	//}
 //   private void initializeStuff()
 //   {
 //   }
    private void Update()
    {
        GetPlayerInput();
        ManagePlayerInput();
    }
    #endregion

    #region Creation and Destruction of Neural Networks
    private void CreateNetwork(int[] layerLengths)
    {
        if (layerLengths.Length < 2)
        {
            Debug.Log("Aborted: Tried to create a neural network with less than two layers! (" + layerLengths.Length + ")");
            return;
        }
        if (layerLengths[0] < 1)
        {
            Debug.Log("Aborted: Tried to create a neural network with no nodes in the input layer! (" + layerLengths[0] + ")");
            return;
        }
        if (layerLengths[layerLengths.Length - 1] < 1)
        {
            Debug.Log("Aborted: Tried to create a neural network with no nodes in the output layer! (" + layerLengths[layerLengths.Length - 1] + ")");
            return;
        }


        NeuralNetwork network = new NeuralNetwork(layerLengths);
        m_neuralNetworks.Add(network);
    }
    private void DestroyNetwork(NeuralNetwork network)
    {
        m_neuralNetworks.Remove(network);
    }
    #endregion

    #region Visualization
    private void VisualizeNetwork(NeuralNetwork network)
    {
        NeuralNetworkVisualizationManager.Instance().CreateVisualization(network);
    }
    #endregion

    #region Manage Input
    private void ManagePlayerInput()
    {
        if (m_isPressingCreate)
            CreateNetwork(m_onClickLayerLengths);
        
        if(m_isPressingVisualize)
        {
            if (m_onClickVisualizationIndex <= m_neuralNetworks.Count)
                VisualizeNetwork(m_neuralNetworks[m_onClickVisualizationIndex]);
        }
    }

    private void GetPlayerInput()
    {
        m_isPressingCreate = false;
        if (Input.GetKeyDown(m_keyCodeCreateNeuralNetwork))
        {
            m_isPressingCreate = true;
        }

        m_isPressingVisualize = false;
        if(Input.GetKeyDown(m_keyCodeVisualizeNetwork))
        {
            m_isPressingVisualize = true;
        }
    }
    #endregion

    #region Statics
    public static NeuralNetworkManager Instance()
    {
        return s_instance;
    }
    #endregion
}
