using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSetUp : MonoBehaviour
{
    public Item item;
    private GameObject itemColider;

    public GameObject SetItem;

    //private BoxCollider component;

    void Start()
    {
        SetItem.SetActive(false);
        //  BoxCollider itemColider = GetComponent<BoxCollider>();
    }

    void Update()
    {
        if (SetItem.gameObject.activeSelf == true)
        {
            gameObject.GetComponent<ItemSetUp>().enabled = false;
            gameObject.GetComponent<BoxCollider>().enabled = false;
        }
    }

}
