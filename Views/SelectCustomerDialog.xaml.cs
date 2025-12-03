using System.Windows;
using System.Windows.Input;
using SnookerGameManagementSystem.ViewModels;

namespace SnookerGameManagementSystem.Views
{
    public partial class SelectCustomerDialog : Window
    {
        private readonly SelectCustomerViewModel _viewModel;

        public SelectCustomerDialog(SelectCustomerViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;
            
            _viewModel.CustomerCreatedAndSelected += OnCustomerCreatedAndSelected;
            Loaded += (s, e) => Focus();
        }

        private void OnCustomerCreatedAndSelected(object? sender, EventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Select_Click(object sender, RoutedEventArgs e)
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

        protected override void OnClosed(EventArgs e)
        {
            _viewModel.CustomerCreatedAndSelected -= OnCustomerCreatedAndSelected;
            base.OnClosed(e);
        }
    }
}
