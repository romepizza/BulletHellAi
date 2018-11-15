using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAiMovement : MonoBehaviour
{
    private static PlayerAiMovement s_instance;

    [Header("------ Settings ------")]
    [SerializeField] private InputType m_inputType;

    [Header("--- Raycast ---")]
    [SerializeField] private float m_distanceMoveThreshold;
    [SerializeField] private LayerMask m_layerMaskPlayer;
    [SerializeField] private LayerMask m_layerMaskEnemy;
    [SerializeField] private float m_additionalRaycastHeight;
    [SerializeField] private float m_switchDirectionTimer;

    [Header("--- Misc ---")]
    [SerializeField] private float m_showCooldown;

    [Header("--- Objects ---")]
    [SerializeField] private TakeScreenshot m_screenshotManager;
    [SerializeField] private Transform m_visualCapturePlayer;
    [SerializeField] private Transform m_captureArea;

    [Header("------ Debug ------")]
     private Vector2Int m_areaSizePixel;
     private Vector2 m_areaSizeWorld;
    private float m_pixelSize;
    private float m_raycastHeight;

    [Space]
     private float[] m_distances;
    private List<int> m_playerIndices;
    private Vector3 m_areaStartPosition;
    private Vector3 m_raycastPosition;

    [Space]
    private float m_distanceAhead;
    private float m_distanceLeftY;
    private float m_distanceRightY;
    private float m_distanceLeftX;
    private float m_distanceRightX;

    [Space]
     private float m_playerLengthWorld;
     private int m_playerLengthPixel;
     private int m_playerPositionPixelMin;
     private int m_playerPositionPixelMax;
     private int m_playerPositionPixelCenterMin;
     private int m_playerPositionPixelCenterMax;

    [Space]
     private int m_dangerIndexLeft;
     private int m_dangerIndexRight;

    [Space]
     private bool m_isMovingLeft;
     private bool m_isMovingRight;
     private int m_moveToIndexPositionLeft;
     private int m_moveToIndexPositionRight;
     private float m_position;
     private float m_wantPosition;

    [Space]
     private float m_actualAdditionalRaycastHeight;
    private float m_showCooldownRdy;
    private float m_changeDirectionTimer;

    #region Enums
    private enum InputType { Screenshot, Raycast }
    #endregion

    #region Mono
    private void Awake()
    {
        if (s_instance != null)
            Debug.Log("Warning: Seems like more than one instance of PlayerAiMovement is running!");
        s_instance = this;
    }
    void Start ()
    {
        if (m_screenshotManager == null)
            Debug.Log("Warning: m_screenshotManager is null!");
	}
    private void Update()
    {
        if(m_showCooldownRdy >= Time.time)
        {
            m_screenshotManager.GetScreenshotDataRaw(0, 0, false, true);
            m_showCooldownRdy = m_showCooldown + Time.time;
        }
    }
    #endregion

    #region Input Control
    public float[] GenerateInputData()
    {
        float[] inputData = null;

        if (m_inputType == InputType.Screenshot)
            inputData = GenerateInputDataScreenshot();
        else if (m_inputType == InputType.Raycast)
            inputData = GenerateInputDataRaycast();

        return inputData;
    }
    private float[] GenerateInputDataScreenshot()
    {
        float[] inputData = new float[ScreenshotManager.Instance().GetOutputNumber()];

        float[][] inputInformation = m_screenshotManager.GetScreenshotDataRaw(0, 0, false, true);
        int width = inputInformation.Length;
        int height = inputInformation[0].Length;

        int[] dangers = new int[width];
        Vector2Int playerPositionMin = new Vector2Int(int.MaxValue, int.MaxValue);
        Vector2Int playerPositionMax = Vector2Int.zero;
        // compute pixel data and fill danger array and player pixel list
        for(int x = 0; x < width;  x++)
        {
            bool dangerFound = false;
            for(int y = 0; y < height; y++)
            {
                float value = inputInformation[x][y];
                if(value > 0) // player
                {
                    playerPositionMin.x = Mathf.Min(playerPositionMin.x, x);
                    playerPositionMin.y = Mathf.Min(playerPositionMin.y, y);
                    playerPositionMax.x = Mathf.Max(playerPositionMax.x, x);
                    playerPositionMax.y = Mathf.Max(playerPositionMax.y, y);
                }
                else if(!dangerFound && value < 0) // enemy
                {
                    dangerFound = true;
                    int danger = height - y;

                    dangers[x] = danger;
                }
            }
        }

        // evaluate new danger values depending on player position
        for (int dangerIndex = 0; dangerIndex < dangers.Length; dangerIndex++)
        {
            int dangerValue = dangers[dangerIndex];
            if (dangerValue == 0)
                continue;
            int newDanger = dangerValue;
            if (dangerIndex < playerPositionMin.x) // pixels to the left
            {
                int distanceX = playerPositionMin.x - dangerIndex;
                newDanger = dangerValue - distanceX;
            }
            else if(dangerIndex > playerPositionMax.x) // pixels to the right
            {
                int distanceX = dangerIndex - playerPositionMax.x;
                newDanger = dangerValue - distanceX;
            }
            else// pixels ahead of player
            {
                newDanger = dangerValue;
            }

            dangers[dangerIndex] = newDanger;
        }

        // set the side dangers correctly if there is not enough space for the player
        int playerLengthX = (int)Mathf.Ceil(m_visualCapturePlayer.localScale.x / m_screenshotManager.GetPixelToWorldScale(0));
        int sideDanger = 0;
        for (int leftIndex = playerLengthX - 1; leftIndex >= 0; leftIndex--)
        {
            if (leftIndex < 0 || leftIndex >= width)
                Debug.Log("Warning!");
            int dangerValue = dangers[leftIndex];
            sideDanger = Mathf.Max(sideDanger, dangerValue);
            int newDanger = sideDanger;
            dangers[leftIndex] = newDanger;
        }
        sideDanger = 0;
        for (int rightIndex = width - 1 - playerLengthX + 1; rightIndex < width; rightIndex++)
        {
            if (rightIndex < 0 || rightIndex >= width)
                Debug.Log("Warning!");
            int dangerValue = dangers[rightIndex];
            sideDanger = Mathf.Max(sideDanger, dangerValue);
            int newDanger = sideDanger;
            dangers[rightIndex] = newDanger;
        }

        int dangerAhead = 0;
        int dangerLeft = 0;
        int dangerRight = 0;
        // decide the danger in the three directions
        int checkPixelExact = playerLengthX != 1 ? 1 : 0;
        for (int leftIndex = playerPositionMin.x - checkPixelExact; leftIndex >= 0; leftIndex--) // evaluate dangerLeft
        {
            if(dangers[leftIndex] > 0)
            {
                dangerLeft = Mathf.Max(dangerLeft, dangers[leftIndex]);
                //break;
            }
        }
        for (int rightIndex = playerPositionMax.x + checkPixelExact; rightIndex < width; rightIndex++) // evaluate dangerRight
        {
            if (dangers[rightIndex] > 0)
            {
                dangerRight = Mathf.Max(dangerRight, dangers[rightIndex]);
                //break;
            }
        }
        //int checkPixelExact = playerLengthX != 1 ? 1 : 0;
        for (int pixelIndex = playerPositionMin.x - checkPixelExact; pixelIndex <= playerPositionMax.x + checkPixelExact; pixelIndex++)
        {
            int i = pixelIndex;
            i = Mathf.Clamp(pixelIndex, 0, width - 1);
            if (dangers[i] > 0)
            {
                dangerAhead = Mathf.Max(dangerAhead, dangers[i] + 1);
                break;
            }
        }


        Debug.Log("A: " + dangerAhead + ", l: " + dangerLeft + ", r: " + dangerRight + "  , min: " + playerPositionMin.x + ", max: " + playerPositionMax.x);
        //Debug.Log("left: " + dangerLeft);
        //Debug.Log("right: " + dangerRight);

        bool stay = dangerAhead == 0 ||  (dangerAhead < dangerLeft && dangerAhead < dangerRight);
        if (stay)
            return inputData;
        bool goLeft = dangerLeft < dangerRight || (dangerLeft == dangerRight && (playerPositionMin.x + playerPositionMax.x) * 0.5f >= width * 0.5f);
        bool goRight = !goLeft;
        inputData[0] = goLeft ? 1 : 0;
        inputData[1] = goRight ? 1 : 0;

        return inputData;
    }
    private float[] GenerateInputDataRaycast()
    {
        float[] inputData = new float[ScreenshotManager.Instance().GetOutputNumber()];

        InitInfo();

        // get the distances to the enemy
        SetDistancesRaw();
        // set the sides as taboo if there is an enemy near the side and its close to the wall
        SetDistancesSides();

        // get player position
        SetPlayerIndices();

        // evaluate distances
        EvaluateDistanceAhead();
        //Debug.Log("0: " + m_distanceAhead);
        //Debug.Log("1: " + (m_distanceMoveThreshold - m_actualAdditionalRaycastHeight - 0.01f));
        //Debug.Log("2: " + (m_distanceAhead >= m_distanceMoveThreshold - m_actualAdditionalRaycastHeight - 0.01f));
        if (m_distanceAhead >= m_distanceMoveThreshold - m_actualAdditionalRaycastHeight - 0.01f)
        {
            //Debug.Log("ASD");
            return MoveToRaycastposition();
        }
        EvaluateDitanceLeftRight();
        decideDirection();

        return MoveToRaycastposition();
    }
    private void InitInfo()
    {
        m_areaSizePixel = new Vector2Int(m_screenshotManager.GetCaptureWidth(), m_screenshotManager.GetCaptureHeight());
        m_areaSizeWorld = new Vector2(m_captureArea.localScale.x, m_captureArea.localScale.y);
        m_distances = new float[m_areaSizePixel.x];
        m_pixelSize = m_screenshotManager.GetPixelToWorldScale(0);
        m_playerLengthWorld = m_visualCapturePlayer.localScale.x;
        m_playerLengthPixel = (int)Mathf.Ceil(m_playerLengthWorld / m_pixelSize);

        m_actualAdditionalRaycastHeight = Mathf.Min(-m_pixelSize * 0.51f, m_additionalRaycastHeight);
        m_raycastHeight = m_areaSizeWorld.y - m_actualAdditionalRaycastHeight;

        m_playerIndices = new List<int>();
        m_playerPositionPixelMin = m_areaSizePixel.x - 1;
        m_playerPositionPixelMax = 0;

        m_distanceAhead = m_raycastHeight;
        m_distanceLeftY = m_raycastHeight;
        m_distanceRightY = m_raycastHeight;
        m_dangerIndexLeft = m_areaSizePixel.x - 1;
        m_dangerIndexRight = 0;
    }
    private void SetDistancesRaw()
    {
        m_raycastPosition = GetRaycastPosition(0);
        for (int i = 0; i < m_distances.Length; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(m_raycastPosition, Vector3.up, out hit, m_raycastHeight, m_layerMaskEnemy))
            {
                float distance = (m_raycastPosition - hit.point).magnitude;
                m_distances[i] = distance;
                Debug.DrawRay(m_raycastPosition, Vector3.up * distance, Color.Lerp(Color.green, Color.red, Utility.MapValuePercent(0, m_raycastHeight, distance != 0 ? distance : 0)));
            }
            else
            {
                m_distances[i] = m_raycastHeight;
                //Debug.DrawRay(m_raycastPosition, Vector3.up * m_captureArea.localScale.y, Color.green);
            }
            m_raycastPosition.x += m_pixelSize;
        }
    }
    private void SetDistancesSides()
    {
        float minValue = m_raycastHeight;
        for (int i = m_playerLengthPixel - 1; i >= 0; i--)
        {
            minValue = Mathf.Min(m_distances[i], minValue);
            m_distances[i] = minValue;
        }
        minValue = m_raycastHeight;
        for (int i = m_areaSizePixel.x - m_playerLengthPixel; i < m_areaSizePixel.x; i++)
        {
            minValue = Mathf.Min(m_distances[i], minValue);
            m_distances[i] = minValue;
        }
    }
    private void SetPlayerIndices()
    {
        m_raycastPosition = GetRaycastPosition(0);
        for (int i = 0; i < m_distances.Length; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(m_raycastPosition, Vector3.up, out hit, m_raycastHeight, m_layerMaskPlayer))
            {
                m_playerIndices.Add(i);
            }
            m_raycastPosition.x += m_pixelSize;
        }

        if(m_playerIndices.Count == 0)
        {
            Debug.Log("Warning! Player not found via racasts!");
            return;
        }

        m_playerPositionPixelMin = m_playerIndices[0];
        m_playerPositionPixelMax = m_playerIndices[m_playerIndices.Count - 1];
        int posInt = (m_playerPositionPixelMin + m_playerPositionPixelMax) / 2;
        float posFloat = (m_playerPositionPixelMin + m_playerPositionPixelMax) * 0.5f;
        if (posFloat - posInt > 0.1f) // check if the position is not an integer
        {
            m_playerPositionPixelCenterMin = posInt;
            m_playerPositionPixelCenterMax = posInt + 1;
        }
        else
        {
            m_playerPositionPixelCenterMax = m_playerPositionPixelCenterMin = posInt;
        }
    }
    private void EvaluateDistanceAhead()
    {
        // get the minimum distance ahead
        for (int pixelIndex = m_playerPositionPixelMin; pixelIndex <= m_playerPositionPixelMax; pixelIndex++)
        {
            m_distanceAhead = Mathf.Min(m_distanceAhead, m_distances[pixelIndex]);
        }

        if (m_distanceAhead == m_raycastHeight)
            return;

        // evaluate the most left and most right indices in which there is danger ahead
        for (int pixelIndex = m_playerPositionPixelMin; pixelIndex <= m_playerPositionPixelMax; pixelIndex++)
        {
            if(m_distances[pixelIndex] == m_distanceAhead)
            {
                m_dangerIndexLeft = Mathf.Min(m_dangerIndexLeft, pixelIndex);
                m_dangerIndexRight = Mathf.Max(m_dangerIndexRight, pixelIndex);
            }
        }

        // expand the most left/right indices to the full span of the enemy
        for(int pixelIndexLeft = m_dangerIndexLeft; pixelIndexLeft >= 0; pixelIndexLeft--)
        {
            if (m_distances[pixelIndexLeft] != m_distanceAhead)
                break;

            m_dangerIndexLeft = pixelIndexLeft;
        }
        for (int pixelIndexRight = m_dangerIndexRight; pixelIndexRight < m_areaSizePixel.x; pixelIndexRight++)
        {
            if (m_distances[pixelIndexRight] != m_distanceAhead)
                break;

            m_dangerIndexRight = pixelIndexRight;
        }
    }
    private void EvaluateDitanceLeftRight()
    {
        m_distanceLeftX = 0;
        m_distanceRightX = 0;

        int startIndexLeft = m_dangerIndexLeft == 0 ? m_dangerIndexLeft : m_dangerIndexLeft - 1;
        int endIndexLeft = startIndexLeft - m_playerLengthPixel + 1;
        int startIndexRight = m_dangerIndexRight == m_distances.Length - 1 ? m_dangerIndexRight : m_dangerIndexRight + 1;
        int endIndexRight = startIndexRight + m_playerLengthPixel - 1;

        bool isAtEndLeft = false;
        bool isAtEndRight = false;
        do
        {
            if (!isAtEndLeft)
            {
                for (int pixelIndexLeft = startIndexLeft; pixelIndexLeft >= endIndexLeft; pixelIndexLeft--)
                {
                    if (pixelIndexLeft < 0)
                    {
                        isAtEndLeft = true;
                        break;
                    }
                    m_distanceLeftY = Mathf.Min(m_distanceLeftY, m_distances[pixelIndexLeft]);
                }
                m_moveToIndexPositionLeft = (startIndexLeft + endIndexLeft) / 2;
                startIndexLeft--;
                endIndexLeft--;
                m_distanceLeftX++;
            }

            if (!isAtEndRight)
            { 
                for (int pixelIndexRight = startIndexRight; pixelIndexRight <= endIndexRight; pixelIndexRight++)
                {
                    if (pixelIndexRight >= m_distances.Length)
                    {
                        isAtEndRight = true;
                        break;
                    }
                    m_distanceRightY = Mathf.Min(m_distanceRightY, m_distances[pixelIndexRight]);
                }
                m_moveToIndexPositionRight = (startIndexRight + endIndexRight) / 2;
                startIndexRight++;
                endIndexRight++;
                m_distanceRightX++;
            }

            // if no obstacle to the left and to the right detected, go to the nearest side
            //if(m_distanceRightY == m_distanceLeftY)
            //{
            //    m_distanceLeftX = m_playerPositionPixelCenterMin - Mathf.Max(0, startIndexLeft);
            //    m_distanceRightX = Mathf.Min(startIndexRight, m_distances.Length - 1) - m_playerPositionPixelCenterMax;
            //}

        } while (m_distanceLeftY == m_distanceRightY && m_distanceRightX == m_distanceLeftX && !isAtEndLeft && !isAtEndRight);

        if (isAtEndRight && isAtEndLeft)
            Debug.Log("Warning!");
        
    }
    private float[] MoveToRaycastposition()
    {
        float[] inputData = new float[ScreenshotManager.Instance().GetOutputNumber()];
        if (!m_isMovingRight && !m_isMovingLeft)
        {
            m_changeDirectionTimer += Time.deltaTime;
            return inputData;
        }
        m_position = m_visualCapturePlayer.position.x - GetAreaStartPosition().x;
        m_wantPosition = m_pixelSize * (0.5f + (m_isMovingLeft ? m_moveToIndexPositionLeft : m_moveToIndexPositionRight));

        if(m_isMovingLeft)
        {
            if (m_position <= m_wantPosition)
                m_isMovingLeft = false;
            else
                inputData[0] = 1;
        }
        if (m_isMovingRight)
        {
            if (m_position >= m_wantPosition)
                m_isMovingRight = false;
            else
                inputData[1] = 1;
        }

        return inputData;
    }
    private void decideDirection()
    {
        if (m_distanceLeftY > m_distanceRightY)
        {
            m_isMovingLeft = true;
            m_isMovingRight = false;
            //inputData[0] = 1;
        }
        else if (m_distanceLeftY < m_distanceRightY)
        {
            m_isMovingLeft = false;
            m_isMovingRight = true;
            //inputData[1] = 1;
        }
        else
        {
            if (m_distanceLeftX < m_distanceRightX)
            {
                m_isMovingLeft = true;
                m_isMovingRight = false;
                //inputData[0] = 1;
            }
            else if (m_distanceLeftX > m_distanceRightX)
            {
                m_isMovingLeft = false;
                m_isMovingRight = true;
                //inputData[1] = 1;
            }
            else // go middle
            {
                int centerPosInt = (int)(m_areaSizePixel.x * 0.5f);
                float centerPosFloat = m_areaSizePixel.x * 0.5f;

                int centerPosMin = centerPosInt;
                int centerPosMax = centerPosInt;
                if (centerPosFloat - centerPosInt > 0.1f) // check if the position is not an integer
                    centerPosMax++;

                if (m_playerPositionPixelCenterMin < centerPosMin)// player is on the right half
                {
                    m_isMovingLeft = false;
                    m_isMovingRight = true;
                }
                else if (m_playerPositionPixelCenterMax > centerPosMax) // player is on the left half
                {
                    m_isMovingLeft = true;
                    m_isMovingRight = false;
                }
                else // if player is in the middle, decide via a timer which increments as the player isn't moving
                {
                    if (m_changeDirectionTimer % m_switchDirectionTimer * 2f > m_switchDirectionTimer)
                    {
                        m_isMovingLeft = false;
                        m_isMovingRight = true;
                    }
                    else
                    {
                        m_isMovingLeft = true;
                        m_isMovingRight = false;
                    }
                }
            }
        }
    }
    private Vector3 GetRaycastPosition(int index)
    {
        return GetAreaStartPosition() + new Vector3(m_pixelSize * (0.5f + index), m_actualAdditionalRaycastHeight, 0);
    }
    private Vector3 GetAreaStartPosition()
    {
        return m_captureArea.position - new Vector3(m_areaSizeWorld.x * 0.5f, m_areaSizeWorld.y * 0.5f, 0);
    }
    #endregion

    #region Statics
    public static PlayerAiMovement Instance()
    {
        return s_instance;
    }
    #endregion
}
