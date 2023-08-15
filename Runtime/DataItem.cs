using System;
using System.ComponentModel;
using UnityEngine;

namespace KleioSim.Tilemaps
{
    public partial class TilemapObservable
    {
        [Serializable]
        public class DataItem : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            [SerializeField]
            private Vector3Int position;
            public Vector3Int Position
            {
                get => position;
                set
                {
                    if(position == value)
                    {
                        return;
                    }

                    var oldValue = position;
                    position = value;

                    PropertyChanged?.Invoke(this, new DataItemPropertyChangedEventArgs(nameof(Position), position, oldValue));
                }
            }

            [SerializeField]
            private string tileKey;
            public string TileKey
            {
                get => tileKey;
                set
                {
                    if(tileKey == value)
                    {
                        return;
                    }

                    var oldValue = tileKey;
                    tileKey = value;

                    PropertyChanged?.Invoke(this, new DataItemPropertyChangedEventArgs(nameof(TileKey), tileKey, oldValue));
                }
            }
        }

        public class DataItemPropertyChangedEventArgs : PropertyChangedEventArgs
        {
            public readonly object newValue;
            public readonly object oldValue;

            public DataItemPropertyChangedEventArgs(string propertyName, object newValue, object oldValue) : base(propertyName)
            {
                this.newValue = newValue;
                this.oldValue = oldValue;
            }
        }
    }
}