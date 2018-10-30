using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiMovement : MonoBehaviour {

    [Header("------- Settings -------")]
    [Header("----- Movement -----")]
    [SerializeField] private float m_movementSpeed;

    [Header("--- Movement Decision ---")]
    [SerializeField] private MovementDecision m_movementDecision;
    [SerializeField] private float m_decisionCooldown;

    [Header("----- Neural Network -----")]
    [SerializeField] private NetworkOutputInterpretation m_interpretationType;
    [SerializeField] private float m_decisionThreshold;

    [Header("--- Objects ---")]
    [SerializeField] private RestrictedArea m_restrictedArea;

    [Header("------- Debug -------")]
    [SerializeField] private Vector3 m_currentMoveDirection;
    [SerializeField] private float m_decisionCooldownRdy;
    [SerializeField] private float m_randomInitialCooldownRdy = 1;


    [SerializeField] private bool m_isPressingUp;
    [SerializeField] private bool m_isPressingDown;
    [SerializeField] private bool m_isPressingLeft;
    [SerializeField] private bool m_isPressingRight;

    public enum NetworkOutputInterpretation { randomAndThreshold, fullyRandom, thresholdOnly }
    public enum MovementDecision { dontDecide, network, algorithm, random }

    #region Mono
    void Start()
    {

    }
    private void Update()
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
        {
            return m_currentMoveDirection;
        }
        m_decisionCooldownRdy = Time.time + m_decisionCooldown;

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

        if (m_movementDecision == MovementDecision.network)
            GetPressingInputViaNetwork();
        if (m_movementDecision == MovementDecision.algorithm)
            GetPressingInputViaAlgorithm();
        if (m_movementDecision == MovementDecision.random)
            GetPressingInputViaRandom();
    }
    void GetPressingInputViaNetwork()
    {
        List<float> output = GetOutputViaNetwork();
        if (output.Count == 0)
            return;

        float upOutput = output[0];
        float downOutput = output[1];
        float leftOutput = output[2];
        float rightOutput = output[3];

        if (m_interpretationType == NetworkOutputInterpretation.thresholdOnly)
        {
            if (upOutput >= m_decisionThreshold)
                m_isPressingUp = true;
            if (downOutput >= m_decisionThreshold)
                m_isPressingDown = true;
            if (leftOutput >= m_decisionThreshold)
                m_isPressingLeft = true;
            if (rightOutput >= m_decisionThreshold)
                m_isPressingRight = true;
        }
        if (m_interpretationType == NetworkOutputInterpretation.fullyRandom)
        {
            Debug.Log("Warning: NetworkOutputInterpretation.fullyRandom not implemented yet!");
        }
        if (m_interpretationType == NetworkOutputInterpretation.randomAndThreshold)
        {
            if (m_randomInitialCooldownRdy > Time.time)
                return;

            if (upOutput < m_decisionThreshold)
                upOutput = 0;
            if (downOutput < m_decisionThreshold)
                downOutput = 0;
            if (leftOutput < m_decisionThreshold)
                leftOutput = 0;
            if (rightOutput < m_decisionThreshold)
                rightOutput = 0;

            // decide left/right
            if (leftOutput != 0 || rightOutput != 0)
            {
                if (leftOutput == 0)
                    m_isPressingRight = true;
                else if (rightOutput == 0)
                    m_isPressingLeft = true;
                else
                {
                    float random = Random.Range(0, leftOutput + rightOutput);
                    if (random < leftOutput)
                        m_isPressingLeft = true;
                    else
                        m_isPressingRight = true;
                }
            }
            // decide up/down
            if (upOutput != 0 || downOutput != 0)
            {
                if (upOutput == 0)
                    m_isPressingDown = true;
                else if (downOutput == 0)
                    m_isPressingUp = true;
                else
                {
                    float random = Random.Range(0, upOutput + downOutput);
                    if (random < upOutput)
                        m_isPressingUp = true;
                    else
                        m_isPressingDown = true;
                }
            }
        }
    }
    void GetPressingInputViaAlgorithm()
    {
        
    }
    void GetPressingInputViaRandom()
    {
        m_isPressingUp = 0.5f >= Random.Range(0f, 1f);
        m_isPressingDown = 0.5f >= Random.Range(0f, 1f);
        m_isPressingLeft = 0.5f >= Random.Range(0f, 1f);
        m_isPressingRight = 0.5f >= Random.Range(0f, 1f);
    }
    #endregion

    #region Get Output Via Network
    List<float> GetOutputViaNetwork()
    {
        Debug.Log("Aborted: MovementDecision.network not implemented yet!");
        List<float> input = GetInputForNetwork();
        return null;
    }
    List<float> GetInputForNetwork()
    {
        return null;
    }
    #endregion
}
