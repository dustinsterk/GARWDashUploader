using Renci.SshNet;
using System.IO.Compression;
class Program
{
    static void Main(string[] args)
    {
        int numOfArgs = args.Length;

        //Check for invalid args
        if (numOfArgs == 0)
        {
            PrintUsage();
            return;
        }

        //Check for invalid args
        if (numOfArgs > 2)
        {
            PrintUsage();
            return;
        }

        //Parse args
        if (numOfArgs == 1)
        {
            if (args[0] == "--purge")
            {
                System.Console.WriteLine("Are you sure you want to remove all 3rd party dash designs from your system? (Y or N)");
                string confirmRemoval = Console.ReadLine();
                if (confirmRemoval.Length == 0)
                {
                    System.Console.WriteLine("Operation aborted, no changes made.");
                    return;
                }

                if (confirmRemoval.ToUpper().Trim() == "Y"){
                    System.Console.WriteLine("Connecting to the GARW via wifi.");
                    //Connect to the GARW and run commands to delete all screens
                    using (var client = new SshClient("192.168.42.1", "root", "root"))
                    {
                        string execCmd = "";
                        try{
                            client.Connect();
                        }
                        catch (Exception e){
                            if (!client.IsConnected)
                            {
                                System.Console.WriteLine("No wifi connection, are you connected to the GARW SSID?");
                                return;
                            }
                        }
                        System.Console.WriteLine("Removing all 3rd party screens...");
                        //delete the entire library directory
                        execCmd = "rm -rf /opt/Garw_IC7/library/";
                        client.RunCommand(execCmd);
                        System.Console.WriteLine("Resetting screen enabled file...");
                        //fix the screen enabled file
                        execCmd = "rm /opt/Garw_IC7/screen_enabled.txt";
                        client.RunCommand(execCmd);
                        execCmd = "echo -e '1\n2\n3\n4\n5' >> /opt/Garw_IC7/screen_enabled.txt";
                        client.RunCommand(execCmd);
                        System.Console.WriteLine("Rebooting the GARW system...");
                        //reboot the GARW
                        execCmd = "reboot now";
                        client.RunCommand(execCmd);
                        //client.Disconnect();
                    }
                }
                else
                {
                    System.Console.WriteLine("Operation aborted, no changes made.");
                    return;
                }
            }
            else 
            {
                PrintUsage();
                return;
            }
        }

        //Parse args
        if (numOfArgs == 2)
        {
            if (args[0] == "--input")
            {
                string uploadZip = args[1];
                string dashFileName = Path.GetFileName(uploadZip);

                //Check the local path of the file to ensure it ia valid
                if (File.Exists(uploadZip)){
                    string fileName = Path.GetFileNameWithoutExtension(uploadZip);
                    //Strip the '_main' to check for the special case and to make the setting screen look cleaner
                    string dashName = fileName.Replace("_main","");
                    string dashPathToAdd = "file:///opt/Garw_IC7/library/" + fileName + "/" + fileName + ".qml";

                    //Check zip file contents for a .qml with the same zip file name and a .png for the settings screen
                    using (ZipArchive archive = ZipFile.OpenRead(uploadZip)){
                        int totalEntries = archive.Entries.Count;
                        int checkEntries = 0;
                        foreach (ZipArchiveEntry e in archive.Entries){
                            if (Path.GetFileName(e.FullName) == fileName + ".qml.png"){
                                checkEntries++;
                            }
                            
                            if (Path.GetFileName(e.FullName) == fileName + ".qml"){
                                checkEntries++;
                            }

                            if (fileName.ToLower().Contains("_main"))
                            {
                                if (Path.GetFileName(e.FullName) == dashName + ".qml"){
                                    checkEntries++;
                                }
                            }
                        }
                        //If required contents are not found, stop and prompt user to check the .zip file
                        if (fileName.ToLower().Contains("_main"))
                        {
                            if (checkEntries != 3){
                                System.Console.WriteLine("Invalid .zip file contents [settings file detected].  You must ensure the '_main'.qml matches the zip name (case sensitive), you also include the base .qml (with no '_main' included in the name), and you have the proper .qml.png file included!");
                                return;
                            }
                        }   
                        else if (checkEntries != 2)
                        {
                            System.Console.WriteLine("Invalid .zip file contents, you must ensure the .qml matches the zip name (case sensitive) and you have the proper .qml.png file included!");
                            return;
                        }

                        System.Console.WriteLine("The dash .zip file is valid, connecting to the GARW via wifi.");
                    }

                    //Create the library directory and associated files on the GARW device
                    using (var client = new SshClient("192.168.42.1", "root", "root"))
                    {
                        try{
                            client.Connect();
                        }
                        catch (Exception e){
                            if (!client.IsConnected)
                            {
                                System.Console.WriteLine("No wifi connection, are you connected to the GARW SSID?");
                                return;
                            }
                        }
                        System.Console.WriteLine("Creating the library folder and metadata files on the GARW device.");
                        client.RunCommand("mkdir -p /opt/Garw_IC7/library");
                        client.RunCommand("touch /opt/Garw_IC7/library/screen_list.txt");
                        client.RunCommand("touch /opt/Garw_IC7/library/screen_locations.txt");
                        client.RunCommand("touch /opt/Garw_IC7/library/screens_available.txt");
                        //check for empty file and add a 0 to the config
                        client.RunCommand("[ -s /opt/Garw_IC7/library/screens_available.txt ] || echo 0 > /opt/Garw_IC7/library/screens_available.txt");
                        client.Disconnect();
                    }

                    //Upload the zip file to the library directory of the GARW
                    using (ScpClient client = new ScpClient("192.168.42.1", "root", "root"))
                    {
                        try{
                            client.Connect();
                        }
                        catch (Exception e){
                            if (!client.IsConnected)
                            {
                                System.Console.WriteLine("No wifi connection, are you connected to the GARW SSID?");
                                return;
                            }
                        }
                        System.Console.WriteLine("Uploading the dash .zip file to the GARW.");
                        using (Stream localFile = File.OpenRead(uploadZip))
                        {
                            client.Upload(localFile, "/opt/Garw_IC7/library/" + dashFileName);
                        }
                        client.Disconnect();
                    }

                    //Connect to the GARW and run commands to upzip and add the dash to the settings section
                    using (var client = new SshClient("192.168.42.1", "root", "root"))
                    {
                        string execCmd = "";
                        try{
                            client.Connect();
                        }
                        catch (Exception e){
                            if (!client.IsConnected)
                            {
                                System.Console.WriteLine("No wifi connection, are you connected to the GARW SSID?");
                                return;
                            }
                        }
                        System.Console.WriteLine("Removing old dash files (if they existed).");
                        //delete the old directory of the dash if it existed
                        execCmd = "rm -rf /opt/Garw_IC7/library/" + fileName;
                        //System.Console.WriteLine(execCmd);
                        client.RunCommand(execCmd);
                        System.Console.WriteLine("Unzipping the dash files and updating GARW metadata files.");
                        //unzip and overwrite the content if it existed
                        execCmd = "unzip -o /opt/Garw_IC7/library/" + dashFileName + " -d /opt/Garw_IC7/library/";
                        //System.Console.WriteLine(execCmd);
                        client.RunCommand(execCmd);
                        //remove the .zip file from the GARW
                        execCmd = "rm /opt/Garw_IC7/library/" + dashFileName;
                        //System.Console.WriteLine(execCmd);
                        client.RunCommand(execCmd);
                        //remove the .zip file cruft from MACOSX uploads (hidden folder)
                        execCmd = "rm -rf /opt/Garw_IC7/library/__MACOSX";
                        //System.Console.WriteLine(execCmd);
                        client.RunCommand(execCmd);
                        //if the dash was not already added prior then add one to screens available for the settings screen
                        execCmd = "grep -L '" + dashName + "' /opt/Garw_IC7/library/screen_list.txt || cat /opt/Garw_IC7/library/screens_available.txt | awk -F: '{print $1=$1+1}' > /opt/Garw_IC7/library/temp.txt";
                        //System.Console.WriteLine(execCmd);
                        client.RunCommand(execCmd);
                        //move file (timing hack for the above command)
                        execCmd = "mv /opt/Garw_IC7/library/temp.txt /opt/Garw_IC7/library/screens_available.txt";
                        //System.Console.WriteLine(execCmd);
                        client.RunCommand(execCmd);
                        //add the name to the screen list for display in settings
                        execCmd = "grep -E '" + dashName + "' /opt/Garw_IC7/library/screen_list.txt || echo '" + dashName + "' >> /opt/Garw_IC7/library/screen_list.txt";
                        //System.Console.WriteLine(execCmd);
                        client.RunCommand(execCmd);
                        //add the location to the new dash screen files
                        execCmd = "grep -E '" + dashPathToAdd + "' /opt/Garw_IC7/library/screen_locations.txt || echo '" + dashPathToAdd + "' >> /opt/Garw_IC7/library/screen_locations.txt";
                        //System.Console.WriteLine(execCmd);
                        client.RunCommand(execCmd);
                        System.Console.WriteLine("Reboot the GARW now? (Y or N)");
                        string confirmRemoval = Console.ReadLine();
                        if (confirmRemoval.Length == 0)
                        {
                            client.Disconnect();
                        }
                        if (confirmRemoval.ToUpper().Trim() == "Y"){
                            System.Console.WriteLine("Rebooting the GARW system...");
                            //reboot the GARW
                            execCmd = "reboot now";
                            client.RunCommand(execCmd);
                        }
                        else
                            client.Disconnect();
                    }
                    System.Console.WriteLine("*** New dash screen added successfully.  Please select the dash from the settings screen of the GARW using your controller app! ***");
                }
                else
                {
                    System.Console.WriteLine("File not found on your local system, please check the file path location.");
                    return;
                }
            }
            else 
            {
                PrintUsage();
                return;
            }
        }
    }

    static void PrintUsage()
    {
            System.Console.WriteLine("GARWDashUploader:");
            System.Console.WriteLine("  Uploads a new QT Dash design to your GARW device.");
            System.Console.WriteLine("Usage:");
            System.Console.WriteLine("  GARWDashUploader [options]");
            System.Console.WriteLine("Options:");
            System.Console.WriteLine("  --input         The path to the .zip file that contains the new dash design to be added to your GARW device.");
            System.Console.WriteLine("  --purge         Removes ALL the 3rd party dash designs from your GARW system (default dash screens are not affected).");
    }
}