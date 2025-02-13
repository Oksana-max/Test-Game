using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public bool isOpen = false;
    public bool isClose = true;
    public bool isBomb = false;
    public int bombAroundCount;

    public Cell(bool isOpen, bool isClose, bool isBomb, int bombAroundCount)
    {
        this.isOpen = isOpen;
        this.isClose = isClose;
        this.isBomb = isBomb;
        this.bombAroundCount = bombAroundCount;
    }


}
