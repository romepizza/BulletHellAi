using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetwork
{

    // Options
    private float m_learnRate;
    private int m_batchSize;
    private ActivisionFunctionType m_activisionFunctionType;
    private CostFunctionType m_costFunctionType;
    private InitializationType m_initializationType;

    // Basic Architecture
    public int[] m_layerLengths { get; private set; }
    public MyMatrix[] m_biases { get; private set; }          //  m_biases[layerIndex][nodeIndex]
    public MyMatrix[] m_weights { get; private set; }       // m_weights[layerIndex][nodeIndex][weightIndex], m_weights[0] are the weights connecting layer one and layer two and so on
    public int m_layerCount { get; private set; }

    // Back Propagation
    private MyMatrix[] m_newBiases;
    private MyMatrix[] m_newWeights;

    private MyMatrix[] m_deltaActivision;
    private MyMatrix[] m_deltaBiases;
    private MyMatrix[] m_deltaWeights;

    private MyMatrix[] m_rawValues;
    private MyMatrix[] m_activisionValues;

    // Batching
    private float[][] m_batchInputs;
    private float[][] m_batchDesiredOutputs;
    private int m_currentBatchIndex;

    // Enums
    public enum ActivisionFunctionType { Sigmoid }
    public enum CostFunctionType { Quadratic }
    public enum InitializationType { Zero, Random }
    

    #region Initialization
    public NeuralNetwork(
        int[] layerLengths,
        float learnRate,
        int batchSize,
        ActivisionFunctionType activisionType,
        CostFunctionType costType,
        InitializationType initializationType)
    {
        m_layerLengths = layerLengths;
        m_layerCount = m_layerLengths.Length;
        m_learnRate = learnRate;

        if(batchSize <= 0)
        {
            Debug.Log("Info: batch size was <= 0 (" + batchSize + "). It was set to a default value of 1!");
            m_batchSize = 1;
        }
        m_batchSize = batchSize;

        m_activisionFunctionType = activisionType;
        m_costFunctionType = costType;
        m_initializationType = initializationType;

        InitializeBiases();
        InitializeWeights();
        InitializeBatch();

        InitializeBackPropagation();

        //m_activisionFunctionType = ActivisionFunctionType.Sigmoid;
    }
    //private void InitializeBiases()
    //{
    //    // initialize biases
    //    m_biases = new float[m_layerCount][];
    //    for (int layerIndex = 1; layerIndex < m_layerCount; layerIndex++)
    //    {
    //        int nodeCount = m_layerLengths[layerIndex];
    //        float[] biasesPerLayer = new float[nodeCount];

    //        for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
    //        {
    //            biasesPerLayer[nodeIndex] = Random.Range(0f, 1f);
    //        }
    //        m_biases[layerIndex] = biasesPerLayer;
    //    }
    //}
    //private void InitializeWeights()
    //{
    //    // initialize weights, omit the last layer
    //    m_weights = new float[m_layerCount][][];
    //    for (int layerIndex = 0; layerIndex < m_layerCount - 1; layerIndex++)
    //    {
    //        int nodeCount = m_layerLengths[layerIndex];
    //        float[][] weightsPerLayer = new float[nodeCount][];

    //        for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
    //        {
    //            int weightCount = m_layerLengths[layerIndex + 1];
    //            float[] weightsPerNode = new float[weightCount];
    //            for (int weightIndex = 0; weightIndex < weightCount; weightIndex++)
    //            {
    //                weightsPerNode[weightIndex] = Random.Range(0f, 1f);
    //            }
    //            weightsPerLayer[nodeIndex] = weightsPerNode;
    //        }

    //        m_weights[layerIndex] = weightsPerLayer;
    //    }
    //}
    //private void InitializeBatch()
    //{
    //    m_currentBatchIndex = 0;

    //    m_batchInputs = new float[m_batchSize][];
    //    int nodeCount = m_layerLengths[m_layerCount - 1];
    //    for (int batchIndex = 0; batchIndex < m_batchSize; batchIndex++)
    //    {
    //        float[] outputNodes = new float[nodeCount];
    //        m_batchInputs[batchIndex] = outputNodes;
    //    }

    //    m_batchDesiredOutputs = new float[m_batchSize][];
    //    for (int batchIndex = 0; batchIndex < m_batchSize; batchIndex++)
    //    {
    //        float[] outputNodes = new float[nodeCount];
    //        m_batchDesiredOutputs[batchIndex] = outputNodes;
    //    }
    //}
    private void InitializeBiases()
    {
        m_biases = new MyMatrix[m_layerCount - 1];
        for(int layerIndex = 1; layerIndex < m_layerCount; layerIndex++)
        {
            int nodeCount = m_layerLengths[layerIndex];
            MyMatrix mat = new MyMatrix(nodeCount, 1);
            if (m_initializationType == InitializationType.Random)
                mat.SetRandomValues(0f, 1f);
            m_biases[layerIndex - 1] = mat;
        }
    }
    private void InitializeWeights()
    {
        m_weights = new MyMatrix[m_layerCount - 1];
        for (int layerIndex = 0; layerIndex < m_layerCount - 1; layerIndex++)
        {
            int nodeCount = m_layerLengths[layerIndex + 1];
            int weightCount = m_layerLengths[layerIndex];
            MyMatrix mat = new MyMatrix(nodeCount, weightCount);
            if (m_initializationType == InitializationType.Random)
                mat.SetRandomValues(0f, 1f);
            m_weights[layerIndex] = mat;
        }
    }
    private void InitializeBatch()
    {
        m_currentBatchIndex = 0;

        m_batchInputs = new float[m_batchSize][];
        int nodeCount = m_layerLengths[m_layerCount - 1];
        for (int batchIndex = 0; batchIndex < m_batchSize; batchIndex++)
        {
            float[] outputNodes = new float[nodeCount];
            m_batchInputs[batchIndex] = outputNodes;
        }

        m_batchDesiredOutputs = new float[m_batchSize][];
        for (int batchIndex = 0; batchIndex < m_batchSize; batchIndex++)
        {
            float[] outputNodes = new float[nodeCount];
            m_batchDesiredOutputs[batchIndex] = outputNodes;
        }
    }
    private void InitializeBackPropagation()
    {
        // biases
        m_newBiases = new MyMatrix[m_biases.Length];
        for(int layerIndex = 0; layerIndex < m_newBiases.Length; layerIndex++)
        {
            m_newBiases[layerIndex] = new MyMatrix(m_biases[layerIndex], false);
        }

        m_deltaBiases = new MyMatrix[m_biases.Length];
        for (int layerIndex = 0; layerIndex < m_deltaBiases.Length; layerIndex++)
        {
            m_deltaBiases[layerIndex] = new MyMatrix(m_biases[layerIndex], false);
        }

        // weights
        m_newWeights = new MyMatrix[m_weights.Length];
        for (int layerIndex = 0; layerIndex < m_newWeights.Length; layerIndex++)
        {
            m_newWeights[layerIndex] = new MyMatrix(m_weights[layerIndex], false);
        }

        m_deltaWeights = new MyMatrix[m_weights.Length];
        for (int layerIndex = 0; layerIndex < m_deltaWeights.Length; layerIndex++)
        {
            m_deltaWeights[layerIndex] = new MyMatrix(m_weights[layerIndex], false);
        }

        // activisions
        m_deltaActivision = new MyMatrix[m_biases.Length];
        for (int layerIndex = 0; layerIndex < m_deltaActivision.Length; layerIndex++)
        {
            m_deltaActivision[layerIndex] = new MyMatrix(m_biases[layerIndex], false);
        }

        m_rawValues = new MyMatrix[m_layerCount - 1];
        for(int layerIndex = 0; layerIndex < m_rawValues.Length; layerIndex++)
        {
            m_rawValues[layerIndex] = new MyMatrix(m_layerLengths[layerIndex + 1], 1);
        }

        m_activisionValues = new MyMatrix[m_layerCount];
        for (int layerIndex = 0; layerIndex < m_activisionValues.Length; layerIndex++)
        {
            m_activisionValues[layerIndex] = new MyMatrix(m_layerLengths[layerIndex], 1);
        }
    }
    #endregion

    #region Feed Forward
    private MyMatrix FeedForward(float[] input)
    {
        MyMatrix output = new MyMatrix(input.Length, 1);

        return output;
    }
    //private MyMatrix[] GetRawValuesAndActivisions(float[] input)
    //{
    //    MyMatrix[] mats = new MyMatrix[m_layerCount];

    //    MyMatrix mat = new MyMatrix(m_layerLengths[0], 2);
    //    mat.SetColumn(input, 0);
    //    mat.SetColumn(input, 1);
    //    mats[0] = mat;
        
    //    for(int layerIndex = 1; layerIndex < mats.Length; layerIndex++)
    //    {
    //        int nodeCount = m_layerLengths[layerIndex];
    //        mat = new MyMatrix(nodeCount, 1);

    //        mats[layerIndex] = mat;
    //    }
    //    return mats;
    //}
    //private float[] GetOutput(float[] input)
    //{
    //    float[] activisionsPreLayer = input;
    //    float[] activisionsCurrentLayer = new float[0];

    //    // feed forward
    //    for(int layerIndex = 1; layerIndex < m_layerCount; layerIndex++)
    //    {
    //        int preLayerIndex = layerIndex - 1;
    //        int nodeCountCurrentLayer = m_layerLengths[layerIndex];
    //        activisionsCurrentLayer = new float[nodeCountCurrentLayer];
    //        for (int nodeIndex = 0; nodeIndex < nodeCountCurrentLayer; nodeIndex++)
    //        {
    //            float activisionValue = 0;

    //            int weightCount = m_layerLengths[preLayerIndex];
    //            for(int weightIndex = 0; weightIndex < weightCount; weightIndex++)
    //            {
    //                int actualNodeIndex = weightIndex;
    //                int actualWeightIndex = nodeIndex;

    //                float weightValue = m_weights[preLayerIndex][actualNodeIndex][actualWeightIndex];
    //                float nodeActivision = activisionsPreLayer[actualNodeIndex];
    //                float rawValue = weightValue * nodeActivision + m_biases[layerIndex][nodeIndex];
    //                activisionValue += rawValue;
    //            }

    //            activisionsCurrentLayer[nodeIndex] = GetActivisionFunction(activisionValue);
    //        }

    //        activisionsPreLayer = activisionsCurrentLayer;
    //    }

    //    return activisionsCurrentLayer;
    //}
    //private float[][] GetActivisions(float[] input)
    //{
    //    float[][] activisions = new float[m_layerCount][];
    //    activisions[0] = input;
    //    //float[] activisionsPreLayer = input;
    //    //float[] activisionsCurrentLayer = new float[0];

    //    // feed forward
    //    for (int layerIndex = 1; layerIndex < m_layerCount; layerIndex++)
    //    {
    //        int preLayerIndex = layerIndex - 1;
    //        int nodeCountCurrentLayer = m_layerLengths[layerIndex];
    //        float[] activisionsCurrentLayer = new float[nodeCountCurrentLayer];
    //        for (int nodeIndex = 0; nodeIndex < nodeCountCurrentLayer; nodeIndex++)
    //        {
    //            float activisionValue = 0;

    //            int weightCount = m_layerLengths[preLayerIndex];
    //            for (int weightIndex = 0; weightIndex < weightCount; weightIndex++)
    //            {
    //                int actualNodeIndex = weightIndex;
    //                int actualWeightIndex = nodeIndex;

    //                float weightValue = m_weights[preLayerIndex][actualNodeIndex][actualWeightIndex];
    //                float nodeActivision = activisions[preLayerIndex][actualNodeIndex];
    //                float rawValue = weightValue * nodeActivision + m_biases[layerIndex][nodeIndex];
    //                activisionValue += rawValue;
    //            }

    //            activisionsCurrentLayer[nodeIndex] = GetActivisionFunction(activisionValue);
    //        }

    //        activisions[layerIndex] = activisionsCurrentLayer;
    //    }

    //    return activisions;
    //}
    #endregion

    #region Training
    public bool AddTrainingData(float[] input, float[] desiredOutput)
    {
        m_batchInputs[m_currentBatchIndex] = input;
        m_batchDesiredOutputs[m_currentBatchIndex] = desiredOutput;
        m_currentBatchIndex++;
        if(m_currentBatchIndex >= m_batchSize)
        {
            PerformBackPropagation();
            InitializeBatch();
            return true;
        }
        return false;
    }
    public void PerformBackPropagation()
    {
        ClearMatrixArray(m_newBiases);
        ClearMatrixArray(m_newWeights);

        for (int batchIndex = 0; batchIndex < m_batchSize; batchIndex++)
        {
            // clear delta matrices
            ClearMatrixArray(m_deltaBiases);
            ClearMatrixArray(m_deltaWeights);
            ClearMatrixArray(m_deltaActivision);
            ClearMatrixArray(m_rawValues);
            ClearMatrixArray(m_activisionValues);

            // feed forward, get all raw values and activision values
            m_activisionValues[0] = new MyMatrix(m_batchInputs[batchIndex]);
            for(int layerIndex = 0; layerIndex < m_layerCount - 1; layerIndex++)
            {
                //MyMatrix mat = MyMatrix.Dot(m_weights[layerIndex], m_activisionValues[layerIndex]);
                //mat = MyMatrix.AddMatrix(mat, m_biases[layerIndex]);

                m_rawValues[layerIndex] = MyMatrix.AddMatrix(MyMatrix.Dot(m_weights[layerIndex], m_activisionValues[layerIndex]), m_biases[layerIndex]);
                m_activisionValues[layerIndex + 1] = GetActivisionFunction(m_rawValues[layerIndex]);
            }

            // back pass, start at the last layer (manually) and loop over the prelayers afterwards
            m_deltaActivision[m_layerCount - 2] = MyMatrix.MultiplyElementWise(GetCostDrivative(m_batchDesiredOutputs[batchIndex], m_activisionValues[m_layerCount - 1]),  GetSigmoidPrime(m_rawValues[m_layerCount - 2]));
            m_newBiases[m_layerCount - 2] = m_deltaActivision[m_layerCount - 2];
            m_newWeights[m_layerCount - 2] = MyMatrix.Dot(m_deltaActivision[m_layerCount - 2], MyMatrix.Transposed(m_activisionValues[m_layerCount - 3]));
            
            for(int layerIndex = m_layerCount - 2; layerIndex > 0; layerIndex--)
            {
                MyMatrix mat1 = MyMatrix.Transposed(m_weights[layerIndex - 1 + 1]);
                MyMatrix mat2 = m_deltaActivision[layerIndex - 1];
                MyMatrix mat3 = MyMatrix.Dot(mat1, mat2);
                MyMatrix mat4 = GetActivisionFunctionPrime(m_rawValues[layerIndex - 1]);
                MyMatrix mat5 = MyMatrix.MultiplyElementWise(mat3, mat4);

                m_deltaActivision[layerIndex - 1] = mat5;
                m_newBiases[layerIndex - 1] = m_deltaActivision[layerIndex - 1];
                m_newWeights[layerIndex - 1] = MyMatrix.Dot(m_deltaActivision[layerIndex - 1], MyMatrix.Transposed(m_activisionValues[layerIndex - 2 + 1]));

                //m_deltaActivision[layerIndex] = MyMatrix.MultiplyElementWise(MyMatrix.Dot(MyMatrix.Transposed(m_weights[layerIndex + 1]), m_deltaActivision[layerIndex]), GetActivisionFunctionPrime(m_rawValues[layerIndex]));
                //m_newBiases[layerIndex] = m_deltaActivision[layerIndex];
                //m_newWeights[layerIndex] = MyMatrix.Dot(m_deltaActivision[layerIndex], MyMatrix.Transposed(m_activisionValues[layerIndex - 1 + 1]));
            }


            // finally add the gradient to the current sum of changes
            for (int layerIndex = 0; layerIndex < m_newBiases.Length; layerIndex++)
                m_newBiases[layerIndex].AddMatrix(m_deltaBiases[layerIndex]);
            for (int layerIndex = 0; layerIndex < m_newWeights.Length; layerIndex++)
                m_newWeights[layerIndex].AddMatrix(m_deltaWeights[layerIndex]);
        }

        // set the final values
        for(int layerIndex = 0; layerIndex < m_biases.Length; layerIndex++)
        {
            m_newBiases[layerIndex].MultiplyByFactor(m_learnRate / m_batchSize);
            m_biases[layerIndex].AddMatrix(m_newBiases[layerIndex]);
        }
        for (int layerIndex = 0; layerIndex < m_weights.Length; layerIndex++)
        {
            m_newWeights[layerIndex].MultiplyByFactor(m_learnRate / m_batchSize);
            m_weights[layerIndex].AddMatrix(m_newWeights[layerIndex]);
        }
    }
    //public void PerformBackPropagation()
    //{
    //    InitializeBackPropagation();

    //    // perform back propagation on each training example and collect the sum of their error gradients
    //    for (int batchIndex = 0; batchIndex < m_batchSize; batchIndex++)
    //    {
    //        // set the values for the output layer first manually
    //        int nodeLayerIndex = m_layerCount - 1;
    //        int nodeCount = m_layerLengths[nodeLayerIndex];
    //        float[][] activisions = GetActivisions(m_batchInputs[batchIndex]);
    //        float[] desiredOutputs = m_batchDesiredOutputs[batchIndex];
    //        for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
    //        {
    //            float actualOutput = activisions[m_layerCount - 1][nodeIndex];
    //            float desiredOutput = desiredOutputs[nodeIndex];
    //            m_deltaActivision[nodeLayerIndex][nodeIndex] = GetCostDerivative(desiredOutput, actualOutput);
    //            m_deltaBiases[nodeLayerIndex][nodeIndex] = m_deltaActivision[nodeLayerIndex][nodeIndex];
    //            // Most of the time, we itereate through the nodes in the next layer and interprete them as the weight index, since we want to have all weights outgoing from
    //            // one specific node. Now though, we want to have all weights going into a specific node. We achieve this by simply flipping the node index and the weight index.
    //            int weightLayerIndex = nodeLayerIndex - 1;
    //            int weightCount = m_layerLengths[weightLayerIndex];
    //            for(int weightIndex = 0; weightIndex < weightCount; weightIndex++)
    //            {
    //                int actualNodeIndex = weightIndex;
    //                int actualWeightIndex = nodeIndex;

    //                float value = m_deltaActivision[nodeLayerIndex][nodeIndex] * m_weights[weightLayerIndex][actualNodeIndex][actualWeightIndex];
    //                m_deltaWeights[weightLayerIndex][actualNodeIndex][actualWeightIndex] = value;
    //            }
    //        }

    //        // set values for the predecessors
    //        for(int layerIndex = m_layerCount - 2; layerIndex >= 0; layerIndex--)
    //        {
    //            nodeCount = m_layerLengths[layerIndex];
    //            int weightLayerIndex = layerIndex - 1;
    //            for(int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
    //            {

    //            }
    //        }

    //        // apply gradient
    //    }

    //    // apply the new values to the network
    //}
    //public void InitializeBackPropagation()
    //{
    //    m_newBiases = new float[m_layerCount][];
    //    for(int layerIndex = 0; layerIndex < m_layerCount; layerIndex++)
    //    {
    //        int nodeCount = m_layerLengths[layerIndex];
    //        float[] newNodes = new float[nodeCount];
    //        m_newBiases[layerIndex] = newNodes;
    //    }

    //    m_newWeights = new float[m_layerCount][][];
    //    for(int layerIndex = 0; layerIndex < m_layerCount - 1; layerIndex++)
    //    {
    //        int nodeCount = m_layerLengths[layerIndex];
    //        float[][] newNodes = new float[nodeCount][];
    //        int weightCount = m_layerLengths[layerIndex + 1];
    //        for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
    //        {
    //            float[] newWeights = new float[weightCount];
    //            newNodes[nodeIndex] = newWeights;
    //        }
    //        m_newWeights[layerIndex] = newNodes;
    //    }

    //    m_deltaActivision = new float[m_layerCount][];
    //    for(int layerIndex = 0; layerIndex < m_layerCount; layerIndex++)
    //    {
    //        int nodeCount = m_layerLengths[layerIndex];
    //        float[] deltaOutputNodes = new float[nodeCount];
    //        m_deltaActivision[layerIndex] = deltaOutputNodes;
    //    }

    //    m_deltaBiases = new float[m_layerCount][];
    //    for(int layerIndex = 0; layerIndex < m_layerCount; layerIndex++)
    //    {
    //        int nodeCount = m_layerLengths[layerIndex];
    //        float[] deltaBiasNodes = new float[nodeCount];
    //        m_deltaBiases[layerIndex] = deltaBiasNodes;
    //    }

    //    m_deltaWeights = new float[m_layerCount][][];
    //    for (int layerIndex = 0; layerIndex < m_layerCount - 1; layerIndex++)
    //    {
    //        int nodeCount = m_layerLengths[layerIndex];
    //        float[][] deltaWeightNodes = new float[nodeCount][];
    //        int weightCount = m_layerLengths[layerIndex + 1];
    //        for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
    //        {
    //            float[] deltaWeightWeights = new float[weightCount];
    //            deltaWeightNodes[nodeIndex] = deltaWeightWeights;
    //        }
    //        m_deltaWeights[layerIndex] = deltaWeightNodes;
    //    }
    //}
    #endregion

    #region Activision Function(s)
    // Generic
    //private float GetActivisionFunction(float rawValue)
    //{
    //    if (m_activisionFunctionType == ActivisionFunctionType.Sigmoid)
    //        return GetSigmoid(rawValue);

    //    return -1;
    //}
    private MyMatrix GetActivisionFunction(MyMatrix mat)
    {
        if (m_activisionFunctionType == ActivisionFunctionType.Sigmoid)
            return GetSigmoid(mat);

        return null;
    }
    //private float GetActivisionFunctionPrime(float rawValue)
    //{
    //    if (m_activisionFunctionType == ActivisionFunctionType.Sigmoid)
    //        return GetSigmoidPrime(rawValue);

    //    return -1;
    //}
    private MyMatrix GetActivisionFunctionPrime(MyMatrix mat)
    {
        if (m_activisionFunctionType == ActivisionFunctionType.Sigmoid)
            return GetSigmoidPrime(mat);

        return null;
    }

    // Sigmoid
    private float GetSigmoid(float rawValue)
    {
        return 1f / (1f + Mathf.Exp(-rawValue));
    }
    private MyMatrix GetSigmoid(MyMatrix mat)
    {
        MyMatrix newMat = new MyMatrix(mat.m_rowCountY, mat.m_columnCountX);
        for(int y = 0; y < newMat.m_rowCountY; y++)
        {
            for(int x = 0; x < newMat.m_columnCountX; x++)
            {
                newMat.m_data[y][x] = GetSigmoid(mat.m_data[y][x]);
            }
        }
        return newMat;
    }

    private float GetSigmoidPrime(float rawValue)
    {
        float sigmoid = GetSigmoid(rawValue);
        return sigmoid * (1 - sigmoid);
    }
    private MyMatrix GetSigmoidPrime(MyMatrix mat)
    {
        MyMatrix newMat = new MyMatrix(mat.m_rowCountY, mat.m_columnCountX);
        for (int y = 0; y < newMat.m_rowCountY; y++)
        {
            for (int x = 0; x < newMat.m_columnCountX; x++)
            {
                newMat.m_data[y][x] = GetSigmoidPrime(mat.m_data[y][x]);
            }
        }
        return newMat;
    }
    #endregion

    #region Cost Function(s)
    // Generic
    private MyMatrix GetCost(float[] desiredOutput, MyMatrix actualOutput)
    {
        if(desiredOutput.Length != actualOutput.m_rowCountY)
        {
            Debug.Log("Aborted: Output lengths didn't match!");
            return null;
        }

        if (m_costFunctionType == CostFunctionType.Quadratic)
            return GetCostQuadratic(desiredOutput, actualOutput);

        return null;
    }
    private MyMatrix GetCostDrivative(float[] desiredOutput, MyMatrix actualOutput)
    {
        if (desiredOutput.Length != actualOutput.m_rowCountY)
        {
            Debug.Log("Aborted: Output lengths didn't match!");
            return null;
        }

        if (m_costFunctionType == CostFunctionType.Quadratic)
            return GetCostQuadraticDerivative(desiredOutput, actualOutput);

        return null;
    }

    // Quadratic
    private float GetCostQuadratic(float desiredOutput, float actualOutput)
    {
        return (desiredOutput - actualOutput) * (desiredOutput - actualOutput);
    }
    private MyMatrix GetCostQuadratic(float[] desiredOutput, MyMatrix actualOutput)
    {
        MyMatrix costMatrix = new MyMatrix(actualOutput, false);
        for(int y = 0; y < costMatrix.m_rowCountY; y++)
            costMatrix.m_data[y][0] = GetCostQuadratic(desiredOutput[y], actualOutput.m_data[y][0]);
        return costMatrix;
    }

    private float GetCostQuadraticDerivative(float desiredOutput, float actualOutput)
    {
        return 2f * (desiredOutput - actualOutput);
    }
    private MyMatrix GetCostQuadraticDerivative(float[] desiredOutput, MyMatrix actualOutput)
    {
        MyMatrix costMatrix = new MyMatrix(actualOutput, false);
        for (int y = 0; y < costMatrix.m_rowCountY; y++)
            costMatrix.m_data[y][0] = GetCostQuadraticDerivative(desiredOutput[y], actualOutput.m_data[y][0]);
        return costMatrix;
    }
    #endregion

    #region Misc
    private void ClearMatrixArray(MyMatrix[] mats)
    {
        for (int layerIndex = 0; layerIndex < mats.Length; layerIndex++)
            mats[layerIndex].ClearMatrix();
    }
    #endregion

    #region Getter
    public float GetBias(int layerIndex, int nodeIndex)
    {
        return m_biases[layerIndex - 1].m_data[nodeIndex][0];
    }
    /// <summary>
    /// Returns the weight connecting the node in the layer 'layerIndex' at the index position 'nodeIndex' with the node in layer 'layerIndex' + 1 at index position 'weightIndex'.
    /// </summary>
    /// <param name="layerIndex"></param>
    /// <param name="nodeIndex"></param>
    /// <param name="weightIndex"></param>
    /// <returns></returns>
    public float GetWeight(int layerIndex, int nodeIndex, int weightIndex)
    {
        return m_weights[layerIndex].m_data[weightIndex][ nodeIndex];
    }
    #endregion
}
