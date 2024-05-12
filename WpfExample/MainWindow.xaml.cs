using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using MarsRoversCore;

namespace WpfExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const string ROVER_ID_EMPTY = "Rover ID: [None]";
        private const string PLATEAU_ID_EMPTY = "Plateau ID: [None]";
        private const string ROVER_ID_PREFIX = "Rover ID: ";
        private const string PLATEAU_ID_PREFIX = "Plateau ID: ";

        #region INotifyPropertyChanged
        protected void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        #endregion

        // For the plateau dimensions
        private static readonly Regex numbersOnlyRegex = new ("^[0-9]+$");

        public IPlateau? plateau = null;
        public int plateauWidth = 0;
        public int plateauHeight = 0;

        // The propertied below are participating - or might have praticipated -in binding
        private string _plateauIDText = PLATEAU_ID_EMPTY;
        private string _roverIDText = ROVER_ID_EMPTY;
        private IRover? _selectedRover = null;

        private Button? selectedButton = null;

        private Grid? plateauGrid = null;

        public string plateauIDText { get=>_plateauIDText; set { _plateauIDText = value; NotifyPropertyChanged("plateauIDText"); } }
        public string roverIDText { get=> _roverIDText; set { _roverIDText = value; NotifyPropertyChanged("roverIDText"); } }
        public IRover? selectedRover { get => _selectedRover; set { _selectedRover = value; NotifyPropertyChanged("selectedRover"); } }

        public MainWindow()
        {
            InitializeComponent();
        }

        private bool allowNumbersOnly(string text)
        {
            return numbersOnlyRegex.IsMatch(text);
        }

        private IPlateau? CreatePlateau(int width, int height)
        {
            return new Plateau(width, height);
        }

        /// <summary>
        /// Creates a grid for the plateaus on the fly and populates it by the UI elements used to show the rovers
        /// </summary>
        /// <returns>The newly created grid or null</returns>
        private Grid? CreatePlateauGrid()
        {
            if (plateau != null)
            {
                Grid DynamicGrid = new();
                DynamicGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                DynamicGrid.VerticalAlignment = VerticalAlignment.Stretch;
                DynamicGrid.ShowGridLines = true;
                DynamicGrid.Background = new SolidColorBrush(Colors.LightSteelBlue);

                // First, add the column number row
                RowDefinition coulmnNumberGridRow = new();
                coulmnNumberGridRow.Height = new(1.0, GridUnitType.Auto);
                DynamicGrid.RowDefinitions.Add(coulmnNumberGridRow);

                // Next, add the row number column
                ColumnDefinition rowNumberGridColumn = new();
                rowNumberGridColumn.Width = new(1.0, GridUnitType.Auto);
                DynamicGrid.ColumnDefinitions.Add(rowNumberGridColumn);

                // Now add the plateau rows - remembering that the row 0 is AT THE BOTTOM
                for (int i = plateau.Height - 1; i >= 0; i--)
                {
                    RowDefinition gridRow = new();
                    gridRow.Height = new(1.0, GridUnitType.Star);
                    DynamicGrid.RowDefinitions.Add(gridRow);

                    var colNumber = new TextBlock();
                    colNumber.Text = i.ToString();
                }

                // Finally, add the plateau columns: the column 0 is on the left so, we're okay
                for (int j = 0; j < plateau.Width; j++)
                {
                    ColumnDefinition gridColumn = new();
                    gridColumn.Width = new(1.0, GridUnitType.Star);
                    DynamicGrid.ColumnDefinitions.Add(gridColumn);
                }

                // Now that the column number row has the correct number of columns
                // and the row number column has the correct number of rows, add the
                // actual numbers to both
                for (int i = 0; i < plateau.Height; i++)
                {
                    var numberText = new TextBlock();
                    numberText.Text = i.ToString();
                    numberText.FontWeight = FontWeights.Bold;
                    numberText.HorizontalAlignment = HorizontalAlignment.Right;
                    numberText.VerticalAlignment = VerticalAlignment.Center;
                    numberText.Margin = new Thickness(10, 5, 2.5, 5);
                    DynamicGrid.Children.Add(numberText);
                    Grid.SetColumn(numberText, 0);
                    Grid.SetRow(numberText, plateau.Height - i);
                }

                for (int j = 0; j < plateau.Width; j++)
                {
                    var numberText = new TextBlock();
                    numberText.Text = j.ToString();
                    numberText.FontWeight = FontWeights.Bold;
                    numberText.HorizontalAlignment = HorizontalAlignment.Center;
                    numberText.VerticalAlignment = VerticalAlignment.Bottom;
                    numberText.Margin = new Thickness(5, 10, 5, 2.5);
                    DynamicGrid.Children.Add(numberText);
                    Grid.SetColumn(numberText, j + 1);
                    Grid.SetRow(numberText, 0);
                }

                // Add the compass icon at the top left corner: it's too small, but maybe can give some idea as to the directions
                var compassIcon = new Image();
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri("pack://application:,,,/WpfExample;component/Images/compass.png");
                image.EndInit();
                compassIcon.Source = image;
                compassIcon.Width = 64;
                compassIcon.Height = 64;
                DynamicGrid.Children.Add(compassIcon);
                Grid.SetColumn(compassIcon, 0);
                Grid.SetRow(compassIcon, 0);

                // At last, add some content to each cell and associate it with the plateau location.
                // Remember that our zero row is AT THE BOTTOM, and that we've added the numbers as the first (the topmost) row
                // and the first (the leftmost) column
                for (int i = 0; i < plateau.Height; i++)
                {
                    for (int j = 0; j < plateau.Width; j++)
                    {
                        var locationButton = new Button();
                        locationButton.HorizontalAlignment = HorizontalAlignment.Stretch;
                        locationButton.VerticalAlignment = VerticalAlignment.Center;
                        var content = new TextBlock();
                        content.BeginInit();
                        content.Text = "+";
                        content.FontSize = 24;
                        content.TextAlignment = TextAlignment.Center;
                        content.HorizontalAlignment = HorizontalAlignment.Center;
                        content.VerticalAlignment = VerticalAlignment.Center;
                        content.Background = SystemColors.ControlLightBrush;
                        content.Foreground = SystemColors.WindowTextBrush;
                        content.EndInit();
                        locationButton.Content = content;
                        locationButton.Tag = plateau?.locations[i, j];
                        locationButton.Click += plateauLocationButton_Click;

                        int gridRow = plateau!.Height - i;
                        int gridCol = j + 1;

                        DynamicGrid.Children.Add(locationButton);
                        Grid.SetColumn(locationButton, gridCol);
                        Grid.SetRow(locationButton, gridRow);
                    }
                }

                mainGrid.Children.Add(DynamicGrid);
                Grid.SetRow(DynamicGrid, 1);
                Grid.SetColumn(DynamicGrid, 0);

                return DynamicGrid;
            }
            else
                return null;
        }

        /// <summary>
        /// If a rover occupies the IPlateauLocation that is the button's Tag,
        /// show an arrow pointing in the direction of the orver's orientation;
        /// if the location is available, show the + sign to indicate that a new 
        /// rover can be deployed here
        /// </summary>
        /// <param name="rover">The rover whose orientatin is being reflected</param>
        /// <param name="button">The button that is being updated</param>
        private void SetRoverButtonText(IRover? rover, Button button)
        {
            if (rover != null && button != null)
            {
                var content = button.Content as TextBlock;
                if (content != null)
                {
                    switch (rover?.Direction)
                    {
                        case 'N':
                            content.Text = "⬆︎";
                            content.FontSize = 28;
                            break;
                        case 'W':
                            content.Text = "⬅︎";
                            content.FontSize = 28;
                            break;
                        case 'S':
                            content.Text = "⬇︎";
                            content.FontSize = 28;
                            break;
                        case 'E':
                            content.Text = "➞";
                            content.FontSize = 28;
                            break;
                        default:
                            content.Text = "+";
                            content.FontSize = 24;
                            break;
                    }
                }
            }
            else if (button != null)
            {
                var content = button.Content as TextBlock;
                if (content != null)
                {
                    content.Text = "+";
                    content.FontSize = 24;
                }
            }
        }

        #region Helpers

        private void DeselectRover()
        {
            selectedRover = null;
            roverIDText = ROVER_ID_EMPTY;
            commsSendButton.IsEnabled = false;
        }

        private void SelectRover(IRover? rover)
        {
            selectedRover = rover;
            roverIDText = string.Format("{0}{1}", ROVER_ID_PREFIX, rover?.ID);
            commsTextBox.Text = selectedRover?.CurrentPosition;
            commsSendButton.IsEnabled = true;
        }

        private void DeselectSelectedRoverButton()
        {
            if (selectedButton != null)
            {
                var content = selectedButton.Content as TextBlock;
                if (content != null)
                {
                    content.Background = SystemColors.ControlLightBrush;
                }
                selectedButton = null;
            }
        }
        private void SelectRoverButton(Button button)
        {
            if (button != null)
            {
                if (button != selectedButton)
                    DeselectSelectedRoverButton();

                var content = button.Content as TextBlock;
                if (content != null)
                {
                    content.Background = SystemColors.HighlightBrush;
                }
                selectedButton = button;
            }
        }

        private Button? ButtonByPlateauCoordinates(int X, int Y)
        {
            if (plateauGrid != null)
            {
                var element = (from UIElement b in plateauGrid.Children 
                              where b is Button && 
                              (b as Button)?.Tag is IPlateauLocation && 
                              ((b as Button)?.Tag as IPlateauLocation)?.X == X && 
                              ((b as Button)?.Tag as IPlateauLocation)?.Y == Y select b).FirstOrDefault();
                return element as Button;
            }
            else
                return null;
        }

        #endregion

        private void plateauLocationButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender as Button != null)
            {
                var button = sender as Button;
                if (button?.Tag != null)
                {
                    if (button?.Tag is IPlateauLocation)
                    {
                        var textBlock = button?.Content as TextBlock;
                        var location = button?.Tag as IPlateauLocation;
                        if (textBlock != null && location != null) 
                        {
                            // We're on:
                            // first, deselect the old button, if any

                            if (location!.IsAvailable)
                            {
                                // Deploy a new rover here
                                var dlg = new DeployDialog();
                                dlg.plateau = plateau;
                                dlg.location = location;
                                var dlgRes = dlg.ShowDialog();
                                if (dlgRes.HasValue ? dlgRes.Value : false)
                                {
                                    DeselectRover();
                                    SelectRover(dlg.rover);
                                    SetRoverButtonText(selectedRover, button!);
                                    DeselectSelectedRoverButton();
                                    SelectRoverButton(button!);
                                }
                                else
                                {
                                    DeselectSelectedRoverButton();
                                    DeselectRover();
                                }
                            }
                            else
                            {
                                // Select the rover for interaction
                                var rover = location!.Rover;
                                if (rover != null)
                                {
                                    DeselectRover();
                                    DeselectSelectedRoverButton();
                                    SelectRover(rover);
                                    SelectRoverButton(button!);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void plateauWidthBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !allowNumbersOnly(e.Text);
        }

        private void plateauHeihtBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !allowNumbersOnly(e.Text);
        }

        private void plateauGenerateButton_Click(object sender, RoutedEventArgs e)
        {
            if (plateauWidth > 0 && plateauHeight > 0)
            {
                plateau = CreatePlateau(plateauWidth, plateauHeight);
                plateauIDText = string.Format("{0}{1:D}", PLATEAU_ID_PREFIX, plateau?.ID);
                plateauGrid = CreatePlateauGrid();
            }
        }

        private void plateauWidthBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Int32.TryParse(plateauWidthBox.Text.Trim(), out int width))
                plateauWidth = width;
        }

        private void plateauHeihtBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Int32.TryParse(plateauHeihtBox.Text.Trim(), out int height))
                plateauHeight = height;
        }

        private void commsSendButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRover != null)
            {
                var response = selectedRover.Command(commsTextBox.Text.Trim());
                commsTextBox.Text = response;

                // WHether it's moved or not, we update the selected rover's location
                var location = selectedRover.location;
                if (location != null)
                {
                    var button = ButtonByPlateauCoordinates(location.X, location.Y);
                    if (button != null)
                    {
                        // Make the button at the old location look like it's available - which it is now
                        if (selectedButton != null)
                            SetRoverButtonText(null, selectedButton);
                        DeselectSelectedRoverButton();

                        // And make the button at the new location show the rover
                        SetRoverButtonText(selectedRover, button);
                        SelectRoverButton(button);
                    }
                }
            }
        }
    }
}