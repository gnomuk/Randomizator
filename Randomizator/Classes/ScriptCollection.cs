using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

public class ScriptInfo : INotifyPropertyChanged
{
    private bool _isChecked;

    public string Name { get; set; }
    public string Description { get; set; }
    public bool Enabled { get; set; }
    public Visibility Show_Gear { get; set; }

    public ScriptInfo(string name, string description, bool enabled, Visibility showGear)
    {
        Name = name;
        Description = description;
        Enabled = enabled;
        Show_Gear = showGear;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string script)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(script));
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

    public void AddScript(string shortName, string name, string description, bool enabled, Visibility showGear)
    {
        var scriptInfo = new ScriptInfo(name, description, enabled, showGear);
        Scripts.Add(new ScriptEntry(shortName, scriptInfo));
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string script)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(script));
    }
}
