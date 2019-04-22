using System.IO;
using System.Management;
using System.Net.NetworkInformation;

namespace UMN_OSDFrontEnd {
    class PreFlightCheckers {
        /// <summary>
        /// Checks to see if any profiles have offline files active.
        /// </summary>
        /// <returns>Boolean of true if offline files are detected in any profile on the system.</returns>
        public bool OfflineFilesDetected() {
            bool OfflineFilesFound = false;

            SelectQuery Query = new SelectQuery( "Win32_UserProfile" );
            ManagementObjectSearcher ObjectSearcher = new ManagementObjectSearcher( Query );

            foreach(ManagementObject Profile in ObjectSearcher.Get()) {
                bool? UserProfileRoamingConfigured = Profile["RoamingConfigured"] as bool?;
                if(UserProfileRoamingConfigured == true) {
                    OfflineFilesFound = true;
                }
            }

            return OfflineFilesFound;
        }

        /// <summary>
        /// Checks for the presense of only one physical disk.
        /// </summary>
        /// <returns>Boolean of true if the physical disks detected are equal to or less than the number indicated.  Though it will be false if less then 1.</returns>
        public bool PhysicalDiskCount(int DiskCheckLimit ) {
            int totalFixedDrives = 0;

            foreach(DriveInfo Drive in DriveInfo.GetDrives() ) {
                if(Drive.DriveType == DriveType.Fixed) {
                    totalFixedDrives++;
                }
            }

            if(totalFixedDrives > DiskCheckLimit || totalFixedDrives < 1) {
                return false;
            } else {
                return true;
            }
        }

        /// <summary>
        /// Checks to see if there is an active Ethernet network connection.
        /// </summary>
        /// <returns>Boolean of true if the device has an active Ethernet connection.</returns>
        public bool EthernetNetworkConnectionDetected() {
            bool ActiveEthernet = false;

            foreach(NetworkInterface Interface in NetworkInterface.GetAllNetworkInterfaces()) {
                if(Interface.OperationalStatus == OperationalStatus.Up && (
                    Interface.NetworkInterfaceType == NetworkInterfaceType.Ethernet
                    || Interface.NetworkInterfaceType == NetworkInterfaceType.GigabitEthernet
                    || Interface.NetworkInterfaceType == NetworkInterfaceType.FastEthernetT
                    || Interface.NetworkInterfaceType == NetworkInterfaceType.FastEthernetFx)) {
                    ActiveEthernet = true;
                }
            }

            return ActiveEthernet;
        }

        /// <summary>
        /// Pings a given IP address and returns true if it was successful.
        /// </summary>
        /// <param name="server">The server IP address or hostname.</param>
        /// <returns>Boolean of true if the ping was successful.</returns>
        public bool TestNetworkConnectivity(string server) {
            Ping ServerPing = new Ping();

            PingReply ServerReply = ServerPing.Send( server, 2000 );
            if(ServerReply.Status == IPStatus.Success) {
                return true;
            } else {
                return false;
            }
        }


    }
}
