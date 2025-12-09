using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using SnookerGameManagementSystem.Services;

namespace SnookerGameManagementSystem.Views
{
    public partial class LicenseToolWindow : Window
    {
        private List<string> _macAddresses = new List<string>();

        public LicenseToolWindow()
        {
            InitializeComponent();
            LoadMacAddresses();
        }

        private void LoadMacAddresses()
        {
            try
            {
                var licenseService = new LicenseValidationService("");
                _macAddresses = licenseService.GetMacAddresses();

                MacAddressPanel.Children.Clear();

                if (_macAddresses.Count == 0)
                {
                    var noMacText = new TextBlock
                    {
                        Text = "? No MAC addresses found on this machine.",
                        Foreground = Brushes.Red,
                        FontSize = 14,
                        Margin = new Thickness(0, 10, 0, 0)
                    };
                    MacAddressPanel.Children.Add(noMacText);
                    return;
                }

                var headerText = new TextBlock
                {
                    Text = $"Found {_macAddresses.Count} network adapter(s):",
                    FontWeight = FontWeights.Bold,
                    FontSize = 14,
                    Margin = new Thickness(0, 0, 0, 15)
                };
                MacAddressPanel.Children.Add(headerText);

                for (int i = 0; i < _macAddresses.Count; i++)
                {
                    var mac = _macAddresses[i];
                    var licenseKey = LicenseValidationService.GenerateLicenseKey(mac);

                    var macPanel = new StackPanel
                    {
                        Margin = new Thickness(0, 0, 0, 20)
                    };

                    var adapterText = new TextBlock
                    {
                        FontWeight = FontWeights.Bold,
                        FontSize = 13,
                        Foreground = new SolidColorBrush(Color.FromRgb(33, 150, 243))
                    };
                    adapterText.Inlines.Add(new Run($"Adapter #{i + 1}"));
                    macPanel.Children.Add(adapterText);

                    var macText = new TextBlock
                    {
                        FontSize = 12,
                        Margin = new Thickness(0, 5, 0, 0)
                    };
                    macText.Inlines.Add(new Run("MAC Address: ") { FontWeight = FontWeights.SemiBold });
                    macText.Inlines.Add(new Run(mac) { FontFamily = new FontFamily("Consolas"), FontSize = 13 });
                    macPanel.Children.Add(macText);

                    var keyText = new TextBlock
                    {
                        FontSize = 11,
                        Margin = new Thickness(0, 3, 0, 0),
                        Foreground = Brushes.Gray
                    };
                    keyText.Inlines.Add(new Run("License Key: ") { FontWeight = FontWeights.SemiBold });
                    keyText.Inlines.Add(new Run(licenseKey) { FontFamily = new FontFamily("Consolas") });
                    macPanel.Children.Add(keyText);

                    var instructionText = new TextBlock
                    {
                        Text = "?? Copy this MAC address to appsettings.json ? License:MacAddress",
                        FontSize = 11,
                        FontStyle = FontStyles.Italic,
                        Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80)),
                        Margin = new Thickness(0, 5, 0, 0)
                    };
                    macPanel.Children.Add(instructionText);

                    var separator = new Border
                    {
                        Height = 1,
                        Background = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                        Margin = new Thickness(0, 15, 0, 0)
                    };
                    macPanel.Children.Add(separator);

                    MacAddressPanel.Children.Add(macPanel);
                }

                var configExample = new TextBlock
                {
                    Margin = new Thickness(0, 10, 0, 0),
                    FontFamily = new FontFamily("Consolas"),
                    FontSize = 11,
                    Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                    Padding = new Thickness(10)
                };
                configExample.Inlines.Add(new Run("Example appsettings.json configuration:\n\n") { FontWeight = FontWeights.Bold });
                configExample.Inlines.Add(new Run("{\n"));
                configExample.Inlines.Add(new Run("  \"License\": {\n"));
                configExample.Inlines.Add(new Run($"    \"MacAddress\": \"{_macAddresses[0]}\"\n"));
                configExample.Inlines.Add(new Run("  }\n"));
                configExample.Inlines.Add(new Run("}"));

                MacAddressPanel.Children.Add(configExample);
            }
            catch (Exception ex)
            {
                var errorText = new TextBlock
                {
                    Text = $"? Error loading MAC addresses:\n{ex.Message}",
                    Foreground = Brushes.Red,
                    FontSize = 12,
                    TextWrapping = TextWrapping.Wrap
                };
                MacAddressPanel.Children.Clear();
                MacAddressPanel.Children.Add(errorText);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadMacAddresses();
            MessageBox.Show(
                "MAC addresses refreshed successfully!",
                "Refreshed",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (_macAddresses.Count == 0)
            {
                MessageBox.Show(
                    "No MAC addresses available to copy.",
                    "Nothing to Copy",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("MAC Addresses for License Configuration:");
            sb.AppendLine();
            
            for (int i = 0; i < _macAddresses.Count; i++)
            {
                sb.AppendLine($"Adapter #{i + 1}:");
                sb.AppendLine($"MAC Address: {_macAddresses[i]}");
                sb.AppendLine($"License Key: {LicenseValidationService.GenerateLicenseKey(_macAddresses[i])}");
                sb.AppendLine();
            }

            sb.AppendLine("Example appsettings.json:");
            sb.AppendLine("{");
            sb.AppendLine("  \"License\": {");
            sb.AppendLine($"    \"MacAddress\": \"{_macAddresses[0]}\"");
            sb.AppendLine("  }");
            sb.AppendLine("}");

            Clipboard.SetText(sb.ToString());

            MessageBox.Show(
                "MAC address information copied to clipboard!",
                "Copied",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
