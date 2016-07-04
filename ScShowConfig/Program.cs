namespace ScShowConfig
{
    using System;
    using System.IO;
    using System.Windows.Forms;
    using Sitecore;
    using Sitecore.Configuration;

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Select a Sitecore App_Config folder to continue...");

            // Select App_Config folder
            string appConfigFolderPath = SelectFolder();

            if (appConfigFolderPath == null)
            {
                Console.WriteLine("No folder selected... Abort!");
                Console.ReadLine();
                return;
            }

            // Copy the folder to our application path.
            string root = AppDomain.CurrentDomain.BaseDirectory;
            string targetAppConfigFolderPath = Path.Combine(root, "App_Config");

            if (Directory.Exists(targetAppConfigFolderPath))
            {
                Directory.Delete(targetAppConfigFolderPath, true);
            }

            CopyFilesRecursively(new DirectoryInfo(appConfigFolderPath), new DirectoryInfo(targetAppConfigFolderPath));

            // Verify that we have a Sitecore.config
            if (!File.Exists(Path.Combine(targetAppConfigFolderPath, "Sitecore.config")))
            {
                Console.WriteLine("Sitecore.config does not exist in App_Config... Abort!");
                Console.ReadLine();
                return;
            }

            // Load the configuration
            Context.Items["sc_IsUnitTesting"] = true;
            State.HttpRuntime.AppDomainAppPath = AppDomain.CurrentDomain.BaseDirectory;

            var xml = Sitecore.Configuration.Factory.GetConfiguration();

            // Save and quit.
            xml.Save("output.xml");
            
            Console.WriteLine("Saved consolidated configuration to output.xml");
            Console.ReadLine();
        }

        private static string SelectFolder()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();
            
            if (!string.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                return dialog.SelectedPath;
            }

            return null;
        }

        private static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
            {
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            }
            foreach (FileInfo file in source.GetFiles())
            {
                file.CopyTo(Path.Combine(target.FullName, file.Name));
            }
        }
    }
}
