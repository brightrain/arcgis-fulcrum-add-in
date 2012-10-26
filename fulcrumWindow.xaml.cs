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
using System.Threading;

namespace fulcrum
{
    /// <summary>
    /// Interaction logic for fulcrumWindow.xaml
    /// </summary>
    public partial class FulcrumWindow : Window
    {
        fulcrumforms fulcrumForms;

        public FulcrumWindow()
        {
            InitializeComponent();
            try
            {
                //grab the forms associated with the api key and put in the box
                fulcrumForms = FulcrumCore.GetForms();
                if (fulcrumForms != null)
                {
                    foreach (fulcrumform form in fulcrumForms.forms)
                    {
                        ListBoxItem item = new ListBoxItem();
                        item.Tag = form.id;
                        item.Content = (form.name);
                        this.listboxForms.Items.Add(item);
                    }
                }
                else
                {
                    MessageBox.Show("Unable to retrieve Fulcrum data at this time.", "Fulcrum Tools", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    this.Hide();
                }
            }
            catch (Exception e)
            {
                string ouch = e.Message;
                MessageBox.Show("Unable to retrieve Fulcrum data at this time.", "Fulcrum Tools", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                this.Hide();
            }
        }
        private void buttonGetRecords_Click(object sender, RoutedEventArgs e)
        {
            ESRI.ArcGIS.Framework.IMouseCursor cursor = new ESRI.ArcGIS.Framework.MouseCursorClass();
            try
            {
                ArcMap.Application.StatusBar.set_Message(0, "Fulcrum import...");
                cursor.SetCursor(2);
                this.Cursor = Cursors.Wait;
                ListBoxItem item = (ListBoxItem)this.listboxForms.SelectedItem;
                //we have set the Name to the form id when populating the list box
                string formid = item.Tag.ToString();
                fulcrumform theForm = fulcrumForms.forms.Find(form => form.id == formid);
                this.progBar.Visibility = System.Windows.Visibility.Visible;
                bool success = FulcrumCore.FireUp(theForm, this.progBar, this.labelStatus);
            }
            catch(Exception ex)
            {
                string bomb = ex.Message;          
            }
            finally
            {
                cursor = null;
                this.progBar.Visibility = System.Windows.Visibility.Hidden;
                ArcMap.Application.StatusBar.set_Message(0, "");
                this.Cursor = Cursors.Arrow;
                GC.Collect();
            }
        }
        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
