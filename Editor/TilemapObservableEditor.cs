using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace KleioSim.Tilemaps
{

    [CustomEditor(typeof(TilemapObservable))]
    class TilemapObservableEditor : Editor
    {
        private SerializedProperty onClickProperty;

        private ReorderableList tieItemList;
        private ReorderableList dataItemList;

        private new TilemapObservable target;

        private Type[] tileSetEnumTypes;
        private int selected;

        private Enum[] currEnumItems => Enum.GetValues(tileSetEnumTypes[selected]).OfType<Enum>().ToArray();

        private void OnEnable()
        {
            onClickProperty = serializedObject.FindProperty(nameof(TilemapObservable.OnClickTile));

            var typeQuery = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes().Where(x =>x.IsEnum &&  x.GetCustomAttribute<TileSetEnumAttribute>() != null));
            tileSetEnumTypes = typeQuery.ToArray();

            target = serializedObject.targetObject as TilemapObservable;
            selected = Array.FindIndex(tileSetEnumTypes, x=>x.FullName == target.tileSetEnumType);

            tieItemList = new ReorderableList(target.TileSets, typeof(TilemapObservable.TilePair));
            tieItemList.displayAdd = false;
            tieItemList.displayRemove = false;
            tieItemList.draggable = false;
            tieItemList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "TileSets"); 
            tieItemList.drawElementCallback = DrawTileItemUI; 

            dataItemList = new ReorderableList(target.Itemsource, typeof(TilemapObservable.DataItem));
            dataItemList.draggable = false;
            dataItemList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "DataItems");
            dataItemList.drawElementCallback = DrawDataItemUI;
            dataItemList.onAddCallback = (list) =>
            {
                var position = Vector3Int.zero;
                if(target.Itemsource.Any())
                {
                    position = new Vector3Int(0, target.Itemsource.Select(x => x.Position.y).Max() + 1);
                }

                list.list.Add(new TilemapObservable.DataItem() { TileKey = currEnumItems[0], Position = position });
                EditorUtility.SetDirty(target);

                //UpdateDesignView();
            };
            dataItemList.onRemoveCallback = (list) =>
            {
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
                EditorUtility.SetDirty(target);

                //list.list.Remove(list.list[list.list.Count-1]);
                //UpdateDesignView();
            };


            //UpdateDesignView();
        }

        void DrawDataItemUI(Rect rect, int index, bool isActive, bool isFocused)
        {
            var item = (TilemapObservable.DataItem)dataItemList.list[index];

            item.TileKey = EditorGUI.EnumPopup
                (
                    new Rect(rect.x + rect.width / 2, rect.y, rect.width / 2, EditorGUIUtility.singleLineHeight),
                    item.TileKey
                );

            EditorGUI.BeginChangeCheck();

            var newPosition = (Vector3Int)EditorGUI.Vector2IntField
                (
                    new Rect(rect.x, rect.y, rect.width / 2, EditorGUIUtility.singleLineHeight),
                    GUIContent.none,
                    (Vector2Int)item.Position
                );

            if (EditorGUI.EndChangeCheck())
            {
                for(int i=0; i< dataItemList.list.Count; i++)
                {
                    if(item != dataItemList.list[i] && newPosition == ((TilemapObservable.DataItem)dataItemList.list[i]).Position)
                    {
                        Debug.LogWarning($"already have position {newPosition} in DataItems!");
                        return;
                    }
                }

                item.Position = newPosition;
            }
        }

        void DrawTileItemUI(Rect rect, int index, bool isActive, bool isFocused)
        {
            var pair = (TilemapObservable.TilePair)tieItemList.list[index];

            if (!Enum.TryParse(tileSetEnumTypes[selected], pair.key, out object tileEnum))
            {
                throw new Exception($"can not parse '{pair.key}' to enum '{tileSetEnumTypes[selected]}'");
            }

            EditorGUI.BeginChangeCheck();

            pair.key = EditorGUI.EnumPopup
                (
                    new Rect(rect.x, rect.y, rect.width / 2, EditorGUIUtility.singleLineHeight),
                    tileEnum as Enum
                ).ToString();

            pair.tileImage = (Sprite)EditorGUI.ObjectField
                (
                    new Rect(rect.x + rect.width / 2, rect.y, rect.width/2, EditorGUIUtility.singleLineHeight), 
                    pair.tileImage, typeof(Sprite), 
                    true
                );

            if(EditorGUI.EndChangeCheck())
            {
                target.Redraw();

                EditorUtility.SetDirty(target);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(onClickProperty, true);

            EditorGUI.BeginChangeCheck();
            selected = EditorGUILayout.Popup("TileEnumType:", selected, tileSetEnumTypes.Select(x => x.Name).ToArray());
            if(EditorGUI.EndChangeCheck())
            {
                target.tileSetEnumType = tileSetEnumTypes[selected].FullName;

                target.Itemsource.Clear();
                target.TileSets.Clear();
                target.TileSets.AddRange(currEnumItems.Select(item => new TilemapObservable.TilePair() { key = item.ToString() }));
                target.Redraw();

                //UpdateDesignView();
            }

            if(selected != -1)
            {
                tieItemList.DoLayoutList();
                dataItemList.DoLayoutList();
            }

            serializedObject.ApplyModifiedProperties();
        }

        //public void UpdateDesignView()
        //{
        //    if (target.Itemsource == null)
        //    {
        //        target.Itemsource = new ObservableCollection<TilemapObservable.DataItem>();
        //    }

        //    var oldItems = target.Itemsource.Except(target.DesignDatas).ToArray();
        //    var newItems = target.DesignDatas.Except(target.Itemsource).ToArray();

        //    foreach (var item in oldItems)
        //    {
        //        target.Itemsource.Remove(item);
        //    }

        //    foreach (var item in newItems)
        //    {
        //        target.Itemsource.Add(item);
        //    }
        //}
    }
}