using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetworkVisualization : MonoBehaviour
{
    [Header("------- Settings -------")]
    private float m_marginX = 0.25f;
    private float m_marginY = 0.25f;
    private OrientationType m_orientation = OrientationType.leftToRight;

    [Header("------- Debug -------")]
    private Vector3 m_originPosition;
    private NeuralNetwork m_network;
    private Transform[] m_layerObjects;
    private Transform[][] m_nodeObjects;
    private Transform[][] m_activisionObjects;
    // private List<...> m_weights;
    private float m_layerGapDistance;
    private float[] m_nodeGapDistances;
    private int m_layerCount;
    private Camera m_camera;
    private Vector2 m_cameraCanvasSize;

    private NeuralNetworkVisualizationManager m_manager;

    private enum OrientationType { topToDown, downToTop, leftToRight, rightToLeft }

    #region Creation
    public void CreateVisualization(NeuralNetwork network, Vector3 originalPosition)
    {
        m_manager = NeuralNetworkVisualizationManager.Instance();
        m_network = network;
        m_layerCount = network.m_layerCount;
        m_originPosition = originalPosition;
        
        m_camera = GetComponentInChildren<Camera>();
        if (m_camera == null)
            Debug.Log("Warning: Camera on Visualization object not found!");
        m_cameraCanvasSize.y = m_camera.orthographicSize * 2f;
        m_cameraCanvasSize.x = m_cameraCanvasSize.y * m_camera.aspect;

        CreateObjects();
        PlaceObjects();
        ColorObjects();
    }
    private void CreateObjects()
    {
        // create node and activision objects
        m_nodeObjects = new Transform[m_layerCount][];
        m_activisionObjects = new Transform[m_layerCount][];
        for(int layerIndex = 0; layerIndex < m_layerCount; layerIndex++)
        {
            int nodeCount = m_network.m_layerLengths[layerIndex];
            Transform[] nodesThisLayer = new Transform[nodeCount];
            Transform[] activisionsThisLayer = new Transform[nodeCount];
            for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
            {
                GameObject node = Instantiate(m_manager.GetNodePrefabHemisphere(), transform);
                node.name = "Node_" + layerIndex + "_" + nodeIndex;
                node.layer = Statics.s_neuralNetworkLayer;
                node.transform.Rotate(node.transform.right, -90f);
                nodesThisLayer[nodeIndex] = node.transform;

                GameObject activision = Instantiate(m_manager.GetNodePrefabHemisphere(), transform);
                activision.name = "Activison_" + layerIndex + "_" + nodeIndex;
                activision.layer = Statics.s_neuralNetworkLayer;
                activision.transform.Rotate(activision.transform.right, 90f);
                activisionsThisLayer[nodeIndex] = activision.transform;
            }

            m_nodeObjects[layerIndex] = nodesThisLayer;
            m_activisionObjects[layerIndex] = activisionsThisLayer;
        }

        // create layer objects
    }
    private void PlaceObjects()
    {
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
                Transform node = m_nodeObjects[layerIndex][nodeIndex];
                node.position = currentLayerPadding + currentNodePadding;

                Transform activision = m_activisionObjects[layerIndex][nodeIndex];
                activision.position = currentLayerPadding + currentNodePadding;

                currentNodePadding += nodePadding;
            }
            currentLayerPadding += layerPadding;
        }
    }
    private void ColorObjects()
    {
        // biases
        float minValueBias = float.MaxValue;
        float maxValueBias = float.MinValue;
        // get min / max value
        for(int layerIndex = 1; layerIndex < m_layerCount; layerIndex++)
        {
            int nodeCount = m_network.m_layerLengths[layerIndex];
            for(int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
            {
                float value = m_network.m_biases[layerIndex][nodeIndex];
                maxValueBias = Mathf.Max(value, maxValueBias);
                minValueBias = Mathf.Min(value, minValueBias);
            }
        }
        // set actual colors
        Color color;
        for (int layerIndex = 1; layerIndex < m_layerCount; layerIndex++)
        {
            int nodeCount = m_network.m_layerLengths[layerIndex];
            for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
            {
                Transform node = m_nodeObjects[layerIndex][nodeIndex];

                float value = m_network.m_biases[layerIndex][nodeIndex];
                float mappedValue = Utility.MapValuePercent(minValueBias, maxValueBias, value);
                if (mappedValue > 0.5f)
                    color = Color.Lerp(Color.yellow, Color.green, (mappedValue - 0.5f) * 2f);
                else
                    color = Color.Lerp(Color.red, Color.yellow, mappedValue * 2f);

                Renderer renderer = node.GetComponent<Renderer>();
                MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                mpb.SetColor("_Color", color);
                renderer.material.EnableKeyword("_EMISSION");
                mpb.SetColor("_EmissionColor", color);
                renderer.SetPropertyBlock(mpb);
            }
        }

    }
    #endregion

    #region Update
    public void UpdateVisualization(NeuralNetwork network)
    {

    }
    #endregion

    #region Manage State
    public void SetActive(bool value)
    {

    }
    public void DestroySelf()
    {

    }
    #endregion

    #region Orientation
    private Vector3 GetOriginPositionOrientation()
    {
        Vector3 position = new Vector3();

        if (m_orientation == OrientationType.topToDown)
        {
            // top left
            position.x = 0.5f * -m_cameraCanvasSize.x * (1 - m_marginX);
            position.y = 0.5f * m_cameraCanvasSize.y * (1 - m_marginY);
        }
        else if (m_orientation == OrientationType.downToTop)
        {
            // down left
            position.x = 0.5f * -m_cameraCanvasSize.x * (1 - m_marginX);
            position.y = 0.5f * -m_cameraCanvasSize.y * (1 - m_marginY);
        }
        else if (m_orientation == OrientationType.leftToRight)
        {
            // top left
            position.x = 0.5f * -m_cameraCanvasSize.x * (1 - m_marginX);
            position.y = 0.5f * m_cameraCanvasSize.y * (1 - m_marginY);
        }
        else if (m_orientation == OrientationType.rightToLeft)
        {
            // top right
            position.x = 0.5f * m_cameraCanvasSize.x * (1 - m_marginX);
            position.y = 0.5f * m_cameraCanvasSize.y * (1 - m_marginY);
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
        if (m_orientation == OrientationType.topToDown)
        {
            padding.x = 0;
            padding.y = -m_cameraCanvasSize.y * (1 - m_marginY) / (numberLayer - 1);
        }
        else if (m_orientation == OrientationType.downToTop)
        {
            padding.x = 0;
            padding.y = m_cameraCanvasSize.y * (1 - m_marginY) / (numberLayer - 1);
        }
        else if (m_orientation == OrientationType.leftToRight)
        {
            padding.x = m_cameraCanvasSize.x * (1 - m_marginX) / (numberLayer - 1);
            padding.y = 0;
        }
        else if (m_orientation == OrientationType.rightToLeft)
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
        if (m_orientation == OrientationType.topToDown)
        {
            padding.x = m_cameraCanvasSize.x * (1 - m_marginX) / (numberNodes - 1);
            padding.y = 0;
        }
        else if (m_orientation == OrientationType.downToTop)
        {
            padding.x = m_cameraCanvasSize.x * (1 - m_marginX) / (numberNodes - 1);
            padding.y = 0;
        }
        else if (m_orientation == OrientationType.leftToRight)
        {
            padding.x = 0;
            padding.y = -m_cameraCanvasSize.y * (1 - m_marginY) / (numberNodes - 1);
        }
        else if (m_orientation == OrientationType.rightToLeft)
        {
            padding.x = 0;
            padding.y = -m_cameraCanvasSize.y * (1 - m_marginY) / (numberNodes - 1);
        }

        return padding;
    }
    #endregion
}
