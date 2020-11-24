using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public static Dictionary<int, Projectile> projectiles = new Dictionary<int, Projectile>();
    private static int nextProjectileId = 1;

    public int id;
    public Rigidbody2D rigBody;
    public int shotByPlayer; //id of player that shot the projectile
    public Vector3 initialForce; //probably change it to float as movespeed
    public float damage;

    private void Start()
    {
        id = nextProjectileId;
        nextProjectileId++;
        projectiles.Add(id, this);

        ServerSend.SpawnProjectile(this, shotByPlayer);

        rigBody.AddForce(initialForce, ForceMode2D.Impulse);
    }
    private void FixedUpdate()
    {
        ServerSend.ProjectilePosition(this);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //print(collision.gameObject.name);
        //print(collision.gameObject);
        //print(collision.gameObject.tag);
        //if (collision.gameObject.CompareTag("Player"))
        //{
        //    collision.gameObject.GetComponent<Player>().TakeDamage(damage);
        //    Destroy(gameObject);
        //}

        //Destroy(gameObject);

        DestroyProjectile();
    }

    public void Initialize(Vector3 _initialMovementDirection, float _initialForceStrength, int _shotByPlayer)
    {
        initialForce = _initialMovementDirection * _initialForceStrength;
        shotByPlayer = _shotByPlayer;
    }

    private void DestroyProjectile()
    {
        ServerSend.ProjectileDestroyed(this);
        Collider2D[] _colliders = Physics2D.OverlapAreaAll(transform.position, transform.position);
        foreach (Collider2D _collider in _colliders)
        {
            print(_collider.gameObject.name);
            print(_collider.gameObject);
            print(_collider.gameObject.tag);
            if (_collider.CompareTag("Player"))
            {
                _collider.GetComponent<Player>().TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }

}
