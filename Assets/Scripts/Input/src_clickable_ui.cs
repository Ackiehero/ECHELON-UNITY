using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class src_clickable_ui : MonoBehaviour
{

    public float alphaThreshold = 0.1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.GetComponent<Image>().alphaHitTestMinimumThreshold = alphaThreshold;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
