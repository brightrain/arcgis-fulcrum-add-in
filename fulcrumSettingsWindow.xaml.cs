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
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.pathToGeoDB = this.textboxGDBLocation.Text;
            Properties.Settings.Default.fulcrumApiKey = this.textboxAPIKey.Text;
            Properties.Settings.Default.Save();
            this.Hide();
        }

        private void buttonBrowse_Click(object sender, RoutedEventArgs e)
        {
            this.textboxGDBLocation.Text = ArcStuff.GetGeodatabaseFromUser();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
