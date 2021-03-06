﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Media;

namespace chat
{
    //Thx to "Csharp Tutorials" at https://youtu.be/X16IyNbcAr0

    public partial class Form1 : Form
    {
        private TcpClient client; //Listens for connections from TCP network clients.
        public StreamReader STR;
        public StreamWriter STW;
        public string recieve;
        public string textToSend;
        private SoundPlayer _soundPlayer;

        // KNAS?
        public Form1()
        {
            InitializeComponent();
            //The name localhost normally resolves to the IPv4 loopback address 127.0.0.1, and to the IPv6 loopback address ::1
            IPAddress[] localIP = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress adress in localIP)
            {
                //Checks for ipV4
                if (adress.AddressFamily == AddressFamily.InterNetwork)
                {
                    edtServerIP.Text = adress.ToString();
                }
            }
            _soundPlayer = new SoundPlayer("Login.wav");
        }

        private void btnServerStart_Click(object sender, EventArgs e)
        {
            try
            {
                //Check with ex Wireshark 
                TcpListener listener = new TcpListener(IPAddress.Any, int.Parse(edtServerPort.Text));
                listener.Start();
                this.BackColor = Color.Green; 
                this.Update();
                redtHistory.AppendText("Server started" + "\n");
                redtHistory.Update();
                client = listener.AcceptTcpClient(); //Accept a pending connection request 
                STR = new StreamReader(client.GetStream());
                STW = new StreamWriter(client.GetStream());
                STW.AutoFlush = true;
                backgroundWorker1.RunWorkerAsync();
                backgroundWorker2.WorkerSupportsCancellation = true;
                _soundPlayer.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void btnClientStart_Click(object sender, EventArgs e)
        {
            try
            {
                client = new TcpClient();

                //Represents a network endpoint as an IP address and a port number.
                IPEndPoint ipEnd = new IPEndPoint(IPAddress.Parse(edtClientIP.Text), int.Parse(edtClientPort.Text));

                //Connects the client to a remote TCP host using the specified host name and port number.
                client.Connect(ipEnd);
                if (client.Connected)
                {
                    this.BackColor = Color.Green;
                    this.Update();
                    redtHistory.AppendText("Connected to Server" + "\n");
                    redtHistory.Update();
                    STR = new StreamReader(client.GetStream());
                    STW = new StreamWriter(client.GetStream());
                    STW.AutoFlush = true;
                    backgroundWorker1.RunWorkerAsync();
                    backgroundWorker2.WorkerSupportsCancellation = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            while (client.Connected)
            {
                this.redtHistory.Invoke(new MethodInvoker(delegate ()
                {
                    redtHistory.Enabled = true;
                }));
                this.edtToSend.Invoke(new MethodInvoker(delegate ()
                {
                    edtToSend.Enabled = true;
                }));
                this.btnSend.Invoke(new MethodInvoker(delegate ()
                {
                    btnSend.Enabled = true;
                }));

                try
                {
                    recieve = STR.ReadLine();
                    this.redtHistory.Invoke(new MethodInvoker(delegate ()
                    {
                        redtHistory.AppendText("You:" + recieve + "\n");
                    }));
                    recieve = "";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            if (client.Connected)
            {
                STW.WriteLine(textToSend);
                this.redtHistory.Invoke(new MethodInvoker(delegate ()
                {
                    redtHistory.AppendText("Me:" + textToSend + "\n");
                }));
            }
            else
            {
                MessageBox.Show("Sending failed");
            }
            backgroundWorker2.CancelAsync();

        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (edtToSend.Text != "")
            {
                textToSend = edtToSend.Text;
                backgroundWorker2.RunWorkerAsync();
            }
            edtToSend.Text = "";
        }
    }
}
