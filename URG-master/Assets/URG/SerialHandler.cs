using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System.Threading;
using SCIP_library;
using System;
using System.Collections.Generic;
 
public class SerialHandler : MonoBehaviour
{
    public string portName = "COM2";
    public int baudRate = 9600;
 
    private SerialPort serialPort_;
    private Thread thread_;
    private bool isRunning_ = false;
 
    private string message_;
    private bool isNewMessageReceived_ = false;
 
    void Start()
    {
        Debug.LogWarning("Start");
        Open();
    }
 
    void Update()
    {
        //Debug.LogWarning("Serial-Update");
        if (isNewMessageReceived_)
        {
            //Main();
            OnDataReceived(message_);
        }
    }
    void OnDestroy()
    {
        Debug.LogWarning("OnDestroy");
        Close();
    }
 
    private void Open()
    {
        serialPort_ = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);
        serialPort_.Open();
 
        isRunning_ = true;
 
        thread_ = new Thread(Read);
        thread_.Start();
    }
 
    private void Read()
    {
        Debug.LogWarning("Read1");
        while (isRunning_ && serialPort_ != null && serialPort_.IsOpen)
        {
        Debug.LogWarning("Read2");
            try
            {
                message_ = serialPort_.ReadLine();
//                Debug.LogWarning(message_);
                isNewMessageReceived_ = true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }
    }
 
    private void Close()
    {
        isRunning_ = false;
 
        if (thread_ != null && thread_.IsAlive)
        {
            thread_.Join();
        }
 
        if (serialPort_ != null && serialPort_.IsOpen)
        {
            serialPort_.Close();
            serialPort_.Dispose();
        }
    }
 
    void OnDataReceived(string message)
    {
        Debug.LogWarning("OnDataReceived1");
        var data = message.Split(
                new string[] { "\t" }, System.StringSplitOptions.None);
        if (data.Length < 2) return;
 
        try
        {
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);
        }
    }
 
    static void Main()
    {
        const int GET_NUM = 10;
        const int start_step = 0;
        const int end_step = 760;
        try {
            string port_name;
            int baudrate;
            get_serial_information(out port_name, out baudrate);

            SerialPort urg = new SerialPort(port_name, baudrate);
            urg.NewLine = "\n\n";

            urg.Open();

            urg.Write(SCIP_Writer.SCIP2());
            urg.ReadLine(); // ignore echo back
            urg.Write(SCIP_Writer.MD(start_step, end_step));
            urg.ReadLine(); // ignore echo back

            List<long> distances = new List<long>();
            long time_stamp = 0;
            for (int i = 0; i < GET_NUM; ++i) {
                string receive_data = urg.ReadLine();
                if (!SCIP_Reader.MD(receive_data, ref time_stamp, ref distances)) {
                    Console.WriteLine(receive_data);
                    break;
                }
                if (distances.Count == 0) {
                    Console.WriteLine(receive_data);
                    continue;
                }
                // show distance data
                Console.WriteLine("time stamp: " + time_stamp.ToString() + " distance[100] : " + distances[100].ToString());
            }

            urg.Write(SCIP_Writer.QT()); // stop measurement mode
            urg.ReadLine(); // ignore echo back
            urg.Close();
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
        } finally {
            Console.WriteLine("Press any key.");
            Console.ReadKey();
        }
    }

    private static void get_serial_information(out string port_name, out int baudrate)
    {
        port_name = "COM4";
        baudrate = 115200;
        Console.WriteLine("Please enter port name. [default: " + port_name + "]");
        string str = Console.ReadLine();
        if (str != "") {
            port_name = str;
        }
        Console.WriteLine("Please enter baudrate. [default: " + baudrate.ToString() + "]");
        str = Console.ReadLine();
        if (str != "") {
            baudrate = int.Parse(str);
        }
        Console.WriteLine("Connect setting = Port name : " + port_name + " Baudrate : " + baudrate.ToString());
    }
}