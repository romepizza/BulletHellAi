using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetwork
{

    //private List<int> m_layerLenghts;
    //private List<List<List<float>>> m_weights;  // m_weights[layerIndex][nodeIndex][weightIndex]
    //private List<List<float>> m_biases;         //  m_biases[layerIndex][nodeIndex]

    public int[] m_layerLengths { get; private set; }
    public float[][] m_biases { get; private set; }          //  m_biases[layerIndex][nodeIndex]
    public float[][][] m_weights { get; private set; }       // m_weights[layerIndex][nodeIndex][weightIndex], m_weights[0] are the weights connecting layer one and layer two and so on
    public int m_layerCount { get; private set; }

    // Options
    private ActivisionFunctionType m_activisionFunctionType;

    // Enums
    private enum ActivisionFunctionType { Sigmoid }
    

    #region Initialization
    public NeuralNetwork(int[] layerLengths)
    {
        m_layerLengths = layerLengths;
        m_layerCount = m_layerLengths.Length;

        InitializeBiases();
        InitializeWeights();

        m_activisionFunctionType = ActivisionFunctionType.Sigmoid;
    }
    private void InitializeBiases()
    {
        // initialize biases
        m_biases = new float[m_layerCount][];
        for (int layerIndex = 1; layerIndex < m_layerCount; layerIndex++)
        {
            int nodeCount = m_layerLengths[layerIndex];
            float[] biasesPerLayer = new float[nodeCount];

            for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
            {
                biasesPerLayer[nodeIndex] = Random.Range(0f, 1f);
            }
            m_biases[layerIndex] = biasesPerLayer;
        }
    }
    private void InitializeWeights()
    {
        // initialize weights, omit the last layer
        m_weights = new float[m_layerCount][][];
        for (int layerIndex = 0; layerIndex < m_layerCount - 1; layerIndex++)
        {
            int nodeCount = m_layerLengths[layerIndex];
            float[][] weightsPerLayer = new float[nodeCount][];

            for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
            {
                int weightCount = m_layerLengths[layerIndex + 1];
                float[] weightsPerNode = new float[weightCount];
                for (int weightIndex = 0; weightIndex < weightCount; weightIndex++)
                {
                    weightsPerNode[weightIndex] = Random.Range(0f, 1f);
                }
                weightsPerLayer[nodeIndex] = weightsPerNode;
            }

            m_weights[layerIndex] = weightsPerLayer;
        }
    }
    #endregion

    #region Feed Forward
    public float[] FeedForward(float[] input)
    {
        float[] activisionsPreLayer = input;
        float[] activisionsCurrentLayer = new float[0];

        
        for(int layerIndex = 1; layerIndex < m_layerCount; layerIndex++)
        {
            int preLayerIndex = layerIndex - 1;
            int nodeCountCurrentLayer = m_layerLengths[layerIndex];
            activisionsCurrentLayer = new float[nodeCountCurrentLayer];
            for (int nodeIndex = 0; nodeIndex < nodeCountCurrentLayer; nodeIndex++)
            {
                float activisionValue = 0;

                int weightCount = m_layerLengths[layerIndex - 1];
                for(int weightIndex = 0; weightIndex < weightCount; weightIndex++)
                {
                    float weightValue = m_weights[preLayerIndex][nodeIndex][weightIndex];
                    float preLayerNodeActivision = activisionsPreLayer[nodeIndex];
                    activisionValue += preLayerNodeActivision * weightValue;
                }

                activisionsCurrentLayer[nodeIndex] = GetActivisionFunction(activisionValue);
            }

            activisionsPreLayer = activisionsCurrentLayer;
        }

        return activisionsCurrentLayer;
    }
    #endregion

    #region Training
    public void addTrainingData(float[] input, float[] desiredOutput)
    {

    }
    #endregion

    #region Activision Function(s)
    private float GetActivisionFunction(float rawValue)
    {
        if (m_activisionFunctionType == ActivisionFunctionType.Sigmoid)
            return GetSigmoid(rawValue);


        return -1;
    }
    private float GetAcctivisionFunctionPrime(float rawValue)
    {
        if (m_activisionFunctionType == ActivisionFunctionType.Sigmoid)
            return GetSigmoidPrime(rawValue);

        return -1;
    }

    // Sigmoid
    private float GetSigmoid(float rawValue)
    {
        return 1f / (1f + Mathf.Exp(-rawValue));
    }
    private float GetSigmoidPrime(float rawValue)
    {
        float sigmoid = GetSigmoid(rawValue);
        return sigmoid * (1 - sigmoid);
    }
    #endregion
}
