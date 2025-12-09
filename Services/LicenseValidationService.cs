using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

namespace SnookerGameManagementSystem.Services
{
    public class LicenseValidationService
    {
        private readonly string _licensedMacAddress;

        public LicenseValidationService(string licensedMacAddress)
        {
            _licensedMacAddress = licensedMacAddress;
        }

        /// <summary>
        /// Validates if the current machine's MAC address matches the licensed MAC address
        /// </summary>
        public bool ValidateLicense()
        {
            try
            {
                var currentMacAddresses = GetMacAddresses();
                
                if (string.IsNullOrWhiteSpace(_licensedMacAddress))
                {
                    return false;
                }

                // Normalize and compare MAC addresses
                var normalizedLicensedMac = NormalizeMacAddress(_licensedMacAddress);
                
                foreach (var mac in currentMacAddresses)
                {
                    var normalizedCurrentMac = NormalizeMacAddress(mac);
                    if (normalizedCurrentMac == normalizedLicensedMac)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LicenseValidation] Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets all MAC addresses from the current machine
        /// </summary>
        public List<string> GetMacAddresses()
        {
            var macAddresses = new List<string>();

            try
            {
                foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    // Only get physical adapters that are operational
                    if (nic.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                        nic.OperationalStatus == OperationalStatus.Up)
                    {
                        var mac = nic.GetPhysicalAddress().ToString();
                        if (!string.IsNullOrWhiteSpace(mac) && mac != "000000000000")
                        {
                            macAddresses.Add(FormatMacAddress(mac));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LicenseValidation] Error getting MAC addresses: {ex.Message}");
            }

            return macAddresses;
        }

        /// <summary>
        /// Formats MAC address with colons (e.g., AA:BB:CC:DD:EE:FF)
        /// </summary>
        private string FormatMacAddress(string mac)
        {
            if (string.IsNullOrWhiteSpace(mac))
                return string.Empty;

            mac = mac.Replace(":", "").Replace("-", "").ToUpper();
            
            if (mac.Length == 12)
            {
                return string.Join(":", Enumerable.Range(0, 6)
                    .Select(i => mac.Substring(i * 2, 2)));
            }

            return mac;
        }

        /// <summary>
        /// Normalizes MAC address for comparison (removes special characters and converts to uppercase)
        /// </summary>
        private string NormalizeMacAddress(string mac)
        {
            return mac.Replace(":", "").Replace("-", "").Replace(" ", "").ToUpper();
        }

        /// <summary>
        /// Generates a license key based on MAC address (for distribution to clients)
        /// </summary>
        public static string GenerateLicenseKey(string macAddress)
        {
            var normalized = macAddress.Replace(":", "").Replace("-", "").Replace(" ", "").ToUpper();
            
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(normalized + "SnookerGame2024"));
                return Convert.ToBase64String(hash).Substring(0, 32);
            }
        }
    }
}
