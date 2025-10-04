using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace QuickMenu
{
    public class QuickMenuWindow : EditorWindow
    {
        [SerializeField]
        VisualTreeAsset visualTreeAsset;

        [SerializeField]
        VisualTreeAsset visualTreeAsset_Item;

        [UnityEditor.ShortcutManagement.Shortcut("Assets/Quick Menu", KeyCode.Menu)]
        static void Open()
        {
            Open(null, null, -1, null);
        }

        [UnityEditor.MenuItem("Assets/Quick Menu (Debug)")]
        static void OpenDebug()
        {
            var window = Create(null, null, -1, null);
            window.ShowUtility();
        }

        public static void Open(
            string initSearchText,
            IEnumerable<IMenuItem> menuItems,
            int maxDisplayItems = -1,  // optional (-1 for infinite),
            object userData = null     // optional
        )
        {
            var e = Event.current;
            if (e is null)
                return;

            var position = GUIUtility.GUIToScreenPoint(e.mousePosition);
            Open(initSearchText, menuItems, maxDisplayItems, userData, position);
        }

        public static void Open(Vector2 position)
        {
            Open(null, null, -1, null, position);
        }

        public static void Open(string initSearchText, IEnumerable<IMenuItem> menuItems, int maxDisplayItems, object userData, Vector2 position)
        {
            var window = Create(initSearchText, menuItems, maxDisplayItems, userData);
            window.position = new();

            // for debugging
            //window.ShowUtility();
            //return;

            window.isPopup = true;
            window.ShowPopup();

            var size = new Vector2(460, 430);
            window.position = new(position.x - 5, position.y - 20, size.x, size.y);
        }

        static QuickMenuWindow Create(string initSearchText, IEnumerable<IMenuItem> menuItems, int maxDisplayItems, object userData)
        {
            var window = CreateInstance<QuickMenuWindow>();

            window.titleContent = new("Quick Menu");
            window.root.style.backgroundColor = new Color(.133f, .133f, .133f);

            var context = new Context
            {
                focusedWindow = WindowType.None,
                objects = Selection.objects,
                @object = Selection.activeObject,
                gameObjects = Selection.gameObjects,
                gameObject = Selection.activeGameObject,
                transforms = Selection.transforms,
                transform = Selection.activeTransform,
                assetGUIDs = Selection.assetGUIDs,
                userData = userData
            };

            if (focusedWindow)
            {
                var type = focusedWindow.GetType();

                if (type == Utils.projectBrowser)
                    context.focusedWindow = WindowType.ProjectBrowser;

                else if (type == Utils.sceneHierarchyWindow)
                    context.focusedWindow = WindowType.SceneHierarchy;

                else if (type == Utils.animatorControllerTool)
                    context.focusedWindow = WindowType.AnimatorControllerTool;

                else
                    context.focusedWindow = WindowType.Other;
            }

            context.assetGUID = context.assetGUIDs.FirstOrDefault();

            context.assetPaths = new();

            if (context.focusedWindow == WindowType.ProjectBrowser)
            {
                if (Utils.TryGetActiveFolderPath(out var path))
                {
                    context.assetGUID = AssetDatabase.AssetPathToGUID(path);
                    context.assetGUIDs = new[] { context.assetGUID };
                    context.assetPaths.Add(path);
                }
            }
            else
            {
                foreach (var assetGUID in context.assetGUIDs)
                    context.assetPaths.Add(AssetDatabase.GUIDToAssetPath(assetGUID));
            }

            context.assetPath = context.assetPaths.FirstOrDefault();
            if (context.assetPath is not null)
            {
                if (Directory.Exists(context.assetPath))
                    context.assetFolder = context.assetPath;
                else
                    context.assetFolder = Path.GetDirectoryName(context.assetPath).Replace('\\', '/');
            }

            context.assets = new();
            foreach (var assetPath in context.assetPaths)
                context.assets.Add(AssetDatabase.LoadAssetAtPath<Object>(assetPath));

            context.asset = context.assets.FirstOrDefault();

            context.scenes = new();
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                var scene = SceneManager.GetSceneAt(index);
                if (Selection.instanceIDs.Contains(scene.handle))
                    context.scenes.Add(scene);
            }
            context.scene = context.scenes.FirstOrDefault();

            window.context = context;

            if (menuItems is null)
                menuItems = GetMenuItemsByAttribute();

            window.initSearchText = initSearchText;
            window.menuItems = menuItems;
            window.maxDisplayItems = maxDisplayItems;

            return window;
        }

        static IEnumerable<IMenuItem> GetMenuItemsByAttribute()
        {
            foreach (var method in TypeCache.GetMethodsWithAttribute<AddQuickMenuItemsAttribute>())
            {
                var enumerable = method.Invoke(null, null) as IEnumerable<IMenuItem>;
                if (enumerable is not null)
                    foreach (var menuItem in enumerable)
                        yield return menuItem;
            }
        }

        Context context;
        bool isPopup;
        bool firstRender;

        string initSearchText;
        IEnumerable<IMenuItem> menuItems;
        int maxDisplayItems;

        VisualElement root => rootVisualElement;
        IVisualElementScheduler schedule => root.schedule;

        VisualElement container;

        VisualElement searchArea;
        TextField searchField;
        Label searchFieldPlaceholder;
        ScrollView searchResults;

        VisualElement itemParametersArea;
        ScrollView itemParameters;

        readonly List<IMenuItem> allItems = new();
        readonly List<IMenuItem> items = new();

        int index = -1;
        IMenuItem item;

        void OnLostFocus()
        {
            if (isPopup)
                Close();
        }

        public void CreateGUI()
        {
            if (!visualTreeAsset)
                return;

            if (!visualTreeAsset_Item)
                return;

            root.AddVisualTreeAsset(visualTreeAsset, stretch: true);
            root.focusable = true;

            root.RegisterCallback<ClickEvent>(_ => Close());
            root.RegisterCallback<KeyDownEvent>(OnKeyDown);

            container = root.Q<VisualElement>("Container");
            container.RegisterCallback<ClickEvent>(e => e.StopPropagation());

            allItems.Clear();
            allItems.AddRange(
                menuItems.OrderBy(x => x.category ?? "")
                         .ThenBy(x => x.subCategory ?? "")
                         .ThenBy(x => x.title)
            );

            searchArea = root.Q<VisualElement>("SearchArea");

            searchField = root.Q<TextField>("SearchField");
            searchField.value = initSearchText;

			searchFieldPlaceholder = root.Q<Label>("SearchFieldPlaceholder");

            searchField.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            searchField.RegisterCallback<KeyUpEvent>(OnKeyUp, TrickleDown.TrickleDown);
            searchField.RegisterValueChangedCallback(SearchField_ValueChanged);
            searchField.Focus();

            searchResults = root.Q<ScrollView>("SearchResults");

            itemParametersArea = root.Q<VisualElement>("ItemParametersArea");
            itemParametersArea.Show(false);

            itemParameters = root.Q<ScrollView>("ItemParameters");

            root.Q<Button>("ItemButtonApply").clicked += () => OnKeyDown_Return(null);
            root.Q<Button>("ItemButtonCancel").clicked += () => OnKeyDown_Escape(null);

            RefreshSearchResults();

            firstRender = true;
        }

        void OnGUI()
        {
            if (!firstRender)
                return;

            firstRender = false;

            var position = this.position;
            position.height -= 60;
            this.position = position;

            // Not needed at the moment
            //root.style.backgroundImage = Utils.Screenshot(position);
        }

        void RefreshSearchResults()
        {
            var text = searchField.text;
            var empty = string.IsNullOrEmpty(text);

            searchFieldPlaceholder.Show(empty);

            var validatedItems = allItems
                .Where(x => x.Validation(context))
                .OrderByDescending(x => x.priority)
                .AsEnumerable();

            if (!empty)
            {
                validatedItems = validatedItems
                    .Select(x => (x, x.priority, match: x.GetMatchcode().SearchMatch(text)))
                    .Where(x => !Mathf.Approximately(x.match, 0))
                    .OrderByDescending(x => x.priority)
                    .ThenByDescending(x => x.match)
                    .Select(x => x.x);
            }

            items.Clear();

            if (text is null || text.Replace(" ", "").Length < 3)
                validatedItems = validatedItems.Where(x => x.visible);
            else
                validatedItems = validatedItems.OrderByDescending(x => x.visible);

            if (maxDisplayItems > -1)
                validatedItems = validatedItems.Take(maxDisplayItems);

            items.AddRange(validatedItems);

            searchResults.Clear();

            foreach (var item in items)
            {
                var itemElement = searchResults.AddVisualTreeAsset(visualTreeAsset_Item, stretch: false);
                itemElement.userData = item;
                SetItemInfo(itemElement, item);

                itemElement.RegisterCallback<MouseEnterEvent>(e =>
                {
                    index = items.IndexOf(item);
                    RefreshIndex(false);
                });

                itemElement.RegisterCallback<ClickEvent>(_ => ClickSearchResult());
            }

            if (searchResults.childCount > 0)
                searchResults[searchResults.childCount - 1].Q("Item").AddToClassList("last");

            RefreshIndex(true);
        }

        void RefreshIndex(bool autoScroll)
        {
            if (!string.IsNullOrEmpty(searchField.text) && searchResults.childCount > 0 && index < 0)
                index = 0;
            else
                index = Mathf.Clamp(index, -1, searchResults.childCount - 1);

            for (int i = 0; i < searchResults.childCount; i++)
            {
                var element = searchResults[i];

                if (index != i)
                {
                    SetItemBackground(element.Q("Item"), null, false);
                    continue;
                }

                SetItemBackground(element.Q("Item"), element.userData as IMenuItem, true);
                if (autoScroll)
                    searchResults.ScrollTo(element);
                continue;

            }
        }

        void SetItemInfo(VisualElement element, IMenuItem item)
        {
            element.Q<Label>("ItemCategory").text = item.category;
            element.Q<Label>("ItemCategory").style.backgroundColor = item.GetCategoryColor(.6f);

            if (string.IsNullOrEmpty(item.subCategory))
            {
                element.Q<Label>("ItemSubCategory").RemoveFromClassList("with-category");
            }
            else
            {
                element.Q<Label>("ItemCategory").AddToClassList("with-sub-category");

                element.Q<Label>("ItemSubCategory").text = item.subCategory;
                element.Q<Label>("ItemSubCategory").style.backgroundColor = item.GetSubCategoryColor(.6f);
            }

            var fontStyle = item.visible ? FontStyle.Bold : FontStyle.Normal;

            element.Q<Label>("ItemTitle").text = item.title;
            element.Q<Label>("ItemTitle").style.unityFontStyleAndWeight = fontStyle;

            var itemDescription = element.Q<Label>("ItemDescription");
            if (string.IsNullOrWhiteSpace(item.description))
            {
                itemDescription.style.display = DisplayStyle.None;
                return;
            }

            itemDescription.style.display = DisplayStyle.Flex;
            itemDescription.text = item.description;
        }

        void SetItemBackground(VisualElement element, IMenuItem item, bool hovered)
        {
            if (hovered)
            {
                element.style.backgroundColor = item.GetCategoryColor(.25f);
                element.style.SetBorderColor(item.GetCategoryColor(.65f));
                return;
            }

            element.style.backgroundColor = new Color(.133f, .133f, .133f);
            element.style.SetBorderColor(new Color());
        }

        void ExecuteCommand()
        {
            if (!item.Command(context))
            {
                if (itemParameters.childCount > 0)
                {
                    var focusElement = itemParameters.focusController.focusedElement;
                    focusElement.Blur();
                    schedule.Execute(() => focusElement.Focus());
                }
                return;
            }

            Close();

        }

        void SearchField_ValueChanged(ChangeEvent<string> e)
        {
            RefreshSearchResults();
        }

        void OnKeyDown(KeyDownEvent e)
        {
            if (item is null && (e.keyCode == KeyCode.DownArrow || e.keyCode == KeyCode.UpArrow))
            {
                e.ForcePreventDefault();

                if (e.keyCode == KeyCode.DownArrow)
                    index++;
                else
                    index = Mathf.Max(0, index - 1);

                RefreshIndex(true);
                return;
            }

            switch (e.keyCode)
            {
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    OnKeyDown_Return(e);
                    break;

                case KeyCode.Escape:
                    OnKeyDown_Escape(e);
                    break;
            }
        }

        void OnKeyUp(KeyUpEvent e)
        {
            if (e.keyCode == KeyCode.Menu)
                e.ForcePreventDefault();
        }

        void OnKeyDown_Return(KeyDownEvent e)
        {
            if (e is not null)
                e.StopPropagation();

            if (item is not null)
            {
                ExecuteCommand();
                return;
            }

            ClickSearchResult();
        }

        void ClickSearchResult()
        {
            itemParameters.Clear();

            if (index == -1)
            {
                searchField.Blur();
                schedule.Execute(searchField.Focus);
                return;
            }

            item = searchResults[index].userData as IMenuItem;

            searchArea.Show(false);
            itemParametersArea.Show(true);

            var itemElement = root.Q("ItemHeader").Q("Item");
            SetItemInfo(itemElement, item);
            SetItemBackground(itemElement, item, true);

            var parameterFields = item.GetParameterFields();
            if (parameterFields is not null)
            {
                foreach (var field in parameterFields)
                {
                    field.name = "ItemParameter";
                    field.RegisterCallback<KeyDownEvent>(OnKeyDown);
                    itemParameters.Add(field);
                }
            }

            if (itemParameters.childCount == 0)
            {
                ExecuteCommand();
                return;
            }

            schedule.Execute(itemParameters[0].Focus);
        }

        void OnKeyDown_Escape(KeyDownEvent e)
        {
            if (e is not null)
                e.StopPropagation();

            if (item is null)
            {
                Close();
                return;
            }

            item = null;

            searchArea.Show(true);
            itemParametersArea.Show(false);

            itemParameters.Clear();

            RefreshSearchResults();

            schedule.Execute(searchField.Focus);
        }
    }
}
