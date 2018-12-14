using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

[System.Serializable]
public struct NNSaveData
{
    public JaggedArrayContainer[] m_biases;
    public JaggedArrayContainer[] m_weights;
}

public class NeuralNetwork
{
    // Options
    private float m_learnRate;
    private int m_batchSize;
    private float m_dropoutKeepRate;
    private float m_activisionCoeffitient;
    private float m_weightDecayRate;
    private ActivisionFunctionType m_activisionFunctionType;
    private ActivisionFunctionType m_activisionFunctionTypeOutput;
    private CostFunctionType m_costFunctionType;
    private InitializationType m_initializationType;

    // Basic Architecture
    public int[] m_layerLengths { get; private set; }
    public MyMatrix[] m_biases { get; private set; }          //  m_biases[layerIndex][nodeIndex]
    public MyMatrix[] m_weights { get; private set; }       // m_weights[layerIndex][nodeIndex][weightIndex], m_weights[0] are the weights connecting layer one and layer two and so on
    public int m_layerCount { get; private set; }

    // Back Propagation
    private MyMatrix[] m_batchedGradientBiases;
    private MyMatrix[] m_batchedGradientWeights;

    private MyMatrix[] m_deltaActivision;
    private MyMatrix[] m_deltaBiases;
    private MyMatrix[] m_deltaWeights;

    private MyMatrix[] m_rawValues;
    private MyMatrix[] m_activisionValues;
    
    // Batching
    private float[][] m_batchInputs;
    private float[][] m_batchDesiredOutputs;
    private int m_currentBatchIndex;

    private ActivisionFunction m_activisionFunction;
    private ActivisionFunction m_activisionFunctionOutput;

    #region Enums
    public enum ActivisionFunctionType { Tanh, ReLU, LReLU, ELU, Sigmoid }
    public enum CostFunctionType { CrossEntropy, Quadratic }
    public enum InitializationType { Xavier, Zero, Random }
    #endregion

    #region Initialization
    public NeuralNetwork(
        int[] layerLengths,
        float learnRate,
        int batchSize,
        float dropoutKeepRate,
        float weightDecayRate,
        ActivisionFunctionType activisionType,
        ActivisionFunctionType activisionTypeOutput,
        CostFunctionType costType,
        InitializationType initializationType,
        float activisionCoeffitient
        )
    {
        m_layerLengths = layerLengths;
        m_layerCount = m_layerLengths.Length;
        m_learnRate = learnRate;
        m_activisionCoeffitient = activisionCoeffitient;
        m_dropoutKeepRate = dropoutKeepRate;
        m_weightDecayRate = weightDecayRate;

        if (batchSize <= 0)
        {
            Debug.Log("Info: batch size was <= 0 (" + batchSize + "). It was set to a default value of 1!");
            m_batchSize = 1;
        }
        m_batchSize = batchSize;

        m_activisionFunctionType = activisionType;
        m_activisionFunctionTypeOutput = activisionTypeOutput;
        m_costFunctionType = costType;
        m_initializationType = initializationType;

        SetActivisionFunction(activisionType);
      
        InitializeBiases(null);
        InitializeWeights(null);
        InitializeBatch();

        InitializeBackPropagation();

        //m_activisionFunctionType = ActivisionFunctionType.Sigmoid;
    }
    public NeuralNetwork(
        NNSaveData data,
        int[] layerLengths,
        float learnRate,
        int batchSize,
        float dropoutKeepRate,
        float weightDecayRate,
        ActivisionFunctionType activisionType,
        ActivisionFunctionType activisionTypeOutput,
        CostFunctionType costType,
        InitializationType initializationType,
        float activisionCoeffitient

        )
    {
        m_layerLengths = layerLengths;
        m_layerCount = layerLengths.Length;
        m_learnRate = learnRate;
        m_activisionCoeffitient = activisionCoeffitient;
        m_dropoutKeepRate = dropoutKeepRate;
        m_weightDecayRate = weightDecayRate;


        if (batchSize <= 0)
        {
            Debug.Log("Info: batch size was <= 0 (" + batchSize + "). It was set to a default value of 1!");
            m_batchSize = 1;
        }
        m_batchSize = batchSize;

        m_activisionFunctionType = activisionType;
        m_activisionFunctionTypeOutput = activisionTypeOutput;
        m_costFunctionType = costType;
        m_initializationType = initializationType;

        SetActivisionFunction(activisionType);

        InitializeBiases(data.m_biases);
        InitializeWeights(data.m_weights);

        InitializeBatch();
        InitializeBackPropagation();

        //m_activisionFunctionType = ActivisionFunctionType.Sigmoid;
    }
    private void InitializeBiases(JaggedArrayContainer[] biasData)
    {
        m_biases = new MyMatrix[m_layerCount - 1];
        for (int layerIndex = 1; layerIndex < m_layerCount; layerIndex++)
        {
            int nodeCount = m_layerLengths[layerIndex];
            MyMatrix mat = new MyMatrix(nodeCount, 1);
            if (biasData == null)
            {
                float variance = 1;

                //if (m_initializationType == InitializationType.Random)
                //    variance = 1f;
                //else if (m_initializationType == InitializationType.Xavier)
                //{
                //    variance = 1f / m_layerLengths[layerIndex - 1];
                //    if (m_activisionFunctionType == ActivisionFunctionType.ReLU || m_activisionFunctionType == ActivisionFunctionType.LReLU || m_activisionFunctionType == ActivisionFunctionType.ELU)
                //        variance *= 2f;
                //    if (m_activisionFunctionType == ActivisionFunctionType.Tanh)
                //        variance = Mathf.Sqrt(variance);
                //}
                mat.SetRandomValues(-variance, variance);
            }
            else
            {
                for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
                    mat.m_data[nodeIndex][0] = biasData[layerIndex - 1].dataFloat[nodeIndex];
            }
            m_biases[layerIndex - 1] = mat;
        }
    }
    private void InitializeWeights(JaggedArrayContainer[] weightData)
    {
        m_weights = new MyMatrix[m_layerCount - 1];
        for (int layerIndex = 0; layerIndex < m_layerCount - 1; layerIndex++)
        {
            int nodeCount = m_layerLengths[layerIndex + 1];
            int weightCount = m_layerLengths[layerIndex];
            MyMatrix mat = new MyMatrix(nodeCount, weightCount);
            if (weightData == null)
            {
                float variance = 0;

                if (m_initializationType == InitializationType.Random)
                    variance = 1f;
                else if (m_initializationType == InitializationType.Xavier)
                {
                    if (m_activisionFunctionType == ActivisionFunctionType.ReLU || m_activisionFunctionType == ActivisionFunctionType.LReLU || m_activisionFunctionType == ActivisionFunctionType.ELU)
                        variance = Mathf.Sqrt(2f / m_layerLengths[layerIndex + 1]);
                    else if (m_activisionFunctionType == ActivisionFunctionType.Tanh || m_activisionFunctionType == ActivisionFunctionType.Sigmoid)
                        variance = Mathf.Sqrt(1f / m_layerLengths[layerIndex + 1]);
                }

                mat.SetRandomValues(-variance, variance);
            }
            else
            {
                for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
                {
                    for (int weightIndex = 0; weightIndex < weightCount; weightIndex++)
                    {
                        //Debug.Log(weightData[layerIndex].array[nodeCount].data.Length + ", " + weightCount);
                        mat.m_data[nodeIndex][weightIndex] = weightData[layerIndex].array[nodeIndex].dataFloat[weightIndex];
                    }
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
        m_batchedGradientBiases = new MyMatrix[m_biases.Length];
        for (int layerIndex = 0; layerIndex < m_batchedGradientBiases.Length; layerIndex++)
        {
            m_batchedGradientBiases[layerIndex] = new MyMatrix(m_biases[layerIndex], false);
        }

        m_deltaBiases = new MyMatrix[m_biases.Length];
        for (int layerIndex = 0; layerIndex < m_deltaBiases.Length; layerIndex++)
        {
            m_deltaBiases[layerIndex] = new MyMatrix(m_biases[layerIndex], false);
        }

        // weights
        m_batchedGradientWeights = new MyMatrix[m_weights.Length];
        for (int layerIndex = 0; layerIndex < m_batchedGradientWeights.Length; layerIndex++)
        {
            m_batchedGradientWeights[layerIndex] = new MyMatrix(m_weights[layerIndex], false);
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
            // compensate weights scale for dropout
            MyMatrix weightsCompensated = null;
            if (m_dropoutKeepRate < 1)
            {
                weightsCompensated = new MyMatrix(m_weights[layerIndex], true);
                weightsCompensated.MultiplyByFactor(m_dropoutKeepRate);
            }
            else
                weightsCompensated = m_weights[layerIndex];

            activision = GetActivisionFunction(MyMatrix.AddMatrix(MyMatrix.Dot(weightsCompensated, activision), m_biases[layerIndex]), layerIndex);
            activisions[layerIndex + 1] = activision;
        }

        return activisions;
    }
    public float[] GetOutput(float[] input)
    {
        MyMatrix activision = new MyMatrix(input);

        for (int layerIndex = 0; layerIndex < m_layerCount - 1; layerIndex++)
        {
            // compensate weights scale for dropout
            MyMatrix weightsCompensated = null;
            if (m_dropoutKeepRate < 1)
            {
                weightsCompensated = new MyMatrix(m_weights[layerIndex], true);
                weightsCompensated.MultiplyByFactor(m_dropoutKeepRate);
            }
            else
                weightsCompensated = m_weights[layerIndex];

            activision = GetActivisionFunction(MyMatrix.AddMatrix(MyMatrix.Dot(weightsCompensated, activision), m_biases[layerIndex]), layerIndex);
        }
        return activision.GetColumnToArray(0);
    }
    private void FillActivisions(float[] input)
    {
        m_activisionValues[0] = new MyMatrix(input);
        for (int layerIndex = 0; layerIndex < m_layerCount - 1; layerIndex++)
        {
            m_rawValues[layerIndex] = MyMatrix.AddMatrix(MyMatrix.Dot(m_weights[layerIndex], m_activisionValues[layerIndex]), m_biases[layerIndex]);
            m_activisionValues[layerIndex + 1] = GetActivisionFunction(m_rawValues[layerIndex], layerIndex);

            if (m_dropoutKeepRate < 1 && layerIndex != m_layerCount - 2)
            {
                if (m_dropoutKeepRate <= 0 || m_dropoutKeepRate > 1)
                    Debug.Log("Warning: m_regularizationKeepRate was corrupt! (" + m_dropoutKeepRate + ")");

                MyMatrix regularizationMask = new MyMatrix(m_activisionValues[layerIndex + 1].m_rowCountY, 1);// new float[m_layerLengths[layerIndex] + 1];
                for (int i = 0; i < regularizationMask.m_rowCountY; i++)
                {
                    if (Random.Range(0f, 1f) < m_dropoutKeepRate)
                        regularizationMask.m_data[i][0] = 1;
                }

                m_rawValues[layerIndex] = MyMatrix.MultiplyElementWise(m_rawValues[layerIndex], regularizationMask);
                m_activisionValues[layerIndex + 1] = MyMatrix.MultiplyElementWise(m_activisionValues[layerIndex + 1], regularizationMask);
            }
        }
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
        ClearMatrixArray(m_batchedGradientBiases);
        ClearMatrixArray(m_batchedGradientWeights);

        for (int batchIndex = 0; batchIndex < m_batchSize; batchIndex++)
        {
            // clear delta matrices
            ClearMatrixArray(m_deltaBiases);
            ClearMatrixArray(m_deltaWeights);
            ClearMatrixArray(m_deltaActivision);
            ClearMatrixArray(m_rawValues);
            ClearMatrixArray(m_activisionValues);

            // feed forward, get all raw values and activision values
            FillActivisions(m_batchInputs[batchIndex]);

            // back pass, start at the last layer (manually) and loop over the prelayers afterwards
            MyMatrix delta = GetCostDerivative(m_rawValues[m_rawValues.Length - 1], m_batchDesiredOutputs[batchIndex], m_activisionValues[m_activisionValues.Length - 1]);// MyMatrix.MultiplyElementWise(GetCostDrivative(m_batchDesiredOutputs[batchIndex], m_activisionValues[m_activisionValues.Length - 1]), GetSigmoidPrime(m_rawValues[m_rawValues.Length - 1]));
            m_deltaBiases[m_deltaBiases.Length - 1] = delta;
            m_deltaWeights[m_deltaWeights.Length - 1] = MyMatrix.Dot(delta, MyMatrix.Transposed(m_activisionValues[m_activisionValues.Length - 2]));

            for(int layerIndex = 2; layerIndex < m_layerCount; layerIndex++)
            {
                MyMatrix weightsTransposed = MyMatrix.Transposed(m_weights[m_weights.Length - layerIndex + 1]);
                delta = MyMatrix.Dot(weightsTransposed, delta);
                MyMatrix activisionsPrime = GetActivisionFunctionPrime(m_rawValues[m_rawValues.Length - layerIndex], m_layerCount - layerIndex - 1);
                delta = MyMatrix.MultiplyElementWise(delta, activisionsPrime);

                m_deltaBiases[m_deltaBiases.Length - layerIndex] = delta;
                m_deltaWeights[m_deltaWeights.Length - layerIndex] = MyMatrix.Dot(delta, MyMatrix.Transposed(m_activisionValues[m_activisionValues.Length - layerIndex - 1]));
            }


            // finally add the gradient to the current sum of changes
            for (int layerIndex = 0; layerIndex < m_batchedGradientBiases.Length; layerIndex++)
                m_batchedGradientBiases[layerIndex].AddMatrix(m_deltaBiases[layerIndex]);
            for (int layerIndex = 0; layerIndex < m_batchedGradientWeights.Length; layerIndex++)
                m_batchedGradientWeights[layerIndex].AddMatrix(m_deltaWeights[layerIndex]);
        }

        // set the final values
        learnRate = (learnRate <= 0 ? m_learnRate : learnRate);
        float weightDecayFactor = 1f - learnRate * m_weightDecayRate / m_batchSize;
        if (weightDecayFactor < 0 || weightDecayFactor > 1)
        {
            Debug.Log(string.Format("Warning: weightDecayFactor is corrupt: learnRate {0} + weightDecayRate {1} / batchSize {2} = {3} ", learnRate, m_weightDecayRate, m_batchSize, weightDecayFactor));
            weightDecayFactor = Mathf.Clamp(weightDecayFactor, 0.00001f, 1f);
        }

        for (int layerIndex = 0; layerIndex < m_biases.Length; layerIndex++)
        {
            m_batchedGradientBiases[layerIndex].MultiplyByFactor(learnRate / m_batchSize);
            m_biases[layerIndex].AddMatrix(m_batchedGradientBiases[layerIndex]);
        }
        for (int layerIndex = 0; layerIndex < m_weights.Length; layerIndex++)
        {
            m_batchedGradientWeights[layerIndex].MultiplyByFactor(learnRate / m_batchSize);

            // apply weight decay
            if (m_weightDecayRate > 0)
                m_weights[layerIndex].MultiplyByFactor(weightDecayFactor);

            m_weights[layerIndex].AddMatrix(m_batchedGradientWeights[layerIndex]);
        }
    }
    #endregion

    #region Activision Function(s)
    // Generic
    private MyMatrix GetActivisionFunction(MyMatrix inputMat, int layerIndex)
    {

        if (layerIndex == m_layerCount - 2)
            return m_activisionFunctionOutput.GetActivision(inputMat);
        return m_activisionFunction.GetActivision(inputMat);
    }
    private MyMatrix GetActivisionFunctionPrime(MyMatrix inputMat, int layerIndex)
    {
        if (layerIndex == m_layerCount - 2)
            return m_activisionFunctionOutput.GetActivisionPrime(inputMat);
        return m_activisionFunction.GetActivisionPrime(inputMat);
    }
    private void SetActivisionFunction(ActivisionFunctionType type)
    {
        if (m_activisionFunctionType == ActivisionFunctionType.Tanh)
            m_activisionFunction = new ActivisionFuntionTanh(m_activisionCoeffitient);
        else if (m_activisionFunctionType == ActivisionFunctionType.ReLU)
            m_activisionFunction = new ActivisionFuntionReLU(m_activisionCoeffitient);
        else if (m_activisionFunctionType == ActivisionFunctionType.LReLU)
            m_activisionFunction = new ActivisionFuntionLReLU(m_activisionCoeffitient);
        else if (m_activisionFunctionType == ActivisionFunctionType.ELU)
            m_activisionFunction = new ActivisionFuntionELU(m_activisionCoeffitient);
        else if (m_activisionFunctionType == ActivisionFunctionType.Sigmoid)
            m_activisionFunction = new ActivisionFuntionSigmoid(m_activisionCoeffitient);

        if (m_activisionFunctionTypeOutput == ActivisionFunctionType.Tanh)
            m_activisionFunctionOutput = new ActivisionFuntionTanh(m_activisionCoeffitient);
        else if (m_activisionFunctionTypeOutput == ActivisionFunctionType.ReLU)
            m_activisionFunctionOutput = new ActivisionFuntionReLU(m_activisionCoeffitient);
        else if (m_activisionFunctionTypeOutput == ActivisionFunctionType.LReLU)
            m_activisionFunctionOutput = new ActivisionFuntionLReLU(m_activisionCoeffitient);
        else if (m_activisionFunctionTypeOutput == ActivisionFunctionType.ELU)
            m_activisionFunctionOutput = new ActivisionFuntionELU(m_activisionCoeffitient);
        else if (m_activisionFunctionTypeOutput == ActivisionFunctionType.Sigmoid)
            m_activisionFunctionOutput = new ActivisionFuntionSigmoid(m_activisionCoeffitient);
    }

    //// Tanh
    //private float GetTanh(float rawValue)
    //{
    //    float eExpP = Mathf.Exp(rawValue);
    //    float eExpN = Mathf.Exp(-rawValue);
    //    return (eExpP - eExpN) / (eExpP + eExpN);
    //}
    //private float GetTanhPrime(float rawValue)
    //{
    //    return 1 - GetTanh(rawValue) * GetTanh(rawValue);
    //}

    //// ReLU
    //private float GetReLU(float rawValue)
    //{
    //    return rawValue > 0 ? rawValue : 0;
    //}
    //private float GetReLUPrime(float rawValue)
    //{
    //    return rawValue > 0 ? 1 : 0;
    //}

    //// LReLU
    //private float GetLReLU(float rawValue)
    //{
    //    return rawValue > 0 ? rawValue : activisionCoeffitient * rawValue;
    //}
    //private float GetLReLUPrime(float rawValue)
    //{
    //    return rawValue > 0 ? 1 : activisionCoeffitient;
    //}

    //// ELU
    //private float GetELU(float rawValue)
    //{
    //    return rawValue > 0 ? 1 : activisionCoeffitient * (Mathf.Exp(rawValue) - 1);
    //}
    //private float GetELUPrime(float rawValue)
    //{
    //    return rawValue > 0 ? 1 : activisionCoeffitient * Mathf.Exp(rawValue);
    //}


    //// Sigmoid
    //private float GetSigmoid(float rawValue)
    //{
    //    return 1f / (1f + Mathf.Exp(-rawValue));
    //}
    //private float GetSigmoidPrime(float rawValue)
    //{
    //    float sigmoid = GetSigmoid(rawValue);
    //    return sigmoid * (1 - sigmoid);
    //}
    #endregion

    #region Cost Function(s)
    // Generic
    public float GetCost(float[] desiredOutput, MyMatrix actualOutput)
    {
        if (desiredOutput.Length != actualOutput.m_rowCountY)
        {
            Debug.Log("Aborted: Output lengths didn't match!");
            return -1;
        }

        if (m_costFunctionType == CostFunctionType.CrossEntropy)
            return GetCostCrossEntropy(desiredOutput, actualOutput);
        else if (m_costFunctionType == CostFunctionType.Quadratic)
            return GetCostQuadratic(desiredOutput, actualOutput);


        return -1;
    }
    public float GetCost(float[] desiredOutput, float[] actualOutput)
    {
        MyMatrix mat = new MyMatrix(actualOutput);

        return GetCost(desiredOutput, mat);
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
        desiredOutput = Mathf.Clamp(desiredOutput, 0.000001f, 0.9999999f);
        actualOutput = Mathf.Clamp(actualOutput, 0.000001f, 0.9999999f);


        float cost = desiredOutput * Mathf.Log(actualOutput) + (1 - desiredOutput) * Mathf.Log(1 - actualOutput);
        

        return cost;
    }
    private float GetCostCrossEntropy(float[] desiredOutput, MyMatrix actualOutput)
    {
        float cost = 0;

        for (int y = 0; y < actualOutput.m_rowCountY; y++)
            cost += GetCostCrossEntropy(desiredOutput[y], actualOutput.m_data[y][0]);

        return -cost / (actualOutput.m_rowCountY > 0 ? actualOutput.m_rowCountY : 1);
    }

    private float GetCostCrossEntropyDerivative(float desiredOutput, float actualOutput)
    {
        return desiredOutput - actualOutput;
    }
    private MyMatrix GetCostCrossEntropyDerivative(MyMatrix rawValues, float[] desiredOutput, MyMatrix actualOutput)
    {
        MyMatrix costMatrix = new MyMatrix(actualOutput, false);
        for (int y = 0; y < costMatrix.m_rowCountY; y++)
            costMatrix.m_data[y][0] = GetCostCrossEntropyDerivative(desiredOutput[y], actualOutput.m_data[y][0]) + m_weightDecayRate / m_batchSize * actualOutput.m_data[y][0];
        return costMatrix;
    }

    // Quadratic
    private float GetCostQuadratic(float desiredOutput, float actualOutput)
    {
        return 0.5f * (desiredOutput - actualOutput) * (desiredOutput - actualOutput);
    }
    private float GetCostQuadratic(float[] desiredOutput, MyMatrix actualOutput)
    {
        float cost = 0;

        for(int y = 0; y < actualOutput.m_rowCountY; y++)
            cost  += GetCostQuadratic(desiredOutput[y], actualOutput.m_data[y][0]);

        return cost / (actualOutput.m_rowCountY > 0 ? actualOutput.m_rowCountY : 1);
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

        costMatrix = MyMatrix.MultiplyElementWise(costMatrix, GetActivisionFunctionPrime(rawValues, m_layerCount - 2));
        return costMatrix;
    }
    #endregion

    #region Misc
    private MyMatrix GetWeightDecayRate(MyMatrix weights)
    {
        MyMatrix mat = new MyMatrix(weights, false);

        for(int y = 0; y < weights.m_rowCountY; y++)
        {
            for (int x = 0; x < weights.m_columnCountX; x++)
            {
                //mat.m_data[y][x] = 
            }
        }

        return mat;
    }
    private void ClearMatrixArray(MyMatrix[] mats)
    {
        for (int layerIndex = 0; layerIndex < mats.Length; layerIndex++)
            mats[layerIndex].ClearMatrix();
    }
    #endregion

    #region Save / Load
    public NNSaveData SaveData()
    {
        // biases
        JaggedArrayContainer[] finalBiasArray = new JaggedArrayContainer[m_biases.Length];
        for(int layerIndex = 0; layerIndex < finalBiasArray.Length; layerIndex++)
        {
            float[] biasData = new float[m_biases[layerIndex].m_rowCountY];
            for (int nodeIndex = 0; nodeIndex < biasData.Length; nodeIndex++)
            {
                biasData[nodeIndex] = m_biases[layerIndex].m_data[nodeIndex][0];
            }
            finalBiasArray[layerIndex] = new JaggedArrayContainer(biasData);
        }

        // weights
        JaggedArrayContainer[] finalWeightArray = new JaggedArrayContainer[m_weights.Length];
        for (int layerIndex = 0; layerIndex < finalWeightArray.Length; layerIndex++)
        {
            JaggedArrayContainer[] weightArray = new JaggedArrayContainer[m_weights[layerIndex].m_rowCountY];
            for (int nodeIndex = 0; nodeIndex < weightArray.Length; nodeIndex++)
            {
                float[] weightData = new float[m_weights[layerIndex].m_columnCountX];
                for (int weightIndex = 0; weightIndex < weightData.Length; weightIndex++)
                {
                    weightData[weightIndex] = m_weights[layerIndex].m_data[nodeIndex][weightIndex];
                }
                weightArray[nodeIndex] = new JaggedArrayContainer();
                weightArray[nodeIndex].dataFloat = weightData;
            }
            finalWeightArray[layerIndex] = new JaggedArrayContainer(weightArray);
        }

        NNSaveData data = new NNSaveData
        {
            m_biases = finalBiasArray,
            m_weights = finalWeightArray
        };

        

        return data;
    }
    public void LoadData(NNSaveData data)
    {
        
    }
    public void ApplyData()
    {
        InitializeBatch();
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
        //Debug.Log(string.Format("0: {0}, {1}, {2}", layerIndex, nodeIndex, weightIndex));
        //Debug.Log(string.Format("1: {0}, {1}, {2}", m_weights.Length, m_weights[layerIndex].m_data[weightIndex].Length, m_weights[layerIndex].m_data.Length));
        return m_weights[layerIndex].m_data[weightIndex][nodeIndex];
    }
    #endregion
}
