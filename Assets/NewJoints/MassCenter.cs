using System;
using UnityEngine;
using System.Collections.Generic;

public class CenterOfMassUpdater : MonoBehaviour
{
    [Header("Objeleri buraya ekle")]
    public List<Transform> objectsToUpdate = new List<Transform>();
    
    Vector2 centerofMass;

    
    void Update()
    {
        foreach (var t in objectsToUpdate)
        {
            centerofMass += (Vector2)t.position;
        }
        
        centerofMass /= objectsToUpdate.Count;
        transform.position = centerofMass;
        
        centerofMass = Vector2.zero;
    }
    
    
    
}