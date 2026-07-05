using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountryMap : MonoBehaviour
{
    public static CountryMap instance;

    public struct CountryData
    {
        public int code;
        public string fullName;
    }

    public Dictionary<string, CountryData> countryCodesToData;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        TextAsset file = Resources.Load<TextAsset>("countries");
        List<string> temp = new List<string>(file.text.Split('\n'));
        countryCodesToData = new Dictionary<string, CountryData>();

        for (int i = 0; i < temp.Count; i++)
        {
            string[] parts = temp[i].Split(';');
            countryCodesToData.Add(parts[0], new CountryData() { code = i, fullName = parts[1] });
        }
    }

    public int getCode(string code)
    {
        return countryCodesToData[code].code;
    }

    public string getFullName(string code)
    {
        return countryCodesToData[code].fullName;
    }
}
