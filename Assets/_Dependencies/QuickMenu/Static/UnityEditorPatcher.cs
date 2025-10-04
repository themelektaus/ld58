using HarmonyLib;

using UnityEditor;

using UnityEngine;

using Flags = System.Reflection.BindingFlags;

namespace QuickMenu
{
    internal static class UnityEditorPatcher
    {
        static bool ShouldShowUp(Event e)
        {
#if UNITY_2023_2_OR_NEWER && !UNITY_6000_0_OR_NEWER
            return e.control;
#else
            return !e.control;
#endif
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        static void Patch()
        {
            var harmony = new Harmony("com.unity.editor.quickmenu");

            harmony.UnpatchAll();

            Patch_ProjectBrowser(harmony);
            Patch_SceneHierarchy_1(harmony);
            Patch_SceneHierarchy_2(harmony);
            Patch_AnimatorControllerTool(harmony);
            //Patch_FindObjectsOfType(harmony);
        }



        static void Patch_ProjectBrowser(Harmony harmony)
        {
            var name = nameof(HandleContextClickInListArea);
            var original = Utils.projectBrowser.GetMethod(name, Flags.Instance | Flags.NonPublic);
            var prefix = typeof(UnityEditorPatcher).GetMethod(name, Flags.Static | Flags.NonPublic);
            harmony.Patch(original, prefix: new(prefix));
        }

        static void Patch_SceneHierarchy_1(Harmony harmony)
        {
            var name = nameof(ItemContextClick);
            var original = Utils.sceneHierarchy.GetMethod(name, Flags.Instance | Flags.NonPublic);
            var prefix = typeof(UnityEditorPatcher).GetMethod(nameof(ItemContextClick), Flags.Static | Flags.NonPublic);
            harmony.Patch(original, prefix: new(prefix));
        }

        static void Patch_SceneHierarchy_2(Harmony harmony)
        {
            var name = nameof(ContextClickOutsideItems);
            var original = Utils.sceneHierarchy.GetMethod(name, Flags.Instance | Flags.NonPublic);
            var prefix = typeof(UnityEditorPatcher).GetMethod(name, Flags.Static | Flags.NonPublic);
            harmony.Patch(original, prefix: new(prefix));
        }

        static void Patch_AnimatorControllerTool(Harmony harmony)
        {
            var name = nameof(DoGraph);
            var original = Utils.animatorControllerTool.GetMethod(name, Flags.Instance | Flags.NonPublic);
            var prefix = typeof(UnityEditorPatcher).GetMethod(name, Flags.Static | Flags.NonPublic);
            harmony.Patch(original, prefix: new(prefix));
        }

        //static void Patch_FindObjectsOfType(Harmony harmony)
        //{
        //    var name = nameof(FindObjectsOfType);
        //    var methods = typeof(Object).GetMethods(Flags.Static | Flags.Public);
        //    MethodInfo original = null;
        //    foreach (var method in methods)
        //    {
        //        if (method.Name != "FindObjectsOfType")
        //            continue;
        //
        //        var g = method.GetGenericArguments();
        //        if (g.Length > 0)
        //            continue;
        //
        //        if (method.GetParameters().Length < 2)
        //            continue;
        //
        //        original = method;
        //
        //        Debug.Log(original.FullDescription());
        //
        //        break;
        //    }
        //
        //    if (original is null)
        //    {
        //        Debug.Log("NOT FOUND");
        //        return;
        //
        //    }
        //
        //    var prefix = typeof(UnityEditorPatcher).GetMethod(name, Flags.Static | Flags.NonPublic);
        //    harmony.Patch(original, prefix: new(prefix));
        //}



        static bool HandleContextClickInListArea(Rect listRect)
        {
            var e = Event.current;

            if (!ShouldShowUp(e))
                return true;

            if (e.type != EventType.ContextClick)
                return true;

            if (!listRect.Contains(e.mousePosition))
                return true;

            if (Selection.instanceIDs.Length == 0)
                return true;

            OpenQuickMenu(e, resetHotControl: true);

            return false;
        }

        static bool ContextClickOutsideItems()
        {
            var e = Event.current;

            if (!ShouldShowUp(e))
                return true;

            Selection.activeObject = null;

            OpenQuickMenu(e);

            return false;
        }

        static bool ItemContextClick(int contextClickedItemID)
        {
            var e = Event.current;

            if (!ShouldShowUp(e))
                return true;

            OpenQuickMenu(e);

            return false;
        }

        static bool DoGraph(Rect graphRect, float zoomLevel)
        {
            var e = Event.current;

            if (!ShouldShowUp(e))
                return true;

            if (e.button != 1)
                return true;

            if (e.type != EventType.MouseUp)
                return true;

            if (Selection.activeObject is not UnityEditor.Animations.AnimatorStateMachine)
                return true;

            OpenQuickMenu(e);

            return false;
        }

        //static bool FindObjectsOfType(System.Type type, bool includeInactive)
        //{
        //    Debug.Log("HURN: " + type.FullName);
        //    return false;
        //}



        static void OpenQuickMenu(Event e, bool resetHotControl = false)
        {
            if (resetHotControl)
                GUIUtility.hotControl = 0;

            e.Use();

            var position = e.mousePosition;
            position = GUIUtility.GUIToScreenPoint(position);
            QuickMenuWindow.Open(position);
        }
    }
}
