﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AiMovement : MonoBehaviour {

    [Header("------- Settings -------")]
    [Header("----- Movement -----")]
    [SerializeField] private float m_movementSpeed;
    [SerializeField] private bool m_allowUpDownMovement;

    [Header("--- Movement Decision ---")]
    [SerializeField] private MovementDecision m_movementDecision;
    [SerializeField] private float m_decisionCooldownMin;
    [SerializeField] private float m_decisionCooldownMax;

    [Header("----- Interpretation Type -----")]
    [SerializeField] private NetworkOutputInterpretation m_interpretationType;
    //[SerializeField] private float m_decisionThreshold;
    [SerializeField] private float m_interpretationVar;

    [Header("--- Display Cost ---")]
    [SerializeField] private bool m_displayCost;

    [Header("--- Seed ---")]
    [SerializeField] private int m_initSeed;


    [Header("--- Objects ---")]
    [SerializeField] private NeuralNetworkContainer m_networkContainer;
    [SerializeField] private RestrictedArea m_restrictedArea;
    [SerializeField] private Text m_costText;

    [Header("------- Debug -------")]
    private Vector3 m_currentMoveDirection;
    private float m_decisionCooldownRdy;
    private float m_randomInitialCooldownRdy = 1;


    private bool m_isPressingUp;
    private bool m_isPressingDown;
    private bool m_isPressingLeft;
    private bool m_isPressingRight;
    private int m_currentSeed;

    #region Enums
    public enum NetworkOutputInterpretation { DynamicRandomWaitThreshold, DynamicRandomThreshold, Random}
    public enum MovementDecision { DontDecide, Network, Algorithm, Random }
    #endregion

    #region Mono
    void Awake()
    {
        m_currentSeed = m_initSeed;
    }
    private void FixedUpdate()
    {
        ManageMovement();
    }
    #endregion

    #region Manage Movement
    void ManageMovement()
    {
        m_currentMoveDirection = GetMoveDirection();
        RestrictMovement();
        ApplyForce();
    }
    void ApplyForce()
    {
        if (m_currentMoveDirection == Vector3.zero)
            return;

        transform.position += /*m_levelOrientation.rotation **/ m_currentMoveDirection * Time.deltaTime;
    }
    void RestrictMovement()
    {
        if (m_isPressingUp && m_restrictedArea.IsOutOfRestrictionPosY(transform.position))
            m_currentMoveDirection.y = 0;
        if (m_isPressingDown && m_restrictedArea.IsOutOfRestrictionNegY(transform.position))
            m_currentMoveDirection.y = 0;
        if (m_isPressingLeft && m_restrictedArea.IsOutOfRestrictionNegX(transform.position))
            m_currentMoveDirection.x = 0;
        if (m_isPressingRight && m_restrictedArea.IsOutOfRestrictionPosX(transform.position))
            m_currentMoveDirection.x = 0;

        
    }
    Vector3 GetMoveDirection()
    {
        if (m_decisionCooldownRdy > Time.time)
            return m_currentMoveDirection;
        m_decisionCooldownRdy = Time.time + GetRandom(m_decisionCooldownMin, m_decisionCooldownMax);

        Vector3 finalMoveDirection = Vector3.zero;

        GetPressingInput();

        if (m_isPressingUp)
            finalMoveDirection.y += m_movementSpeed;
        if (m_isPressingDown)
            finalMoveDirection.y -= m_movementSpeed;
        if (m_isPressingLeft)
            finalMoveDirection.x -= m_movementSpeed;
        if (m_isPressingRight)
            finalMoveDirection.x += m_movementSpeed;


        return finalMoveDirection;
    }
    #endregion

    #region Get Pressing Input
    void GetPressingInput()
    {
        m_isPressingUp = false;
        m_isPressingDown = false;
        m_isPressingLeft = false;
        m_isPressingRight = false;

        if (m_movementDecision == MovementDecision.Network)
            GetPressingInputViaNetwork();
        if (m_movementDecision == MovementDecision.Algorithm)
            GetPressingInputViaAlgorithm();
        if (m_movementDecision == MovementDecision.Random)
            GetPressingInputViaRandom();

        if (!m_allowUpDownMovement)
            m_isPressingUp = m_isPressingDown = false;
    }
    void GetPressingInputViaNetwork()
    {
        float[] output = GetOutputViaNetwork();
        if (output.Length == 0)
            return;

        float leftOutput = output[0];
        float rightOutput = output[1];
        
        float upOutput = 0;
        float downOutput = 0;
        if (output.Length > 3)
        {
            upOutput = output[2];
            downOutput = output[3];
        }
        float noOutput = 0;
        if (output.Length == 3 || output.Length == 5)
        {
            noOutput = output.Length == 3 ? output[2] : output[4];
        }

        if (m_interpretationType == NetworkOutputInterpretation.DynamicRandomWaitThreshold)
        {
            if (m_randomInitialCooldownRdy > Time.time)
                return;

            if (noOutput < leftOutput * m_interpretationVar || noOutput < rightOutput * m_interpretationVar) // stay
                noOutput = 0;
            if (leftOutput < rightOutput * m_interpretationVar || leftOutput < noOutput * m_interpretationVar) // go left
                leftOutput = 0;
            if (rightOutput < leftOutput * m_interpretationVar || rightOutput < noOutput * m_interpretationVar) // go right
                rightOutput = 0;

            

            float random = GetRandom(0, leftOutput + rightOutput + noOutput);
            if (random < leftOutput)
                m_isPressingLeft = true;
            else if (random < rightOutput + leftOutput)
                m_isPressingRight = true;

            // Threshold
            //if (output.Length == 2)
            //{
            //if (noOutput == 1 && (leftOutput == 1 || rightOutput == 1))
            //{
            //    Debug.Log("Warning: Output is corrupted! " + leftOutput + ", " + rightOutput + ", " + noOutput);
            //    leftOutput = rightOutput = 0;
            //}


            //if (noOutput > rightOutput && noOutput > leftOutput) // stay
            //        leftOutput = rightOutput = 0;
            //    else if (leftOutput < rightOutput * m_interpretationVar) // go left
            //        leftOutput = 0;
            //    else if (rightOutput < leftOutput * m_interpretationVar) // go right
            //        rightOutput = 0;
            //    else // else stay
            //        leftOutput = rightOutput = 0;
            //}
            //else if(output.Length == 3)
            //{



            //}
            //else
            //    Debug.Log("Warning: Feature not implemented yet!");


            // decide left/right
            //if (leftOutput != 0 || rightOutput != 0)
            //{



            //if (leftOutput > 0)
            //    m_isPressingLeft = true;
            //else if (rightOutput > 0)
            //    m_isPressingRight = true;

            //if (leftOutput == 0 && noOutput == 0)
            //        m_isPressingRight = true;
            //    else if (rightOutput == 0 && noOutput == 0)
            //        m_isPressingLeft = true;
            //    else if (rightOutput == 0 && leftOutput == 0)
            //        ;
            //    else
            //        ;
            //}

        }
        if (m_interpretationType == NetworkOutputInterpretation.DynamicRandomThreshold)
        {
            Debug.Log("Warning: Obsolet Code!");

            if (m_randomInitialCooldownRdy > Time.time)
                return;

            // Threshold
            if (output.Length == 2)
            {
                if (leftOutput < rightOutput * m_interpretationVar)
                    leftOutput = 0;
                if (rightOutput < leftOutput * m_interpretationVar)
                    rightOutput = 0;
            }
            if (output.Length == 3)
            {
                float max = Mathf.Max(leftOutput, upOutput, noOutput);
                if (leftOutput <= max * m_interpretationVar)
                    leftOutput = 0;
                if (rightOutput <= max * m_interpretationVar)
                    rightOutput = 0;
                if (noOutput <= max * m_interpretationVar)
                    noOutput = 0;
            }
            if (output.Length > 3)
            {
                if (upOutput < downOutput * m_interpretationVar)
                    upOutput = 0;
                if (downOutput < upOutput * m_interpretationVar)
                    downOutput = 0;
            }

            // decide left/right
            if (leftOutput != 0 || rightOutput != 0)
            {
                if (leftOutput == 0 && noOutput == 0)
                    m_isPressingRight = true;
                else if (rightOutput == 0 && noOutput == 0)
                    m_isPressingLeft = true;
                else if (rightOutput == 0 && leftOutput == 0)
                    ;
                else
                {
                    float random = GetRandom(0, leftOutput + rightOutput + noOutput);
                    if (random < leftOutput)
                        m_isPressingLeft = true;
                    else if (random < leftOutput + rightOutput)
                        m_isPressingRight = true;
                    else
                        ;
                }
            }
            // decide up/down
            if (output.Length > 3)
            {
                Debug.Log("Warning: Code not uptodate!");
                if (upOutput != 0 || downOutput != 0)
                {
                    if (upOutput == 0)
                        m_isPressingDown = true;
                    else if (downOutput == 0)
                        m_isPressingUp = true;
                    else
                    {
                        float random = GetRandom(0, upOutput + downOutput);
                        if (random < upOutput)
                            m_isPressingUp = true;
                        else
                            m_isPressingDown = true;
                    }
                }
            }
        }
        if (m_interpretationType == NetworkOutputInterpretation.Random)
        {
            Debug.Log("Warning: Obsolet Code!");
            Debug.Log("Warning: Code not uptodate!");
            if (m_randomInitialCooldownRdy > Time.time)
                return;

            // decide left/right
            
            float random = GetRandom(0, leftOutput + rightOutput);
            if (random < leftOutput)
                m_isPressingLeft = true;
            else
                m_isPressingRight = true;

            // decide up/down
            if (output.Length > 3)
            {
                
                random = GetRandom(0, upOutput + downOutput);
                if (random < upOutput)
                    m_isPressingUp = true;
                else
                    m_isPressingDown = true;
            }
        }
        //if (m_interpretationType == NetworkOutputInterpretation.RandomAndThreshold)
        //{
        //    Debug.Log("Warning: Code not uptodate!");
        //    if (m_randomInitialCooldownRdy > Time.time)
        //        return;


        //    if (leftOutput < m_decisionThreshold)
        //        leftOutput = 0;
        //    if (rightOutput < m_decisionThreshold)
        //        rightOutput = 0;
        //    if (output.Length > 3)
        //    {
        //        if (upOutput < m_decisionThreshold)
        //            upOutput = 0;
        //        if (downOutput < m_decisionThreshold)
        //            downOutput = 0;
        //    }


        //    // decide left/right
        //    if (leftOutput != 0 || rightOutput != 0)
        //    {
        //        if (leftOutput == 0)
        //            m_isPressingRight = true;
        //        else if (rightOutput == 0)
        //            m_isPressingLeft = true;
        //        else
        //        {
        //            float random = GetRandom(0, leftOutput + rightOutput);
        //            if (random < leftOutput)
        //                m_isPressingLeft = true;
        //            else
        //                m_isPressingRight = true;
        //        }
        //    }
        //    // decide up/down
        //    if (output.Length > 3)
        //    {
        //        if (upOutput != 0 || downOutput != 0)
        //        {
        //            if (upOutput == 0)
        //                m_isPressingDown = true;
        //            else if (downOutput == 0)
        //                m_isPressingUp = true;
        //            else
        //            {
        //                float random = GetRandom(0, upOutput + downOutput);
        //                if (random < upOutput)
        //                    m_isPressingUp = true;
        //                else
        //                    m_isPressingDown = true;
        //            }
        //        }
        //    }
        //}
    }
    void GetPressingInputViaAlgorithm()
    {
        
    }
    void GetPressingInputViaRandom()
    {
        m_isPressingUp = 0.5f >= GetRandom(0f, 1f);
        m_isPressingDown = 0.5f >= GetRandom(0f, 1f);
        m_isPressingLeft = 0.5f >= GetRandom(0f, 1f);
        m_isPressingRight = 0.5f >= GetRandom(0f, 1f);
    }
    #endregion

    #region Get Output Via Network
    float[] GetOutputViaNetwork()
    {
        float[] input = GetInputForNetwork();
        float[] output = m_networkContainer.m_network.GetOutput(input);
        m_networkContainer.m_visualizationNetwork.UpdateActivisionsNetwork(input);
        //UpdateCostText(m_networkContainer.m_network.GetCost(PlayerMovementManager.Instance().GenerateInputData(), output));
        return output;
    }
    float[] GetInputForNetwork()
    {
        SampleContainer sample = m_networkContainer.m_sampleManager.GenerateSampleThis();
        //m_networkContainer.m_visualization.UpdateVisualizationSample(sample);
        float[] input = sample.m_input;
        return input;
    }
    #endregion

    #region Misc
    private void UpdateCostText(float cost)
    {
        if (m_costText == null)
            return;

        if(!m_displayCost)
        {
            m_costText.enabled = false;
            return;
        }

        m_costText.text = (cost != 0 ? 1 / cost : 0).ToString("0.000");
    }
    private float GetRandom(float min, float max)
    {
        float random = 0;
        if (m_initSeed >= 0)
            random = Utility.GetRandomWithSeed(min, max, m_currentSeed++);
        else
            random = Random.Range(min, max);
        return random;
    }
    private int GetRandom(int min, int max)
    {
        int random = 0;
        if (m_initSeed >= 0)
            random = Utility.GetRandomWithSeed(min, max, m_currentSeed++);
        else
            random = Random.Range(min, max);
        return random;
    }
    #endregion
}
