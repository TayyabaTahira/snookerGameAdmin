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
            SearchBox.Focus();
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
