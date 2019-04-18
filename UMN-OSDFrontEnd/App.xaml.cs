using System.Xml;
using System.Windows;

namespace UMN_OSDFrontEnd {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private XmlDocument xmlAppSettings = new XmlDocument();
        public XmlNodeList xmlAppSettingsNodes;

        private void Application_Startup( object sender, StartupEventArgs e ) {
            
        }
    }
}
