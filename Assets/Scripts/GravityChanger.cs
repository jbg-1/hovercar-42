using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GravityChanger : MonoBehaviour {

    public bool useGravityTransformDown = true;
    public float gravityStrength = 10;
    public Vector3 gravityDirection = -Vector3.up;

    private void Start()
    {
        if (useGravityTransformDown)
        {
            gravityDirection = -transform.up * gravityStrength;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        CarController carControllerVar;
        if(other.TryGetComponent<CarController>(out carControllerVar))
        {
            carControllerVar.ChangeGravityDirectionTo(gravityDirection);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position - transform.up * gravityStrength * 2);

    }
#endif

}

#if UNITY_EDITOR
[CustomEditor(typeof(GravityChanger))]
[CanEditMultipleObjects]
public class GravityChangerEditor : Editor
{
    SerializedProperty gravityStrength;
    SerializedProperty gravityDirection;

    void OnEnable()
    {
        gravityStrength = serializedObject.FindProperty("gravityStrength");
        gravityDirection = serializedObject.FindProperty("gravityDirection");
    }

    override public void OnInspectorGUI()
    {
        serializedObject.Update();
        GravityChanger script = target as GravityChanger;

        script.useGravityTransformDown = EditorGUILayout.Toggle("useGravityTransformDown", script.useGravityTransformDown);

        if (script.useGravityTransformDown)
        {
            //script.gravityStrength = EditorGUILayout.FloatField("gravityStrength", script.gravityStrength); 
            EditorGUILayout.PropertyField(gravityStrength);
        }
        else
        {
            //script.gravityDirection = EditorGUILayout.Vector3Field("gravityDirection", script.gravityDirection);
            EditorGUILayout.PropertyField(gravityDirection);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif

