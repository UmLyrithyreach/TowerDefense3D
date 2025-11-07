using UnityEngine;

/// <summary>
/// Smooth, right-click drag camera panning with optional clamping and height lock.
/// </summary>
[DisallowMultipleComponent]
public class CameraMovement : MonoBehaviour
{
    [Header("Pan Settings")]
    [Tooltip("Speed multiplier for right-click drag panning.")]
    [Min(0.1f)] public float panSpeed = 25f;

    [Tooltip("Smoothing factor for movement (0 = instant).")]
    [Range(0f, 20f)] public float smooth = 8f;

    [Header("Height Lock / Zoom")]
    [Tooltip("If true, keeps the camera Y constant.")]
    public bool lockHeight = true;

    [Tooltip("Fixed Y height when lockHeight is true. Leave 0 to use initial height.")]
    public float fixedHeight = 0f;

    [Header("Bounds")]
    [Tooltip("Clamp movement to active terrain bounds.")]
    public bool clampToTerrain = true;

    [Tooltip("Extra padding from terrain edges when clamping.")]
    [Min(0f)] public float clampPadding = 5f;

    [Tooltip("Use manual bounds instead of terrain.")]
    public bool useManualBounds = false;

    [Tooltip("Manual bounds (X,Z).")]
    public Vector2 manualMinXZ = new(-50f, -50f);
    public Vector2 manualMaxXZ = new(50f, 50f);

    // --- Private ---
    private float _initialHeight;
    private Vector3 _targetPosition;
    private Terrain _terrain;

    private void Awake()
    {
        _terrain = Terrain.activeTerrain;
        _targetPosition = transform.position;
        _initialHeight = fixedHeight != 0f ? fixedHeight : transform.position.y;
    }

    private void Update()
    {
        HandlePan();
        if (lockHeight) _targetPosition.y = _initialHeight;
        ClampToBounds();

        // Apply smooth movement
        transform.position = smooth > 0f
            ? Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * smooth)
            : _targetPosition;
    }

    private void HandlePan()
    {
        if (!Input.GetMouseButton(1)) return; // Only pan on right-click drag

        float dx = Input.GetAxis("Mouse X");
        float dy = Input.GetAxis("Mouse Y");

        if (Mathf.Approximately(dx, 0f) && Mathf.Approximately(dy, 0f)) return;

        // Flattened camera directions (XZ plane)
        Vector3 forwardFlat = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 rightFlat = new Vector3(transform.right.x, 0f, transform.right.z).normalized;

        // Drag direction (invert to feel more natural)
        Vector3 delta = (-rightFlat * dx - forwardFlat * dy) * panSpeed * Time.deltaTime;
        _targetPosition += delta;
    }

    private void ClampToBounds()
    {
        if (clampToTerrain && _terrain != null)
        {
            Vector3 origin = _terrain.transform.position;
            Vector3 size = _terrain.terrainData.size;

            float minX = origin.x + clampPadding;
            float maxX = origin.x + size.x - clampPadding;
            float minZ = origin.z + clampPadding;
            float maxZ = origin.z + size.z - clampPadding;

            _targetPosition.x = Mathf.Clamp(_targetPosition.x, minX, maxX);
            _targetPosition.z = Mathf.Clamp(_targetPosition.z, minZ, maxZ);
        }
        else if (useManualBounds)
        {
            _targetPosition.x = Mathf.Clamp(_targetPosition.x, manualMinXZ.x + clampPadding, manualMaxXZ.x - clampPadding);
            _targetPosition.z = Mathf.Clamp(_targetPosition.z, manualMinXZ.y + clampPadding, manualMaxXZ.y - clampPadding);
        }
    }
}
