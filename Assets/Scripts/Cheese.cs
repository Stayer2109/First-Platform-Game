using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cheese : MonoBehaviour, IItems
{
    public void Collect()
    {
        Destroy(gameObject);
    }
}
