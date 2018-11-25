using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyMatrix
{
    public int m_columnCountX { get; private set; }
    public int m_rowCountY { get; private set; }
    public float[][] m_data; // m_data[y][x]

    #region Constructors
    public MyMatrix(int rowCountY, int columnCountX)
    {
        m_rowCountY = rowCountY;
        m_columnCountX = columnCountX;

        m_data = new float[m_rowCountY][];
        for(int row = 0; row < m_rowCountY; row++)
        {
            float[] dataX = new float[m_columnCountX];
            m_data[row] = dataX;
        }
    }
    public MyMatrix(MyMatrix otherMat, bool copyValues)
    {
        m_rowCountY = otherMat.m_rowCountY;
        m_columnCountX = otherMat.m_columnCountX;

        m_data = new float[m_rowCountY][];
        for (int row = 0; row < m_rowCountY; row++)
        {
            float[] dataX = new float[m_columnCountX];
            m_data[row] = dataX;
        }

        if(copyValues)
        {
            for(int y = 0; y < m_rowCountY; y++)
            {
                for(int x = 0; x < m_columnCountX; x++)
                {
                    m_data[y][x] = otherMat.m_data[y][x];
                }
            }
        }
    }
    /// <summary>
    /// Creates a MyMatrix(data.Length, 1) and inputs 'data' to this first column.
    /// </summary>
    /// <param name="data"></param>
    public MyMatrix(float[] data)
    {
        m_rowCountY = data.Length;
        m_columnCountX = 1;

        m_data = new float[m_rowCountY][];
        for (int row = 0; row < m_rowCountY; row++)
        {
            float[] dataX = new float[1];
            dataX[0] = data[row];
            m_data[row] = dataX;
        }
    }
    #endregion

    #region Object Operations
    public void ClearMatrix()
    {
        for (int y = 0; y < m_rowCountY; y++)
        {
            for (int x = 0; x < m_columnCountX; x++)
            {
                m_data[y][x] = 0f;
            }
        }
    }
    public void SetRandomValues(float minValue, float maxValue)
    {
        for(int y = 0; y < m_rowCountY; y++)
        {
            for(int x = 0; x < m_columnCountX; x++)
            {
                m_data[y][x] = Random.Range(minValue, maxValue);
            }
        }
    }
    public void AddMatrix(MyMatrix otherMat)
    {
        if(otherMat.m_rowCountY != m_rowCountY)
        {
            Debug.Log("Aborted: row count were not the same! (this: " + m_rowCountY + ", other: " + otherMat.m_rowCountY + ")");
            return;
        }
        if (otherMat.m_columnCountX != m_columnCountX)
        {
            Debug.Log("Aborted: column count were not the same! (this: " + m_columnCountX + ", other: " + otherMat.m_columnCountX + ")");
            return;
        }

        for(int y = 0; y < m_rowCountY; y++)
        {
            for (int x = 0; x < m_columnCountX; x++)
            {
                m_data[y][x] += otherMat.m_data[y][x];
            }
        }
    }
    public void MultiplyByFactor(float factor)
    {
        for (int y = 0; y < m_rowCountY; y++)
        {
            for (int x = 0; x < m_columnCountX; x++)
            {
                m_data[y][x] *= factor;
            }
        }
    }
    #endregion

    #region Static Operations
    public static MyMatrix Transposed(MyMatrix mat)
    {
        MyMatrix newMatrix = new MyMatrix(mat.m_columnCountX, mat.m_rowCountY);

        for(int x = 0; x < newMatrix.m_columnCountX; x++)
        {
            for (int y = 0; y < newMatrix.m_rowCountY; y++)
                newMatrix.m_data[y][x] = mat.m_data[x][y];
        }
        
        return newMatrix;
    }
    public static MyMatrix Dot(MyMatrix mat1, MyMatrix mat2)
    {
        if (mat1.m_columnCountX != mat2.m_rowCountY)
        {
            Debug.Log("Aborted: m_rows != otherMatrix.m_columns (" + mat1.m_columnCountX + "/" + mat2.m_rowCountY + ")\nmat1:\n" + mat1.ToString() + "\nmat2:\n" + mat2.ToString());
            return null;
        }

        MyMatrix newMatrix = new MyMatrix(mat1.m_rowCountY, mat2.m_columnCountX);

        for (int y = 0; y < newMatrix.m_rowCountY; y++)
        { 
            for (int x = 0; x < newMatrix.m_columnCountX; x++)
            {
                float value = 0;
                for(int i = 0; i < mat1.m_columnCountX; i++)
                {
                    value += mat1.m_data[y][i] * mat2.m_data[i][x];
                }
                newMatrix.m_data[y][x] = value;
            }
        }

        return newMatrix;
    }
    public static MyMatrix AddMatrix(MyMatrix mat1, MyMatrix mat2)
    {
        if (mat1.m_rowCountY != mat2.m_rowCountY)
        {
            Debug.Log("Aborted: row count were not the same! (mat1: " + mat1.m_rowCountY + ", mat2: " + mat2.m_rowCountY + ")");
            return null;
        }
        if (mat1.m_columnCountX != mat2.m_columnCountX)
        {
            Debug.Log("Aborted: column count were not the same! (mat1: " + mat1.m_columnCountX + ", mat2: " + mat2.m_columnCountX + ")");
            return null;
        }

        MyMatrix newMat = new MyMatrix(mat1, false);

        for (int y = 0; y < mat1.m_rowCountY; y++)
        {
            for (int x = 0; x < mat1.m_columnCountX; x++)
            {
                newMat.m_data[y][x] = mat1.m_data[y][x] + mat2.m_data[y][x];
            }
        }

        return newMat;
    }
    public static MyMatrix MultiplyElementWise(MyMatrix mat1, MyMatrix mat2)
    {
        if(mat1.m_rowCountY != mat2.m_rowCountY)
        {
            Debug.Log("Aborted: row counts didn't match! (mat1: " + mat1.m_rowCountY + ", mat2: " + mat2.m_rowCountY + ")\nmat1:\n" + mat1.ToString() + "\nmat2:\n" + mat2.ToString());
            return null;
        }
        if (mat1.m_columnCountX != mat2.m_columnCountX)
        {
            Debug.Log("Aborted: column counts didn't match! (mat1: " + mat1.m_columnCountX + ", mat2: " + mat2.m_columnCountX + ")\nmat1:\n" + mat1.ToString() + "\nmat2:\n" + mat2.ToString());
            return null;
        }

        MyMatrix newMatrix = new MyMatrix(mat1, false);

        for(int y = 0; y < newMatrix.m_rowCountY; y++)
        {
            for(int x = 0; x < newMatrix.m_columnCountX; x++)
            {
                newMatrix.m_data[y][x] = mat1.m_data[y][x] * mat2.m_data[y][x];
            }
        }

        return newMatrix;
    }
    #endregion

    #region Getter
    public MyMatrix GetColumn(int colummnX)
    {
        MyMatrix newMatrix = new MyMatrix(1, m_rowCountY);
        for(int y = 0; y < newMatrix.m_rowCountY; y++)
            newMatrix.m_data[0][y] = m_data[colummnX][y];
        return newMatrix;
    }
    public float[] GetColumnToArray(int columnIndex)
    {
        if(columnIndex >= m_rowCountY)
        {
            Debug.Log("Aborted: column index was too high! (param: " + columnIndex + ", max " + (m_rowCountY - 1) + ")");
        }

        float[] data = new float[m_rowCountY];

        for(int y = 0; y < m_rowCountY; y++)
        {
            data[y] = m_data[y][0];
        }

        return data;
    }
    public override string ToString()
    {
        string s = "";

        for(int y = 0; y < m_rowCountY; y++)
        {
            s += "( ";
            for (int x = 0; x < m_columnCountX; x++)
            {
                s += m_data[y][x].ToString("0.00") + "; ";
            }
            s += " )\n";
        }

        return s;
    }
    public float[][] GetMatrixToArray()
    {
        float[][] data = new float[m_rowCountY][];
        for(int y = 0; y < m_rowCountY; y++)
        {
            float[] dataX = new float[m_data[m_rowCountY].Length];
            for(int x = 0; x < dataX.Length; x++)
            {
                dataX[x] = m_data[y][x];
            }
            data[y] = dataX;
        }
        return data;
    }
    #endregion
}
