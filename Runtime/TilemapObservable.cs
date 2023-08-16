using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

namespace KleioSim.Tilemaps
{
    [RequireComponent(typeof(Tilemap))]
    public partial class TilemapObservable : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private List<TilePair> tileSets;
        public List<TilePair> TileSets
        {
            get
            {
                if(tileSets == null)
                {
                    tileSets = new List<TilePair>();
                }

                return tileSets;
            }
            set
            {
                tileSets = value;
            }
        }

        [SerializeField]
        private List<DataItem> designDatas;
        public List<DataItem> DesignDatas
        {
            get
            {
                if (designDatas == null)
                {
                    designDatas = new List<DataItem>();
                }

                return designDatas;
            }
            set
            {
                designDatas = value;
            }
        }

        public string tileSetEnumType;

        public UnityEvent<DataItem> OnClickTile;

        private Tilemap tilemap => GetComponent<Tilemap>();

        private ObservableCollection<DataItem> itemsource;
        public ObservableCollection<DataItem> Itemsource
        {
            get => itemsource;
            set
            {
                if (itemsource == value)
                {
                    return;
                }

                if (itemsource != null)
                {
                    itemsource.CollectionChanged -= Itemsource_CollectionChanged;
                }

                itemsource = value;
                itemsource.CollectionChanged += Itemsource_CollectionChanged;

                Redraw();
            }
        }

        public void Redraw()
        {
            tilemap.ClearAllTiles();
            foreach (var item in itemsource)
            {
                var pair = tileSets.Single(x => x.key == item.TileKey);
                tilemap.SetTile(item.Position, pair.tile);
            }
        }

        private void Itemsource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (DataItem newItem in e.NewItems)
                {
                    newItem.PropertyChanged += this.OnItemPropertyChanged;

                    var pair = tileSets.SingleOrDefault(x => x.key == newItem.TileKey);
                    if(pair != null)
                    {
                        tilemap.SetTile(newItem.Position, pair.tile);
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (DataItem oldItem in e.OldItems)
                {
                    oldItem.PropertyChanged -= this.OnItemPropertyChanged;

                    tilemap.SetTile(oldItem.Position, null);
                }
            }
        }

        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var args = e as DataItemPropertyChangedEventArgs;

            switch(args.PropertyName)
            {
                case nameof(DataItem.Position):
                    {
                        var item = sender as DataItem;
                        var pair = tileSets.Single(x => x.key == item.TileKey);
                        tilemap.SetTile(item.Position, pair.tile);
                        tilemap.SetTile((Vector3Int)args.oldValue, null);
                    }
                    break;
                case nameof(DataItem.TileKey):
                    {
                        var pair = tileSets.Single(x => x.key == (string)args.newValue);
                        var item = sender as DataItem;
                        tilemap.SetTile(item.Position, pair.tile);
                    }
                    break;
                default:
                    throw new Exception($"not support propertyName{args.PropertyName}");
            }
        }

        public void Start()
        {
            if (Camera.main.gameObject.GetComponent<Physics2DRaycaster>() == null)
            {
                Camera.main.gameObject.AddComponent<Physics2DRaycaster>();
            }

            if (gameObject.GetComponent<CompositeCollider2D>() == null)
            {
                var collider = gameObject.AddComponent<CompositeCollider2D>();
                collider.geometryType = CompositeCollider2D.GeometryType.Polygons;

                GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            }

            if (gameObject.GetComponent<TilemapCollider2D>() == null)
            {
                var collider = gameObject.AddComponent<TilemapCollider2D>();
                collider.usedByComposite = true;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var tpos = tilemap.WorldToCell(eventData.pointerCurrentRaycast.worldPosition);

            OnClickTile?.Invoke(itemsource.Single(x=>x.Position == tpos));
        }
    }
}