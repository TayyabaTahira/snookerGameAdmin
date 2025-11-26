using System.Windows;
using SnookerGameManagementSystem.ViewModels;

namespace SnookerGameManagementSystem.Views
{
    public partial class EditCustomerDialog : Window
    {
        public EditCustomerDialog(EditCustomerViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as EditCustomerViewModel;
            if (viewModel != null && await viewModel.SaveAsync())
            {
                DialogResult = true;
                Close();
            }
        }
    }
}
