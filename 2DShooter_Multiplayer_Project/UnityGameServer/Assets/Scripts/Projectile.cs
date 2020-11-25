using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public static Dictionary<int, Projectile> projectiles = new Dictionary<int, Projectile>();
    private static int nextProjectileId = 1;

    public int id;
    public Rigidbody rigBody;
    public int shotByPlayer; //id of player that shot the projectile
    public Vector3 initialForce; //probably change it to float as movespeed
    public float damage;
    public float damageRadius;

    private void Start()
    {
        id = nextProjectileId;
        nextProjectileId++;
        projectiles.Add(id, this);

        ServerSend.SpawnProjectile(this, shotByPlayer);

        rigBody.AddForce(initialForce, ForceMode.Impulse);
    }
    private void FixedUpdate()
    {
        ServerSend.ProjectilePosition(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        print($"ONCOLLISION: " + other.gameObject.name);
        print($"ONCOLLISION: " + other.gameObject);
        print($"ONCOLLISION: " + other.gameObject.tag);
        //if (collision.gameObject.CompareTag("Player"))
        //{
        //    collision.gameObject.GetComponent<Player>().TakeDamage(damage);
        //    Destroy(gameObject);
        //}

        //Destroy(gameObject);
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<Player>().TakeDamage(damage);
        }

        DestroyProjectile();
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    print($"ONCOLLISION: " + collision.gameObject.name);
    //    print($"ONCOLLISION: " + collision.gameObject);
    //    print($"ONCOLLISION: " + collision.gameObject.tag);
    //    //if (collision.gameObject.CompareTag("Player"))
    //    //{
    //    //    collision.gameObject.GetComponent<Player>().TakeDamage(damage);
    //    //    Destroy(gameObject);
    //    //}

    //    //Destroy(gameObject);
    //    if (collision.gameObject.CompareTag("Player"))
    //    {
    //        collision.gameObject.GetComponent<Player>().TakeDamage(damage);
    //    }

    //    DestroyProjectile();
    //}

    public void Initialize(Vector3 _initialMovementDirection, float _initialForceStrength, int _shotByPlayer)
    {
        initialForce = _initialMovementDirection * _initialForceStrength;
        shotByPlayer = _shotByPlayer;
    }

    private void DestroyProjectile()
    {
        ServerSend.ProjectileDestroyed(this);
        Collider[] _colliders = Physics.OverlapSphere(transform.position, damageRadius);
        foreach (Collider _collider in _colliders)
        {
            print(_collider.gameObject.name);
            print(_collider.gameObject);
            print(_collider.gameObject.tag);
            if (_collider.CompareTag("Player"))
            {
                _collider.GetComponent<Player>().TakeDamage(damage);
                Destroy(gameObject);
            }
        }
        projectiles.Remove(id);
        Destroy(gameObject);
    }

}
