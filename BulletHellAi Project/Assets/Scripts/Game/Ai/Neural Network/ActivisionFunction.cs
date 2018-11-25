using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivisionFunction
{
    public float m_coeffitient { get; protected set; }



    public virtual MyMatrix GetActivision(MyMatrix input)
    {
        return null;
    }

    public virtual MyMatrix GetActivisionPrime(MyMatrix input)
    {
        return null;
    }

    public virtual MyMatrix SquashActivision(MyMatrix input)
    {
        // squash elements into a range of [0:1]
        return null;
    }
}


// Tanh
public class ActivisionFuntionTanh : ActivisionFunction
{
    public ActivisionFuntionTanh(float coeffitient)
    {
        m_coeffitient = coeffitient;
    }

    public override MyMatrix GetActivision(MyMatrix input)
    {
        MyMatrix newMat = new MyMatrix(input.m_rowCountY, input.m_columnCountX);
        for (int y = 0; y < newMat.m_rowCountY; y++)
        {
            for (int x = 0; x < newMat.m_columnCountX; x++)
            {
                float inputVal = input.m_data[y][x];
                float output = 0;

                float eExpP = Mathf.Exp(inputVal);
                float eExpN = Mathf.Exp(-inputVal);
                output = (eExpP - eExpN) / (eExpP + eExpN);

                newMat.m_data[y][x] = output;
            }
        }

        return newMat;
    }

    public override MyMatrix GetActivisionPrime(MyMatrix input)
    {
        MyMatrix newMat = new MyMatrix(input.m_rowCountY, input.m_columnCountX);
        for (int y = 0; y < newMat.m_rowCountY; y++)
        {
            for (int x = 0; x < newMat.m_columnCountX; x++)
            {
                float inputVal = input.m_data[y][x];
                float output = 0;

                float eExpP = Mathf.Exp(inputVal);
                float eExpN = Mathf.Exp(-inputVal);
                float tanh = (eExpP - eExpN) / (eExpP + eExpN);

                output = 1 - tanh * tanh;

                newMat.m_data[y][x] = output;
            }
        }

        return newMat;
    }

    public override MyMatrix SquashActivision(MyMatrix input)
    {
        // Sigmoid activisions output is in range [0:1] by default
        return input;
    }


    
}

// ReLU
public class ActivisionFuntionReLU : ActivisionFunction
{
    public ActivisionFuntionReLU(float coeffitient)
    {
        m_coeffitient = coeffitient;
    }

    public override MyMatrix GetActivision(MyMatrix input)
    {
        MyMatrix newMat = new MyMatrix(input.m_rowCountY, input.m_columnCountX);
        for (int y = 0; y < newMat.m_rowCountY; y++)
        {
            for (int x = 0; x < newMat.m_columnCountX; x++)
            {
                float inputVal = input.m_data[y][x];
                float output = 0;

                output = inputVal > 0 ? inputVal : 0;

                newMat.m_data[y][x] = output;
            }
        }

        return newMat;
    }

    public override MyMatrix GetActivisionPrime(MyMatrix input)
    {
        MyMatrix newMat = new MyMatrix(input.m_rowCountY, input.m_columnCountX);
        for (int y = 0; y < newMat.m_rowCountY; y++)
        {
            for (int x = 0; x < newMat.m_columnCountX; x++)
            {
                float inputVal = input.m_data[y][x];
                float output = 0;
                
                output = inputVal > 0 ? 1 : 0;

                newMat.m_data[y][x] = output;
            }
        }

        return newMat;
    }

    public override MyMatrix SquashActivision(MyMatrix input)
    {
        // Sigmoid activisions output is in range [0:1] by default
        return input;
    }



}

// LeakyReLU
public class ActivisionFuntionLReLU : ActivisionFunction
{
    public ActivisionFuntionLReLU(float coeffitient)
    {
        m_coeffitient = coeffitient;
    }

    public override MyMatrix GetActivision(MyMatrix input)
    {
        MyMatrix newMat = new MyMatrix(input.m_rowCountY, input.m_columnCountX);
        for (int y = 0; y < newMat.m_rowCountY; y++)
        {
            for (int x = 0; x < newMat.m_columnCountX; x++)
            {
                float inputVal = input.m_data[y][x];
                float output = 0;

                output = inputVal > 0 ? inputVal : m_coeffitient * inputVal;

                newMat.m_data[y][x] = output;
            }
        }

        return newMat;
    }

    public override MyMatrix GetActivisionPrime(MyMatrix input)
    {
        MyMatrix newMat = new MyMatrix(input.m_rowCountY, input.m_columnCountX);
        for (int y = 0; y < newMat.m_rowCountY; y++)
        {
            for (int x = 0; x < newMat.m_columnCountX; x++)
            {
                float inputVal = input.m_data[y][x];
                float output = 0;

                output = inputVal > 0 ? 1 : m_coeffitient;

                newMat.m_data[y][x] = output;
            }
        }

        return newMat;
    }

    public override MyMatrix SquashActivision(MyMatrix input)
    {
        return input;
    }
}

public class ActivisionFuntionELU : ActivisionFunction
{
    public ActivisionFuntionELU(float coeffitient)
    {
        m_coeffitient = coeffitient;
    }

    public override MyMatrix GetActivision(MyMatrix input)
    {
        MyMatrix newMat = new MyMatrix(input.m_rowCountY, input.m_columnCountX);
        for (int y = 0; y < newMat.m_rowCountY; y++)
        {
            for (int x = 0; x < newMat.m_columnCountX; x++)
            {
                float inputVal = input.m_data[y][x];
                float output = 0;

                output = inputVal > 0 ? inputVal : m_coeffitient * (Mathf.Exp(inputVal) - 1);

                newMat.m_data[y][x] = output;
            }
        }

        return newMat;
    }

    public override MyMatrix GetActivisionPrime(MyMatrix input)
    {
        MyMatrix newMat = new MyMatrix(input.m_rowCountY, input.m_columnCountX);
        for (int y = 0; y < newMat.m_rowCountY; y++)
        {
            for (int x = 0; x < newMat.m_columnCountX; x++)
            {
                float inputVal = input.m_data[y][x];
                float output = 0;

                output = inputVal > 0 ? 1 : m_coeffitient * Mathf.Exp(inputVal);

                newMat.m_data[y][x] = output;
            }
        }

        return newMat;
    }

    public override MyMatrix SquashActivision(MyMatrix input)
    {
        return input;
    }
}

public class ActivisionFuntionSigmoid : ActivisionFunction
{
    public ActivisionFuntionSigmoid(float coeffitient)
    {
        m_coeffitient = coeffitient;
    }

    public override MyMatrix GetActivision(MyMatrix input)
    {
        MyMatrix newMat = new MyMatrix(input.m_rowCountY, input.m_columnCountX);
        for (int y = 0; y < newMat.m_rowCountY; y++)
        {
            for (int x = 0; x < newMat.m_columnCountX; x++)
            {
                float inputVal = input.m_data[y][x];
                float output = 0;

                output = 1f / (1f + Mathf.Exp(-inputVal));

                newMat.m_data[y][x] = output;
            }
        }

        return newMat;
    }

    public override MyMatrix GetActivisionPrime(MyMatrix input)
    {
        MyMatrix newMat = new MyMatrix(input.m_rowCountY, input.m_columnCountX);
        for (int y = 0; y < newMat.m_rowCountY; y++)
        {
            for (int x = 0; x < newMat.m_columnCountX; x++)
            {
                float inputVal = input.m_data[y][x];
                float output = 0;

                float sigmoid = 1f / (1f + Mathf.Exp(-inputVal));
                output = sigmoid * (1 - sigmoid);

                newMat.m_data[y][x] = output;
            }
        }

        return newMat;
    }

    public override MyMatrix SquashActivision(MyMatrix input)
    {
        // Sigmoid activisions output is in range [0:1] by default
        return input;
    }
}