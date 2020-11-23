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
    public UDP udp;

    private bool isConnected = false;


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
        udp = new UDP();
    }

    private void OnApplicationQuit()
    {
        //unity doesnt really close connection unless you reenter play mode
        Disconnect();
    }

    public void ConnectToServer()
    {
        InitializeClientData();

        isConnected = true;

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
                    instance.tcp.Disconnect();
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
                Disconnect();
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

        private void Disconnect()
        {
            instance.Disconnect();

            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
    
    }

    public class UDP
    {
        public UdpClient socket;
        public IPEndPoint endPoint;

        public UDP()
        {
            endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
        }

        //connectg method with local port number, //different from server port number
        public void Connect(int _localPort)
        {
            socket = new UdpClient(_localPort); //bind local port to udp client

            socket.Connect(endPoint);
            socket.BeginReceive(ReceiveCallback, null);

            //creating new packet and instantly sending it, this packet purpose is to initiate the connectio nwith server and open up local port, so client can receive messages

            using (Packet _packet = new Packet())
            {
                SendData(_packet); //no need to write id to packet as  senddata does it for us
            }
        }


        public void SendData(Packet _packet)
        {
            try
            {
                //insert client id into the packet because we will reuse the value on the server to determine who sent it
                //beacuse of the way UDP works we cant give every client their own UDP client instance on the server, because of problems with ports being closed, typically only 1 udp client is used on the server
                //all udp communication is handled by a single udp client isntance, unless we include the client id the server wont have a way to determine who sent the packet
                _packet.InsertInt(instance.myId); //inserting client id into the packet
                if (socket != null)
                {
                    socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                }
            }
            catch (Exception _ex)
            {

                Debug.Log($"Error sending data to server via UDP: {_ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                byte[] _data = socket.EndReceive(_result, ref endPoint);

                //call socket beginreceive
                socket.BeginReceive(ReceiveCallback, null);

                if (_data.Length < 4) //make sure that an actual packet is to handle before handling the data
                {
                    instance.Disconnect();
                    return;
                    
                }

                HandleData(_data);
            }
            catch (Exception)
            {
                Disconnect();
            }
        }

        private void HandleData(byte[] _data)
        {
            using (Packet _packet = new Packet(_data))
            {
                int _packetLength = _packet.ReadInt(); //this removes the first 4 bytes from the array, which represent the length of the packet
                _data = _packet.ReadBytes(_packetLength);
            }

            ThreadManager.ExecuteOnMainThread(() => //inside function we create new packet with new shortened byte array
            {
                using (Packet _packet = new Packet(_data))
                {
                    int _packetId = _packet.ReadInt();
                    packetHandlers[_packetId](_packet);
                }
            });
        }

        private void Disconnect()
        {
            instance.Disconnect();

            endPoint = null;
            socket = null;
        }
    }
    private void InitializeClientData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            {(int)ServerPackets.welcome, ClientHandle.Welcome },
            {(int)ServerPackets.spawnPlayer, ClientHandle.SpawnPlayer },
            {(int)ServerPackets.playerPosition, ClientHandle.PlayerPosition },
            {(int)ServerPackets.playerRotation, ClientHandle.PlayerRotation },
        };

        Debug.Log("Initilaized packets.");
    }

    private void Disconnect()
    {
        if (isConnected)
        {
            isConnected = false;
            tcp.socket.Close();
            udp.socket.Close();

            Debug.Log("Disconnected from server.");
        }
    }
}
