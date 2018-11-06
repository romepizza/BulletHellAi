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
    [SerializeField] private Color m_colorMinValueWeight;
    [SerializeField] private Color m_colorAverageValueWeight;
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
    private int creationIndex;

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

        Vector3 position = Vector3.zero;
        Vector3 rotation = Vector3.zero;

        if (creationIndex == 0)
        {
            position.x = -10;
            rotation.y = -90;
        }
        else if (creationIndex == 1)
        {
            position.x = 10;
            rotation.y = 90;
        }
        else if (creationIndex == 2)
        {
            position.y = 10;
            rotation.x = 90;
        }
        else if (creationIndex == 3)
        {
            position.y = -10;
            rotation.x = -90;
        }
        else if (creationIndex == 4)
        {
            position.z = -10;
            rotation.y = 180;
        }
        else
            return;

        creationIndex++;

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
    public Color GetColorWeightMin()
    {
        return m_colorMinValueWeight;
    }
    public Color GetColorWeightAverage()
    {
        return m_colorAverageValueWeight;
    }
    public Color GetColorWeightMax()
    {
        return m_colorMaxValueWeight;
    }
    
    #endregion

    #region Statics
    public static NeuralNetworkVisualizationManager Instance()
    {
        return s_instance;
    }
    #endregion
}
