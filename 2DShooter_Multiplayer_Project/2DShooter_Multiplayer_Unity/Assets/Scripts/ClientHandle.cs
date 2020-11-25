using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet _packet)
    {
        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();

        Debug.Log($"Message from server: {_msg}");
        Client.instance.myId = _myId;
        ClientSend.WelcomeReceived();

        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void SpawnPlayer(Packet _packet)
    {
        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();

        GameManager.instance.SpawnPlayer(_id, _username, _position, _rotation);
    }

    public static void PlayerPosition(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        GameManager.players[_id].transform.position = _position;
    }

    public static void PlayerRotation(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Quaternion _rotation = _packet.ReadQuaternion();

        GameManager.players[_id].transform.rotation = _rotation;
    }

    public static void PlayerDisconnected(Packet _packet)
    {
        int _id = _packet.ReadInt();

        Destroy(GameManager.players[_id].gameObject);
        GameManager.players.Remove(_id);
    }

    //[System.Serializable]
    //class CPlayerHealth
    //{
    //    public int id;
    //    public float health;

    //    public CPlayerHealth(int id, float health) { this.id = id; this.health = health; }

    //    public class Serializer<T> where T : ISerializable
    //    {
    //        public byte[] toBytes(T instance)
    //        {
    //            var binaryFormatter = new BinaryFormatter();
    //            var memoryStream = new MemoryStream();
    //            binaryFormatter.Serialize(memoryStream, instance);
    //            return memoryStream.ToArray();
    //        }

    //        public static object fromBytes(byte[] d)
    //        {
    //            List<T> foo = ...;

    //            return (T)object;
    //        }

    //    }


    //    public void writeToPacket<T>T instance)
    //    {
    //        doSomething(Serializer<T>.fromBytes(new byte[] { 1 }));
    //    }

    //    public void doSomething(string s)
    //    {
    //        // ...
    //    }

    //    public void doSomething(object o)
    //    {
    //        // ...
    //    }

    //    public void doSomething(CPlayerHealth cph)
    //    {
    //        //. ...
            
    //    }

    //    public static CPlayerHealth readFromPacket(Packet _packet)
    //    {
    //       return new CPlayerHealth(_packet.ReadInt(), _packet.ReadFloat());
    //        _packet.Write(new CPlayerHealth(...));
    //        _packet.Read<CPlayerHealth>();
    //    }
    //}

    public static void PlayerHealth(Packet _packet)
    {

        int _id = _packet.ReadInt();
        float _health = _packet.ReadFloat();
        GameManager.players[_id].SetHealth(_health);

        //var foo = new CPlayerHealth(123, 123.0f);
        //byte[] d = foo.toBytes();

        //var health = CPlayerHealth.readFromPacket(_packet);

        //GameManager.players[_id].SetHealth(health.health);
    }
    
    public static void PlayerRespawned(Packet _packet)
    {
        int _id = _packet.ReadInt();
        GameManager.players[_id].Respawn();
    }

    public static void SpawnProjectile(Packet _packet)
    {
        int _projectileId = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();
        int _shotByPlayer = _packet.ReadInt();

        GameManager.instance.SpawnProjectile(_projectileId, _position);
        //GameManager.players[_shotByPlayer].
    }

    public static void ProjectilePosition(Packet _packet)
    {
        int _projectileId = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        GameManager.projectiles[_projectileId].transform.position = _position;
    }

    public static void ProjectileDestroyed(Packet _packet)
    {
        int _projectileId = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        GameManager.projectiles[_projectileId].Explode(_position);
    }
}
