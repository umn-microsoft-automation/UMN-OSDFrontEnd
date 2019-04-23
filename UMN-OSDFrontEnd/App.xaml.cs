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

        private void Application_DispatcherUnhandledException( object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e ) {
            MessageBox.Show( "An unhandled exception just occurred: " + e.Exception.Message, "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Warning );
            MessageBox.Show( e.Exception.StackTrace, e.Exception.Message, MessageBoxButton.OK, MessageBoxImage.Error );
            e.Handled = true;
        }
    }
}
