﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace GameServer
{
    class Client
    {
        public static int dataBufferSize = 4096; 
        public int id;
        public TCP tcp;

        public Client(int _clientId)
        {
            id = _clientId;
            tcp = new TCP(id);
        }

        public class TCP
        {
            public TcpClient socket; //will store the instance which we get from serverconnectcallback
            private readonly int id;
            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;

            public TCP(int _id)
            {
                id = _id;
            }

            public void Connect(TcpClient _socket)
            {
                socket = _socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                stream = socket.GetStream();

                receivedData = new Packet();

                receiveBuffer = new byte[dataBufferSize];

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                ServerSend.Welcome(id, "Welcome to the server!");
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
                    Console.WriteLine($"Error sending data to player {id} via TCP: {_ex}");
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

                    Console.WriteLine($"Error receiving TCP data: {_ex}");
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
                            Server.packetHandlers[_packetId](id, _packet);
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
    }
}
