using System.Collections;
using System.Reflection;
using System.Text.Json;

namespace Server.Services.DataStore;

[RegisterImplementation(ServiceRegisterType.Singleton, typeof(DataStoreService))]
public class DataStoreService
{
    readonly object lockject = new();
    readonly static string exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) + Path.DirectorySeparatorChar;
    public string ConfigPath { get; } = exePath + "config.json";
    readonly string configBackupPath = exePath + "config_backup.json";
    public bool UnsavedChanges { get; private set; } = false;
    readonly JsonSerializerOptions options = new() { WriteIndented = true };
    public DataStoreRoot Data
    {
        get
        {
            lock (lockject)
            {
                UnsavedChanges = true;
                return data;
            }
        }
        set
        {
            UnsavedChanges = true;
            data = value;
        }
    }
    private DataStoreRoot data = new();

    public DataStoreService()
    {
        if (Exists())
            Load();
        else
            Data = new();
    }

    public bool Exists()
    {
        return File.Exists(ConfigPath);
    }
    public void Save()
    {
        lock (lockject)
        {
            if (File.Exists(ConfigPath))
                File.Copy(ConfigPath, configBackupPath, true);
            File.WriteAllText(ConfigPath, JsonSerializer.Serialize(Data, options));

            UnsavedChanges = false;
        }
    }
    public void Load()
    {
        lock (lockject)
        {
            if (Exists())
                Data = JsonSerializer.Deserialize<DataStoreRoot>(File.ReadAllText(ConfigPath)) ?? new();
            else
                Data = new();
        }
    }
    public void LoadFrom(string json)
    {
        lock (lockject)
        {
            Data = JsonSerializer.Deserialize<DataStoreRoot>(json) ?? Data;
        }
    }
    public override string ToString()
    {
        string output = "";

        FieldInfo[] Infos = typeof(DataStoreRoot).GetFields();
        foreach (FieldInfo info in Infos)
        {
            output += "\n" + info.Name + ": ";

            if (info.FieldType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(info.FieldType) && info.GetValue(Data) as IEnumerable != null)
            {
                output += "\n";
                IEnumerable? a = (IEnumerable?)info.GetValue(Data);
                IEnumerator? e = a?.GetEnumerator();
                e?.Reset();
                if (e == null) continue;
                while (e.MoveNext())
                {
                    output += e.Current + ", ";
                }
            }
            else
            {
                output += info.GetValue(Data) + "\n";
            }
        }

        return output;
    }
}