using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    class ServerSend
    {
        private static void SendTCPData(int _toClient, Packet _packet)
        {
            //This method is for preparing the packet to be send
            _packet.WriteLength();
            Server.clients[_toClient].tcp.SendData(_packet);
        }

        //send packet to all connected clients
        private static void SendTCPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i < Server.MaxPlayers; i++)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }

        private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i < Server.MaxPlayers; i++)
            {
                if (i != _exceptClient)
                {
                    Server.clients[i].tcp.SendData(_packet);
                }
            }
        }


        //sends packet to all connected clients except 1

        public static void Welcome(int _toClient, string _msg) //to which client and which message
        {
            //inheriting from IDisposable, we need to make sure we dispose it when we are done with it, either packet dispose method at the end
            //or define packet instance inside a using block --> its cleaner
            using (Packet _packet = new Packet((int)ServerPackets.welcome))
            {
                _packet.Write(_msg);
                _packet.Write(_toClient);

                SendTCPData(_toClient, _packet);
            }
        }
    }
}
