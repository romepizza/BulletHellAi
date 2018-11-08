using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAiMovement : MonoBehaviour
{
    private static PlayerAiMovement s_instance;

    [Header("------ Settings ------")]
    [SerializeField] private InputType m_inputType;
    [Header("--- Objects ---")]
    [SerializeField] private TakeScreenshot m_screenshotManager;
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

        float[][] inputInformation = m_screenshotManager.GetScreenshotDataRaw();
        int[] dangers = new int[inputInformation.Length];
        List<Vector2Int> playerPixels = new List<Vector2Int>();

        int width = inputInformation.Length;
        int height = inputInformation[0].Length;

        // compute pixel data and fill danger array and player pixel list
        Vector2Int cacheVector = Vector2Int.zero;
        for(int x = 0; x < width;  x++)
        {
            bool dangerFound = false;
            for(int y = 0; y < height; y++)
            {
                if (dangerFound)
                    continue;

                float value = inputInformation[x][y];
                if(value > 0) // player
                {
                    cacheVector.x = x;
                    cacheVector.y = y;
                    playerPixels.Add(cacheVector);
                }
                else if(value < 0) // enemy
                {
                    dangerFound = true;
                    int danger = height - y;

                    dangers[x] = danger;
                }
            }
        }


        // evaluate player position
        Vector2Int playerPosition = Vector2Int.zero;
        Vector2Int minPixel = new Vector2Int(int.MaxValue, int.MaxValue);
        Vector2Int maxPixel = Vector2Int.zero;
        for (int pixelIndex = 0; pixelIndex < playerPixels.Count; pixelIndex++)
        {
            minPixel.x = Mathf.Min(minPixel.x, playerPixels[pixelIndex].x);
            minPixel.y = Mathf.Min(minPixel.y, playerPixels[pixelIndex].y);
            maxPixel.x = Mathf.Max(minPixel.x, playerPixels[pixelIndex].x);
            maxPixel.y = Mathf.Max(minPixel.y, playerPixels[pixelIndex].y);
        }

        playerPosition.x = (minPixel.x + maxPixel.x) / 2;
        playerPosition.y = (minPixel.y + maxPixel.y) / 2;

        // evaluate new danger values depending on player position
        for (int dangerIndex = 0; dangerIndex < dangers.Length; dangerIndex++)
        {
            int danger = dangers[dangerIndex];
            int distanceToPlayer = Mathf.Max(playerPosition.x, danger) - Mathf.Min(playerPosition.x, danger);
            int newDanger = danger - distanceToPlayer;


            dangers[dangerIndex] = newDanger;
        }

        // decide if the ai should move
        bool move = false;
        for(int x = minPixel.x; x <= maxPixel.x; x++)
        {
            if (dangers[x] > 0)
                move = true;
        }

        if (!move)
            return inputData;


        // decide whether to go left or right
        int dangerLeft = int.MaxValue;
        int dangerRight = int.MaxValue;
        for(int leftIndex = playerPosition.x; leftIndex >= 0; leftIndex--) // evaluate dangerLeft
        {
            if(dangers[leftIndex] > 0)
            {
                dangerLeft = dangers[leftIndex];
                break;
            }
        }
        for (int rightIndex = playerPosition.x; rightIndex < width; rightIndex++) // evaluate dangerRight
        {
            if (dangers[rightIndex] > 0)
            {
                dangerLeft = dangers[rightIndex];
                break;
            }
        }

        bool goLeft = dangerLeft < dangerRight;
        bool goRight = dangerLeft > dangerRight;
        inputData[0] = goLeft ? 1 : 0;
        inputData[1] = goRight ? 1 : 0;

        return inputData;
    }
    private float[] GenerateInputDataRaycast()
    {
        float[] inputData = null;

        Debug.Log("Aborted: Raycast not implemented yet!");

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
