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
            
            // Debug logging for window lifecycle
            Loaded += (s, e) => System.Diagnostics.Debug.WriteLine($"[DashboardWindow] Loaded event fired");
            Activated += (s, e) => System.Diagnostics.Debug.WriteLine($"[DashboardWindow] Activated event fired");
            Deactivated += (s, e) => System.Diagnostics.Debug.WriteLine($"[DashboardWindow] Deactivated event fired");
            Closing += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"[DashboardWindow] Closing event fired");
                System.Diagnostics.Debug.WriteLine($"[DashboardWindow] StackTrace: {Environment.StackTrace}");
            };
            Closed += (s, e) => System.Diagnostics.Debug.WriteLine($"[DashboardWindow] Closed event fired");
            
            System.Diagnostics.Debug.WriteLine($"[DashboardWindow] Constructor completed: {this.GetHashCode()}");
        }
    }
}
