using Renci.SshNet;
using System.IO.Compression;
class Program
{
    static void Main(string[] args)
    {
        //Check to ensure command line usage is correct, must pass a full path to the new dashscreen .zip file
        if (args.Length == 0)
        {
            System.Console.WriteLine("Please pass in the full path to the new GARW dash .zip file as a command line argument.");
            return;
        }

        string uploadZip = args[0];
        string dashFileName = Path.GetFileName(uploadZip);

        //Check the local path of the file to ensure it ia valid
        if (File.Exists(uploadZip)){
            string dashName = Path.GetFileNameWithoutExtension(uploadZip);
            string dashPathToAdd = "file:///opt/Garw_IC7/library/" + dashName + "/" + dashName + ".qml";

            //Check zip file contents for a .qml with the same zip file name and a .png for the settings screen
            using (ZipArchive archive = ZipFile.OpenRead(uploadZip)){
                int totalEntries = archive.Entries.Count;
                int checkEntries = 0;
                foreach (ZipArchiveEntry e in archive.Entries){
                    if (Path.GetFileName(e.FullName) == dashName + ".qml.png"){
                        checkEntries++;
                    }
                    
                    if (Path.GetFileName(e.FullName) == dashName + ".qml"){
                        checkEntries++;
                    }
                }
                //If both are not found, stop and prompt user to check the .zip file
                if (checkEntries != 2){
                    System.Console.WriteLine("Invalid .zip file contents, you must ensure the .qml matches the zip name (case sensitive) and you have the proper .png file included!");
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
                execCmd = "rm -r /opt/Garw_IC7/library/__MACOSX";
                //System.Console.WriteLine(execCmd);
                client.RunCommand(execCmd);
                //if the dash was not already added prior then add one to screens available for the settings screen
                execCmd = "grep -L '" + dashName.Replace("_main","") + "' /opt/Garw_IC7/library/screen_list.txt || cat /opt/Garw_IC7/library/screens_available.txt | awk -F: '{print $1=$1+1}' > /opt/Garw_IC7/library/temp.txt";
                //System.Console.WriteLine(execCmd);
                client.RunCommand(execCmd);
                //move file (timing hack for the above command)
                execCmd = "mv /opt/Garw_IC7/library/temp.txt /opt/Garw_IC7/library/screens_available.txt";
                //System.Console.WriteLine(execCmd);
                client.RunCommand(execCmd);
                //add the name to the screen list for display in settings
                execCmd = "grep -E '" + dashName.Replace("_main","") + "' /opt/Garw_IC7/library/screen_list.txt || echo '" + dashName.Replace("_main","") + "' >> /opt/Garw_IC7/library/screen_list.txt";
                //System.Console.WriteLine(execCmd);
                client.RunCommand(execCmd);
                //add the location to the new dash screen files
                execCmd = "grep -E '" + dashPathToAdd + "' /opt/Garw_IC7/library/screen_locations.txt || echo '" + dashPathToAdd + "' >> /opt/Garw_IC7/library/screen_locations.txt";
                //System.Console.WriteLine(execCmd);
                client.RunCommand(execCmd);
                client.Disconnect();
            }
            System.Console.WriteLine("*** New dash screen added successfully.  Please select the dash from the settings screen of the GARW using your controller app! ***");
        }
        else
        {
            System.Console.WriteLine("File not found on your local system, please check the file path.");
            return;
        }
    }
}