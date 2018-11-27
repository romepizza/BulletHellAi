using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetworkVisualizationManager : MonoBehaviour
{
    private static NeuralNetworkVisualizationManager s_instance;

    [Header("------- Settings -------")]

    [Header("--- Colors ---")]
    [Header("- Activision -")]
    [SerializeField] private Color m_colorMinValueActivision;
    [SerializeField] private Color m_colorAverageValueActivision;
    [SerializeField] private Color m_colorMaxValueActivision;

    [Header("- Bias -")]
    [SerializeField] private Color m_colorMinValueBias;
    [SerializeField] private Color m_colorAverageValueBias;
    [SerializeField] private Color m_colorMaxValueBias;

    [Header("- Weight -")]
    [SerializeField] private float m_disableThresholdWeights;
    [SerializeField] private float[] m_colorThresholdsWeight;
    [SerializeField] private Color m_colorMinValueWeight;
    [SerializeField] private Color m_colorMinAvgValueWeight;
    [SerializeField] private Color m_colorAverageValueWeight;
    [SerializeField] private Color m_colorMaxAvgValueWeight;
    [SerializeField] private Color m_colorMaxValueWeight;

    [Header("--- Objects ---")]
    //[SerializeField] private Transform m_parentTransform;
    [SerializeField] private GameObject m_objectPrefab;
    [Space]
    [SerializeField] private GameObject m_layerPrefab;
    [SerializeField] private GameObject m_nodePrefabSphere;
    [SerializeField] private GameObject m_nodePrefabHemisphere;
    [SerializeField] private GameObject m_weightPrefab;

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

    public void CreateVisualization(NeuralNetworkContainer networkContainer)
    {
        GameObject g = Instantiate(m_objectPrefab, networkContainer.transform);

        NeuralNetworkVisualization visualizationScript = g.GetComponent<NeuralNetworkVisualization>();
        if(visualizationScript == null)
            visualizationScript = g.AddComponent<NeuralNetworkVisualization>();

        visualizationScript.CreateVisualization(networkContainer);//, position, rotation, 10);

        m_activeVisualizations.Add(visualizationScript);
    }
    public void UpdateVisualization()
    {

    }

    #region Getter
    // prefabs
    public GameObject GetLayerPrefab()
    {
        return m_layerPrefab;
    }
    public GameObject GetNodePrefabHemisphere()
    {
        return m_nodePrefabHemisphere;
    }
    public GameObject GetNodePrefabSphere()
    {
        return m_nodePrefabSphere;
    }
    public GameObject GetWeightPrefab()
    {
        return m_weightPrefab;
    }
    
    // colors
    // actvision
    public Color GetColorActivisionMin()
    {
        return m_colorMinValueActivision;
    }
    public Color GetColorActivisionAverage()
    {
        return m_colorAverageValueActivision;
    }
    public Color GetColorActivisionMax()
    {
        return m_colorMaxValueActivision;
    }
    // bias
    public Color GetColorBiasMin()
    {
        return m_colorMinValueBias;
    }
    public Color GetColorBiasAverage()
    {
        return m_colorAverageValueBias;
    }
    public Color GetColorBiasMax()
    {
        return m_colorMaxValueBias;
    }
    // weight
    public float[] GetColorThresholdsWeight()
    {
        return m_colorThresholdsWeight;
    }
    public float GetDisableThresholdWeights()
    {
        return m_disableThresholdWeights;
    }
    public Color GetColorWeightMin()
    {
        return m_colorMinValueWeight;
    }
    public Color GetColorWeightMinAvg()
    {
        return m_colorMinAvgValueWeight;
    }
    public Color GetColorWeightAverage()
    {
        return m_colorAverageValueWeight;
    }
    public Color GetColorWeightMax()
    {
        return m_colorMaxValueWeight;
    }
    public Color GetColorWeightMaxAvg()
    {
        return m_colorMaxAvgValueWeight;
    }


    #endregion

    #region Statics
    public static NeuralNetworkVisualizationManager Instance()
    {
        return s_instance;
    }
    #endregion
}
