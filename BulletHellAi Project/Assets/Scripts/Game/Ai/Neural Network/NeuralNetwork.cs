using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct NNSaveData
{
    public int[] m_layerLengths;
    public MyMatrix[] m_biases;
    public MyMatrix[] m_weights;
}

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
    public enum CostFunctionType { CrossEntropy, Quadratic }
    public enum InitializationType { Zero, Random }


    #region Initialization
    public NeuralNetwork(
        int[] layerLengths,
        float learnRate,
        int batchSize,
        ActivisionFunctionType activisionType,
        CostFunctionType costType,
        InitializationType initializationType
        )
    {
        m_layerLengths = layerLengths;
        m_layerCount = m_layerLengths.Length;
        m_learnRate = learnRate;

        if (batchSize <= 0)
        {
            Debug.Log("Info: batch size was <= 0 (" + batchSize + "). It was set to a default value of 1!");
            m_batchSize = 1;
        }
        m_batchSize = batchSize;

        m_activisionFunctionType = activisionType;
        m_costFunctionType = costType;
        m_initializationType = initializationType;

      
        InitializeBiases(null);
        InitializeWeights(null);
        InitializeBatch();

        InitializeBackPropagation();

        //m_activisionFunctionType = ActivisionFunctionType.Sigmoid;
    }
    public NeuralNetwork(
        int[] layerLengths,
        float learnRate,
        int batchSize,
        ActivisionFunctionType activisionType,
        CostFunctionType costType,
        InitializationType initializationType,
        NNSaveData data
        )
    {
        m_layerLengths = data.m_layerLengths;
        m_layerCount = data.m_layerLengths.Length;
        m_learnRate = learnRate;

        if (batchSize <= 0)
        {
            Debug.Log("Info: batch size was <= 0 (" + batchSize + "). It was set to a default value of 1!");
            m_batchSize = 1;
        }
        m_batchSize = batchSize;

        m_activisionFunctionType = activisionType;
        m_costFunctionType = costType;
        m_initializationType = initializationType;

        InitializeBiases(data.m_biases);
        InitializeWeights(data.m_weights);

        InitializeBatch();
        InitializeBackPropagation();

        //m_activisionFunctionType = ActivisionFunctionType.Sigmoid;
    }
    private void InitializeBiases(MyMatrix[] data)
    {
        m_biases = new MyMatrix[m_layerCount - 1];
        for (int layerIndex = 1; layerIndex < m_layerCount; layerIndex++)
        {
            int nodeCount = m_layerLengths[layerIndex];
            MyMatrix mat = new MyMatrix(nodeCount, 1);
            if (data == null)
            {
                if (m_initializationType == InitializationType.Random)
                    mat.SetRandomValues(-1f, 1f);
            }
            else
            {
                for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
                    mat.m_data[nodeIndex][0] = data[layerIndex].m_data[nodeIndex][0];
            }
            m_biases[layerIndex - 1] = mat;
        }
    }
    private void InitializeWeights(MyMatrix[] data)
    {
        m_weights = new MyMatrix[m_layerCount - 1];
        for (int layerIndex = 0; layerIndex < m_layerCount - 1; layerIndex++)
        {
            int nodeCount = m_layerLengths[layerIndex + 1];
            int weightCount = m_layerLengths[layerIndex];
            MyMatrix mat = new MyMatrix(nodeCount, weightCount);
            if (data == null)
            {
                if (m_initializationType == InitializationType.Random)
                    mat.SetRandomValues(-1f, 1f);
            }
            else
            {
                for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
                {
                    for (int weightIndex = 0; weightIndex < weightCount; weightIndex++)
                        mat.m_data[nodeIndex][weightIndex] = data[layerIndex].m_data[nodeIndex][weightIndex];
                }
            }
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
        for (int layerIndex = 0; layerIndex < m_newBiases.Length; layerIndex++)
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
        for (int layerIndex = 0; layerIndex < m_rawValues.Length; layerIndex++)
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
    public MyMatrix[] GetActivisions(float[] input)
    {
        MyMatrix[] activisions = new MyMatrix[m_layerCount];
        MyMatrix activision = new MyMatrix(input);
        activisions[0] = activision;

        for(int layerIndex = 0; layerIndex < m_layerCount - 1; layerIndex++)
        {
            activision = GetActivisionFunction(MyMatrix.AddMatrix(MyMatrix.Dot(m_weights[layerIndex], activision), m_biases[layerIndex]));
            activisions[layerIndex + 1] = activision;
        }

        return activisions;
    }
    public float[] GetOutput(float[] input)
    {
        MyMatrix activision = new MyMatrix(input);

        for (int layerIndex = 0; layerIndex < m_layerCount - 1; layerIndex++)
            activision = GetActivisionFunction(MyMatrix.AddMatrix(MyMatrix.Dot(m_weights[layerIndex], activision), m_biases[layerIndex]));

        return activision.GetColumnToArray(0);
    }
    #endregion

    #region Training
    public bool AddTrainingData(float[] input, float[] desiredOutput, float learnRate)
    {
        m_batchInputs[m_currentBatchIndex] = input;
        m_batchDesiredOutputs[m_currentBatchIndex] = desiredOutput;
        m_currentBatchIndex++;
        if(m_currentBatchIndex >= m_batchSize)
        {
            PerformBackPropagation(learnRate);
            InitializeBatch();
            return true;
        }
        return false;
    }
    public void PerformBackPropagation(float learnRate)
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
                m_rawValues[layerIndex] = MyMatrix.AddMatrix(MyMatrix.Dot(m_weights[layerIndex], m_activisionValues[layerIndex]), m_biases[layerIndex]);
                m_activisionValues[layerIndex + 1] = GetActivisionFunction(m_rawValues[layerIndex]);
            }

            // back pass, start at the last layer (manually) and loop over the prelayers afterwards
            MyMatrix delta = GetCostDerivative(m_rawValues[m_rawValues.Length - 1], m_batchDesiredOutputs[batchIndex], m_activisionValues[m_activisionValues.Length - 1]);// MyMatrix.MultiplyElementWise(GetCostDrivative(m_batchDesiredOutputs[batchIndex], m_activisionValues[m_activisionValues.Length - 1]), GetSigmoidPrime(m_rawValues[m_rawValues.Length - 1]));
            m_newBiases[m_newBiases.Length - 1] = delta;
            m_newWeights[m_newWeights.Length - 1] = MyMatrix.Dot(delta, MyMatrix.Transposed(m_activisionValues[m_activisionValues.Length - 2]));


            for(int layerIndex = m_layerCount - 1; layerIndex > 1; layerIndex--)
            {
                MyMatrix weightsTransposed = MyMatrix.Transposed(m_weights[m_weights.Length - layerIndex + 1]);
                delta = MyMatrix.Dot(weightsTransposed, delta);
                MyMatrix activisionsPrime = GetActivisionFunctionPrime(m_rawValues[m_rawValues.Length - layerIndex]);
                delta = MyMatrix.MultiplyElementWise(delta, activisionsPrime);

                m_newBiases[m_newBiases.Length - layerIndex] = delta;
                m_newWeights[m_newWeights.Length - layerIndex] = MyMatrix.Dot(delta, MyMatrix.Transposed(m_activisionValues[m_activisionValues.Length - layerIndex - 1]));
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
            m_newBiases[layerIndex].MultiplyByFactor((learnRate <= 0 ? m_learnRate : learnRate) / m_batchSize);
            m_biases[layerIndex].AddMatrix(m_newBiases[layerIndex]);
        }
        for (int layerIndex = 0; layerIndex < m_weights.Length; layerIndex++)
        {
            m_newWeights[layerIndex].MultiplyByFactor((learnRate <= 0 ? m_learnRate : learnRate) / m_batchSize);
            m_weights[layerIndex].AddMatrix(m_newWeights[layerIndex]);
        }
    }
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

        if (m_costFunctionType == CostFunctionType.CrossEntropy)
            return GetCostCrossEntropy(desiredOutput, actualOutput);
        else if (m_costFunctionType == CostFunctionType.Quadratic)
            return GetCostQuadratic(desiredOutput, actualOutput);

        return null;
    }
    private MyMatrix GetCostDerivative(MyMatrix rawValues, float[] desiredOutput, MyMatrix actualOutput)
    {
        if (desiredOutput.Length != actualOutput.m_rowCountY)
        {
            Debug.Log("Aborted: Output lengths didn't match!");
            return null;
        }

        if (m_costFunctionType == CostFunctionType.CrossEntropy)
            return GetCostCrossEntropyDerivative(rawValues, desiredOutput, actualOutput);
        else if(m_costFunctionType == CostFunctionType.Quadratic)
            return GetCostQuadraticDerivative(rawValues, desiredOutput, actualOutput);

        return null;
    }

    // Cross Entropy
    private float GetCostCrossEntropy(float desiredOutput, float actualOutput)
    {
        Debug.Log("Warning: Not implemented yet!");
        return (desiredOutput - actualOutput) * (desiredOutput - actualOutput);
    }
    private MyMatrix GetCostCrossEntropy(float[] desiredOutput, MyMatrix actualOutput)
    {
        MyMatrix costMatrix = new MyMatrix(actualOutput, false);
        for (int y = 0; y < costMatrix.m_rowCountY; y++)
            costMatrix.m_data[y][0] = GetCostCrossEntropy(desiredOutput[y], actualOutput.m_data[y][0]);
        return costMatrix;
    }

    private float GetCostCrossEntropyDerivative(float desiredOutput, float actualOutput)
    {
        return desiredOutput - actualOutput;
    }
    private MyMatrix GetCostCrossEntropyDerivative(MyMatrix rawValues, float[] desiredOutput, MyMatrix actualOutput)
    {
        MyMatrix costMatrix = new MyMatrix(actualOutput, false);
        for (int y = 0; y < costMatrix.m_rowCountY; y++)
            costMatrix.m_data[y][0] = GetCostCrossEntropyDerivative(desiredOutput[y], actualOutput.m_data[y][0]);
        return costMatrix;
    }

    // Quadratic
    private float GetCostQuadratic(float desiredOutput, float actualOutput)
    {
        return 0.5f * (desiredOutput - actualOutput) * (desiredOutput - actualOutput);
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
        return (desiredOutput - actualOutput);
    }
    private MyMatrix GetCostQuadraticDerivative(MyMatrix rawValues, float[] desiredOutput, MyMatrix actualOutput)
    {
        MyMatrix costMatrix = new MyMatrix(actualOutput, false);
        for (int y = 0; y < costMatrix.m_rowCountY; y++)
            costMatrix.m_data[y][0] = GetCostQuadraticDerivative(desiredOutput[y], actualOutput.m_data[y][0]);

        costMatrix = MyMatrix.MultiplyElementWise(costMatrix, GetSigmoidPrime(rawValues));
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

    #region Save / Load
    public NNSaveData SaveData()
    {
        NNSaveData data = new NNSaveData {
            
        };

        return data;
    }
    public void LoadData(NNSaveData data)
    {
        
    }
    public void ApplyData()
    {

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
