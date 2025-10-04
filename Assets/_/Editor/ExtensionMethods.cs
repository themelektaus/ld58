using System;
using System.Linq;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

using Object = UnityEngine.Object;

namespace Prototype.Editor
{
    public static class ExtensionMethods
    {
        public static void AddVisualTreeAsset<T>(this VisualElement @this)
        {
            @this.AddVisualTreeAsset(typeof(T).Name);
        }

        public static void AddVisualTreeAsset(this VisualElement @this, string name)
        {
            Utils.LoadAsset<VisualTreeAsset>(name).CloneTree(@this);
        }

        public static TemplateContainer AddVisualTreeAsset(this VisualElement @this, VisualTreeAsset visualTreeAsset)
        {
            return @this.AddVisualTreeAsset(visualTreeAsset, stretch: true);
        }

        public static TemplateContainer AddVisualTreeAsset(this VisualElement @this, VisualTreeAsset visualTreeAsset, bool stretch)
        {
            var templateContainer = visualTreeAsset.Instantiate();
            @this.AddTemplateContainer(templateContainer, stretch);
            return templateContainer;
        }

        static void AddTemplateContainer(this VisualElement @this, TemplateContainer templateContainer, bool stretch)
        {
            @this.Add(templateContainer);
            if (stretch)
                templateContainer.StretchToParentSize();
        }

        public static VisualElement CreateRootElement(this UnityEditor.Editor _, Color? backgroundColor = null)
        {
            var root = new VisualElement();

            if (backgroundColor.HasValue)
            {
                root.pickingMode = PickingMode.Ignore;
                root.style.backgroundColor = backgroundColor.Value;
                root.style.SetMargin(-25, -5, -7, -15);
                root.style.SetPadding(25, 0, 7);
            }

            return root;
        }

        public static void SetMargin(this IStyle @this, float all)
        {
            @this.SetMargin(all, all, all, all);
        }

        public static void SetMargin(this IStyle @this, float vertical, float horizontal)
        {
            @this.SetMargin(vertical, horizontal, vertical, horizontal);
        }

        public static void SetMargin(this IStyle @this, float top, float horizontal, float bottom)
        {
            @this.SetMargin(top, horizontal, bottom, horizontal);
        }

        public static void SetMargin(this IStyle @this, float top, float right, float bottom, float left)
        {
            @this.marginTop = top;
            @this.marginRight = right;
            @this.marginBottom = bottom;
            @this.marginLeft = left;
        }

        public static void SetPadding(this IStyle @this, float all)
        {
            @this.SetPadding(all, all, all, all);
        }

        public static void SetPadding(this IStyle @this, float vertical, float horizontal)
        {
            @this.SetPadding(vertical, horizontal, vertical, horizontal);
        }

        public static void SetPadding(this IStyle @this, float top, float horizontal, float bottom)
        {
            @this.SetPadding(top, horizontal, bottom, horizontal);
        }

        public static void SetPadding(this IStyle @this, float top, float right, float bottom, float left)
        {
            @this.paddingTop = top;
            @this.paddingRight = right;
            @this.paddingBottom = bottom;
            @this.paddingLeft = left;
        }

        public static void SetBorderWidth(this IStyle @this, float all)
        {
            @this.borderTopWidth = all;
            @this.borderRightWidth = all;
            @this.borderBottomWidth = all;
            @this.borderLeftWidth = all;
        }

        public static void SetBorderColor(this IStyle @this, Color? all)
        {
            if (all.HasValue)
            {
                @this.borderTopColor = all.Value;
                @this.borderRightColor = all.Value;
                @this.borderBottomColor = all.Value;
                @this.borderLeftColor = all.Value;
                return;
            }

            @this.borderTopColor = new(StyleKeyword.None);
            @this.borderRightColor = new(StyleKeyword.None);
            @this.borderBottomColor = new(StyleKeyword.None);
            @this.borderLeftColor = new(StyleKeyword.None);
        }

        public static void ForceHeight(this IStyle @this, float height)
        {
            @this.height = height;
            @this.minHeight = height;
            @this.maxHeight = height;
        }

        public static void Show(this VisualElement @this, bool visible)
        {
            @this.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public static void ToListElement<T>(
            this VisualElement @this,
            string itemVisualTreeAssetName,
            SerializedProperty property,
            Func<Object[], T[]> filter,
            Action<SerializedProperty, T> addAction
        ) where T : Object
        {
            void Refresh() => @this.ToListElement(itemVisualTreeAssetName, property, filter, addAction);

            @this.Clear();

            int count = property.arraySize;

            for (int i = 0; i < count; i++)
            {
                var itemElement = new VisualElement { style = { flexDirection = FlexDirection.Row } };

                var item = property.GetArrayElementAtIndex(i);

                var objectElement = new BindableElement { style = { flexGrow = 1 } };
                objectElement.AddVisualTreeAsset(itemVisualTreeAssetName);
                objectElement.BindProperty(item);
                itemElement.Add(objectElement);

                int index = i;
                var deleteButton = new Button(() =>
                {
                    property.DeleteArrayElementAtIndex(index);
                    property.serializedObject.ApplyModifiedProperties();
                    Refresh();
                })
                {
                    text = "X",
                    style = { marginLeft = 5 }
                };

                itemElement.Add(deleteButton);

                @this.Add(itemElement);
            }

            @this.Add(_CreateDropZone(filter, dragAndDropObjects =>
            {
                foreach (var dragAndDropObject in dragAndDropObjects)
                {
                    property.InsertArrayElementAtIndex(property.arraySize);
                    var item = property.GetArrayElementAtIndex(property.arraySize - 1);
                    addAction(item, dragAndDropObject);
                }

                property.serializedObject.ApplyModifiedProperties();

                Refresh();
            }));
        }

        public static void ToListElement<T>(
            this VisualElement @this,
            Action<T, VisualElement> createItemGUI,
            Func<T[]> getItems,
            Func<Object[], T[]> filter,
            Action<T> addItem,
            Action<T> deleteItem
        ) where T : Object
        {
            void Refresh() => @this.ToListElement(createItemGUI, getItems, filter, addItem, deleteItem);

            @this.Clear();

            var container = new ScrollView();
            container.style.flexGrow = 1;
            @this.Add(container);

            var items = getItems();

            foreach (var item in items)
            {
                var itemWrapper = new VisualElement { style = { flexDirection = FlexDirection.Row } };

                var itemElement = new VisualElement { style = { flexGrow = 1 } };
                createItemGUI(item, itemElement);
                itemWrapper.Add(itemElement);

                var _item = item;
                var deleteButton = new Button(() =>
                {
                    deleteItem(_item);
                    Refresh();
                })
                {
                    text = "X"
                };

                itemWrapper.Add(deleteButton);

                container.Add(itemWrapper);
            }

            var dropZone = _CreateDropZone(
                filter: x => filter(x),
                drop: objects =>
                {
                    foreach (var @object in objects)
                        addItem(@object);
                    Refresh();
                }
            );
            @this.Add(dropZone);
        }

        static VisualElement _CreateDropZone<T>(Func<Object[], T[]> filter, Action<T[]> drop) where T : Object
        {
            var dropZone = new VisualElement();
            dropZone.style.SetBorderWidth(1);
            dropZone.style.SetBorderColor(new(1, 1, 1, .2f));
            dropZone.style.SetMargin(5);
            dropZone.style.ForceHeight(30);

            var dropZoneInfo = new Label($"Drop Zone for {typeof(T).Name}s")
            {
                style = {
                    height = dropZone.style.height,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    color = new Color(1, 1, 1, .5f)
                }
            };
            dropZone.Add(dropZoneInfo);

            dropZone.RegisterCallback<DragUpdatedEvent>(e =>
            {
                if (filter(DragAndDrop.objectReferences).Length == 0)
                {
                    dropZone.style.SetBorderColor(Color.red);
                    return;
                }

                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                dropZone.style.SetBorderColor(Color.cyan);
            });

            dropZone.RegisterCallback<DragPerformEvent>(e =>
            {
                var @object = DragAndDrop.objectReferences.FirstOrDefault();
                if (!@object)
                    return;

                var dragAndDropObjects = filter(DragAndDrop.objectReferences);
                if (dragAndDropObjects.Length == 0)
                    return;

                drop(dragAndDropObjects);
            });

            dropZone.RegisterCallback<DragLeaveEvent>(e =>
            {
                dropZone.style.SetBorderColor(new(1, 1, 1, .2f));
            });

            dropZone.RegisterCallback<DragExitedEvent>(e =>
            {
                dropZone.style.SetBorderColor(new(1, 1, 1, .2f));
            });

            return dropZone;
        }

        public static bool SearchMatch(this string @this, string keywords)
        {
            var x = keywords.Split(' ').Where(x => x.Length > 0);
            return x.All(x => @this.Contains(x, StringComparison.InvariantCultureIgnoreCase));
        }

        public static VisualElement CreateDefaultEditor(this SerializedObject serializedObject)
        {
            var container = new VisualElement();
            var iterator = serializedObject.GetIterator();
            if (iterator.NextVisible(true))
            {
                do
                {
                    PropertyField field = new(iterator.Copy())
                    {
                        name = "PropertyField:" + iterator.propertyPath
                    };
                    if (iterator.propertyPath == "m_Script" && serializedObject.targetObject is not null)
                        field.SetEnabled(false);
                    container.Add(field);
                }
                while (iterator.NextVisible(false));
            }
            return container;
        }

        public static void ToTypeField(this VisualElement @this, SerializedProperty property)
        {
            @this.Clear();

            @this.style.flexDirection = FlexDirection.Row;

            var label = new Label(property.displayName);
            label.style.flexGrow = 1;
            label.style.SetPadding(3, 4);
            @this.Add(label);

            var field = new Label(property.stringValue.Split('.').LastOrDefault());
            field.style.flexGrow = 7;
            field.style.backgroundColor = new Color(0, 0, 0, .4f);
            field.style.SetPadding(2, 4);
            field.style.SetBorderColor(new(1, 1, 1, .3f));
            field.style.SetBorderWidth(1);

            field.RegisterCallback<MouseEnterEvent>(_ => field.style.SetBorderColor(new(1, 1, .5f, .7f)));
            field.RegisterCallback<MouseLeaveEvent>(_ => field.style.SetBorderColor(new(1, 1, 1, .3f)));

            field.RegisterCallback<ClickEvent>(_ =>
            {
                QuickMenu.QuickMenuWindow.Open(
                    initSearchText: property.stringValue,
                    menuItems: Utils.types
                        .Where(x => !x.IsGenericType)
                        .Where(x => !x.FullName.Contains('<') && !x.FullName.Contains('>'))
                        .Select(x => new QuickMenu.TypeSelectorMenuItem(x, x =>
                        {
                            field.text = x.FullName.Split('.').LastOrDefault();
                            property.stringValue = x.FullName;
                            property.serializedObject.ApplyModifiedProperties();
                        })),
                    maxDisplayItems: 20
                );
            });

            @this.Add(field);
        }

        public static void ToSceneField(this VisualElement @this, SerializedProperty property)
        {
            @this.Clear();

            @this.style.flexDirection = FlexDirection.Row;

            var label = new Label(property.displayName);
            label.style.flexGrow = 1;
            label.style.SetPadding(3, 4);
            @this.Add(label);

            var field = new Label(property.stringValue.Split('.').LastOrDefault());
            field.style.flexGrow = 7;
            field.style.backgroundColor = new Color(0, 0, 0, .4f);
            field.style.SetPadding(2, 4);
            field.style.SetBorderColor(new(1, 1, 1, .3f));
            field.style.SetBorderWidth(1);

            field.RegisterCallback<MouseEnterEvent>(_ => field.style.SetBorderColor(new(1, 1, .5f, .7f)));
            field.RegisterCallback<MouseLeaveEvent>(_ => field.style.SetBorderColor(new(1, 1, 1, .3f)));

            field.RegisterCallback<ClickEvent>(_ =>
            {
                QuickMenu.QuickMenuWindow.Open(
                    initSearchText: property.stringValue,
                    menuItems: QuickMenu.MenuItem_OpenScene.sceneAssets
                        .Select(x => new QuickMenu.SceneSelectorMenuItem(new()
                        {
                            sceneAsset = x.Key,
                            path = x.Value
                        }, x =>
                            {
                                field.text = x.sceneAsset.name;
                                property.stringValue = x.sceneAsset.name;
                                property.serializedObject.ApplyModifiedProperties();
                            }))
                );
            });

            @this.Add(field);
        }
    }
}
