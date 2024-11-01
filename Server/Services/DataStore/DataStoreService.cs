using System.Collections;
using System.Reflection;
using Newtonsoft.Json;

namespace Server.Services.DataStore;

[RegisterImplementation(ServiceRegisterType.Singleton, typeof(DataStoreService))]
public interface IDataStoreService
{
    public DataStoreRoot Data { get; set; }
    public string ConfigPath { get; }
    public bool Exists();
    public void Save();
    public void Load();
    public void LoadFrom(string json);
    public string ToString();
}

public class DataStoreService : IDataStoreService
{
    readonly object lockject = new();
    readonly static string exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) + Path.DirectorySeparatorChar;
    public string ConfigPath { get; } = exePath + "config.json";
    readonly string configBackupPath = exePath + "config_backup.json";
    bool UnsavedChanges = false;
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

    DataStoreService()
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
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(Data, Formatting.Indented));

            UnsavedChanges = false;
        }
    }
    public void Load()
    {
        lock (lockject)
        {
            if (Exists())
                Data = JsonConvert.DeserializeObject<DataStoreRoot>(File.ReadAllText(ConfigPath)) ?? new();
            else
                Data = new();
        }
    }
    public void LoadFrom(string json)
    {
        lock (lockject)
        {
            Data = JsonConvert.DeserializeObject<DataStoreRoot>(json) ?? Data;
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