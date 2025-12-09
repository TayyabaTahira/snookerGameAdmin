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
            // Prevent ComboBox from handling mouse wheel - let it bubble to parent ScrollViewer
            e.Handled = true;
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = MouseWheelEvent,
                Source = sender
            };
            var parent = ((System.Windows.Controls.Control)sender).Parent as UIElement;
            parent?.RaiseEvent(eventArg);
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Let the ScrollViewer inside the dropdown handle scrolling without closing
            // Don't mark as handled - allow normal scrolling behavior
        }

        protected override void OnClosed(EventArgs e)
        {
            _viewModel.CustomerCreatedAndSelected -= OnCustomerCreatedAndSelected;
            base.OnClosed(e);
        }
    }
}
