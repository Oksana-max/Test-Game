using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] public bool isOpen = false;
    [SerializeField] public bool isClose = true;
    [SerializeField] public bool isBomb = false;
    [SerializeField] public int bombAroundCount;



    public Cell(bool isOpen, bool isClose, bool isBomb, int bombAroundCount)
    {
        this.isOpen = isOpen;
        this.isClose = isClose;
        this.isBomb = isBomb;
        this.bombAroundCount = bombAroundCount;
    }


}
