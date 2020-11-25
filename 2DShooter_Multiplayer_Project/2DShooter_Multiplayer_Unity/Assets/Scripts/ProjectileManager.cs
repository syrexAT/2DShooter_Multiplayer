﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public int id;
    
    public void Initialize(int _id)
    {
        id = _id;
    }
    
    public void Explode(Vector3 _position)
    {
        transform.position = _position;
        GameManager.projectiles.Remove(id);
        GameManager.projectiles.Remove(id);
        Destroy(gameObject);
    }
}
