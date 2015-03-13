using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SOD_CS_Library;

namespace MakeMeMath
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DealWithSoD();
        }

        int projectionAPIId = 1; // TODO: replace with the actual ID of the projection client providing the projection API

        #region Game Engine
        // Questions
        // TODO: questions "repository"

        // "State Machine"
        // TODO: game engine

        // TODO: create datapoints dynamically
        // TODO: send msgs to the server
        
        #endregion

        #region SoD Config

        private void DealWithSoD()
        {
            if (SoD == null)
            {
                configureSoD();
                configureDevice();
                registerSoDEvents();
                connectSoD();
            }
        }
                
        #region SOD parameters
        static SOD_CS_Library.SOD SoD;
        // Device parameters. Set 
        // TODO: FILL THE FOLLOWING VARIABLES AND WITH POSSIBLE VALUES
        static int _deviceID = 0;                   // OPTIONAL. If it's not unique, it will be "randomly" assigned by locator.
        static string _deviceName = "GameEngine";   // You can name your device
        static string _deviceType = "GameEngine";   // Cusomize device
        static bool _deviceIsStationary = true;     // If mobile device, assign false.
        static double _deviceWidthInM = 1          // Device width in metres
                        , _deviceHeightInM = 1.5   // Device height in metres
                        , _deviceLocationX = 8     // Distance in metres from the sensor which was first connected to the server
                        , _deviceLocationY = 8      // Distance in metres from the sensor which was first connected to the server
                        , _deviceLocationZ = 8      // Distance in metres from the sensor which was first connected to the server
                        , _deviceOrientation = 0    // Device orientation in Degrees, if mobile device, 0.
                        , _deviceFOV = 0;           // Device Field of View in degrees


        // observers can let device know who enters/leaves the observe area.
        static string _observerType = "rectangular";
        static double _observeHeight = 1;
        static double _observeWidth = 1;
        static double _observerDistance = 1;
        static double _observeRange = 1;
        /*
         * You can also do Radial type observer. Simply change _observerType to "radial": 
         *      static string _observerType = "radial";
         * Then observeRange will be taken as the radius of the observeRange.
        */

        // SOD connection parameters
        static string _SODAddress = "beastwin.marinhomoreira.com"; // LOCATOR URL or IP
        static int _SODPort = 3000; // Port of LOCATOR
        #endregion
        
        public static void configureSoD()
        {
            // Configure and instantiate SOD object
            string address = _SODAddress;
            int port = _SODPort;
            SoD = new SOD_CS_Library.SOD(address, port);

            // configure and connect
            configureDevice();
        }

        private static void configureDevice()
        {
            // This method takes all the parameters you specified above and set the properties accordingly in the SOD object.
            // Configure device with its dimensions (mm), location in physical space (X, Y, Z in meters, from sensor), orientation (degrees), Field Of View (FOV. degrees) and name
            SoD.ownDevice.SetDeviceInformation(_deviceWidthInM, _deviceHeightInM, _deviceLocationX, _deviceLocationY, _deviceLocationZ, _deviceType, _deviceIsStationary);
            SoD.ownDevice.orientation = _deviceOrientation;
            SoD.ownDevice.FOV = _deviceFOV;
            if (_observerType == "rectangular")
            {
                SoD.ownDevice.observer = new SOD_CS_Library.observer(_observeWidth, _observeHeight, _observerDistance);
            }
            else if (_observerType == "radial")
            {
                SoD.ownDevice.observer = new SOD_CS_Library.observer(_observeRange);
            }

            // Name and ID of device - displayed in Locator
            SoD.ownDevice.ID = _deviceID;
            SoD.ownDevice.name = _deviceName;
        }

        /// <summary>
        /// Connect SOD to Server
        /// </summary>
        public static void connectSoD()
        {
            SoD.SocketConnect();
        }


        /// <summary>
        /// Disconnect SOD from locator.
        /// </summary>
        public static void disconnectSoD()
        {
            SoD.Close();
        }

        /// <summary>
        /// Reconnect SOD to the locator.
        /// </summary>
        public static void reconnectSoD()
        {
            SoD.ReconnectToServer();
        }


        #endregion

        #region SoD Events

        private static void registerSoDEvents()
        {
            // register for 'connect' event with io server
            // SOD Default Events, 
            SoD.On("connect", (data) =>
            {
                Console.WriteLine("\r\nConnected...");
                Console.WriteLine("Registering with server...\r\n");
                SoD.RegisterDevice();  //register the device with server everytime it connects or re-connects
            });

            // Sample event handler for when any device connects to server
            SoD.On("someDeviceConnected", (msgReceived) =>
            {
                Console.WriteLine("Some device connected to server: " + msgReceived.data);
            });

            // listener for event a person walks into a device
            SoD.On("enterObserveRange", (msgReceived) =>
            {
                // Parse the message 
                Console.WriteLine(" person " + msgReceived.data["payload"]["invader"] + " enter " + msgReceived.data["payload"]["observer"]["type"] + ": " + msgReceived.data["payload"]["observer"]["ID"]);
            });

            // listener for event a person grab in the observeRange of another instance.
            SoD.On("grabInObserveRange", (msgReceived) =>
            {
                Console.WriteLine(" person " + msgReceived.data["payload"]["invader"] + " perform Grab gesture in a " + msgReceived.data["payload"]["observer"]["type"] + ": " + msgReceived.data["payload"]["observer"]["ID"]);
            });

            // listener for event a person leaves a device.
            SoD.On("leaveObserveRange", (msgReceived) =>
            {
                Console.WriteLine(" person " + msgReceived.data["payload"]["invader"] + " leaves " + msgReceived.data["payload"]["observer"]["type"] + ": " + msgReceived.data["payload"]["observer"]["ID"]);
            });

            // Sample event handler for when any device disconnects from server
            SoD.On("someDeviceDisconnected", (msgReceived) =>
            {
                Console.WriteLine("Some device disconnected from server : " + msgReceived.data["name"]);
            });
            // END SOD default events

            // SOD custom events, change "" to the events you want to listen too. msgReceived is the callback message 
            SoD.On("", (msgReceived) =>
            {
                Console.WriteLine(msgReceived);
            });

        }

        #endregion

        #region Remote I/O

        private void SendMsg(string msg)
        {
            // TODO: Change this example with the actual values and create new methods based on the messages to be sent to the projection client.
            Dictionary<string, string> payload = new Dictionary<string, string>();
            payload.Add("X", "135");
            payload.Add("Y", "135");
            payload.Add("question", "2+2 = ?");
            SoD.SendToDevices.WithID(projectionAPIId, "projectSomething", payload);
        }

        #endregion
    }
}
