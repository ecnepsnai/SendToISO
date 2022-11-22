using DiscUtils.Iso9660;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace SendToISO
{
    internal class Program
    {
        [STAThreadAttribute]
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                return 0;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "ISO Disc Archive|*.iso";
            saveFileDialog.Title = "Save ISO Archive";
            if (saveFileDialog.ShowDialog() != DialogResult.OK)
            {
                return 0;
            }

            string name = new DirectoryInfo(saveFileDialog.FileName).Name;

            CDBuilder builder = new CDBuilder();
            builder.UseJoliet = true;
            builder.VolumeIdentifier = name;

            try
            {
                foreach (string path in args)
                {
                    if (Directory.Exists(path))
                    {
                        AddDirectory(builder, null, new DirectoryInfo(path).Name, path);
                    }
                    if (File.Exists(path))
                    {
                        builder.AddFile(new DirectoryInfo(path).Name, path);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 1;
            }
            
            try
            {
                builder.Build(saveFileDialog.FileName);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 1;
            }

            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "explorer";
            info.Arguments = string.Format("/e, /select, \"{0}\"", saveFileDialog.FileName);
            Process.Start(info);
            return 0;
        }

        static void AddDirectory(CDBuilder builder, string parent, string dirName, string actualPath)
        {
            string relativeDirPath;
            if (parent != null)
            {
                relativeDirPath = parent + "/" + dirName;
            }
            else
            {
                relativeDirPath = dirName;
            }

            builder.AddDirectory(relativeDirPath);

            foreach (var file in Directory.GetFiles(actualPath))
            {
                string relativeFilePath = relativeDirPath + "/" + new DirectoryInfo(file).Name;
                builder.AddFile(relativeFilePath, file);
            }

            foreach (var subdir in Directory.GetDirectories(actualPath))
            {
                AddDirectory(builder, relativeDirPath, new DirectoryInfo(subdir).Name, subdir);
            }
        }
    }
}
