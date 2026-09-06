using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor(typeof(MovableObject))]
public class ClickHandler : Editor
{
    private void OnSceneGUI()
    {
        // // 1. Захватываем приоритет событий
        // int controlID = GUIUtility.GetControlID(FocusType.Passive);
        // HandleUtility.AddDefaultControl(controlID);
        //
        // // 2. Собираем все выделенные объекты с нужным компонентом
        // var selectedSetters = Selection.gameObjects
        //     .Select(go => go.GetComponent<MovableObject>())
        //     .Where(s => s != null)
        //     .ToArray();
        //
        // if (selectedSetters.Length == 0) return;
        //
        // Event e = Event.current;
        //
        // // 3. Обрабатываем клик левой кнопкой (без флага)
        // if (e.type == EventType.MouseDown && e.button == 0)
        // {
        //     // Вариант А: плоскость по первому объекту (если Z не ноль)
        //     Vector3 referencePos = selectedSetters[0].transform.position;
        //     Plane plane = new Plane(Vector3.forward, referencePos);
        //
        //     Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        //     if (plane.Raycast(ray, out float distance))
        //     {
        //         Vector3 hitPoint = ray.GetPoint(distance);
        //
        //         // Назначаем точку всем
        //         foreach (var setter in selectedSetters)
        //         {
        //             Undo.RecordObject(setter, "Set Target Point");
        //             setter.targetPoint = hitPoint;
        //             EditorUtility.SetDirty(setter);
        //         }
        //
        //         SceneView.RepaintAll();
        //         e.Use(); // Забираем событие себе
        //     }
        // }
        //
        // // 4. Визуализация точек для выделенных объектов
        // Handles.color = Color.green;
        // foreach (var setter in selectedSetters)
        // {
        //     Handles.DrawSolidDisc(setter.targetPoint, Vector3.forward, 0.2f);
        // }
    }
}