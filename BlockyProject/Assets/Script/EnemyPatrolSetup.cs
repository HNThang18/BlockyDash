using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class PatrolSetup
{
    [Header("Patrol Point Setup")]
    [Tooltip("Distance between patrol points")]
    public float patrolDistance = 5f;
    
    [Tooltip("Create points horizontally (left/right)")]
    public bool horizontalPatrol = true;
    
    [Tooltip("Offset from enemy position for point A")]
    public Vector2 pointAOffset = Vector2.left * 2.5f;
    
    [Tooltip("Offset from enemy position for point B")]
    public Vector2 pointBOffset = Vector2.right * 2.5f;
}

public class EnemyPatrolSetup : MonoBehaviour
{
    [Header("Automatic Patrol Point Creation")]
    public PatrolSetup patrolSetup = new PatrolSetup();
    
    [Space]
    [Header("Manual Assignment")]
    [Tooltip("If you already have patrol points, assign them here")]
    public Transform existingPointA;
    public Transform existingPointB;
    
    #if UNITY_EDITOR
    [Space]
    [Header("Setup Tools")]
    [Button("Create Patrol Points")]
    public bool createPatrolPoints;
    
    [Button("Clear Patrol Points")]
    public bool clearPatrolPoints;
    #endif
    
    private void Start()
    {
        // Auto-assign existing points if they're set
        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null)
        {
            if (existingPointA != null && existingPointB != null)
            {
                enemy.pointA = existingPointA;
                enemy.pointB = existingPointB;
                Debug.Log($"Assigned existing patrol points to {gameObject.name}");
            }
        }
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(EnemyPatrolSetup))]
    public class EnemyPatrolSetupEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            EnemyPatrolSetup setup = (EnemyPatrolSetup)target;
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Create Patrol Points"))
            {
                setup.CreatePatrolPoints();
            }
            
            if (GUILayout.Button("Clear Patrol Points"))
            {
                setup.ClearPatrolPoints();
            }
        }
    }
    #endif
    
    public void CreatePatrolPoints()
    {
        // Clear existing points first
        ClearPatrolPoints();
        
        // Create Point A
        GameObject pointAObj = new GameObject($"{gameObject.name}_PatrolPoint_A");
        pointAObj.transform.position = transform.position + (Vector3)patrolSetup.pointAOffset;
        pointAObj.transform.parent = transform.parent; // Put in same parent as enemy
        
        PatrolPoint pointAScript = pointAObj.AddComponent<PatrolPoint>();
        pointAScript.pointName = "Point A";
        pointAScript.gizmoColor = Color.green;
        
        // Create Point B
        GameObject pointBObj = new GameObject($"{gameObject.name}_PatrolPoint_B");
        pointBObj.transform.position = transform.position + (Vector3)patrolSetup.pointBOffset;
        pointBObj.transform.parent = transform.parent; // Put in same parent as enemy
        
        PatrolPoint pointBScript = pointBObj.AddComponent<PatrolPoint>();
        pointBScript.pointName = "Point B";
        pointBScript.gizmoColor = Color.blue;
        
        // Assign to enemy
        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.pointA = pointAObj.transform;
            enemy.pointB = pointBObj.transform;
            
            // Mark as dirty for saving
            #if UNITY_EDITOR
            EditorUtility.SetDirty(enemy);
            EditorUtility.SetDirty(this);
            #endif
        }
        
        // Update the existing point references
        existingPointA = pointAObj.transform;
        existingPointB = pointBObj.transform;
        
        Debug.Log($"Created patrol points for {gameObject.name}");
    }
    
    public void ClearPatrolPoints()
    {
        // Find and destroy existing patrol points
        Transform[] children = GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            if (child != transform && child.name.Contains("PatrolPoint"))
            {
                #if UNITY_EDITOR
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
                #else
                Destroy(child.gameObject);
                #endif
            }
        }
        
        // Also look for patrol points in the same parent
        if (transform.parent != null)
        {
            PatrolPoint[] patrolPoints = transform.parent.GetComponentsInChildren<PatrolPoint>();
            foreach (PatrolPoint point in patrolPoints)
            {
                if (point.name.Contains(gameObject.name))
                {
                    #if UNITY_EDITOR
                    if (Application.isPlaying)
                        Destroy(point.gameObject);
                    else
                        DestroyImmediate(point.gameObject);
                    #else
                    Destroy(point.gameObject);
                    #endif
                }
            }
        }
        
        // Clear references
        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.pointA = null;
            enemy.pointB = null;
        }
        
        existingPointA = null;
        existingPointB = null;
        
        Debug.Log($"Cleared patrol points for {gameObject.name}");
    }
}

// Custom attribute for button-like behavior in inspector
public class ButtonAttribute : PropertyAttribute
{
    public string MethodName { get; }
    
    public ButtonAttribute(string methodName)
    {
        MethodName = methodName;
    }
}
