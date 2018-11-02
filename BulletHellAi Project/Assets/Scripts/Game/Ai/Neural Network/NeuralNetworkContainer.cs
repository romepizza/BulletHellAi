using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetworkContainer : MonoBehaviour
{
    [Header("------ Settings ------")]
    [SerializeField] private int[] m_layerLengths;


    [Header("--- Objects ---")]
    //[SerializeField] private SampleManager m_sampleManager;
    //[SerializeField] private NeuralNetworkTrainingManager m_trainingManager;

    [Header("------ Debug ------")]
    bool placeHolder;
    public NeuralNetwork m_network { get; private set; }


    #region Initialization
    public void InitializeContainer(NeuralNetwork network)
    {
        m_network = network;
    }
    #endregion

    #region Getter
    public int[] GetLayerLengths()
    {
        return m_layerLengths;
    }
    #endregion
}
