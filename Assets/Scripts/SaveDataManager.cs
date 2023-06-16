//information on Newtonsoft.Json from https://videlais.com/2021/02/25/using-jsonutility-in-unity-to-save-and-load-game-data/
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using Newtonsoft.Json;

//Manages data from different events in game. 
public class SaveDataManager : MonoBehaviour {
    public void SaveNewData(SaveData data) {
        string filePath = Application.persistentDataPath + "/" + data.saveName + ".json";
        string jsonData = JsonConvert.SerializeObject(data);
        File.WriteAllText(filePath, jsonData);
    }

    public SaveData LoadSaveData(string fileName) {
        string filePath = Application.persistentDataPath + "/" + fileName + ".json";
        if (File.Exists(filePath)) {
            string jsonData = File.ReadAllText(filePath);
            SaveData data = JsonUtility.FromJson<SaveData>(jsonData);
            //Debug.Log("file found");
            return data;
        }
        return null;
    }
}
