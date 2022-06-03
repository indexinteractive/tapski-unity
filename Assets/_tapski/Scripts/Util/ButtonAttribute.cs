using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// https://gist.github.com/jbubriski/585164db8dd0592c4bc957caeb368f8b

/// <summary>
/// Put this attribute above one of your MonoBehaviour method and it will draw
/// a button in the inspector automatically.
///
/// NOTE: the method must not have any params and can not be static.
///
/// <code>
/// <para>[Button]</para>
/// <para>public void MyMethod()</para>
/// <para>{</para>
/// <para>    Debug.Log( "HELLO HELLO HELLO!!" );</para>
/// <para>}</para>
/// </code>
/// </summary>

#if UNITY_EDITOR
[System.AttributeUsage(System.AttributeTargets.Method)]
public class ButtonAttribute : System.Attribute { }

/// <summary>
/// Searches through a target class in order to find all button attributes (<see cref="ButtonAttribute"/>).
/// </summary>
public class ButtonAttributeHelper
{
    private static object[] emptyParamList = new object[0];

    private IList<MethodInfo> methods = new List<MethodInfo>();
    private Object targetObject;

    public void Init(Object targetObject)
    {
        this.targetObject = targetObject;
        methods =
            targetObject.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m =>
                        m.GetCustomAttributes(typeof(ButtonAttribute), false).Length == 1 &&
                        m.GetParameters().Length == 0 &&
                        !m.ContainsGenericParameters
                ).ToList();
    }

    public void DrawButtons()
    {
        if (methods.Count > 0)
        {
            EditorGUILayout.HelpBox("Click to execute methods!", MessageType.None);
            ShowMethodButtons();
        }
    }

    private void ShowMethodButtons()
    {
        foreach (MethodInfo method in methods)
        {
            string buttonText = ObjectNames.NicifyVariableName(method.Name);
            if (GUILayout.Button(buttonText))
            {
                method.Invoke(targetObject, emptyParamList);
            }
        }
    }
}
#endif