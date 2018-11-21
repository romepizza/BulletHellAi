﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class NeuralNetworkData
{
    public static void Save(NNCSaveData container, string fileName)
    {
        string path = string.Format("{0}/{1}.json", GetDirectoryPath(), fileName);

        using (StreamWriter sw = new StreamWriter(path))
        {
            sw.WriteLine(JsonUtility.ToJson(container, true));
        }
    }

    public static NNCSaveData Load(string fileName)
    {
        string path = string.Format("{0}/{1}.json", GetDirectoryPath(), fileName);
        if(!File.Exists(path))
        {
            Debug.Log("Aborted: Path doesn't exist! (" + path + ")");
            return new NNCSaveData();
        }

        using (StreamReader sr = new StreamReader(path))
        {
            return JsonUtility.FromJson<NNCSaveData>(sr.ReadToEnd());
        }
    }

    private static string GetDirectoryPath()
    {
        string directoryName;

        directoryName = "Neural Network Saves";

        if (Application.isEditor)
            directoryName = Path.GetFullPath(Application.dataPath + "/../" + directoryName);
        else
            directoryName = Application.dataPath + "/" + directoryName;


        Directory.CreateDirectory(directoryName);

        //fileName = string.Format("{0}/{1}_{2}x{3}_{4}.json", directoryName, fileName, 0, 0, 0);// m_currentWidth, m_currentHeight, m_fileNameCounter, m_format.ToString().ToLower());

        //m_fileNameCounter++;

        return directoryName;
    }
}