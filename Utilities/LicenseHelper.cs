using System;
using System.Windows;
using SnookerGameManagementSystem.Services;

namespace SnookerGameManagementSystem.Utilities
{
    public static class LicenseHelper
    {
        /// <summary>
        /// Shows current machine's MAC addresses - useful for generating client licenses
        /// </summary>
        public static void ShowCurrentMacAddresses()
        {
            var licenseService = new LicenseValidationService("");
            var macAddresses = licenseService.GetMacAddresses();

            if (macAddresses.Count == 0)
            {
                MessageBox.Show(
                    "No MAC addresses found on this machine.",
                    "MAC Address Info",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var message = "MAC Addresses on this machine:\n\n";
            foreach (var mac in macAddresses)
            {
                message += $"• {mac}\n";
                message += $"  License Key: {LicenseValidationService.GenerateLicenseKey(mac)}\n\n";
            }

            message += "\nCopy one of these MAC addresses to the appsettings.json\n";
            message += "under License:MacAddress for client distribution.";

            MessageBox.Show(
                message,
                "MAC Address Information",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
