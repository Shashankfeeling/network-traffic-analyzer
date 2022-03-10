using System.Windows.Forms;
using System.Runtime.InteropServices;
using IronPython.Hosting;
using SharpPcap.LibPcap;
using PacketDotNet;
using SharpPcap;

namespace Network_Traffic_analyzer
{
    public partial class Form1 : Form
    {

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]

        private static extern IntPtr CreateRoundRectRgn
(
    int nLeftRect,
    int nTopRect,
    int nRightRect,
    int nBottomRect,
    int nWidthEllipse,
    int nHeightEllipse
);
        List<LibPcapLiveDevice> interfaceList = new List<LibPcapLiveDevice>();
        int selectedIntIndex;
        LibPcapLiveDevice wifi_device;
        CaptureFileWriterDevice captureFileWriter;
        Dictionary<int, Packet> capturedPackets_list = new Dictionary<int, Packet>();

        int packetNumber = 1;
        string time_str = "", sourceIP = "", destinationIP = "", protocol_type = "";

        bool startCapturingAgain = false;

        Thread sniffing;



        public Form1()
        {
            InitializeComponent();
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 25, 25));
        }
        private void sniffing_Proccess()
        {
            // Open the device for capturing
            int readTimeoutMilliseconds = 1000;
            wifi_device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);

            // Start the capturing process
            if (wifi_device.Opened)
            {
                captureFileWriter = new CaptureFileWriterDevice(wifi_device, Environment.CurrentDirectory + "capture.pcap");
                wifi_device.Capture();
            }
        }

        public void Device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            // dump to a file
            captureFileWriter.Write(e.Packet);


            // start extracting properties for the listview 
            DateTime time = e.Packet.Timeval.Date;
            time_str = (time.Hour + 1) + ":" + time.Minute + ":" + time.Second + ":" + time.Millisecond;
            length = e.Packet.Data.Length.ToString();


            var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);

            // add to the list
            capturedPackets_list.Add(packetNumber, packet);


            var ipPacket = (IpPacket)packet.Extract(typeof(IpPacket));


            if (ipPacket != null)
            {
                System.Net.IPAddress srcIp = ipPacket.SourceAddress;
                System.Net.IPAddress dstIp = ipPacket.DestinationAddress;
                protocol_type = ipPacket.Protocol.ToString();
                sourceIP = srcIp.ToString();
                destinationIP = dstIp.ToString();



                var protocolPacket = ipPacket.PayloadPacket;

                ListViewItem item = new ListViewItem(packetNumber.ToString());
                item.SubItems.Add(time_str);
                item.SubItems.Add(sourceIP);
                item.SubItems.Add(destinationIP);
                item.SubItems.Add(protocol_type);
                item.SubItems.Add(length);


                Action action = () => packetTable.Items.Add(item);
                packetTable.Invoke(action);

                ++packetNumber;
            }
        }
        private void Dashboard_Click(object sender, EventArgs e)
        {

            pnlNav.Height = dashboardButton.Height;
            pnlNav.Top = dashboardButton.Top;
            pnlNav.Left = dashboardButton.Left;
            dashboardButton.BackColor = Color.FromArgb(46, 51, 73);
        }
        private void btnadd_Click(object sender, EventArgs e)
        {
            pnlNav.Height = addIPButton.Height;
            pnlNav.Top = addIPButton.Top;
            pnlNav.Left = addIPButton.Left;
            addIPButton.BackColor = Color.FromArgb(46, 51, 73);
        }
        private void btnloc_Click(object sender, EventArgs e)
        {
            pnlNav.Height = IPlocationButton.Height;
            pnlNav.Top = IPlocationButton.Top;
            pnlNav.Left = IPlocationButton.Left;
            addIPButton.BackColor = Color.FromArgb(46, 51, 73);
        }
        private void btnext_Click(object sender, EventArgs e)
        {
            pnlNav.Height = exitButton.Height;
            pnlNav.Top = exitButton.Top;
            pnlNav.Left = exitButton.Left;
            exitButton.BackColor = Color.FromArgb(46, 51, 73);
        }

        private void Dashboard_leave(object sender, EventArgs e)
        {
            dashboardButton.BackColor = Color.FromArgb(24, 30, 54);
        }
        private void btnadd_leave(object sender, EventArgs e)
        {
            addIPButton.BackColor = Color.FromArgb(24, 30, 54);
        }
        private void btnloc_leave(object sender, EventArgs e)
        {
            IPlocationButton.BackColor = Color.FromArgb(24, 30, 54);
        }
        private void btnext_leave(object sender, EventArgs e)
        {
            exitButton.BackColor = Color.FromArgb(24, 30, 54);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            if (startCapturingAgain == false) //first time 
            {
                System.IO.File.Delete(Environment.CurrentDirectory + "capture.pcap");
                wifi_device.OnPacketArrival += new PacketArrivalEventHandler(Device_OnPacketArrival);
                sniffing = new Thread(new ThreadStart(sniffing_Proccess));
                sniffing.Start();
                startButton.Enabled = false;
                pauseButton.Enabled = true;

            }
            else if (startCapturingAgain)
            {
                if (MessageBox.Show("Your packets are captured in a file. Starting a new capture will override existing ones.", "Confirm", MessageBoxButtons.OK, MessageBoxIcon.Warning) == DialogResult.OK)
                {
                    // user clicked ok
                    System.IO.File.Delete(Environment.CurrentDirectory + "capture.pcap");
                    packetTable.Items.Clear();
                    capturedPackets_list.Clear();
                    packetNumber = 1;
                    wifi_device.OnPacketArrival += new PacketArrivalEventHandler(Device_OnPacketArrival);
                    sniffing = new Thread(new ThreadStart(sniffing_Proccess));
                    sniffing.Start();
                    startButton.Enabled = false;
                    stopButton.Enabled = true;
                }
            }
            startCapturingAgain = true;
        }
    }
}