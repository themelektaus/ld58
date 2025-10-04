using Newtonsoft.Json;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

using Image = UnityEngine.UI.Image;

namespace Prototype
{
    [System.Serializable]
    public class Reference
    {
        public const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public enum Type
        {
            Member,
            AnimatorParameter,
            ShaderProperty
        }

        public Type type = Type.Member;
        public Object @object;
        public string path;

        [ReadOnly] public string serializedData;

        object obj;
        object info;
        object[] parameters;
        string sub;
        string internalPath;

        public bool invert { get; private set; }

        int hashCode;

        public bool isEmpty => !@object || path.IsNullOrEmpty();

        void Update()
        {
            var hashCode = System.HashCode.Combine(@object, path);

            if (this.hashCode == hashCode)
                return;

            this.hashCode = hashCode;

            obj = null;
            info = null;
            parameters = null;
            sub = null;
            internalPath = path;
            invert = false;

            if (!@object || internalPath is null)
                return;

            internalPath = internalPath.Trim();
            invert = internalPath.StartsWith('!');

            internalPath = internalPath.TrimStart('!').Trim();
            if (internalPath == string.Empty)
                return;

            obj = @object;

            if (type == Type.Member)
            {
                var parts = internalPath.Split('.');
                for (var i = 0; i < parts.Length; i++)
                {
                    if (obj is null)
                    {
                        Debug.LogWarning($"{internalPath}\nCan not get member \"{parts[i]}\" of null");
                        return;
                    }

                    var match = Regex.Match(parts[i], @"(.+)\((.*)\)$");
                    if (match.Success)
                    {
                        var groups = match.Groups;
                        var methodInfo = obj.GetType().GetMethod(groups[1].Value, FLAGS);

                        var methodParameters = JsonConvert.DeserializeObject<object[]>($"[{groups[2].Value}]");

                        for (var j = 0; j < methodParameters.Length; j++)
                        {
                            if (methodParameters[j] is long longValue)
                                methodParameters[j] = (int) longValue;
                        }

                        if (FinalizeUpdate(methodInfo, methodParameters, parts, i))
                            return;

                        continue;
                    }

                    var fieldInfo = obj.GetType().GetField(parts[i], FLAGS);
                    if (fieldInfo is null)
                    {
                        var propertyInfo = obj.GetType().GetProperty(parts[i], FLAGS);
                        if (propertyInfo is null)
                            return;

                        if (FinalizeUpdate(propertyInfo, null, parts, i))
                            return;

                        continue;
                    }

                    if (FinalizeUpdate(fieldInfo, null, parts, i))
                        return;
                }

                Debug.Log($"Can not find member \"{internalPath}\"");
                return;
            }

            if (type == Type.AnimatorParameter)
            {
                if (@object is Animator animator)
                    info = animator.GetParameter(internalPath);
                return;
            }

            if (type == Type.ShaderProperty)
            {
                if (@object is Material material)
                    info = material;

                if (@object is Renderer renderer)
                    info = renderer.material;

                if (@object is Image image)
                    info = image.material;

                return;
            }
        }

        bool FinalizeUpdate(object info, object[] parameters, string[] parts, int i)
        {
            if (i < parts.Length - 1)
            {
                var type = GetType(info);

                if (type is null)
                {
                    Debug.LogError("info has no type");
                    return true;
                }

                if (type.IsValueType && !type.IsEnum)
                {
                    this.info = info;
                    this.parameters = parameters;
                    sub = type.GetField(parts[i + 1], FLAGS).Name;
                    return true;
                }

                obj = GetValue(obj, info, parameters);

                return false;
            }

            this.info = info;
            this.parameters = parameters;

            return true;
        }

        public System.Type GetFieldType()
        {
            Update();
            if (info is not null)
                return GetType(info);
            return null;
        }

        public Reference Of(Object memberOwner)
        {
            @object = memberOwner;
            return this;
        }

        public object Get()
        {
            Update();

            if (info is null)
                return null;

            if (sub is not null)
            {
                var fieldValue = GetValue(obj, info, parameters);
                return fieldValue.GetType().GetField(sub, FLAGS).GetValue(fieldValue);
            }

            if (type == Type.AnimatorParameter)
                return (obj as Animator).GetParameterValue(internalPath);

            if (type == Type.ShaderProperty)
            {
                if (info is Material material)
                    return material.GetValue(internalPath);

                if (info is Image image)
                    return image.material.GetValue(internalPath);
            }

            return GetValue(obj, info, parameters);
        }

        public T Get<T>(object @object, T defaultValue = default)
        {
            if (@object is null)
                return defaultValue;

            if (@object is SmoothValue<T> smoothValue)
                return smoothValue.current;

            return (T) @object;
        }

        public T Get<T>(T defaultValue = default)
        {
            return Get<T>(Get(), defaultValue);
        }

        public void Set(object value) => Set(value, .5f);

        public void Set(object value, float boolThreshold)
        {
            Update();

            if (info is null)
                return;

            if (sub is not null)
            {
                var fieldValue = GetValue(obj, info, parameters);
                fieldValue.GetType().GetField(sub, FLAGS).SetValue(fieldValue, value);
                SetValue(obj, info, fieldValue, boolThreshold);
                return;
            }

            if (type == Type.AnimatorParameter)
            {
                (obj as Animator).SetParameterValue(internalPath, value);
                return;
            }

            if (type == Type.ShaderProperty)
            {
                (info as Material).SetValue(internalPath, (float) value);
                return;
            }

            SetValue(obj, info, value, boolThreshold);
        }

        static System.Type GetType(object info)
        {
            if (info is FieldInfo fieldInfo)
                return fieldInfo.FieldType;

            if (info is PropertyInfo propertyInfo)
                return propertyInfo.PropertyType;

            if (info is MethodInfo methodInfo)
                return methodInfo.ReturnType;

            return null;
        }

        static object GetValue(object obj, object info, object[] parameters)
        {
            if (info is FieldInfo fieldInfo)
                return fieldInfo.GetValue(obj);

            if (info is PropertyInfo propertyInfo)
                return propertyInfo.GetValue(obj);

            if (info is MethodInfo methodInfo)
                return methodInfo.Invoke(obj, parameters);

            return null;
        }

        static void SetValue(object obj, object info, object value, float boolThreshold)
        {
            if (info is FieldInfo fieldInfo)
            {
                SetValue(fieldInfo.FieldType, value, x => fieldInfo.SetValue(obj, x), boolThreshold);
                return;
            }

            if (info is PropertyInfo propertyInfo)
            {
                SetValue(propertyInfo.PropertyType, value, x => propertyInfo.SetValue(obj, x), boolThreshold);
                return;
            }

            if (info is MethodInfo methodInfo)
            {
                if (value is float floatValue)
                {
                    var parameters = methodInfo.GetParameters();
                    if (parameters.Length == 1)
                    {
                        if (parameters[0].ParameterType == typeof(bool))
                        {
                            methodInfo.Invoke(obj, new object[] { Mathf.Abs(floatValue) > boolThreshold });
                            return;
                        }

                        methodInfo.Invoke(obj, new object[] { floatValue });
                        return;
                    }

                    methodInfo.Invoke(obj, new object[0]);
                    return;
                }
            }
        }

        static void SetValue(System.Type type, object value, System.Action<object> setValue, float boolThreshold)
        {
            if (value is float floatValue)
            {
                if (type == typeof(bool))
                {
                    setValue(Mathf.Abs(floatValue) > boolThreshold);
                    return;
                }

                if (type == typeof(Vector2))
                {
                    setValue(Vector2.one * floatValue);
                    return;
                }

                if (type == typeof(Vector3))
                {
                    setValue(Vector3.one * floatValue);
                    return;
                }
            }

            setValue(value);
        }
    }
}
