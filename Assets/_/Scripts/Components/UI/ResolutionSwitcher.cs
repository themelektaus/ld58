using System.Collections.Generic;
using System.Linq;

using UnityEngine;

#if UNITY_EDITOR
using System;
using System.Reflection;

using UnityEditor;
#endif

using Dropdown = NodeEditor.CustomDropdown;

namespace Prototype
{
    [RequireComponent(typeof(Dropdown))]
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Resolution Switcher")]
    public class ResolutionSwitcher : MonoBehaviour
    {
#if UNITY_EDITOR
        static Assembly GetAssembly()
        {
            return typeof(Editor).Assembly;
        }

        static Type GetGameView()
        {
            return GetAssembly().GetType("UnityEditor.GameView");
        }

        static EditorWindow GetGameViewWindow(Type gameView)
        {
            return EditorWindow.GetWindow(gameView);
        }

        static Type GetGameViewSizes()
        {
            return GetAssembly().GetType("UnityEditor.GameViewSizes");
        }

        static object GetGameViewSizesInstance(Type gameViewSizes)
        {
            var singleton = typeof(ScriptableSingleton<>).MakeGenericType(gameViewSizes);
            return singleton.GetProperty("instance").GetValue(null, null);
        }

        static int GetIndex()
        {
            var gameView = GetGameView();
            var window = GetGameViewWindow(gameView);
            var property = window.GetType().GetProperty("selectedSizeIndex");
            return (int) property.GetValue(window);
        }

        static void SetIndex(int index)
        {
            var gameView = GetGameView();
            var window = GetGameViewWindow(gameView);
            var method = gameView.GetMethod("SizeSelectionCallback");
            method.Invoke(window, new object[] { index, null });
        }

        static List<string> GetEntries()
        {
            var gameViewSizes = GetGameViewSizes();
            var gameViewSizesInstance = GetGameViewSizesInstance(gameViewSizes);
            var group = gameViewSizes.GetMethod("GetGroup").Invoke(gameViewSizesInstance, new object[] { 0 });
            var texts = group.GetType().GetMethod("GetDisplayTexts");
            return (texts.Invoke(group, null) as string[]).ToList();
        }

        void Start()
        {
            var entries = GetEntries();

            var dropdown = GetComponent<Dropdown>();

            foreach (var entry in entries)
                dropdown.CreateNewItemFast(entry, null);

            dropdown.SetupDropdown();

            var index = GetIndex();
            if (index != -1)
                dropdown.ChangeDropdownInfo(index);

            dropdown.dropdownEvent.AddListener(index => SetIndex(index));
        }
#else
        List<(int index, int width, int height)> items;

        void Start()
        {
            items = Screen.resolutions
                .Select(x => (-1, x.width, x.height))
                .Distinct()
                .ToList();

            for (var i = 0; i < items.Count; i++)
                items[i] = (i, items[i].width, items[i].height);

            var dropdown = GetComponent<Dropdown>();

            foreach (var (_, width, height) in items)
                dropdown.CreateNewItemFast($"{width}x{height}", null);

            dropdown.SetupDropdown();

            if (items.Count > 0)
            {
                dropdown.ChangeDropdownInfo(
                    items
                        .OrderBy(x => Mathf.Abs(Screen.width - x.width) + Mathf.Abs(Screen.height - x.height))
                        .FirstOrDefault().index
                );
            }

            dropdown.dropdownEvent.AddListener(index =>
            {
                var (_, width, height) = items[index];
                Screen.SetResolution(width, height, Screen.fullScreen);
            });
        }
#endif
    }
}
