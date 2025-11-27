using System.Windows;
using SnookerGameManagementSystem.ViewModels;

namespace SnookerGameManagementSystem.Views
{
    public partial class SelectCustomerDialog : Window
    {
        public SelectCustomerDialog(SelectCustomerViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += (s, e) => Focus();
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
    }
}
