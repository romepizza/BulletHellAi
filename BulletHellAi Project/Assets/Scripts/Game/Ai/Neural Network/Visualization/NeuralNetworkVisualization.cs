using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct NNVSaveData
{
    public bool m_visualize;
    public float m_height;
    public float m_width;
    public float m_marginX;
    public float m_marginY;
    public NeuralNetworkVisualization.OrientationType m_orientation;

    public float m_nodesScaleFactorGlobal;
    public float m_weightsScaleFactorGloal;
    public float[] m_nodesScaleFactorLayerWise;
    public float[] m_weightsScaleFactorLayerWise;

    public Vector3 m_relativePosition;
    public Vector3 m_relativeRotation;
}

public class NeuralNetworkVisualization : MonoBehaviour
{
    [Header("------- Settings -------")]
    [SerializeField] private bool m_visualize;
    [SerializeField] private float m_height;
    [SerializeField] private float m_width;
    [SerializeField] private float m_marginX = 0.25f;
    [SerializeField] private float m_marginY = 0.25f;
    [SerializeField] private OrientationType m_orientation = OrientationType.LeftToRight;

    [Header("--- Scale ---")]
    [SerializeField] private float m_nodesScaleFactorGlobal = 1;
    [SerializeField] private float m_weightsScaleFactorGloal = 1;
    [SerializeField] private float[] m_nodesScaleFactorLayerWise;
    [SerializeField] private float[] m_weightsScaleFactorLayerWise;

    [Header("--- Transform ---")]
    [SerializeField] private Vector3 m_relativePosition;
    [SerializeField] private Vector3 m_relativeRotation;
    
    [Header("--- Objects ---")]
    [SerializeField] private Transform m_parentTransform;
    private NeuralNetworkContainer m_networkContainer;

    [Header("------- Debug -------")]
    //private Vector3 m_originPosition;
    private NeuralNetwork m_network;
    private Transform[] m_layerTransforms;
    private Transform[][] m_nodeTransforms;
    private Transform[][] m_activisionTransforms;
    private Transform[][][] m_weightTransforms;
    private int m_layerCount;
    //private Camera m_camera;
    private Vector2 m_cameraCanvasSize;
    private bool m_isDestroyed = true;

    private NeuralNetworkVisualizationManager m_manager;

    #region Enums
    public enum OrientationType { TopToDown, DownToTop, LeftToRight, RightToLeft }
    #endregion

    #region Mono
    private void Awake()
    {
        m_networkContainer = GetComponent<NeuralNetworkContainer>();
    }
    #endregion

    #region Creation
    public void CreateVisualization(NeuralNetworkContainer networkContainer)//, Vector3 position, Vector3 rotation, float size)
    {
        if (!m_visualize)
            return;

        m_isDestroyed = false;

        m_manager = NeuralNetworkVisualizationManager.Instance();
        m_network = networkContainer.m_network;
        m_layerCount = m_network.m_layerCount;
        
        m_cameraCanvasSize.y = m_height;
        m_cameraCanvasSize.x = m_width;// m_cameraCanvasSize.y * Statics.GetMainCamera().aspect;

        CreateObjects();
        PositionObjects();
        UpdateVisualization();

        m_parentTransform.position = m_parentTransform.parent.transform.position;
        m_parentTransform.position += m_relativePosition;
        m_parentTransform.rotation = m_parentTransform.parent.transform.rotation;
        m_parentTransform.Rotate(m_relativeRotation);

    }
    private void CreateObjects()
    {
        // create layer objects
        m_layerTransforms = new Transform[m_layerCount];
        for(int layerIndex = 0; layerIndex < m_layerCount; layerIndex++)
        {
            GameObject layerObject = Instantiate(m_manager.GetLayerPrefab(), m_parentTransform);
            layerObject.name = "Layer_" + layerIndex;
            layerObject.layer = Statics.s_neuralNetworkLayer;
            m_layerTransforms[layerIndex] = layerObject.transform;
        }

        // create node and activision objects
        m_nodeTransforms = new Transform[m_layerCount][];
        m_activisionTransforms = new Transform[m_layerCount][];
        for(int layerIndex = 0; layerIndex < m_layerCount; layerIndex++)
        {
            int nodeCount = m_network.m_layerLengths[layerIndex];
            Transform[] nodesThisLayer = new Transform[nodeCount];
            Transform[] activisionsThisLayer = new Transform[nodeCount];
            for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
            {
                GameObject node = Instantiate(m_manager.GetNodePrefabHemisphere(), m_layerTransforms[layerIndex]);
                node.name = "Node_" + layerIndex + "_" + nodeIndex;
                node.layer = Statics.s_neuralNetworkLayer;
                node.transform.Rotate(node.transform.right, -90f);
                nodesThisLayer[nodeIndex] = node.transform;

                GameObject activision = Instantiate(m_manager.GetNodePrefabHemisphere(), m_layerTransforms[layerIndex]);
                activision.name = "Activison_" + layerIndex + "_" + nodeIndex;
                activision.layer = Statics.s_neuralNetworkLayer;
                activision.transform.Rotate(activision.transform.right, 90f);
                activisionsThisLayer[nodeIndex] = activision.transform;
            }

            m_nodeTransforms[layerIndex] = nodesThisLayer;
            m_activisionTransforms[layerIndex] = activisionsThisLayer;
        }

        // create weight objects
        m_weightTransforms = new Transform[m_layerCount][][];
        for(int layerIndex = 0; layerIndex < m_layerCount - 1; layerIndex++)
        {
            int nodeCountThisLayer = m_network.m_layerLengths[layerIndex];
            int nodeCountNextLayer = m_network.m_layerLengths[layerIndex + 1];
            Transform[][] nodesThisLayer = new Transform[nodeCountThisLayer][];
            for(int nodeIndex = 0; nodeIndex < nodeCountThisLayer; nodeIndex++)
            {
                Transform[] weightTransforms = new Transform[nodeCountNextLayer];
                for(int weightIndex = 0; weightIndex < nodeCountNextLayer; weightIndex++)
                {
                    GameObject weight = Instantiate(m_manager.GetWeightPrefab(), m_nodeTransforms[layerIndex][nodeIndex], true);
                    weight.name = "Weight_" + layerIndex + "_" + nodeIndex + "_" + weightIndex;
                    weight.layer = Statics.s_neuralNetworkLayer;
                    weightTransforms[weightIndex] = weight.transform;
                }
                nodesThisLayer[nodeIndex] = weightTransforms;
            }
            m_weightTransforms[layerIndex] = nodesThisLayer;
        }
    }
    private void PositionObjects()
    {
        // position nodes and activisions
        Vector3 originPosition = GetOriginPositionOrientation();
        Vector3 layerPadding = GetLayerPaddingOrientation(m_layerCount);
        Vector3 nodePadding = new Vector3();

        Vector3 currentLayerPadding = originPosition;
        Vector3 currentNodePadding = originPosition;

        for (int layerIndex = 0; layerIndex < m_layerCount; layerIndex++)
        {
            int nodeCountThisLayer = m_network.m_layerLengths[layerIndex];
            nodePadding = GetNodePddingOrientation(nodeCountThisLayer);
            currentNodePadding = Vector3.zero;

            for (int nodeIndex = 0; nodeIndex < nodeCountThisLayer; nodeIndex++)
            {
               // position
                Transform node = m_nodeTransforms[layerIndex][nodeIndex];
                node.position = currentLayerPadding + currentNodePadding;

                Transform activision = m_activisionTransforms[layerIndex][nodeIndex];
                activision.position = currentLayerPadding + currentNodePadding;

                // scale
                float scaleFactor = m_nodesScaleFactorGlobal;
                if (m_nodesScaleFactorLayerWise != null && layerIndex < m_nodesScaleFactorLayerWise.Length)
                    scaleFactor *= m_nodesScaleFactorLayerWise[layerIndex];
                node.localScale = node.localScale * scaleFactor;
                activision.localScale = activision.localScale * scaleFactor;

                currentNodePadding += nodePadding;
            }
            currentLayerPadding += layerPadding;
        }

        // position weights
        for (int layerIndex = 0; layerIndex < m_layerCount - 1; layerIndex++)
        {
            int nodeCountThisLayer = m_network.m_layerLengths[layerIndex];
            int weightCount = m_network.m_layerLengths[layerIndex + 1];
            for (int nodeIndex = 0; nodeIndex < nodeCountThisLayer; nodeIndex++)
            {
                Transform nodeThisLayer = m_nodeTransforms[layerIndex][nodeIndex];
                for (int weightIndex = 0; weightIndex < weightCount; weightIndex++)
                {
                    Transform weight = m_weightTransforms[layerIndex][nodeIndex][weightIndex];
                    Transform nodeNextLayer = m_nodeTransforms[layerIndex + 1][weightIndex];
                    Vector3 displacement = nodeNextLayer.position - nodeThisLayer.position;
                    float distanceX = displacement.x;
                    float distanceY = displacement.y;

                    // scale
                    float scaleFactorWeight = m_weightsScaleFactorGloal;
                    if (m_weightsScaleFactorLayerWise != null && layerIndex < m_weightsScaleFactorLayerWise.Length )
                        scaleFactorWeight *= m_weightsScaleFactorLayerWise[layerIndex];
                    float scaleFactorNode = m_nodesScaleFactorGlobal;
                    if (m_nodesScaleFactorLayerWise != null && layerIndex < m_nodesScaleFactorLayerWise.Length)
                        scaleFactorNode *= m_nodesScaleFactorLayerWise[layerIndex];
                    if (scaleFactorNode == 0)
                        scaleFactorNode = 1;
                    weight.localScale = new Vector3(weight.localScale.x * displacement.magnitude, weight.localScale.y * scaleFactorWeight, weight.localScale.z) / scaleFactorNode;

                    // position
                    Vector3 position = (nodeNextLayer.position + nodeThisLayer.position) * 0.5f;
                    weight.position = position;

                    // rotation
                    float rotationZ = 0;
                    rotationZ = Mathf.Rad2Deg * Mathf.Atan(distanceY / distanceX);
                    weight.Rotate(weight.forward, rotationZ);
                }
            }
        }
    }
    #endregion

    #region Update
    public void UpdateVisualization()
    {
        if (!m_visualize || m_isDestroyed)
            return;

        UpdateLayer();
        UpdateBiases();
        //UpdateActivisions();
        UpdateWeights();
    }
    private void UpdateLayer()
    {
        for(int layerIndex = 1; layerIndex < m_layerTransforms.Length; layerIndex++)
        {
            NeuralNetworkValueContainer valueContainer = m_layerTransforms[layerIndex].GetComponent<NeuralNetworkValueContainer>();
            if (valueContainer == null)
                valueContainer = m_layerTransforms[layerIndex].gameObject.AddComponent<NeuralNetworkValueContainer>();
            float[] values = new float[m_nodeTransforms[layerIndex].Length];
            for(int nodeIndex = 0; nodeIndex < m_nodeTransforms[layerIndex].Length; nodeIndex++)
            {
                values[nodeIndex] = m_network.GetBias(layerIndex, nodeIndex);
            }
            valueContainer.m_valuesBias = values;
        }
    }
    private void UpdateBiases()
    {
        float[] minValues = new float[m_layerCount];
        float[] maxValues = new float[m_layerCount];
        //float minValueBias = float.MaxValue;
        //float maxValueBias = float.MinValue;
        // get min / max value
        for (int layerIndex = 1; layerIndex < m_layerCount; layerIndex++)
        {
            minValues[layerIndex] = float.MaxValue;
            maxValues[layerIndex] = float.MinValue;
            int nodeCount = m_network.m_layerLengths[layerIndex];
            for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
            {
                float value = m_network.GetBias(layerIndex, nodeIndex);
                minValues[layerIndex] = Mathf.Min(value, minValues[layerIndex]);
                maxValues[layerIndex] = Mathf.Max(value, maxValues[layerIndex]);

                NeuralNetworkValueContainer valueContainer = m_nodeTransforms[layerIndex][nodeIndex].GetComponent<NeuralNetworkValueContainer>();
                if (valueContainer == null)
                    valueContainer = m_nodeTransforms[layerIndex][nodeIndex].gameObject.AddComponent<NeuralNetworkValueContainer>();
                valueContainer.m_value = value;
            }
        }
        // set actual colors
        Color color;
        for (int layerIndex = 1; layerIndex < m_layerCount; layerIndex++)
        {
            int nodeCount = m_network.m_layerLengths[layerIndex];
            for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
            {
                Transform node = m_nodeTransforms[layerIndex][nodeIndex];

                float value = m_network.GetBias(layerIndex, nodeIndex);
                float mappedValue = Utility.MapValuePercent(minValues[layerIndex], maxValues[layerIndex], value);
                if (mappedValue > 0.5f)
                    color = Color.Lerp(m_manager.GetColorBiasAverage(), m_manager.GetColorBiasMax(), (mappedValue - 0.5f) * 2f);
                else
                    color = Color.Lerp(m_manager.GetColorBiasMin(), m_manager.GetColorBiasAverage(), mappedValue * 2f);

                Renderer renderer = node.GetComponent<Renderer>();
                MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                mpb.SetColor("_Color", color);
                renderer.material.EnableKeyword("_EMISSION");
                mpb.SetColor("_EmissionColor", color);
                renderer.SetPropertyBlock(mpb);
            }
        }
    }
    public void UpdateActivisions(float[] input)
    {
        if (!m_visualize || m_isDestroyed)
            return;

        MyMatrix[] activisions = m_network.GetActivisions(input);

        // set actual colors
        Color color;
        for (int layerIndex = 0; layerIndex < m_layerCount; layerIndex++)
        {
            NeuralNetworkValueContainer valueContainerLayer = m_layerTransforms[layerIndex].GetComponent<NeuralNetworkValueContainer>();
            if (valueContainerLayer == null)
                valueContainerLayer = m_layerTransforms[layerIndex].gameObject.AddComponent<NeuralNetworkValueContainer>();
            float[] values = new float[m_nodeTransforms[layerIndex].Length];
            

            int nodeCount = m_network.m_layerLengths[layerIndex];
            for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
            {
                Transform activision = m_activisionTransforms[layerIndex][nodeIndex];

                float value = activisions[layerIndex].m_data[nodeIndex][0];

                float mappedValue = 0;
                if(layerIndex == 0)
                    mappedValue = Utility.MapValuePercent(0f, 1f, value);
                else
                    mappedValue = Utility.MapValuePercent(0f, 1f, value);

                if (mappedValue > 0.5f)
                    color = Color.Lerp(m_manager.GetColorBiasAverage(), m_manager.GetColorBiasMax(), (mappedValue - 0.5f) * 2f);
                else
                    color = Color.Lerp(m_manager.GetColorBiasMin(), m_manager.GetColorBiasAverage(), mappedValue * 2f);

                Renderer renderer = activision.GetComponent<Renderer>();
                MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                mpb.SetColor("_Color", color);
                renderer.material.EnableKeyword("_EMISSION");
                mpb.SetColor("_EmissionColor", color);
                renderer.SetPropertyBlock(mpb);

                NeuralNetworkValueContainer valueContainerNode = activision.GetComponent<NeuralNetworkValueContainer>();
                if (valueContainerNode == null)
                    valueContainerNode = activision.gameObject.AddComponent<NeuralNetworkValueContainer>();
                valueContainerNode.m_value = value;
                values[nodeIndex] = value;

                // set the color of activisions and biases in the input layer
                if(layerIndex == 0)
                {
                    Transform bias = m_nodeTransforms[layerIndex][nodeIndex];

                    renderer = bias.GetComponent<Renderer>();
                    mpb = new MaterialPropertyBlock();
                    mpb.SetColor("_Color", color);
                    renderer.material.EnableKeyword("_EMISSION");
                    mpb.SetColor("_EmissionColor", color);
                    renderer.SetPropertyBlock(mpb);
                }
            }

            valueContainerLayer.m_valuesActivision = values;
        }
    }
    private void UpdateWeights()
    {
        float[] minValues = new float[m_layerCount];
        float[] maxValues = new float[m_layerCount];
        //float minValueWeight = float.MaxValue;
        //float maxValueWeight = float.MinValue;
        // get min / max value
        for (int layerIndex = 0; layerIndex < m_layerCount - 1; layerIndex++)
        {
            minValues[layerIndex] = float.MaxValue;
            maxValues[layerIndex] = float.MinValue;

            int nodeCount = m_network.m_layerLengths[layerIndex];
            int weightCount = m_network.m_layerLengths[layerIndex + 1];
            for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
            {
                NeuralNetworkValueContainer valueContainerNode = m_nodeTransforms[layerIndex][nodeIndex].GetComponent<NeuralNetworkValueContainer>();
                if (valueContainerNode == null)
                    valueContainerNode = m_nodeTransforms[layerIndex][nodeIndex].gameObject.AddComponent<NeuralNetworkValueContainer>();
                float[] weightValues = new float[weightCount];

                for (int weightIndex = 0; weightIndex < weightCount; weightIndex++)
                {
                    float value = m_network.GetWeight(layerIndex, nodeIndex, weightIndex);
                    minValues[layerIndex] = Mathf.Min(value, minValues[layerIndex]);
                    maxValues[layerIndex] = Mathf.Max(value, maxValues[layerIndex]);

                    NeuralNetworkValueContainer valueContainerWeight = m_weightTransforms[layerIndex][nodeIndex][weightIndex].GetComponent<NeuralNetworkValueContainer>();
                    if (valueContainerWeight == null)
                        valueContainerWeight = m_weightTransforms[layerIndex][nodeIndex][weightIndex].gameObject.AddComponent<NeuralNetworkValueContainer>();
                    valueContainerWeight.m_value = value;
                    weightValues[weightIndex] = value;
                }

                valueContainerNode.m_valuesWeight = weightValues;
            }
        }

        // set actual colors
        Color color;
        for (int layerIndex = 0; layerIndex < m_layerCount - 1; layerIndex++)
        {
            int nodeCount = m_network.m_layerLengths[layerIndex];
            int weightCount = m_network.m_layerLengths[layerIndex + 1];
            for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
            {
                for (int weightIndex = 0; weightIndex < weightCount; weightIndex++)
                {
                    Transform weight = m_weightTransforms[layerIndex][nodeIndex][weightIndex];

                    float value = m_network.GetWeight(layerIndex, nodeIndex, weightIndex);
                    float mappedValue = Utility.MapValuePercent(minValues[layerIndex], maxValues[layerIndex], value);

                    if (mappedValue > 0.5f)
                        color = Color.Lerp(m_manager.GetColorWeightAverage(), m_manager.GetColorWeightMax(), (mappedValue - 0.5f) * 2f);
                    else
                        color = Color.Lerp(m_manager.GetColorWeightMin(), m_manager.GetColorWeightAverage(), mappedValue * 2f);

                    Renderer renderer = weight.GetComponent<Renderer>();
                    renderer.material.SetColor("_TintColor", color);
                }
            }
        }
    }
    #endregion

    #region Manage State
    public void DestroySelf()
    {
        m_isDestroyed = true;

        m_layerTransforms = null;
        m_nodeTransforms = null;
        m_activisionTransforms = null;
        m_weightTransforms = null;

        foreach (Transform child in m_parentTransform.transform)
        {
            Destroy(child.gameObject);
        }
    }
    #endregion

    #region Save / Load
    public NNVSaveData SaveData()
    {
        NNVSaveData data = new NNVSaveData
        {
            m_visualize = m_visualize,
            m_height =  m_height,
            m_width =  m_width,
            m_marginX =  m_marginX,
            m_marginY = m_marginY,
            m_orientation = m_orientation,

            m_nodesScaleFactorGlobal = m_nodesScaleFactorGlobal,
            m_weightsScaleFactorGloal = m_weightsScaleFactorGloal,
            m_nodesScaleFactorLayerWise = m_nodesScaleFactorLayerWise,
            m_weightsScaleFactorLayerWise = m_weightsScaleFactorLayerWise,

            m_relativePosition = m_relativePosition,
            m_relativeRotation = m_relativeRotation
        };

        return data;
    }
    public void LoadData(NNVSaveData data)
    {
        m_visualize = data.m_visualize;
        m_height = data.m_height;
        m_width = data.m_width;
        m_marginX = data.m_marginX;
        m_marginY = data.m_marginY;
        m_orientation = data.m_orientation;

        m_nodesScaleFactorGlobal = data.m_nodesScaleFactorGlobal;
        m_weightsScaleFactorGloal = data.m_weightsScaleFactorGloal;
        m_nodesScaleFactorLayerWise = data.m_nodesScaleFactorLayerWise;
        m_weightsScaleFactorLayerWise = data.m_weightsScaleFactorLayerWise;

        m_relativePosition = data.m_relativePosition;
        m_relativeRotation = data.m_relativeRotation;
    }
    public void ApplyData()
    {
        DestroySelf();
        CreateVisualization(m_networkContainer);
    }
    #endregion

    #region Orientation
    private Vector3 GetOriginPositionOrientation()
    {
        Vector3 position = m_parentTransform.position;

        if (m_orientation == OrientationType.TopToDown)
        {
            // top left
            position.x += 0.5f * -m_cameraCanvasSize.x * (1 - m_marginX);
            position.y += 0.5f * m_cameraCanvasSize.y * (1 - m_marginY);
        }
        else if (m_orientation == OrientationType.DownToTop)
        {
            // down left
            position.x += 0.5f * -m_cameraCanvasSize.x * (1 - m_marginX);
            position.y += 0.5f * -m_cameraCanvasSize.y * (1 - m_marginY);
        }
        else if (m_orientation == OrientationType.LeftToRight)
        {
            // top left
            position.x += 0.5f * -m_cameraCanvasSize.x * (1 - m_marginX);
            position.y += 0.5f * m_cameraCanvasSize.y * (1 - m_marginY);
        }
        else if (m_orientation == OrientationType.RightToLeft)
        {
            // top right
            position.x += 0.5f * m_cameraCanvasSize.x * (1 - m_marginX);
            position.y += 0.5f * m_cameraCanvasSize.y * (1 - m_marginY);
        }

        return position;
    }
    private Vector3 GetLayerPaddingOrientation(int numberLayer)
    {
        if (numberLayer <= 1)
        {
            Debug.Log("Aborted: numberLayer was <= 1!");
            return new Vector3();
        }

        Vector3 padding = new Vector3();
        if (m_orientation == OrientationType.TopToDown)
        {
            padding.x = 0;
            padding.y = -m_cameraCanvasSize.y * (1 - m_marginY) / (numberLayer - 1);
        }
        else if (m_orientation == OrientationType.DownToTop)
        {
            padding.x = 0;
            padding.y = m_cameraCanvasSize.y * (1 - m_marginY) / (numberLayer - 1);
        }
        else if (m_orientation == OrientationType.LeftToRight)
        {
            padding.x = m_cameraCanvasSize.x * (1 - m_marginX) / (numberLayer - 1);
            padding.y = 0;
        }
        else if (m_orientation == OrientationType.RightToLeft)
        {
            padding.x = -m_cameraCanvasSize.x * (1 - m_marginX) / (numberLayer - 1);
            padding.y = 0;
        }

        return padding;
    }
    private Vector3 GetNodePddingOrientation(int numberNodes)
    {
        if (numberNodes <= 0)
        {
            Debug.Log("Aborted: numberNodes was <= 0!");
            return new Vector3();
        }

        Vector3 padding = new Vector3();
        if (m_orientation == OrientationType.TopToDown)
        {
            padding.x = m_cameraCanvasSize.x * (1 - m_marginX) / (numberNodes - 1);
            padding.y = 0;
        }
        else if (m_orientation == OrientationType.DownToTop)
        {
            padding.x = m_cameraCanvasSize.x * (1 - m_marginX) / (numberNodes - 1);
            padding.y = 0;
        }
        else if (m_orientation == OrientationType.LeftToRight)
        {
            padding.x = 0;
            padding.y = -m_cameraCanvasSize.y * (1 - m_marginY) / (numberNodes - 1);
        }
        else if (m_orientation == OrientationType.RightToLeft)
        {
            padding.x = 0;
            padding.y = -m_cameraCanvasSize.y * (1 - m_marginY) / (numberNodes - 1);
        }

        return padding;
    }
    #endregion
}
