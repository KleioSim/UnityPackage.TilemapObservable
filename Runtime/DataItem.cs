using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
            private string tileKeyStr;
            [SerializeField]
            private string tileKeyEnumType;

            private Enum tileKey;
            public Enum TileKey
            {
                get
                {
                    if(tileKey == null)
                    {
                        var typeQuery = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes().Where(x => x.IsEnum && x.GetCustomAttribute<TileSetEnumAttribute>() != null));
                        var enmType = typeQuery.Single(x => x.FullName == tileKeyEnumType);

                        tileKey = Enum.Parse(enmType, tileKeyStr) as Enum;
                    }
                    return tileKey;
                }
                set
                {
                    if(tileKey == value)
                    {
                        return;
                    }

                    var oldValue = tileKey;
                    tileKey = value;

                    tileKeyStr = tileKey.ToString();
                    tileKeyEnumType = tileKey.GetType().FullName;

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