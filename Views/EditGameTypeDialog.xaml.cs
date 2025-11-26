using System.Windows;
using SnookerGameManagementSystem.ViewModels;

namespace SnookerGameManagementSystem.Views
{
    public partial class EditGameTypeDialog : Window
    {
        public EditGameTypeDialog(EditGameTypeViewModel viewModel)
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
            var viewModel = DataContext as EditGameTypeViewModel;
            if (viewModel != null && await viewModel.SaveAsync())
            {
                DialogResult = true;
                Close();
            }
        }
    }
}
