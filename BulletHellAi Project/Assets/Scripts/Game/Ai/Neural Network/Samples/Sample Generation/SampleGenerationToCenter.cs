using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleGenerationToCenter : SampleGenerationBase
{
    public override List<SampleContainer> GenerateSamples(int width, int height, int obstacleLength, int playerLength)
    {
        m_data.Clear();
        int dataLength = obstacleLength + playerLength;

        for (int w = 0; w < width; w++)
        {
            float[] input = new float[dataLength];
            float[] desiredOutput = new float[m_screenshotManager.GetOutputNumber()];

            // on the right half, go left
            desiredOutput[0] = w >= width / 2 ? 1 : 0;
            // on the left half, go right
            desiredOutput[1] = w < width / 2 ? 1 : 0;

            int playerIndex = w + obstacleLength;
            input[playerIndex] = 1;

            SampleContainer container = new SampleContainer(input, desiredOutput, null, width, height);
            m_data.Add(container);
        }

        return m_data;
    }
}
