using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace KleioSim.Tilemaps
{
    public partial class TilemapObservable
    {
        [Serializable]
        public class TilePair
        {
            public string key;
            public Sprite tileImage
            {
                get
                {
                    if(tile == null)
                    {
                        tile = ScriptableObject.CreateInstance<Tile>();
                    }
                    return tile.sprite;
                }
                set
                {
                    tile.sprite = value;
                }
            }

            public Tile tile;
        }
    }
}