using System.Windows;
using SnookerGameManagementSystem.ViewModels;

namespace SnookerGameManagementSystem.Views
{
    public partial class CustomerManagementWindow : Window
    {
        public CustomerManagementWindow(CustomerManagementViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            
            // Ensure proper cleanup
            Closed += (s, e) =>
            {
                DataContext = null;
            };
        }
    }
}
