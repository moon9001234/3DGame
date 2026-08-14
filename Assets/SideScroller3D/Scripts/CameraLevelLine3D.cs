using System.Collections.Generic;
using UnityEngine;

public class CameraLevelLine3D : MonoBehaviour
{
    private static readonly List<CameraLevelLine3D> ActiveControllers = new List<CameraLevelLine3D>();
    private static readonly List<LineState> RuntimeLines = new List<LineState>();
    private static int lastEvaluateFrame = -1;
    private static bool hasSelectedCameraY;
    private static float selectedCameraY;
    private static int selectedLevelIndex;
    private static bool hasFallTrackingLevel;
    private static int highestFallTrackingLevelIndex;
    private static bool lethalFallTriggered;

    [Header("Camera Level Line")]
    [Tooltip("Camera controller to drive. If empty, the Main Camera SideScrollerCamera is used.")]
    [SerializeField] private SideScrollerCamera sideScrollerCamera;

    [Tooltip("Player used for height checks. If empty, the first PlayerMotor3D is used.")]
    [SerializeField] private PlayerMotor3D player;

    [Tooltip("World Y height where the camera switches to the next level.")]
    [SerializeField] private float switchHeight = 6f;

    [Tooltip("Number of level lines generated from this controller.")]
    [SerializeField] private int lineCount = 1;

    [Tooltip("World Y spacing between generated level lines.")]
    [SerializeField] private float lineSpacing = 3f;

    [Tooltip("Camera Y used below the first line when scene camera Y is not used.")]
    [SerializeField] private float lowerCameraY = 8.4f;

    [Tooltip("Camera Y used above the first line. When scene camera Y is used, only the offset from lowerCameraY is used.")]
    [SerializeField] private float upperCameraY = 14.4f;

    [Tooltip("Use the scene Main Camera Y as the first runtime camera level.")]
    [SerializeField] private bool useSceneCameraYAsLowerLevel = true;

    [Tooltip("Padding around each line before switching levels.")]
    [SerializeField] private float switchPadding = 0.15f;

    [Tooltip("When multiple CameraLevelLine3D controllers overlap, the highest priority controller drives the camera.")]
    [SerializeField] private int priority;

    [Tooltip("Width of the level line gizmo in the Scene view.")]
    [SerializeField] private float gizmoWidth = 60f;

    [Header("Fall Death")]
    [Tooltip("Kill the player after falling down too many camera level lines.")]
    [SerializeField] private bool killPlayerWhenFallingTooManyLines = true;

    [Tooltip("Number of crossed level lines required to kill the player while falling.")]
    [SerializeField] private int lethalFallLineCount = 3;

    private void OnEnable()
    {
        if (!ActiveControllers.Contains(this))
        {
            ActiveControllers.Add(this);
        }

        ResolveReferences();
        hasSelectedCameraY = false;
        ResetFallDeathTracking();
        lastEvaluateFrame = -1;
    }

    private void LateUpdate()
    {
        EvaluateAllLines();
    }

    private void OnDisable()
    {
        ActiveControllers.Remove(this);
        hasSelectedCameraY = false;
        ResetFallDeathTracking();
        lastEvaluateFrame = -1;

        SideScrollerCamera cameraController = ResolveCamera();
        if (cameraController != null && ActiveControllers.Count == 0)
        {
            cameraController.ClearCameraLevelY();
        }
    }

    private static void EvaluateAllLines()
    {
        if (lastEvaluateFrame == Time.frameCount)
        {
            return;
        }

        lastEvaluateFrame = Time.frameCount;
        RuntimeLines.Clear();

        CameraLevelLine3D controller = null;
        PlayerMotor3D sharedPlayer = null;

        for (int i = ActiveControllers.Count - 1; i >= 0; i--)
        {
            CameraLevelLine3D candidate = ActiveControllers[i];
            if (candidate == null || !candidate.isActiveAndEnabled)
            {
                ActiveControllers.RemoveAt(i);
                continue;
            }

            candidate.ResolveReferences();
            if (candidate.sideScrollerCamera == null || candidate.player == null)
            {
                continue;
            }

            if (controller == null || candidate.priority > controller.priority)
            {
                controller = candidate;
                sharedPlayer = candidate.player;
            }

            candidate.AppendRuntimeLines(RuntimeLines);
        }

        if (controller == null || sharedPlayer == null || RuntimeLines.Count == 0)
        {
            return;
        }

        float groundCheckY = sharedPlayer.GroundCheckPosition.y;
        EnsureCameraBounds(controller.sideScrollerCamera);
        SelectCameraYFromHeightLevels(groundCheckY);
        HandleLethalFall(controller, sharedPlayer, groundCheckY);
        controller.sideScrollerCamera.SetCameraLevelY(selectedCameraY);
    }

    private static void HandleLethalFall(CameraLevelLine3D controller, PlayerMotor3D sharedPlayer, float groundCheckY)
    {
        if (controller == null || sharedPlayer == null || !controller.killPlayerWhenFallingTooManyLines || RuntimeLines.Count == 0)
        {
            return;
        }

        int currentLevelIndex = GetInitialLevelIndex(groundCheckY);
        if (!hasFallTrackingLevel)
        {
            highestFallTrackingLevelIndex = currentLevelIndex;
            hasFallTrackingLevel = true;
            lethalFallTriggered = false;
            return;
        }

        if (currentLevelIndex > highestFallTrackingLevelIndex)
        {
            highestFallTrackingLevelIndex = currentLevelIndex;
            lethalFallTriggered = false;
            return;
        }

        int requiredLineCount = Mathf.Max(1, controller.lethalFallLineCount);
        if (highestFallTrackingLevelIndex - currentLevelIndex < requiredLineCount || lethalFallTriggered)
        {
            return;
        }

        Health health = sharedPlayer.GetComponent<Health>();
        if (health == null)
        {
            health = sharedPlayer.GetComponentInParent<Health>();
        }

        if (health == null)
        {
            health = sharedPlayer.GetComponentInChildren<Health>();
        }

        if (health != null)
        {
            health.Kill();
        }

        lethalFallTriggered = true;
        highestFallTrackingLevelIndex = currentLevelIndex;
    }

    private static void ResetFallDeathTracking()
    {
        hasFallTrackingLevel = false;
        highestFallTrackingLevelIndex = -1;
        lethalFallTriggered = false;
    }

    private static void EnsureCameraBounds(SideScrollerCamera cameraController)
    {
        if (cameraController == null || RuntimeLines.Count == 0)
        {
            return;
        }

        float minCameraY = RuntimeLines[0].LowerCameraY;
        float maxCameraY = RuntimeLines[0].UpperCameraY;
        for (int i = 1; i < RuntimeLines.Count; i++)
        {
            minCameraY = Mathf.Min(minCameraY, RuntimeLines[i].LowerCameraY);
            maxCameraY = Mathf.Max(maxCameraY, RuntimeLines[i].UpperCameraY);
        }

        cameraController.EnsureCameraYBounds(minCameraY - 1f, maxCameraY + 1f);
    }

    private void AppendRuntimeLines(List<LineState> lines)
    {
        int count = Mathf.Max(1, lineCount);
        float spacing = Mathf.Max(0.01f, lineSpacing);
        float cameraStep = CalculateCameraStep();
        float runtimeLowerCameraY = GetRuntimeLowerCameraY();
        float runtimeUpperCameraY = runtimeLowerCameraY + (upperCameraY - lowerCameraY);

        for (int i = 0; i < count; i++)
        {
            lines.Add(new LineState(
                switchHeight + spacing * i,
                i == 0 ? runtimeLowerCameraY : runtimeUpperCameraY + cameraStep * (i - 1),
                runtimeUpperCameraY + cameraStep * i,
                Mathf.Max(0f, switchPadding),
                priority));
        }
    }

    private float GetRuntimeLowerCameraY()
    {
        if (useSceneCameraYAsLowerLevel && sideScrollerCamera != null)
        {
            return sideScrollerCamera.SceneCameraY;
        }

        return lowerCameraY;
    }

    private float CalculateCameraStep()
    {
        return Mathf.Max(0.01f, lineSpacing);
    }

    private static void SelectCameraYFromHeightLevels(float groundCheckY)
    {
        RuntimeLines.Sort(CompareLinesByHeight);

        if (!hasSelectedCameraY)
        {
            selectedLevelIndex = GetInitialLevelIndex(groundCheckY);
            selectedCameraY = GetCameraYForLevelIndex(selectedLevelIndex);
            hasSelectedCameraY = true;
            return;
        }

        if (selectedLevelIndex + 1 < RuntimeLines.Count)
        {
            LineState nextLine = RuntimeLines[selectedLevelIndex + 1];
            if (groundCheckY > nextLine.Height + nextLine.Padding)
            {
                selectedLevelIndex++;
            }
        }

        if (selectedLevelIndex >= 0 && selectedLevelIndex < RuntimeLines.Count)
        {
            LineState currentLine = RuntimeLines[selectedLevelIndex];
            if (groundCheckY < currentLine.Height - currentLine.Padding)
            {
                selectedLevelIndex--;
            }
        }

        selectedLevelIndex = Mathf.Clamp(selectedLevelIndex, -1, RuntimeLines.Count - 1);
        selectedCameraY = GetCameraYForLevelIndex(selectedLevelIndex);
    }

    private static int GetInitialLevelIndex(float groundCheckY)
    {
        int levelIndex = -1;
        for (int i = 0; i < RuntimeLines.Count; i++)
        {
            if (groundCheckY >= RuntimeLines[i].Height)
            {
                levelIndex = i;
            }
        }

        return levelIndex;
    }

    private static float GetCameraYForLevelIndex(int levelIndex)
    {
        if (RuntimeLines.Count == 0)
        {
            return 0f;
        }

        if (levelIndex < 0)
        {
            return RuntimeLines[0].LowerCameraY;
        }

        int clampedIndex = Mathf.Clamp(levelIndex, 0, RuntimeLines.Count - 1);
        return RuntimeLines[clampedIndex].UpperCameraY;
    }

    private static int CompareLinesByHeight(LineState a, LineState b)
    {
        int heightCompare = a.Height.CompareTo(b.Height);
        return heightCompare != 0 ? heightCompare : b.Priority.CompareTo(a.Priority);
    }

    private void ResolveReferences()
    {
        ResolveCamera();
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerMotor3D>();
        }
    }

    private SideScrollerCamera ResolveCamera()
    {
        if (sideScrollerCamera != null)
        {
            return sideScrollerCamera;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            sideScrollerCamera = mainCamera.GetComponent<SideScrollerCamera>();
        }

        if (sideScrollerCamera == null)
        {
            sideScrollerCamera = FindFirstObjectByType<SideScrollerCamera>();
        }

        return sideScrollerCamera;
    }

    private void OnDrawGizmos()
    {
        int count = Mathf.Max(1, lineCount);
        float spacing = Mathf.Max(0.01f, lineSpacing);

        for (int i = 0; i < count; i++)
        {
            DrawLineGizmo(switchHeight + spacing * i, i);
        }
    }

    private void DrawLineGizmo(float height, int lineIndex)
    {
        Vector3 center = transform.position;
        center.y = height;
        Vector3 left = center + Vector3.left * (gizmoWidth * 0.5f);
        Vector3 right = center + Vector3.right * (gizmoWidth * 0.5f);

        float alpha = lineIndex == 0 ? 0.9f : 0.55f;
        Gizmos.color = new Color(0f, 0.9f, 1f, alpha);
        Gizmos.DrawLine(left, right);

        float padding = Mathf.Max(0f, switchPadding);
        if (padding <= 0f)
        {
            return;
        }

        Gizmos.color = new Color(0f, 0.9f, 1f, alpha * 0.35f);
        Gizmos.DrawLine(left + Vector3.up * padding, right + Vector3.up * padding);
        Gizmos.DrawLine(left + Vector3.down * padding, right + Vector3.down * padding);
    }

    private readonly struct LineState
    {
        public LineState(float height, float lowerCameraY, float upperCameraY, float padding, int priority)
        {
            Height = height;
            LowerCameraY = lowerCameraY;
            UpperCameraY = upperCameraY;
            Padding = padding;
            Priority = priority;
        }

        public float Height { get; }
        public float LowerCameraY { get; }
        public float UpperCameraY { get; }
        public float Padding { get; }
        public int Priority { get; }
    }
}
