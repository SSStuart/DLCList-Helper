using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Xml;

class Program
{
    private static string toolPath = "";
    private static readonly string pathRockstar = @"C:\Program Files\Rockstar Games\Grand Theft Auto V\";
    private static readonly string pathSteam = @"C:\Program Files (x86)\Steam\steamapps\common\Grand Theft Auto V\";
    private static readonly string pathEpic = @"C:\Program Files\Epic Games\GTAV";
    private static string pathGTA = "ERROR";
    private static string pathInput = "";
    private static readonly List<string> dlcpacks = [];
    private static int dlcpacksBaseCount = 0;
    private static bool detectedGameFolder = false;
    private static bool manualInput = false;
    private static readonly string openIVexe = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"New Technology Studio\Apps\OpenIV\OpenIV.exe");


    [STAThread]
    static void Main(string[] args)
    {
        Console.Title = "DLCList Helper";
        Console.WriteLine("   ┌------------------┐" +
                        "\n   |  DLCList Helper  |   by SSStuart" +
                        "\n   └------------------┘\n" +
                        "  A small console app that generates the dlclist.xml file (GTA 5 modding) based on\n" +
                        "  the folders contained in the \"dlcpacks\" folder (original and \"mods\").\n" +
                        "  Simplifies the installation of add-on mods and the updating of the \"mods\" folder\n" +
                        "  after a game update.\n\n");


        // Retrieving installation path of the tool
        toolPath = Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? "";
        if (toolPath == "")
        {
            Console.WriteLine("[!] Unable to find the location of the app.");
            Console.ReadKey();
            Environment.Exit(0);
        }
        Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\DLCListHelper_output\");
        string outputDirectory = Directory.GetCurrentDirectory() + @"\DLCListHelper_output\";
        // Deleting old OIV file
        if (File.Exists(outputDirectory + @"\dlclistUpdate.oiv"))
            try
            {
                File.Delete(outputDirectory + @"\dlclistUpdate.oiv");
            }
            catch (Exception e)
            {
                Console.WriteLine("[!] Error while deleting old OIV file. Make sure to close OpenIV and retry.\n [i] Error details:");
                Console.WriteLine(e.Message);
                Console.ReadKey();
                Environment.Exit(0);
            }


        // Detecting game folder location
        if (File.Exists("../GTA5.exe"))  // Parent folder
        {
            pathGTA = toolPath;
            Console.WriteLine("[i] Parent directory is game folder (" + pathGTA + ").");
            detectedGameFolder = true;
        }
        else if (File.Exists("../GTAV/GTA5.exe"))  // Sibling folder
        {
            pathGTA = toolPath + @"\GTAV";
            Console.WriteLine("[i] Detected sibling directory \"..\\GTAV\" as game folder (" + pathGTA + ").");
            detectedGameFolder = true;
        }
        else if (File.Exists("../Grand Theft Auto V/GTA5.exe"))  // Sibling folder
        {
            pathGTA = toolPath + @"\Grand Theft Auto V";
            Console.WriteLine("[i] Detected sibling directory \"..\\Grand Theft Auto V\" as game folder (" + pathGTA + ").");
            detectedGameFolder = true;
        }
        else if (Directory.Exists(pathRockstar))  // Default location (Rockstar)
        {
            pathGTA = pathRockstar;
            Console.WriteLine("[i] GTA5 (Rockstar) folder detected (" + pathGTA + ").");
            detectedGameFolder = true;
        }
        else if (Directory.Exists(pathSteam))  // Default location (Steam)
        {
            pathGTA = pathSteam;
            Console.WriteLine("[i] GTA5 (Steam) folder detected (" + pathGTA + ").");
            detectedGameFolder = true;
        }
        else if (Directory.Exists(pathEpic))  // Default location (Epic Games)
        {
            pathGTA = pathEpic;
            Console.WriteLine("[i] GTA5 (Epic Games) folder detected (" + pathGTA + ").");
            detectedGameFolder = true;
        }

        // Ask for confirmation for the detected folder
        if (detectedGameFolder)
        {
            Console.WriteLine("[?] Continue with this folder ? [Y/N]");
            if (Console.ReadKey().Key == ConsoleKey.N)
                manualInput = true;
        }
        // Manual input of the location
        if (!detectedGameFolder || manualInput)
        {
            Console.WriteLine("[i] Unable to automatically detect the game folder location. Please specify the game folder location manually.");
            Console.WriteLine("[?] Would you like to use the folder picker (GUI) ? [Y/n]");
            string input = Console.ReadLine()?.Trim().ToLower() ?? "";
            bool useGui = input == "y" || input == "";

            if (useGui)
            {
                using (var dialog = new FolderBrowserDialog())
                {
                    dialog.Description = "Select the game folder";
                    dialog.UseDescriptionForTitle = true; // Windows 10+

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        pathInput = dialog.SelectedPath;
                    }
                }
            }

            if (pathInput == "")
                do
                {
                    Console.WriteLine("\n[>] Please enter the GTA5 folder path (You can also place the tool folder in the game folder directly, or in the parent folder to detect the game location automatically) : ");
                    pathInput = Console.ReadLine() ?? "";
                } while (!Directory.Exists(pathInput));
            pathGTA = pathInput;
        }


        // Retrieving content of "update/x64/dlcpacks/"
        if (!Directory.Exists(pathGTA + "/update/x64/dlcpacks/"))
        {
            Console.WriteLine("\n[!] The folder \"/update/x64/dlcpacks/\" cannot be found. Check that this folder is in the game folder or enter the path to the game folder manually.");
            Console.ReadKey();
            Environment.Exit(0);
        }
        string[] dlcpacksBaseList = Directory.GetDirectories(pathGTA + "/update/x64/dlcpacks/");
        Console.WriteLine("\n[i] Basegame DLC Packs :");
        foreach (string dlcpack in dlcpacksBaseList)
        {
            string dlcpackName = dlcpack.Split("/")[^1];
            dlcpacks.Add(dlcpackName);

            Console.WriteLine($" - {dlcpackName}");
        }


        // Retrieving content of "mods/update/x64/dlcpacks/"
        if (!Directory.Exists(pathGTA + "/mods/update/x64/dlcpacks/"))
        {
            Console.WriteLine("\n[!] The folder \"/mods/update/x64/dlcpacks/\" cannot be found. Check that this folder is in the game folder or enter the path to the game folder manually.");
            Console.ReadKey();
            Environment.Exit(0);
        }
        string[] dlcpacksList = Directory.GetDirectories(pathGTA + "/mods/update/x64/dlcpacks/");
        Console.WriteLine("[i] Mods folder DLC Packs :");
        foreach (string dlcpack in dlcpacksList)
        {
            string dlcpackName = dlcpack.Split("/")[^1];
            if (!dlcpacks.Contains(dlcpackName))
            {
                dlcpacks.Add(dlcpackName);
                Console.WriteLine($" - {dlcpackName}");
            }
            else
            {
                Console.WriteLine($" - {dlcpackName} [ignored]");
            }

        }


        // Creting temp folder for OIV creation
        Directory.CreateDirectory(outputDirectory + @"\dlclistUpdate\");
        Directory.CreateDirectory(outputDirectory + @"\dlclistUpdate\content\");

        // Creating assembly.xml file
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("DLCList_Helper.assemblyTemplate.xml");
        using var reader = new StreamReader(stream!);
        string xmlContent = reader.ReadToEnd();
        XmlDocument docAssembly = new();
        docAssembly.PreserveWhitespace = true;
        docAssembly.LoadXml(xmlContent);
        XmlNode root = docAssembly.DocumentElement;
        //  Changes description
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

        //  Modification of the dlclist.xml file
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

        string outputFile = outputDirectory + @"\dlclistUpdate.oiv";
        // Compressing the file into a zip file (.oiv)
        ZipFile.CreateFromDirectory(outputDirectory + @"\dlclistUpdate\", outputFile);

        Directory.Delete(outputDirectory + @"\dlclistUpdate\", true);

        // OIV package installation with OpenIV
        if (File.Exists(openIVexe))
        {
            // Auto
            Console.WriteLine("\n\n[i] Opening OIV file for installation... \nIf OpenIV is not starting: launch OpenIV > Enable 'Edit mode' > Drag & drop the .oiv file inside the 'DLCListHelper_output' folder on OpenIV.");
            Process.Start(openIVexe, $"\"{outputFile}\"");
        } else
        {
            // Manual (OpenIV not found)
            Console.WriteLine("[!] Unable to launch OpenIV. Launch OpenIV yourself, then Enable 'Edit mode' > Drag & drop the .oiv file inside the 'DLCListHelper_output' folder on OpenIV.");
        Process.Start("explorer.exe", outputDirectory);
        }

            Console.WriteLine("\n\n    Press any key to close the tool");
        Console.ReadKey();
        Environment.Exit(0);
    }
}