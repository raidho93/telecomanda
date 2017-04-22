using SlimDX.DirectInput;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace telecomanda
{
    public partial class Form1 : Form
    {
        SerialPort port;
        string scom;
        string output;
        string oldoutput;

        public Form1()
        {
            InitializeComponent();
            Start();
        }

        private void Start()
        {
            connectbluetooth();

            // Initialize DirectInput
            var directInput = new DirectInput();

            // Find a Joystick Guid
            var joystickGuid = Guid.Empty;

            foreach (var deviceInstance in directInput.GetDevices(DeviceType.Gamepad,
                        DeviceEnumerationFlags.AllDevices))
                joystickGuid = deviceInstance.InstanceGuid;

            // If Joystick not found, throws an error
            if (joystickGuid == Guid.Empty)
            {
                Console.WriteLine("No Gamepad found.");
                Console.ReadKey();
                Environment.Exit(1);
            }

            // Instantiate the joystick
            var joystick = new Joystick(directInput, joystickGuid);

            Console.WriteLine("Found Joystick/Gamepad with GUID: {0}", joystickGuid);

            // Query all suported ForceFeedback effects
            var allEffects = joystick.GetEffects();
            foreach (var effectInfo in allEffects)
                Console.WriteLine("Effect available {0}", effectInfo.Name);

            // Set BufferSize in order to use buffered data.
            joystick.Properties.BufferSize = 128;

            // Acquire the joystick
            joystick.Acquire();

            // Poll events from joystick
            while (true)
            {
                joystick.Poll();
                var datas = joystick.GetBufferedData();
                foreach (var state in datas)
                {
                    if (state.Y != 0)
                    {
                        ControllEntities.acceleratie = 100 - Convert.ToInt32(state.Y / 655.36);
                    }

                    if (state.X != 0)
                    {
                        ControllEntities.rotire = Convert.ToInt32(state.X / 655.36);
                    }

                    if (state.RotationY != 0)
                    {
                        ControllEntities.pitch = 100 - Convert.ToInt32(state.RotationY / 655.36);
                    }

                    if (state.RotationX != 0)
                    {
                        ControllEntities.roll = Convert.ToInt32(state.RotationX / 655.36);
                    }

                    bool[] buttons = state.GetButtons();


                    ControllEntities.X = buttons[2];
                    ControllEntities.Y = buttons[3];
                    ControllEntities.A = buttons[0];
                    ControllEntities.B = buttons[1];
                    ControllEntities.LB = buttons[4];
                    ControllEntities.RB = buttons[5];
                    ControllEntities.Start = buttons[7];
                    ControllEntities.Back = buttons[6];
                    ControllEntities.RPress = buttons[9];
                    ControllEntities.LPress = buttons[8];
                }

                output = ControllEntities.acceleratie + " " 
                    + ControllEntities.rotire + " " 
                    + ControllEntities.pitch + " " 
                    + ControllEntities.roll + " "
                    + Convert.ToByte(ControllEntities.X) + " "
                    + Convert.ToByte(ControllEntities.Y) + " "
                    + Convert.ToByte(ControllEntities.A) + " "
                    + Convert.ToByte(ControllEntities.B) + " "
                    + Convert.ToByte(ControllEntities.LB) + " "
                    + Convert.ToByte(ControllEntities.RB) + " "
                    + Convert.ToByte(ControllEntities.Start) + " "
                    + Convert.ToByte(ControllEntities.Back) + " "
                    + Convert.ToByte(ControllEntities.RPress) + " "
                    + Convert.ToByte(ControllEntities.LPress) +"~";
              
                if(output!= oldoutput)
                {
                    oldoutput = output;
                    send(output);
                    //Console.WriteLine(output);
                }

                Thread.Sleep(50);
            }
        }

        bool connectbluetooth()
        {
            try
            {
                //if (listBox1.SelectedIndex != 0)
                {
                    scom = "COM3"; //listBox1.SelectedItem.ToString();
                    port = new SerialPort(scom, 9600);
                    port.Open();
                    Console.WriteLine("CONNECTION OPEN SUCCESSFUL");
                    return true;
                }
                //else
                //    MessageBox.Show("Please select com port", "Missing port", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //return false;
            }
            catch (Exception a)
            {
                if (DialogResult.Retry == MessageBox.Show(a.Message, "problem occured", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Warning))
                    connectbluetooth();
                else
                    return false;
                return false;
            }
        }

        bool send(string text)
        {
            try
            {
                port.WriteTimeout = 10000;//define how much time wait for send data
                port.Write(text);
                return true;
            }
            catch (Exception a)
            {
                if (DialogResult.Retry == MessageBox.Show(a.Message, "Problem occured", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Warning))
                    send(text);
                else
                    return false;
                return false;
            }
        }
    }
}
