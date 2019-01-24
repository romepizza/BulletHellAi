using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleGenerationAwayFromEdges : SampleGenerationBase
{
    [Header("----- Settings -----")]
    [SerializeField] private int m_absolutePixelNumber = 1;
    //[SerializeField] private int m_playerSizeNumber = 1;

    public override List<SampleContainer> GenerateSamples(int width, int height, int obstacleLength, int playerLength)
    {
        m_data.Clear();
        int dataLength = obstacleLength + playerLength;
        int playerWidth = playerLength / width;//(int)(Mathf.Sqrt((width) + 0.01f));
        int playerHeight = playerWidth;

        //Debug.Log("playerWidth: " + playerWidth);
        for (int i = 0; i < m_absolutePixelNumber; i++)
        {
            float[] inputRight = new float[dataLength];
            float[] inputLeft = new float[dataLength];
            for (int w = 0; w < (playerWidth * (1)); w++)
            {
                for (int h = 0; h < playerHeight; h++)
                {
                    // on the right half, go left
                    inputRight[obstacleLength + w + i + h * width] = 1;
                    //Debug.Log("0: " + obstacleLength + " + " + w + " + " + i + "(" + h + " * " + width + ") = " + (obstacleLength + w + i + h * width));
                    // on the left half, go right
                    //Debug.Log("1: " + (obstacleLength + playerLength - (w + i + h * width) - 1));// "o " + obstacleLength + " + pL " + playerLength + " - (w " + w + " + h " + h + " * wi " + width + ") - 1 = " + (obstacleLength + playerLength - (w + h * width) - 1));
                    inputLeft[obstacleLength + playerLength - (w + i + h * width) - 1] = 1;
                }
            }
            float[] desiredOutput = new float[m_screenshotManager.GetOutputNumber()];
            desiredOutput[1] = 1;
            SampleContainer container = new SampleContainer(inputRight, desiredOutput, null, width, height);
            m_data.Add(container);
            desiredOutput = new float[m_screenshotManager.GetOutputNumber()];
            desiredOutput[0] = 1;
            SampleContainer container2 = new SampleContainer(inputLeft, desiredOutput, null, width, height);
            m_data.Add(container2);
        }
        return m_data;
    }   
}