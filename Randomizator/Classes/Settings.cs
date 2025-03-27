using System;
using System.IO;
using Newtonsoft.Json;

public class Settings
{
    public RouletteSettings Roulette { get; set; }
    public OtherSettings Other { get; set; }

    public class RouletteSettings
    {
        public int From { get; set; }
        public int To { get; set; }
    }

    public class OtherSettings
    {
        public string RouletteBind { get; set; }
        public bool RandomSampling { get; set; }
        public bool CombineScripts { get; set; }
    }

    // Сохранение настроек в файл
    public void SaveToFile(string filePath)
    {
        string json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }

    // Загрузка настроек из файла
    public static Settings LoadFromFile(string filePath)
    {
        string json = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<Settings>(json);
    }
}