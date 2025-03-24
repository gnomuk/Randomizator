using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

public class ScriptInfo : INotifyPropertyChanged
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool Enabled { get; set; }
    public Visibility Show_Gear { get; set; }
    public Config Config { get; set; }

    public ScriptInfo(string name, string description, bool enabled, Visibility showGear, Config binds)
    {
        Name = name;
        Description = description;
        Enabled = enabled;
        Show_Gear = showGear;
        Config = binds;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string script)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(script));
    }
}

public class Config
{
    public Dictionary<string, string> Binds { get; set; }
    public int Int { get; set; }

    public Config(Dictionary<string, string> binds, int @int)
    {
        Binds = binds;
        Int = @int;
    }
}

public class ScriptEntry
{
    public string ShortName { get; set; }
    public ScriptInfo Info { get; set; }

    public ScriptEntry(string shortName, ScriptInfo info)
    {
        ShortName = shortName;
        Info = info;
    }
}

public class ScriptCollection : INotifyPropertyChanged
{
    public ObservableCollection<ScriptEntry> Scripts { get; set; }

    public ScriptCollection()
    {
        Scripts = new ObservableCollection<ScriptEntry>();
    }

    public void AddScript(string shortName, string name, string description, bool enabled, Visibility showGear, Dictionary<string, string> binds, int @int = 0)
    {
        var scriptInfo = new ScriptInfo(name, description, enabled, showGear, new Config(binds, @int));
        Scripts.Add(new ScriptEntry(shortName, scriptInfo));
    }

    public void EditScript(ScriptEntry item, Dictionary<string, string> binds)
    {
        foreach (var scriptEntry in Scripts) if (scriptEntry == item) scriptEntry.Info.Config.Binds = binds;
    }

    public ScriptEntry FindScriptByShortName(string shortName)
    {
        return Scripts.FirstOrDefault(script => script.ShortName == shortName);
    }

    public void SaveToJson(string filePath)
    {
        var json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }

    public static ScriptCollection LoadFromJson(string filePath)
    {
        var json = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<ScriptCollection>(json);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string script)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(script));
    }
}
