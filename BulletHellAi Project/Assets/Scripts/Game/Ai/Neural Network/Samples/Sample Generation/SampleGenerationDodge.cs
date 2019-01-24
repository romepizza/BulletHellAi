using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleGenerationDodge : SampleGenerationBase
{
    [Header("----- Settings -----")]
    [SerializeField] private bool m_stayInsteadOfDodge;
    [SerializeField] private float m_considerRangeXCenter;
    [SerializeField] private float m_considerRangeXEdge;
    [SerializeField] private float m_considerRangeY;
    [SerializeField] private AnimationCurve m_rangeCurveXCenter;
    [SerializeField] private AnimationCurve m_rangeCurveXEdge;
    [SerializeField] private AnimationCurve m_rangeCurveY;
    [SerializeField] private bool m_singlePictures;

    public override List<SampleContainer> GenerateSamples(int width, int height, int obstacleLength, int playerLength)
    {
        m_data.Clear();
        int dataLength = obstacleLength + playerLength;
        int playerWidth = playerLength / width;
        int playerHeight = playerWidth;

        int minIndexPlayerY = 0;
        int maxIndexPlayerY = playerHeight;

        for (int playerPositionX = 0; playerPositionX < (width - playerWidth + 1); playerPositionX++)
        {
            bool playerIsRightHalf = playerPositionX >= width / 2;
            float playerPositionXCenter = (playerPositionX + playerPositionX + playerWidth - 1) * 0.5f;
            int minIndexX = Mathf.Max(0, playerPositionX - (int)((playerIsRightHalf ? m_considerRangeXCenter : m_considerRangeXEdge) * width));
            int maxIndexX = Mathf.Min(playerPositionX + (int)((playerIsRightHalf ? m_considerRangeXEdge : m_considerRangeXCenter) * width), width - 1);
            int minIndexY = 0;
            int maxIndexY = Mathf.Min((int)(m_considerRangeY * height), height - 1);


            float[] inputLeft = new float[dataLength];
            float[] inputRight = new float[dataLength];
            // set values for obstacles
            for (int y = minIndexY; y <= maxIndexY; y++)
            {
                for (int x = minIndexX; x <= maxIndexX; x++)
                {
                    if (m_singlePictures)
                    {
                        inputLeft = new float[dataLength];
                        inputRight = new float[dataLength];
                    }

                    for (int yPlayer = minIndexPlayerY; yPlayer < maxIndexPlayerY; yPlayer++)
                    {
                        for (int xPlayer = playerPositionX; xPlayer < Mathf.Min(width, playerPositionX + playerWidth); xPlayer++)
                        {
                            int index2 = obstacleLength + xPlayer + yPlayer * width;
                            inputLeft[index2] = 1;
                            inputRight[index2] = 1;
                        }
                    }
                    //Debug.Log(x + " - " + playerPositionXCenter + " / " + width + " = " + (Mathf.Abs(x - playerPositionXCenter) / width) + ", eval = " + distFactorX);
                    float distFactorY = m_rangeCurveY.Evaluate((float)Mathf.Abs(y - (x == playerPositionX ? (maxIndexPlayerY - 1) : 0)) / height);
                    int index = x + y * width;
                    if(playerIsRightHalf && x < playerPositionXCenter)
                    {
                        float distFactor = m_rangeCurveXCenter.Evaluate(Mathf.Abs(x - playerPositionXCenter) / width);
                        inputLeft[index] = distFactor * distFactorY; // player right half, left side
                    }
                    else if(playerIsRightHalf)
                    {
                        float distFactor = m_rangeCurveXEdge.Evaluate(Mathf.Abs(x - playerPositionXCenter) / width);
                        inputRight[index] = distFactor * distFactorY; // player right half, right side
                    }
                    if (!playerIsRightHalf && x > playerPositionXCenter)
                    {
                        float distFactor = m_rangeCurveXCenter.Evaluate(Mathf.Abs(x - playerPositionXCenter) / width);
                        inputLeft[index] = distFactor * distFactorY; // player left half, right side
                    }
                    else if(!playerIsRightHalf)
                    {
                        float distFactor = m_rangeCurveXEdge.Evaluate(Mathf.Abs(x - playerPositionXCenter) / width);
                        inputRight[index] = distFactor * distFactorY;// player left half, left side
                    }


                    if (m_singlePictures)
                    {
                        bool isLeftSide = x < playerPositionXCenter;
                        bool isRightSide = x > playerPositionXCenter;
                        bool isAhead = x == playerPositionXCenter;

                        float[] desiredOutput = new float[m_screenshotManager.GetOutputNumber()];

                        if (isRightSide)
                        {
                            if(!playerIsRightHalf && m_stayInsteadOfDodge)
                                desiredOutput[2] = 1;
                            else
                                desiredOutput[0] = 1;
                        }
                        if (isLeftSide)
                        {
                            if (playerIsRightHalf && m_stayInsteadOfDodge)
                                desiredOutput[2] = 1;
                            else
                                desiredOutput[1] = 1;
                        }
                        if (isAhead)
                        {
                            if(playerIsRightHalf)
                                desiredOutput[0] = 1;
                            else
                                desiredOutput[1] = 1;
                        }
                        if(inputLeft[index] > 0)
                        {
                            SampleContainer container = new SampleContainer(inputLeft, desiredOutput, null, width, height);
                            m_data.Add(container);
                        }
                        if(inputRight[index] > 0)
                        {
                            SampleContainer container = new SampleContainer(inputRight, desiredOutput, null, width, height);
                            m_data.Add(container);
                        }
                        //SampleContainer containerRight = new SampleContainer(inputRight, desiredOutputRight, null, width, height);
                        //m_data.Add(containerRight);
                    }
                    // player is on the right half
                }
            }

            if (!m_singlePictures)
            {
                float[] desiredOutputLeft = new float[m_screenshotManager.GetOutputNumber()];
                float[] desiredOutputRight = new float[m_screenshotManager.GetOutputNumber()];

                if (playerIsRightHalf)
                {
                    desiredOutputRight[0] = 1;
                    desiredOutputLeft[m_stayInsteadOfDodge ? 2 : 1] = 1;
                }
                else
                {
                    desiredOutputRight[1] = 1;
                    desiredOutputLeft[m_stayInsteadOfDodge ? 2 : 0] = 1;
                }
                
                SampleContainer containerLeft = new SampleContainer(inputLeft, desiredOutputLeft, null, width, height);
                m_data.Add(containerLeft);
                SampleContainer containerRight = new SampleContainer(inputRight, desiredOutputRight, null, width, height);
                m_data.Add(containerRight);
            }

            //// set values for obstacles
            //for (int h = minIndexY; h <= maxIndexY; h++)
            //{
            //    for (int w = minIndexX; w <= maxIndexX; w++)
            //    {
            //        float[] input = new float[dataLength];
            //        //input[playerPositionX + obstacleLength] = 1;
            //        for (int y = minPlayerY; y < maxPlayerY; y++)
            //        {
            //            for (int x = playerPositionX; x < Mathf.Min(width, playerPositionX + playerWidth); x++)
            //            {
            //                int index2 = obstacleLength + x + y * width;
            //                input[index2] = 1;
            //            }
            //        }

            //        float distFactorX = m_rangeCurveX.Evaluate(Mathf.Abs(w - playerPositionXCenter) / width);
            //        float distFactorY = m_rangeCurveY.Evaluate(Mathf.Abs(h - maxIndexY) / height);
            //        int index = w + h * w;
            //        input[index] = distFactorX * distFactorY;

            //        float[] desiredOutput = new float[m_screenshotManager.GetOutputNumber()];
            //        // player is on the right half
            //        if (playerIsRightHalf)
            //        {
            //            // current w is to the right, go left
            //            if(w < playerPositionXCenter)
            //                desiredOutput[0] = 1;
            //            else
            //                desiredOutput[1] = 1;
            //        }
            //        // player is on the left half
            //        else
            //        {
            //            // current w is to the left, go right 
            //            if (w > playerPositionXCenter)
            //                desiredOutput[1] = 1;
            //            else
            //                desiredOutput[0] = 1;
            //        }

            //        SampleContainer container = new SampleContainer(input, desiredOutput, null, width, height);
            //        m_data.Add(container);
            //    }
            //}
        }

        return m_data;
    }
    
}