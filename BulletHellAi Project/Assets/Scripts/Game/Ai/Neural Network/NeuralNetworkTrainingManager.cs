using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetworkTrainingManager : MonoBehaviour
{
    [Header("------ Settings -------")]
    [SerializeField] private float m_trainingCooldown;
    [Header("--- Objects ---")]
    [SerializeField] private NeuralNetworkContainer m_container;
    [SerializeField] private SampleManager m_sampleManager;

    [Header("------ Debug -------")]
    //private NeuralNetworkContainer.InputType m_inputType;
    private NeuralNetwork m_network;


    private float m_trainingCooldownRdy;

    #region Mono
    private void Start()
    {
        //m_inputType = m_container.GetInputType();
        m_network = m_container.m_network;

        m_trainingCooldownRdy = Time.time + m_trainingCooldown;
    }
    private void Update()
    {
        ManageTraining();
    }
    #endregion


    #region Training
    private void ManageTraining()
    {
        if (m_trainingCooldownRdy < Time.time)
            return;
        m_trainingCooldownRdy = Time.time + m_trainingCooldown;

        TrainNetwork();
    }
    public void TrainNetwork()
    {
        SampleContainer sample = m_sampleManager.GenerateSample();

        m_network.addTrainingData(sample.m_input, sample.m_desiredOutput);
    }
    #endregion
}
