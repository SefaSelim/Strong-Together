using System;
using System.Collections.Generic;
using UnityEngine;

public class SpheresManager : MonoBehaviour
{
    public static SpheresManager Instance;
    
    
    private CircleSpringSpawner3D_XY_Runtime spawner;
    public bool isGrounded = false; 
    public List<GameObject> Spheres => spawner.GetListOfSpheres();
    
    
    private void Awake()
    {
        Instance = this;
        spawner = GetComponent<CircleSpringSpawner3D_XY_Runtime>();
    }
    
    
}
