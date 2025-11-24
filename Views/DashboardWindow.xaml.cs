using System.Windows;
using SnookerGameManagementSystem.ViewModels;

namespace SnookerGameManagementSystem.Views
{
    public partial class DashboardWindow : Window
    {
        public DashboardWindow(DashboardViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
