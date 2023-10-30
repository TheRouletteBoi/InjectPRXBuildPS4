using YamlDotNet.Serialization;
using OrbisLib2.Targets;
using OrbisLib2.Common.Database.Types;
using OrbisLib2.Dialog;
using static SQLite.SQLite3;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Mail;


// TODO(Roulette): Add checks when attached and when loading sprx into console
// TODO(Roulette): Add default values if no injectinfo.yaml is found


Console.WriteLine("Loading Sprx Into Console...");

string yamlFileName = "injectinfo.yaml";
string yamlContent = File.ReadAllText(yamlFileName);

var yamlDeserializer = new DeserializerBuilder().Build();
var yamlTargetContent = yamlDeserializer.Deserialize<YamlTargetConfiguration>(yamlContent);
YamlTarget yamlTarget = yamlTargetContent.target;


if (!File.Exists(yamlTarget.source_path))
    Console.WriteLine($"SPRX file on computer does not exists. Check path {yamlTarget.source_path}");


if (IsApiAvailable())
{
    Attach();

    var currentTarget = TargetManager.SelectedTarget;
    string console_ip = currentTarget.IPAddress;
    string console_name = currentTarget.Name;

    Console.WriteLine($"Target Name {console_name}");
    Console.WriteLine($"Console IP {console_ip}");
    Console.WriteLine($"Sprx directory {yamlTarget.source_path}");
    Console.WriteLine($"Sprx destination {yamlTarget.destination_path}{Path.GetFileName(yamlTarget.source_path)}");

    // Load sprx from .yaml path
    await LoadSomethingLocal(yamlTarget.source_path, yamlTarget.destination_path, true);


#if UNUSED_CODE
UnloadPrx(yamlTarget.destination_path);
Thread.Sleep(100);
LoadPrx(yamlTarget.destination_path);
#endif


    // Save Console name and IP to yaml file
    var yamlSerializer = new SerializerBuilder().Build();
    var yamlTargetConfiguration = new YamlTargetConfiguration
    {
        target = new YamlTarget
        {
            target_name = currentTarget.Name,
            console_ip = currentTarget.IPAddress,
            source_path = yamlTarget.source_path,
            destination_path = yamlTarget.destination_path
        }
    };

    string serializedYamlContent = yamlSerializer.Serialize(yamlTargetConfiguration);
    File.WriteAllText(yamlFileName, serializedYamlContent);
}
else
{
    Console.WriteLine("Error: Cannot connect to Orbis API");
}




















bool IsApiAvailable()
{
    var currentTarget = TargetManager.SelectedTarget;
    if (currentTarget.Info.Status != TargetStatusType.APIAvailable)
        return false;

    return true;
}

async void Attach()
{
    var currentTarget = TargetManager.SelectedTarget;

    var (result, pidList) = await currentTarget.GetProcList();

    if (!result.Succeeded)
    {
        Console.WriteLine("Failed to get process list");
        return;
    }

    var ebootProcess = pidList.FirstOrDefault(p => p.Name == "eboot.bin");

    if (ebootProcess == null)
    {
        Console.WriteLine("Failed to find eboot.bin process");
        return;
    }

    await TargetManager.SelectedTarget.Debug.Attach(ebootProcess.ProcessId);
}

async void LoadPrx(string sprxPath)
{
    // Get the Library list so we can check if its loaded already.
    (var result, var libraryList) = await TargetManager.SelectedTarget.Debug.GetLibraries();
    if (!result.Succeeded)
    {
        Console.WriteLine($"Failed to load library {Path.GetFileName(sprxPath)}.");
        return;
    }

    // Search for the library in the list.
    var library = libraryList.Find(x => x.Path == sprxPath);
    if (library != null)
    {
        Console.WriteLine($"Error: Failed to load SPRX \"{sprxPath}\" since it is already loaded.");
    }

    // Try to load the library.
    (result, var _) = await TargetManager.SelectedTarget.Debug.LoadLibrary(sprxPath);

    // If we failed abort here.
    if (!result.Succeeded)
    {
        Console.WriteLine($"Failed to load library {Path.GetFileName(sprxPath)}.");
        return;
    }
}

async void UnloadPrx(string sprxPath)
{
    // Get the Library list so we can check if its loaded already.
    (var result, var libraryList) = await TargetManager.SelectedTarget.Debug.GetLibraries();
    if (!result.Succeeded)
    {
        Console.WriteLine($"Failed to unload library {Path.GetFileName(sprxPath)}.");
        return;
    }

    // Search for the library in the list.
    var library = libraryList.Find(x => x.Path == sprxPath);
    if (library == null)
    {
        Console.WriteLine($"Error: Failed to unload SPRX \"{sprxPath}\" since it is not loaded.");
        return;
    }

    // Try to unload the library.
    result = await TargetManager.SelectedTarget.Debug.UnloadLibrary((int)library.Handle);

    // If we failed abort here.
    if (!result.Succeeded)
    {
        Console.WriteLine($"Failed to unload library {Path.GetFileName(sprxPath)}.");
        return;
    }
}

async void ReloadSprx(string sprxPath)
{
    // Get the Library list so we can check if its loaded already.
    (var result, var libraryList) = await TargetManager.SelectedTarget.Debug.GetLibraries();
    if (!result.Succeeded)
    {
        Console.WriteLine($"Failed to reload library {Path.GetFileName(sprxPath)}.");
        return;
    }

    // Search for the library in the list.
    var library = libraryList.Find(x => x.Path == sprxPath);
    if (library == null)
    {
        Console.WriteLine($"Error: Failed to reload SPRX \"{sprxPath}\" since it is not loaded.");
    }

    // Try to unload the library.
    (result, var _) = await TargetManager.SelectedTarget.Debug.ReloadLibrary((int)library.Handle, sprxPath);

    // If we failed abort here.
    if (!result.Succeeded)
    {
        Console.WriteLine($"Failed to reload library {Path.GetFileName(sprxPath)}.");
        return;
    }
}

async void InjectBuild_Incomplete(string sprxPath)
{
    // Get the Library list so we can check if its loaded already.
    (var result, var libraryList) = await TargetManager.SelectedTarget.Debug.GetLibraries();
    if (!result.Succeeded)
    {
        Console.WriteLine("Error: Failed to load sprx.");
        return;
    }

    // Search for the library in the list and unload the sprx if already loaded.
    var library = libraryList.Find(x => x.Path == sprxPath);
    if (library != null)
    {
        // Try to unload the library.
        (result, var handle) = await TargetManager.SelectedTarget.Debug.ReloadLibrary((int)library.Handle, sprxPath);

        // If we failed abort here.
        if (!result.Succeeded)
        {
            Console.WriteLine("Error: Failed to load sprx.");
            return;
        }

        Console.WriteLine($"The sprx {sprxPath} has been reloaded with handle {handle}.");
    }
    else
    {
        (result, int handle) = await TargetManager.SelectedTarget.Debug.LoadLibrary(sprxPath);

        if (!result.Succeeded || handle == -1)
        {
            Console.WriteLine("Error: Failed to load sprx.");
            return;
        }

        Console.WriteLine($"The sprx {sprxPath} has been loaded with handle {handle}.");

        // Remove the temp file.
        await TargetManager.SelectedTarget.DeleteFile(sprxPath);
    }
}

async Task LoadSomethingLocal(string path, string temporarySprxPath, bool deleteAfterwards)
{
    // Get the file data.
    var binaryData = await File.ReadAllBytesAsync(path);

    switch (Path.GetExtension(path).ToLower())
    {
        case ".sprx":
            {
                // Send the sprx to a temporary path.
                //var tempSprxPath = $"/data/Orbis Suite/{Path.GetFileName(path)}";
                var tempSprxPath = $"{temporarySprxPath}{Path.GetFileName(path)}";
                var result = await TargetManager.SelectedTarget.SendFile(binaryData, tempSprxPath);

                if (!result.Succeeded)
                {
                    Console.WriteLine("Error: Failed to load sprx.");
                    return;
                }

                // Get the Library list so we can check if its loaded already.
                (result, var libraryList) = await TargetManager.SelectedTarget.Debug.GetLibraries();
                if (!result.Succeeded)
                {
                    Console.WriteLine("Error: Failed to load sprx.");
                    return;
                }

                // Search for the library in the list and unload the sprx if already loaded.
                var library = libraryList.Find(x => x.Path == tempSprxPath);
                if (library != null)
                {
                    // Try to unload the library.
                    (result, var handle) = await TargetManager.SelectedTarget.Debug.ReloadLibrary((int)library.Handle, tempSprxPath);

                    // If we failed abort here.
                    if (!result.Succeeded)
                    {
                        Console.WriteLine("Error: Failed to load sprx.");
                        return;
                    }

                    Console.WriteLine($"{Path.GetFileName(path)} has been reloaded with handle {handle}.");
                }
                else
                {
                    (result, int handle) = await TargetManager.SelectedTarget.Debug.LoadLibrary(tempSprxPath);

                    if (!result.Succeeded || handle == -1)
                    {
                        Console.WriteLine("Error: Failed to load sprx.");
                        return;
                    }

                    Console.WriteLine($"The sprx {Path.GetFileName(path)} has been loaded with handle {handle}.");


                    if (deleteAfterwards)
                    {
                        Console.WriteLine($"Deleting {Path.GetFileName(path)}");

                        // Remove the temp file.
                        await TargetManager.SelectedTarget.DeleteFile(tempSprxPath);
                    }
                }

                break;
            }

        case ".bin":
            {
                if (!await TargetManager.SelectedTarget.Payload.InjectPayload(binaryData))
                {
                    Console.WriteLine("Failed to send payload to target please try again.");
                    return;
                }

                Console.WriteLine("The payload has been sucessfully sent.");

                break;
            }

        case ".elf":
            {
                Console.WriteLine("ELF's are not currently supported.");

                break;
            }
    }
}

public class YamlTargetConfiguration
{
    public YamlTarget target { get; set; }
}

public class YamlTarget
{
    public string target_name { get; set; }

    public string console_ip { get; set; }

    [YamlMember(ScalarStyle = YamlDotNet.Core.ScalarStyle.SingleQuoted)]
    public string source_path { get; set; }

    [YamlMember(ScalarStyle = YamlDotNet.Core.ScalarStyle.SingleQuoted)]
    public string destination_path { get; set; }
}