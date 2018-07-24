using System;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace CommandPrompt
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            CreateRootFolder();

            AddPrompt(CurrentFolder.Path);

            // Just after launching the App, Entry.Focus() doesn't work like 
            // ScrollToAsync() due to timing probelm(?), so maake it into Timer 
            Device.StartTimer(TimeSpan.FromMilliseconds(1), () =>
            {
                CommandLines.Last().CommandEntry.Focus();
                return false;   // Timer Cycle is only one time
            });
            
        }

        
        // Focus Command Line, if Tap Console
        void OnTapConsole(object sender, EventArgs args)
        {
            CommandLines.Last().CommandEntry.Focus();
        }


        // Identify Input Command and Execute it
        async void CommandExecute(string command)
        {            
            string result = "";

            // Get the first three characters of command
            string com = command;
            if(command != null)
                if(command.Length>3)
                    com = command.Substring(0, 3);


            // Identify the command from the first three characters            
            switch (com)
            {
                case "":
                case null:
                    AddPrompt(CurrentFolder.Path);
                    return;

                case "dir":
                    result = dir();
                    break;

                case "pwd":
                    result = CurrentFolder.Path + "\n";
                    break;

                case "mkd":
                    result = await mkdir(command);
                    break;

                case "cd ":
                    result = await cd(command);
                    break;

                case "del":
                    result = await del(command);
                    break;                

                case "mor":
                    result = await more(command);
                    break;

                case "ed ":
                    result = await ed(command);
                    break;

                default:
                    result = "No Command" + "\n";
                    break;
            }

            // Display the Command Execute Result following the Command Line
            Console.Children.Add(new Label() { Text = result });

            // Store the result
            CommandLines.Last().Results = result;            

            // Add a new Command Line
            AddPrompt(CurrentFolder.Path);
            
        }


        // Display All Folders and Files
        string dir()
        {
            string result = "";
            Folder folder = CurrentFolder;

            foreach(Folder subfolder in folder.SubFolders)            
                result += string.Format("<DIR>  {0}\n", subfolder.Name);
            
            foreach(File file in folder.Files)            
                result += file.Name + "\n";
            
            return result;
        }


        // Make Directory
        async Task<string> mkdir(string command)
        {
            string result = " ";
            string name = "";
            Folder parent = CurrentFolder;

            if (command.Length < 6) return "Command Invalid";
            else if (command.Substring(0, 6) != "mkdir ")
                return "Command Invalid";
            else if (command.Substring(6) == "")
                return "No Folder Name";
            else name = command.Substring(6);

            Folder folder = new Folder()
            {
                Name = name,
                Parent = parent
            };

            // Check the existence of the IFolder corresponding to the Object "folder"
            bool IsExist = await ICheckFolderExist(name, parent.iFolder);
            if (IsExist) return name.ToString() + " already exists";

            folder.iFolder = await ICreateFolder(folder, parent.iFolder, false);
            parent.SubFolders.Add(folder);

            //Label3.Text = folder.iFolder.Name;

            return result;
        }

        // Change Directory
        async Task<string> cd(string command)
        {
            string result = " ";
            string name = "";

            if (command.Substring(3) == "")
                return "No Folder Name";
            else name = command.Substring(3);

            // Back to the Parent Folder
            if(name == "..")
            {
                if (CurrentFolder == LocalStorage) return "No Parent Folder";
                CurrentFolder = CurrentFolder.Parent;
                return result;
            }

            // Check the existence of the IFolder corresponding to the Object "folder"
            bool IsExist = await ICheckFolderExist(name, CurrentFolder.iFolder);
            if (!IsExist) return name + " doesn't exist";

            foreach(Folder folder in CurrentFolder.SubFolders)
            {
                if (folder.Name == name)
                {
                    CurrentFolder = folder;
                    break;
                }
            }

            return result;
        }

        // Delete a Folder or a File
        async Task<string> del(string command)
        {
            string result = " ";
            string name = "";

            if (command.Substring(0, 4) != "del ")
                return "Command Invalid";
            else if (command.Substring(4) == "")
                return "No Folder/File Name";
            else name = command.Substring(4);
            
            // Check the existence of the IFolder or the IFile
            bool IsExist1 = await ICheckFolderExist(name, CurrentFolder.iFolder);
            bool IsExist2 = await ICheckFileExist(name, CurrentFolder.iFolder);            

            if(IsExist1)        // Delete Folder
            {
                Folder DelFolder = new Folder();
                foreach(Folder folder in CurrentFolder.SubFolders)
                {
                    if(folder.Name==name)
                    {
                        await IDeleteFolder(folder.iFolder, folder.IParent);
                        DelFolder = folder;
                        break;
                    }
                }
                CurrentFolder.SubFolders.Remove(DelFolder);
            }
            else if(IsExist2)   // Delete File
            {
                File DelFile = new File();
                foreach (File file in CurrentFolder.Files)
                {
                    if (file.Name == name)
                    {
                        await IDeleteFile(file.iFile, file.IParent);
                        DelFile = file;
                        break;
                    }
                }
                CurrentFolder.Files.Remove(DelFile);                
            }
            else return name + " doesn't exist";

            return result;
        }


        // Display File Content
        async Task<string> more(string command)
        {
            //string result = " ";
            string name = "";
            Folder parent = CurrentFolder;

            if (command.Length < 5) return "Command Invalid";
            else if (command.Substring(0, 5) != "more ")
                return "Command Invalid";
            else if (command.Substring(5) == "")
                return "No File Name";
            else name = command.Substring(5);

            foreach (File file in parent.Files)            
                if (file.Name == name)                
                    return await IReadFile(file.iFile);                
            
            return "No such File";
        }

        // Edit File
        async Task<string> ed(string command)
        {
            string result = " ";
            string name = "";
            Folder parent = CurrentFolder;

            if (command.Substring(3) == "")
                return "No File Name";
            else name = command.Substring(3);

            // Check the existence of the File
            bool IsExist = await ICheckFileExist(name, parent.iFolder);

            if(IsExist)
            {
                foreach(File file in parent.Files)
                {
                    if (file.Name == name)
                    {                        
                        FileEditor.Text = await IReadFile(file.iFile);
                        CurrentFile = file;
                        break;
                    }
                }
            }
            else
            {
                File file = new File()
                {
                    Name = name,
                    Parent = parent
                };
                file.iFile = await ICreateFile(file, parent.iFolder);
                parent.Files.Add(file);

                CurrentFile = file;
            }

            FileEditorTitle.Text = "File Name: " + CurrentFile.Name;            
            FileEditorGrid.IsVisible = true;

            // Editor.Focus() doesn't work due to timing probelm(?), 
            // so maake it into Timer 
            Device.StartTimer(TimeSpan.FromMilliseconds(1), () =>
            {
                FileEditor.Focus();
                return false;   // Timer Cycle is only one time
            });            

            return result;
        }
        
        // Buttons of FileEditor Menu: save File, cancel Editor
        void SaveFile(object sender, EventArgs e)
        {
            // PCL Storage: Write content in IFile
            IWriteFile(CurrentFile.iFile, FileEditor.Text);
        }
        void CancelEditor(object sender, EventArgs e)
        {
            FileEditorGrid.IsVisible = false;
            CommandLines.Last().CommandEntry.Focus();
        }




        void Button01(object sender, EventArgs args)
        {
            
        }

        void Button02(object sender, EventArgs args)
        {            
            IDleteAllObjects(LocalStorage);
            CommandLines.Last().CommandEntry.Focus();
        }
    }
}
