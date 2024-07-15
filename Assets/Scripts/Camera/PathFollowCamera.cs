using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class PathFollowCamera : SpectatorCamera
{
    [System.Serializable]
    public class PathLookAtPair
    {
        public Vector3 path;
        public Vector3 lookAt;
    }
    [SerializeField] private LayerMask interest;
    [SerializeField] private LayerMask vis;
    [SerializeField] private float radius = 50;


#if UNITY_EDITOR
    [SerializeField] public bool showPath;
    [SerializeField] public bool editPath;
    [SerializeField] public bool showLookAt;
    [SerializeField] public bool editLookAt;
    [HideInInspector] public Vector3 lookAt = Vector3.zero;
#endif


    [SerializeField] public List<PathLookAtPair> paths = new List<PathLookAtPair>();


    [SerializeField] private float speed = 1;

    [ReadOnlyField] [SerializeField] private int next;

    [ReadOnlyField] [SerializeField] private Transform focusCar;

    

    private float currentTime;

    private void Update()
    {
        if (camera.enabled)
        {
            if(focus == CameraFocus.Map || focusCar == null)
            {
                currentTime += Time.deltaTime * speed;

                if(currentTime < paths.Count - 1)
                {
                    transform.position = Vector3.Lerp(paths[(int)currentTime].path, paths[(int)currentTime + 1].path, currentTime % 1);
                    transform.LookAt(Vector3.Lerp(paths[(int)currentTime].lookAt, paths[(int)currentTime + 1].lookAt, currentTime % 1));
                }
                else
                {
                    onMapShown();
                    Debug.Log(gameObject.name);
                }
            }
            else
            {
                transform.LookAt(focusCar.position);
                if(!Physics.Raycast(transform.position, focusCar.position-transform.position,out RaycastHit hit, radius*3f, vis) || !(interest == (interest | (1 << hit.transform.gameObject.layer))))
                {
                    Vector3 right = transform.position + Vector3.right;
                    if (!Physics.Raycast(right, focusCar.position - right, out RaycastHit hit2, radius * 3f, vis) || !(interest == (interest | (1 << hit2.transform.gameObject.layer))))
                    {
                        Vector3 left = transform.position + Vector3.left;
                        if (!Physics.Raycast(left, focusCar.position - left, out RaycastHit hit3, radius * 3f, vis) || !(interest == (interest | (1 << hit3.transform.gameObject.layer))))
                        {
                            findInterest();
                        }
                    }
                }

            }
        }
    }



    public override void EnableCamera()
    {
        base.EnableCamera();
        if (focus == CameraFocus.Map)
        {
            currentTime = 0;
        }
        else
        {
            findInterest();
        }
    }

    public bool findInterest()
    {
        for (int i = paths.Count - 1; i >= 0; i--)
        {
            Collider[] colliders = Physics.OverlapSphere(paths[i].lookAt, radius, interest.value);
            if (colliders.Length > 0)
            {
                currentTime = i;
                transform.position = paths[i].path;
                focusCar = colliders[0].transform;
                return true;
            }
        }
        return false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (showPath)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < paths.Count - 1; i++)
            {
                Gizmos.DrawLine(paths[i].path, paths[i + 1].path);
            }
        }

        if (showLookAt)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < paths.Count - 1; i++)
            {
                Gizmos.DrawLine(paths[i].lookAt, paths[i + 1].lookAt);
            }
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(lookAt, radius);
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(PathFollowCamera))]
public class DrawWireArc : Editor
{
    void OnSceneGUI()
    {
        Handles.color = Color.red;
        PathFollowCamera pathFollowCamera = target as PathFollowCamera;

        if (pathFollowCamera.paths == null ||
            pathFollowCamera.paths.Count == 0)
            return;

        if (pathFollowCamera.editPath)
        {
            for (int i = 0; i < pathFollowCamera.paths.Count; i++)
            {
                Vector3 x = pathFollowCamera.paths[i].path;
                pathFollowCamera.paths[i].path = Handles.DoPositionHandle(pathFollowCamera.paths[i].path, Quaternion.identity);

                if (pathFollowCamera.paths[i].path != x)
                {
                    pathFollowCamera.transform.position = pathFollowCamera.paths[i].path;
                    pathFollowCamera.transform.LookAt(pathFollowCamera.paths[i].lookAt);
                    pathFollowCamera.lookAt = pathFollowCamera.paths[i].lookAt;
                }
            }
        }

        if (pathFollowCamera.editLookAt)
        {
            for (int i = 0; i < pathFollowCamera.paths.Count; i++)
            {
                Vector3 x = pathFollowCamera.paths[i].lookAt;
                pathFollowCamera.paths[i].lookAt = Handles.DoPositionHandle(pathFollowCamera.paths[i].lookAt, Quaternion.identity);

                if (pathFollowCamera.paths[i].lookAt != x)
                {
                    pathFollowCamera.transform.position = pathFollowCamera.paths[i].path;
                    pathFollowCamera.transform.LookAt(pathFollowCamera.paths[i].lookAt);
                    pathFollowCamera.lookAt = pathFollowCamera.paths[i].lookAt;
                }
            }
        }
    }
}
#endif
