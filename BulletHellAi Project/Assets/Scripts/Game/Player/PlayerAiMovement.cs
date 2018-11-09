using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAiMovement : MonoBehaviour
{
    private static PlayerAiMovement s_instance;

    [Header("------ Settings ------")]
    [SerializeField] private InputType m_inputType;

    [Header("--- Raycast ---")]
    [SerializeField] private LayerMask m_layerMask;

    [Header("--- Objects ---")]
    [SerializeField] private TakeScreenshot m_screenshotManager;
    [SerializeField] private Transform m_visualCapturePlayer;
    [SerializeField] private Transform m_captureArea;
    [Header("------ Debug ------")]
    bool b;

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
        float[] inputData = new float[4];

        float[][] inputInformation = m_screenshotManager.GetScreenshotDataRaw(false);
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
        int playerLengthX = (int)Mathf.Ceil(m_visualCapturePlayer.localScale.x / ScreenshotManager.Instance().GetPixelToWorldScale(1));
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
        float[] inputData = new float[4];

        Vector2Int pixelCount = new Vector2Int(ScreenshotManager.Instance().GetCaptureWidth(), ScreenshotManager.Instance().GetCaptureHeight());
        float pixelSize = ScreenshotManager.Instance().GetPixelToWorldScale(1);
        float playerSizeX = m_visualCapturePlayer.localScale.x;

        float[] distances = new float[pixelCount.x];

        // get the distances to the enemy
        Vector3 raycastPosition = m_captureArea.position;
        raycastPosition.x -= m_captureArea.localScale.x * 0.5f;
        raycastPosition.y -= m_captureArea.localScale.y * 0.5f;
        raycastPosition.x += pixelSize * 0.5f;
        for (int i = 0; i < distances.Length; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(raycastPosition, Vector3.up, out hit, m_captureArea.localScale.y, m_layerMask))
            {
                float distance = (raycastPosition - hit.point).magnitude;
                distances[i] = distance;
                Debug.DrawRay(raycastPosition, Vector3.up * distance, Color.Lerp(Color.green, Color.red, Utility.MapValuePercent(0, m_captureArea.localScale.y, distance != 0 ? 1 / distance : 0)));
            }
            raycastPosition.x += pixelSize;
        }

        return inputData;
    }
    #endregion

    #region Statics
    public static PlayerAiMovement Instance()
    {
        return s_instance;
    }
    #endregion
}
