using System.Collections;

[System.Serializable]
public class SaveData {
    public string saveName;
    public int savedInt;
    public bool savedBool;
    public string savedString;

    public SaveData(string name, int saveInt, bool saveBool, string saveString)
    {
        saveName = name;
        savedInt = saveInt;
        savedBool = saveBool;
        savedString = saveString;
    }
    public SaveData(string name, int saveInt)
    {
        saveName = name;
        savedInt = saveInt;
    }
    public SaveData(string name, bool saveBool)
    {
        saveName = name;
        savedBool = saveBool;
    }
    public SaveData(string name, string saveString)
    {
        saveName = name;
        savedString = saveString;
    }
}