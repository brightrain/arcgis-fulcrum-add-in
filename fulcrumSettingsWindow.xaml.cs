using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Forms;

namespace fulcrum
{
    /// <summary>
    /// Interaction logic for fulcrumSettingsWindow.xaml
    /// </summary>
    public partial class fulcrumSettingsWindow : Window
    {
        public fulcrumSettingsWindow()
        {
            InitializeComponent();
            this.textboxGDBLocation.Text = Properties.Settings.Default.pathToGeoDB;
            this.textboxAPIKey.Text = Properties.Settings.Default.fulcrumApiKey;
            this.textboxPhotoLocation.Text = Properties.Settings.Default.pathToPhotoFolder;
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.pathToGeoDB = this.textboxGDBLocation.Text;
            Properties.Settings.Default.fulcrumApiKey = this.textboxAPIKey.Text;
            Properties.Settings.Default.pathToPhotoFolder = this.textboxPhotoLocation.Text;
            Properties.Settings.Default.Save();
            this.Hide();
        }

        private void buttonBrowse_Click(object sender, RoutedEventArgs e)
        {
            this.textboxGDBLocation.Text = ArcStuff.GetGeodatabaseFromUser();
        }

        private void buttonBrowseToPhotoFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (System.Windows.Forms.FolderBrowserDialog folderBrowser = new System.Windows.Forms.FolderBrowserDialog())
                {
                    // My Computer will be the root folder in the dialog.
                    folderBrowser.RootFolder = Environment.SpecialFolder.MyComputer;
                    // Select the default directory on entry.
                    folderBrowser.SelectedPath = Environment.SpecialFolder.MyComputer.ToString();
                    // Prompt the user with a custom message.
                    folderBrowser.Description = "Select the folder where downloaded photos will be stored";
                    if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        //put the path in the text box
                        this.textboxPhotoLocation.Text = folderBrowser.SelectedPath;
                    }
                }
            }
                catch(Exception ex)
            {
                    string choke = ex.Message;
                }
            }
    }
}
