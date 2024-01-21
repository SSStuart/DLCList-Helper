using System.Diagnostics;
using System.IO.Compression;
using System.Xml;

Console.WriteLine(  "   ┌------------------┐" +
                  "\n   |  DLCList Helper  |   by SSStuart" +
                  "\n   └------------------┘\n" +
                  "  A small console app that generates the dlclist.xml file (GTA 5 modding) based on\n" +
                  "  the folders contained in the \"dlcpacks\" folder (original and \"mods\").\n" +
                  "  Simplifies the installation of add-on mods and the updating of the \"mods\" folder\n" +
                  "  after a game update.\n\n");

var openIVexe = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"New Technology Studio\Apps\OpenIV\OpenIV.exe");

// Récupération du chemin d'installation:
string pathRockstar = "";   // x
string pathSteam = "";      // x
string pathEpic = "";       // x
string pathGTA;
string pathInput;
List<string> dlcpacks = new();
int dlcpacksBaseCount = 0;



Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\DLCListHelper_output\");
string outputDirectory = Directory.GetCurrentDirectory() + @"\DLCListHelper_output\";
// Suppression de l'ancien OIV
if (File.Exists(outputDirectory + @"\dlclistUpdate.oiv"))
    try
    {
        File.Delete(outputDirectory + @"\dlclistUpdate.oiv");
    } catch (Exception e)
    {
        Console.WriteLine("[!] Error while deleting old OIV file. Make sure to close OpenIV and retry.\n [i] Error details:");
        Console.WriteLine(e.Message);
        Console.ReadKey();
        Environment.Exit(0);
    }
    
// Regarde si l'outil se trouve dans le dossier du jeu ou dans le dossier parent

if(File.Exists("./GTA5.exe"))
{
    pathGTA = Directory.GetCurrentDirectory();
    Console.WriteLine("[i] Current directory is game folder.");
} else if (File.Exists("./GTAV/GTA5.exe"))
{
    pathGTA = Directory.GetCurrentDirectory()+ @"\GTAV";
    Console.WriteLine("[i] Detected subdirectory \"GTAV\" as game folder.");
} else if (File.Exists("./Grand Theft Auto V/GTA5.exe"))
{
    pathGTA = Directory.GetCurrentDirectory() + @"\Grand Theft Auto V";
    Console.WriteLine("[i] Detected subdirectory \"Grand Theft Auto V\" as game folder.");
} else if (Directory.Exists(pathRockstar))  // NOT IMPLEMENTED
{
    Console.WriteLine("[i] GTA5 (Rockstar) folder detected.");
    pathGTA = pathRockstar;
} else if (Directory.Exists(pathSteam))  // NOT IMPLEMENTED
{
    Console.WriteLine("[i] GTA5 (Steam) folder detected.");
    pathGTA = pathSteam;
} else if (Directory.Exists(pathEpic))  // NOT IMPLEMENTED
{
    Console.WriteLine("[i] GTA5 (Epic Games) folder detected.");
    pathGTA = pathEpic;
} else
{
    do
    {
        Console.WriteLine("[>] Please enter the GTA5 folder path (You can also place this tool in the game folder itself or in its parent folder to detect the game location automatically) : ");
        pathInput = Console.ReadLine();
    } while (!Directory.Exists(pathInput));
    pathGTA = pathInput;
}

// Récupération du contenu du dossier "update/x64/dlcpacks/"
string[] dlcpacksBaseList = Directory.GetDirectories(pathGTA + "/update/x64/dlcpacks/");
Console.WriteLine("\n[i] Basegame DLC Packs :");
foreach (string dlcpack in dlcpacksBaseList)
{
    string dlcpackName = dlcpack.Split("/")[^1];
    dlcpacks.Add(dlcpackName);

    Console.WriteLine($" - {dlcpackName}");
}

// Récupération du contenu du dossier "mods/update/x64/dlcpacks/"
string[] dlcpacksList = Directory.GetDirectories(pathGTA + "/mods/update/x64/dlcpacks/");
Console.WriteLine("[i] Mods folder DLC Packs :");
foreach (string dlcpack in dlcpacksList)
{
    string dlcpackName = dlcpack.Split("/")[^1];
    if (!dlcpacks.Contains(dlcpackName))
    {
        dlcpacks.Add(dlcpackName);
        Console.WriteLine($" - {dlcpackName}");
    }else
    {
        Console.WriteLine($" - {dlcpackName} [ignored]");
    }
    
}


// Création du dossier temporaire de création de l'OIV
Directory.CreateDirectory(outputDirectory + @"\dlclistUpdate\");
Directory.CreateDirectory(outputDirectory + @"\dlclistUpdate\content\");
// Ajout de l'icon.png (obligatoire ?)



if (!File.Exists(Directory.GetCurrentDirectory() + @"\assemblyTemplate.xml"))
{
    Console.WriteLine("\n[!] Missing assemblyTemplate.xml file. Please make sure it is in the same folder as this tool.");
    Console.ReadKey();
    Environment.Exit(0);
}
// Création du fichier assembly.xml
XmlDocument docAssembly = new();
docAssembly.PreserveWhitespace = true;
docAssembly.Load(@".\assemblyTemplate.xml");
XmlNode root = docAssembly.DocumentElement;
//  Description des changements
string changes = "\n      <![CDATA[[BASEGAME FOLDER]\n";
foreach (string dlcpack in dlcpacks)
{
    dlcpacksBaseCount++;
    if (dlcpacksBaseCount == dlcpacksBaseList.Length + 1)
    {
        changes += "\n[MODS FOLDER]\n";
    }
    changes += $"      -{dlcpack}\n";
}
changes += "      ]]>\n    ";

root.SelectNodes("metadata/largeDescription")[0].InnerXml = changes;

//  Modification du fichier dlclist.xml
foreach (string dlcpack in dlcpacks)
{
    XmlNode addNode = docAssembly.CreateNode(XmlNodeType.Element, "add", null);
    XmlAttribute appendAttribute = docAssembly.CreateAttribute("append");
    appendAttribute.Value = "Last";
    addNode.Attributes.Append(appendAttribute);
    XmlAttribute pathAttribute = docAssembly.CreateAttribute("xpath");
    pathAttribute.Value = "/SMandatoryPacksData/Paths";
    addNode.Attributes.Append(pathAttribute);

    root.SelectNodes("content/archive/xml")[0].AppendChild(addNode);
    root.SelectNodes("content/archive/xml")[0].AppendChild(docAssembly.CreateWhitespace("\n    "));

    XmlNode itemNode = docAssembly.CreateNode(XmlNodeType.Element, "Item", null);
    itemNode.InnerText = "dlcpacks:/" + dlcpack + "/";
    addNode.AppendChild(itemNode);
}


docAssembly.Save(outputDirectory + @"\dlclistUpdate\assembly.xml");

// Compression du dossier en zip (.oiv)
ZipFile.CreateFromDirectory(outputDirectory + @"\dlclistUpdate\", outputDirectory + @"\dlclistUpdate.oiv");

Directory.Delete(outputDirectory + @"\dlclistUpdate\", true);


if (Process.GetProcessesByName("OpenIV").Length == 0)
    Process.Start(openIVexe);

Process.Start("explorer.exe", outputDirectory);

Console.WriteLine("\n\n[i] Please drag and drop the generated OIV file on OpenIV (with Edit mode enabled) and install it (or use \"Package Installer\").\n    Press any key to close the tool");
Console.ReadKey();
Environment.Exit(0);