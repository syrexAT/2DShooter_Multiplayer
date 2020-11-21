using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;


public class Client : MonoBehaviour
{
    public static Client instance;
    public static int dataBufferSize = 4096;

    public string ip = "127.0.0.1";
    public int port = 26950;
    public int myId = 0;
    public TCP tcp;

    private delegate void PacketHandler(Packet _packet);
    private static Dictionary<int, PacketHandler> packetHandlers;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance destroyed");
            Destroy(this);
        }
    }

    private void Start()
    {
        tcp = new TCP();
    }

    public void ConnectToServer()
    {
        InitializeClientData();

        tcp.Connect();
    }
    
    public class TCP
    {
        public TcpClient socket;

        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        public void Connect()
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            receiveBuffer = new byte[dataBufferSize];
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
        }

        private void ConnectCallback(IAsyncResult _result)
        {
            socket.EndConnect(_result);

            if (!socket.Connected)
            {
                return;
            }

            stream = socket.GetStream();

            receivedData = new Packet();

            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }

        public void SendData(Packet _packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error sending data to server via TCP: {_ex}");
                throw;
            }
        } 

        private void ReceiveCallback(IAsyncResult _result)
        {
            //Try catch so errors dont cause crash
            try
            {
                int _byteLength = stream.EndRead(_result); //in order to receive data, we need to call endread method which returns an int representing the number of bytes we read from the stream
                if (_byteLength <= 0)
                {
                    //DISCONNECT
                    return;
                }
                //If we have received data, we create new array witht length of bytelength and copy the received bytes into the new array, after that we need to handle the data
                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);

                receivedData.Reset(HandleData(_data));
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch (Exception _ex)
            {
                //TODO Disconnectg
            }
        }

        private bool HandleData(byte[] _data)
        {
            int _packetLength = 0;

            receivedData.SetBytes(_data);

            if (receivedData.UnreadLength() >= 4) //if it does we have the start of one of our packets
            {
                _packetLength = receivedData.ReadInt(); //store that length
                if (_packetLength <= 0)
                {
                    return true; //because in that case we want to reset receivedData
                }
            }

            while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength()) //checks if the packet length is greater than 0 but less than the unread bytes in receiveddata, as long as this while is running it means that receiveddata contains another completet packet which we can handle
            {
                byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                //because the code wont run on the same thread, we call this function and create a new packet and read out its ID
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        packetHandlers[_packetId](_packet);
                    }
                });

                _packetLength = 0;
                if (receivedData.UnreadLength() >= 4) //if it does we have the start of one of our packets
                {
                    _packetLength = receivedData.ReadInt(); //store that length
                    if (_packetLength <= 0)
                    {
                        return true; //because in that case we want to reset receivedData
                    }
                }
            }

            if (_packetLength <= 1)
            {
                return true;
            }

            return false;
        }
    
    }

    private void InitializeClientData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            {(int)ServerPackets.welcome, ClientHandle.Welcome }
        };

        Debug.Log("Initilaized packets.");
    }
}
