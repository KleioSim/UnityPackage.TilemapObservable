using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KleioSim.Tilemaps
{
    [TileSetEnum()]
    public enum Shape
    {
        Square,
        Circle
    }

    [TileSetEnum()]
    public enum Color
    {
        Red,
        Green,
    }

    public class Sample : MonoBehaviour
    {
        public void OnClick(TilemapObservable.DataItem item)
        {
            Debug.Log($"onclick {item.Position}, {item.TileKey}");
        }
    }
}
