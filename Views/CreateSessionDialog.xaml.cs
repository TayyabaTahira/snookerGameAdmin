using System.Windows;
using System.Windows.Input;
using SnookerGameManagementSystem.ViewModels;

namespace SnookerGameManagementSystem.Views
{
    public partial class CreateSessionDialog : Window
    {
        public CreateSessionDialog(CreateSessionViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void CreateTable_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ComboBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = MouseWheelEvent,
                Source = sender
            };
            var parent = ((System.Windows.Controls.Control)sender).Parent as UIElement;
            parent?.RaiseEvent(eventArg);
        }
    }
}
