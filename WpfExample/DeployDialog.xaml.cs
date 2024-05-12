using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using MarsRoversCore;

namespace WpfExample
{
    /// <summary>
    /// Interaction logic for DeployDialog.xaml
    /// </summary>
    public partial class DeployDialog : Window, INotifyPropertyChanged
    {
        protected void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private bool errorViewed = false; 
        private string _errorMessage = string.Empty;
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; NotifyPropertyChanged("ErrorMessage"); } }

        // Read the deployed rover from  here
        public IRover? rover { get; set; } = null;

        // Set thess up bbefore opening the dialog, so it knows where to deploy
        public IPlateau? plateau { get; set; } = null;
        public IPlateauLocation? location { get; set; } = null;

        public DeployDialog()
        {
            InitializeComponent();
        }

        private void Deploy_Click(object sender, RoutedEventArgs e)
        {
            if (plateau != null && location != null)
            {
                string errorMessage = string.Empty;
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendFormat("{0} {1} ", location.X, location.Y);
                if (northButton.IsChecked.HasValue ? northButton.IsChecked.Value : false)
                    stringBuilder.Append("N");
                else if (southButton.IsChecked.HasValue ? southButton.IsChecked.Value : false)
                    stringBuilder.Append("S");
                else if (westButton.IsChecked.HasValue ? westButton.IsChecked.Value : false)
                    stringBuilder.Append("W");
                else if (eastButton.IsChecked.HasValue ? eastButton.IsChecked.Value : false)
                    stringBuilder.Append("E");

                rover = Rover.Deploy<Rover>(plateau, stringBuilder.ToString(), out errorMessage);

                if (errorMessage != string.Empty) 
                { 
                    ErrorMessage = errorMessage; 
                }
            }

            DialogResult = rover != null;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void deployDialogBox_Closing(object sender, CancelEventArgs e)
        {
            // If there as an error, we want the  user read it and then click again
            e.Cancel = !string.IsNullOrEmpty(_errorMessage) && !errorViewed;
            if (e.Cancel)
            {
                if (!errorViewed)
                    errorViewed = true;
            }
        }

        private void orientationButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                if (sender is ToggleButton)
                {
                    var button = (ToggleButton)sender;
                    // We fake a group here: uncheck everyone who is not us
                    if (button != northButton && northButton != null)
                        northButton.IsChecked = false;
                    if (button != westButton && westButton != null)
                        westButton.IsChecked = false;
                    if (button != southButton && southButton != null)
                        southButton.IsChecked = false;
                    if (button != eastButton && eastButton != null)
                        eastButton.IsChecked = false;
                }
            }
        }
    }
}
