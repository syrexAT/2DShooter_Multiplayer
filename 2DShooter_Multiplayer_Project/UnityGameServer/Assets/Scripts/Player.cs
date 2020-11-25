using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;

    public Transform shootOrigin;
    public float health;
    public float maxHealth;
    public Rigidbody rigBody;

    public float projectileSpeed;

    //since it will be applied every tick, divide it by ticks per second, is the same as multiplying by time.deltatime
    public float moveSpeed = 5f / Constants.TICKS_PER_SEC;
    private bool[] inputs;

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        health = maxHealth;

        inputs = new bool[4];
    }

    private void Update()
    {
        Vector3 pos = transform.position;
        pos.z = 0;
        transform.position = pos;
        Quaternion rot = transform.rotation;
        
    }

    public void FixedUpdate()
    {
        

        if (health <= 0f)
        {
            return;
        }
        Vector2 _inputDirection = Vector2.zero;
        if (inputs[0])
        {
            _inputDirection.y += 1;
        }

        if (inputs[1])
        {
            _inputDirection.y -= 1;
        }

        if (inputs[2])
        {
            _inputDirection.x += 1;
        }

        if (inputs[3])
        {
            _inputDirection.x -= 1;
        }

        Move(_inputDirection);
    }

    private void Move(Vector2 _inputDirection)
    {
        Vector3 _forward = new Vector3(0, 1, 0);
        Vector3 _right = new Vector3(-1, 0, 0);

        Vector3 _moveDirection = _right * _inputDirection.x + _forward * _inputDirection.y;
        transform.position += _moveDirection * moveSpeed;

        //Vector3 _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;

        //transform.position += _moveDirection * moveSpeed;

        //send player pos and rot player packet
        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }

    public void SetInput(bool[] _inputs, Quaternion _rotation)
    {
        inputs = _inputs;
        transform.rotation = _rotation;
    }

    public void Shoot(Vector3 _viewDirection)
    {
        if (health <= 0)
        {
            return;
        }

        NetworkManager.instance.InstantiateProjectile(shootOrigin).Initialize(_viewDirection, projectileSpeed, id);

        //call takedamage function when projectile collides with player

        if (Physics.Raycast(shootOrigin.position, _viewDirection, out RaycastHit _hit, 25f))
        {
            if (_hit.collider.CompareTag("Player"))
            {
                _hit.collider.GetComponent<Player>().TakeDamage(50f);
            }
        }
    }

    public void TakeDamage(float _damage)
    {
        if (health <= 0)
        {
            return; //player is dead
        }

        health -= _damage;
        if (health <= 0f)
        {
            health = 0f;
            //rigbody disablen? movement ausschalten
            transform.position = new Vector3(0f, 0f, 0f);
            ServerSend.PlayerPosition(this);
            StartCoroutine(Respawn());
        }

        ServerSend.PlayerHealth(this);
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(5f);

        health = maxHealth;
        //RIGBODY / Movement reenablen!
        ServerSend.PlayerRespawned(this);
    }
}
