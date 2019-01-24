using System.Collections;
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
            sw.Close();
        }
    }

    public static NNCSaveData Load(string fileName)
    {
        string path = string.Format("{0}/{1}.json", GetDirectoryPath(), fileName);
        if(!File.Exists(path))
        {
            Debug.Log("Aborted: Path doesn't exist! (" + path + ")");
            return new NNCSaveData { m_isCorrupted = true };
        }

        using (StreamReader sr = new StreamReader(path))
        {
            return JsonUtility.FromJson<NNCSaveData>(sr.ReadToEnd());
        }
    }
    public static NNCSaveData Load(TextAsset dataFile)
    {
        if(dataFile == null)
        {
            Debug.Log("Aborted: dataFile was null!");
            return new NNCSaveData { m_isCorrupted = true };
        }

        return JsonUtility.FromJson<NNCSaveData>(dataFile.text);
    }

    private static string GetDirectoryPath()
    {
        string directoryName;

        directoryName = "Neural Network Saves";

        if (Application.isEditor)
            directoryName = Path.GetFullPath(Application.dataPath + "\\" + directoryName);
        else
            directoryName = Application.dataPath + "\\" + directoryName;


        Directory.CreateDirectory(directoryName);

        return directoryName;
    }
}
