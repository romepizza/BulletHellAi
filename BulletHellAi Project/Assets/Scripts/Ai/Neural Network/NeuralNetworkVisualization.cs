using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetworkVisualization : MonoBehaviour
{
    private Vector3 m_originPosition;
    private NeuralNetwork m_network;
    private Transform[][] m_nodes;
    private Transform[][] m_activisions;
    // private List<...> m_weights;
    private float m_layerGapDistance;
    private float[] m_nodeGapDistances;
    private int m_layerCount;

    private NeuralNetworkVisualizationManager m_manager;

    #region Mono
    private void Start()
    {
        m_manager = NeuralNetworkVisualizationManager.Instance();
    }
    #endregion

    #region Creation
    public void CreateVisualization(NeuralNetwork network, Vector3 originalPosition)
    {
        m_network = network;
        m_layerCount = network.m_layerCount;
        m_originPosition = originalPosition;

        CreateObjects();
        PlaceObjects();
        ColorObjects();
    }
    private void CreateObjects()
    {
        // create node and activision objects
        m_nodes = new Transform[m_layerCount][];
        m_activisions = new Transform[m_layerCount][];
        for(int layerIndex = 0; layerIndex < m_layerCount; layerIndex++)
        {
            int nodeCount = m_network.m_layerLengths[layerIndex];
            Transform[] nodesThisLayer = new Transform[nodeCount];
            Transform[] activisionsThisLayer = new Transform[nodeCount];
            for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
            {
                GameObject node = Instantiate(m_manager.GetNodePrefabHemisphere(), transform);
                nodesThisLayer[nodeIndex] = node.transform;

                GameObject activision = Instantiate(m_manager.GetNodePrefabHemisphere(), transform);
                activision.transform.Rotate(activision.transform.right, 180f);
                activisionsThisLayer[nodeIndex] = activision.transform;
            }

            m_nodes[layerIndex] = nodesThisLayer;
            m_activisions[layerIndex] = activisionsThisLayer;
        }
    }
    private void PlaceObjects()
    {

    }
    private void ColorObjects()
    {

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

    #region Misc

    #endregion
}
