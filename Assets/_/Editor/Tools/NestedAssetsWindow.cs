using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

using Type = System.Type;

namespace Prototype.Editor
{
    public class NestedAssetsWindow : EditorWindow
    {
        [SerializeField] bool locked;
        [SerializeField] Object mainAsset;
        [SerializeField] string mainAssetPath;

        VisualElement content;

        static GUIStyle lockButtonStyle;

        struct Rule
        {
            public Type asset;
            public (Type type, string extension)[] nestedAssets;
        }

        // explicit allowed rules to prevent serialization errors
        static readonly HashSet<Rule> rules = new()
        {
            new()
            {
                asset = typeof(UnityEngine.ScriptableObject),
                nestedAssets = new[]
                {
                    (typeof(UnityEngine.ScriptableObject), "asset")
                }
            },
            new()
            {
                asset = typeof(UnityEngine.ScriptableObject),
                nestedAssets = new[]
                {
                    (typeof(Texture2D), "")
                }
            },
            new()
            {
                asset = typeof(AnimatorController),
                nestedAssets = new[]
                {
                    (typeof(AnimationClip), "anim"),
                    (typeof(Material), "mat")
                }
            },
            new()
            {
                asset = typeof(Mesh),
                nestedAssets = new[]
                {
                    (typeof(Material), "mat")
                }
            }
        };

        [MenuItem(Const.MENU_ASSETS + "/Manage Nested Assets")]
        public static void Open()
        {
            GetWindow<NestedAssetsWindow>("Nested Assets");
        }

        static (Object asset, string assetPath) GetSelectedMainAsset()
        {
            foreach (var guid in Selection.assetGUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadMainAssetAtPath(path);
                return (asset, path);
            }
            return (null, null);
        }

        static bool CanNest(Object asset, Object assetToNest)
        {
            if (!asset || !assetToNest)
                return false;

            if (asset == assetToNest)
                return false;

            if (!AssetDatabase.IsMainAsset(asset))
                return false;

            if (!AssetDatabase.IsMainAsset(assetToNest))
                return false;

            if (!CheckRule(asset.GetType(), assetToNest.GetType()))
                return false;

            var assetToNestPath = AssetDatabase.GetAssetPath(assetToNest);
            var assetToNestChilds = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetToNestPath);
            if (assetToNestChilds.Length > 0)
            {
                assetToNest.LogWarning($"Not allowed ({assetToNestChilds.Length} child(s))");
                return false;
            }

            return true;
        }

        static bool CheckRule(Type assetType, Type childAssetType)
        {
            foreach (var rule in rules.Where(x => assetType == x.asset || assetType.IsSubclassOf(x.asset)))
                foreach (var nestedAsset in rule.nestedAssets)
                    if (childAssetType == nestedAsset.type || childAssetType.IsSubclassOf(nestedAsset.type))
                        return true;
            return false;
        }

        static Object CreateObject(Object baseObject)
        {
            if (baseObject is Material material)
                return new Material(material);

            var type = baseObject.GetType();

            if (type.IsSubclassOf(typeof(ScriptableObject)))
                return CreateInstance(type);

            return System.Activator.CreateInstance(type) as Object;
        }

        void ShowButton(Rect position)
        {
            if (lockButtonStyle is null)
                lockButtonStyle = "IN LockButton";

            var locked = this.locked;
            this.locked = GUI.Toggle(position, locked, GUIContent.none, lockButtonStyle);
            if (this.locked != locked && !this.locked)
                OnSelectionChange();
        }

        void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            content = new();
            content.style.SetPadding(5);

            root.Add(content);

            OnSelectionChange();
        }

        void OnSelectionChange()
        {
            Refresh();
        }

        void Refresh()
        {
            if (!locked || !mainAsset)
            {
                var (asset, assetPath) = GetSelectedMainAsset();
                mainAsset = asset;
                mainAssetPath = assetPath;
            }

            content.Clear();

            if (!mainAsset)
                return;

            content.Add(CreateObjectField(mainAsset));

            var label = new Label("Content");
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.SetMargin(10, 0, 5);
            content.Add(label);

            var nestedObjectsList = new VisualElement();

            nestedObjectsList.ToListElement(
                createItemGUI: (item, element) =>
                {
                    element.style.flexDirection = FlexDirection.Row;

                    var objectField = CreateObjectField(item);
                    objectField.style.flexGrow = 1;
                    objectField.style.flexShrink = 1;

                    var renameButton = new Button();

                    var textField = new TextField()
                    {
                        style = {
                            flexGrow = 1,
                            flexShrink = 1
                        }
                    };

                    void SaveRenamedObject()
                    {
                        element.Clear();

                        item.name = textField.text;

                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

                        element.Add(objectField);
                        element.Add(renameButton);
                    }

                    var saveButton = new Button();
                    var cancelButton = new Button();

                    renameButton.text = "Rename";
                    renameButton.clicked += () =>
                    {
                        element.Clear();

                        textField.value = objectField.value.name;

                        element.Add(textField);
                        element.Add(saveButton);
                        element.Add(cancelButton);

                        textField.Focus();
                    };

                    saveButton.text = "Save";
                    saveButton.clicked += SaveRenamedObject;

                    cancelButton.text = "Cancel";
                    cancelButton.clicked += () =>
                    {
                        element.Clear();
                        element.Add(objectField);
                        element.Add(renameButton);
                    };

                    textField.RegisterCallback<KeyDownEvent>(e =>
                    {
                        if (e.keyCode != KeyCode.Return)
                            return;

                        SaveRenamedObject();
                    });

                    element.Add(objectField);
                    element.Add(renameButton);
                },
                getItems: () =>
                {
                    return GetNestedAssets();
                },
                filter: items =>
                {
                    return items.Where(x => CanNest(mainAsset, x)).ToArray();
                },
                addItem: item =>
                {
                    var newObject = CreateObject(item);
                    EditorUtility.CopySerialized(item, newObject);

                    var itemPath = AssetDatabase.GetAssetPath(item);
                    if (itemPath.IsNullOrEmpty())
                        return;

                    AssetDatabase.AddObjectToAsset(newObject, mainAsset);
                    AssetDatabase.SaveAssets();

                    if (GetNestedAssets().Any(x => x == newObject))
                    {
                        AssetDatabase.DeleteAsset(itemPath);
                        AssetDatabase.SaveAssets();
                    }
                },
                deleteItem: item =>
                {
                    var path = AssetDatabase.GetAssetPath(item);
                    if (string.IsNullOrEmpty(path))
                        return;

                    path = path[..(path.Length - System.IO.Path.GetFileName(path).Length)];

                    var itemType = item.GetType();

                    string ext = null;
                    foreach (var nestedAsset in rules.SelectMany(x => x.nestedAssets))
                    {
                        if (itemType != nestedAsset.type)
                            continue;

                        ext = nestedAsset.extension;
                        break;
                    }

                    if (ext is null)
                    {
                        this.LogWarning($"File extension of type \"{itemType}\" is unknown");
                        return;
                    }

                    if (ext == "")
                    {
                        AssetDatabase.RemoveObjectFromAsset(item);
                        AssetDatabase.SaveAssets();
                        return;
                    }

                    var newPath = AssetDatabase.GenerateUniqueAssetPath($"{path}{item.name}.{ext}");

                    var newObject = CreateObject(item);
                    EditorUtility.CopySerialized(item, newObject);
                    AssetDatabase.CreateAsset(newObject, newPath);
                    AssetDatabase.SaveAssets();

                    if (AssetDatabase.Contains(newObject))
                    {
                        AssetDatabase.RemoveObjectFromAsset(item);
                        AssetDatabase.SaveAssets();
                    }
                }
            );

            content.Add(nestedObjectsList);
        }

        ObjectField CreateObjectField(Object @object)
        {
            var objectField = new ObjectField()
            {
                objectType = @object.GetType(),
                value = @object
            };
            objectField.SetEnabled(false);
            objectField.RegisterValueChangedCallback(x => objectField.SetValueWithoutNotify(x.previousValue));
            return objectField;
        }

        Object[] GetNestedAssets()
        {
            return AssetDatabase.LoadAllAssetRepresentationsAtPath(mainAssetPath);
        }
    }
}
