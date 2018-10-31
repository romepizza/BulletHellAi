using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetworkVisualizationManager : MonoBehaviour
{
    private static NeuralNetworkVisualizationManager s_instance;

    [Header("------- Settings -------")]
    [Header("--- Objects ---")]
    [SerializeField] private Transform m_parentTransform;
    [SerializeField] private GameObject m_objectPrefab;
    [Space]
    [SerializeField] private GameObject m_nodePrefabSphere;
    [SerializeField] private GameObject m_nodePrefabHemisphere;

    [Header("------- Debug -------")]
    private List<NeuralNetworkVisualization> m_activeVisualizations;

    #region Mono
    private void Awake()
    {
        if (s_instance != null)
            Debug.Log("Warning: More than one instance of NeuralNetworkVisualizationManager is active!");
        s_instance = this;
    }
    private void Start()
    {
        m_activeVisualizations = new List<NeuralNetworkVisualization>();
    }
    #endregion

    public void CreateVisualization(NeuralNetwork neuralNetwork)
    {
        GameObject g = Instantiate(m_objectPrefab, m_parentTransform);

        NeuralNetworkVisualization visualizationScript = g.GetComponent<NeuralNetworkVisualization>();
        if(visualizationScript == null)
            visualizationScript = g.AddComponent<NeuralNetworkVisualization>();
        visualizationScript.CreateVisualization(neuralNetwork, Vector3.zero);

        m_activeVisualizations.Add(visualizationScript);
    }
    public void UpdateVisualization()
    {

    }

    #region Getter
    public GameObject GetNodePrefabHemisphere()
    {
        return m_nodePrefabHemisphere;
    }
    public GameObject GetNodePrefabSphere()
    {
        return m_nodePrefabSphere;
    }
    #endregion

    #region Statics
    public static NeuralNetworkVisualizationManager Instance()
    {
        return s_instance;
    }
    #endregion
}
