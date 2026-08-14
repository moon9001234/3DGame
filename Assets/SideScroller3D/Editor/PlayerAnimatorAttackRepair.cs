using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[InitializeOnLoad]
public static class PlayerAnimatorAttackRepair
{
    private const string ControllerPath = "Assets/SideScroller3D/Animation/PlayerVisual.controller";
    private const string PlayerVisualPrefabPath = "Assets/SideScroller3D/Prefabs/Player_Model.prefab";
    private const string PlayerFbxPath = "Assets/Art/FBX/Player/TV_Man.fbx";
    private const string FirstAttackStateName = "Attack_01";
    private const string SecondAttackStateName = "Attack_02";
    private const string ThirdAttackStateName = "Attack_03";
    private const string IdleStateName = "Idle";
    private const string RunStateName = "Run";
    private const string AttackIdleStateName = "Atk_Idle";
    private const string DashStateName = "Dash";
    private const string DashEndStateName = "Dash_End";
    private const string JumpUpStateName = "Jump_Up";
    private const string JumpDownStateName = "Jump_Down";
    private const string SpeedParameterName = "Speed";
    private const string GroundedParameterName = "Grounded";
    private const string VerticalSpeedParameterName = "VerticalSpeed";
    private const string InCombatParameterName = "InCombat";
    private const string DashingParameterName = "Dashing";
    private const string SecondAttackTriggerName = "Attack2";
    private const string ThirdAttackTriggerName = "Attack3";
    private const string FirstAttackClipName = "Player_Atk01_01";
    private const string SecondAttackClipName = "Player_Atk01_02";
    private const string ThirdAttackClipName = "Player_Atk01_03";

    static PlayerAnimatorAttackRepair()
    {
        EditorApplication.delayCall += EnsurePlayerAttackStates;
    }

    [MenuItem("Tools/3D 遊戲工具/修復玩家攻擊 Animator")]
    public static void EnsurePlayerAttackStates()
    {
        RepairPlayerAnimator();
    }

    [MenuItem("Tools/3D 遊戲工具/修復玩家 Animator")]
    public static void EnsurePlayerAnimatorStates()
    {
        RepairPlayerAnimator();
    }

    private static void RepairPlayerAnimator()
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null || controller.layers.Length == 0)
        {
            Debug.LogWarning($"Player animator controller not found at {ControllerPath}.");
            return;
        }

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        bool changed = false;

        changed |= EnsureParameter(controller, SpeedParameterName, AnimatorControllerParameterType.Float);
        changed |= EnsureParameter(controller, GroundedParameterName, AnimatorControllerParameterType.Bool);
        changed |= EnsureParameter(controller, VerticalSpeedParameterName, AnimatorControllerParameterType.Float);
        changed |= EnsureParameter(controller, InCombatParameterName, AnimatorControllerParameterType.Bool);
        changed |= EnsureParameter(controller, DashingParameterName, AnimatorControllerParameterType.Bool);
        changed |= EnsureParameter(controller, SecondAttackTriggerName, AnimatorControllerParameterType.Trigger);
        changed |= EnsureParameter(controller, ThirdAttackTriggerName, AnimatorControllerParameterType.Trigger);

        AnimationClip firstAttackClip = FindClip(FirstAttackClipName);
        AnimationClip secondAttackClip = FindClip(SecondAttackClipName);
        AnimationClip thirdAttackClip = FindClip(ThirdAttackClipName);

        AnimatorState firstAttack = FindUniqueState(stateMachine, FirstAttackStateName, ref changed);
        if (firstAttack == null)
        {
            firstAttack = stateMachine.AddState(FirstAttackStateName, new Vector3(120f, 400f, 0f));
            changed = true;
        }

        AnimatorState secondAttack = FindUniqueState(stateMachine, SecondAttackStateName, ref changed);
        if (secondAttack == null)
        {
            secondAttack = stateMachine.AddState(SecondAttackStateName, new Vector3(360f, 400f, 0f));
            changed = true;
        }

        AnimatorState thirdAttack = FindUniqueState(stateMachine, ThirdAttackStateName, ref changed);
        if (thirdAttack == null)
        {
            thirdAttack = stateMachine.AddState(ThirdAttackStateName, new Vector3(600f, 400f, 0f));
            changed = true;
        }

        changed |= AssignMotion(firstAttack, firstAttackClip);
        changed |= AssignMotion(secondAttack, secondAttackClip);
        changed |= AssignMotion(thirdAttack, thirdAttackClip);
        changed |= SetStatePosition(stateMachine, firstAttack, new Vector3(120f, 400f, 0f));
        changed |= SetStatePosition(stateMachine, secondAttack, new Vector3(360f, 400f, 0f));
        changed |= SetStatePosition(stateMachine, thirdAttack, new Vector3(600f, 400f, 0f));

        AnimatorState dash = FindState(stateMachine, DashStateName);
        AnimatorState dashEnd = FindState(stateMachine, DashEndStateName);
        AnimatorState idle = FindState(stateMachine, IdleStateName);
        AnimatorState run = FindState(stateMachine, RunStateName);
        AnimatorState attackIdle = FindState(stateMachine, AttackIdleStateName);
        AnimatorState jumpUp = FindState(stateMachine, JumpUpStateName);
        AnimatorState jumpDown = FindState(stateMachine, JumpDownStateName);

        changed |= RemoveTransition(dash, dashEnd, DashingParameterName);
        changed |= EnsureFloatBoolBoolTransition(dash, run, SpeedParameterName, AnimatorConditionMode.Greater, 0.1f, GroundedParameterName, true, DashingParameterName, false, 0.08f);
        changed |= EnsureFloatBoolBoolBoolTransition(dash, idle, SpeedParameterName, AnimatorConditionMode.Less, 0.1f, GroundedParameterName, true, InCombatParameterName, false, DashingParameterName, false, 0.08f);
        changed |= EnsureFloatBoolBoolBoolTransition(dash, attackIdle, SpeedParameterName, AnimatorConditionMode.Less, 0.1f, GroundedParameterName, true, InCombatParameterName, true, DashingParameterName, false, 0.08f);
        changed |= EnsureFloatBoolBoolTransition(dash, jumpDown, VerticalSpeedParameterName, AnimatorConditionMode.Less, -0.05f, GroundedParameterName, false, DashingParameterName, false, 0.08f);
        changed |= EnsureFloatBoolBoolTransition(dash, jumpUp, VerticalSpeedParameterName, AnimatorConditionMode.Greater, -0.05f, GroundedParameterName, false, DashingParameterName, false, 0.08f);

        if (attackIdle != null)
        {
            changed |= EnsureExitTransition(firstAttack, attackIdle, 0.98f, 0.08f);
            changed |= EnsureExitTransition(secondAttack, attackIdle, 0.98f, 0.08f);
            changed |= EnsureExitTransition(thirdAttack, attackIdle, 0.98f, 0.08f);
        }

        changed |= EnsureTriggerTransition(firstAttack, secondAttack, SecondAttackTriggerName, 0.03f);
        changed |= EnsureTriggerTransition(secondAttack, thirdAttack, ThirdAttackTriggerName, 0.03f);

        bool prefabChanged = EnsurePlayerWeaponAttackProfile();
        if (!changed && !prefabChanged)
        {
            return;
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(ControllerPath);
        Debug.Log("Player animator repaired: combo attacks and Dash exit transitions are available.");
    }

    private static bool EnsurePlayerWeaponAttackProfile()
    {
        GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerVisualPrefabPath);
        if (prefabAsset == null)
        {
            return false;
        }

        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PlayerVisualPrefabPath);
        try
        {
            PlayerWeaponHitbox hitbox = prefabRoot.GetComponentInChildren<PlayerWeaponHitbox>(true);
            if (hitbox == null || hitbox.GetComponent<PlayerWeaponAttackProfile>() != null)
            {
                return false;
            }

            hitbox.gameObject.AddComponent<PlayerWeaponAttackProfile>();
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, PlayerVisualPrefabPath);
            return true;
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }

    private static bool EnsureParameter(AnimatorController controller, string parameterName, AnimatorControllerParameterType type)
    {
        foreach (AnimatorControllerParameter parameter in controller.parameters)
        {
            if (parameter.name == parameterName)
            {
                return false;
            }
        }

        controller.AddParameter(parameterName, type);
        return true;
    }

    private static AnimatorState FindState(AnimatorStateMachine stateMachine, string stateName)
    {
        foreach (ChildAnimatorState childState in stateMachine.states)
        {
            if (childState.state != null && childState.state.name == stateName)
            {
                return childState.state;
            }
        }

        return null;
    }

    private static AnimatorState FindUniqueState(AnimatorStateMachine stateMachine, string stateName, ref bool changed)
    {
        AnimatorState firstMatch = null;
        foreach (ChildAnimatorState childState in stateMachine.states)
        {
            if (childState.state == null || childState.state.name != stateName)
            {
                continue;
            }

            if (firstMatch == null)
            {
                firstMatch = childState.state;
                continue;
            }

            stateMachine.RemoveState(childState.state);
            changed = true;
        }

        return firstMatch;
    }

    private static AnimationClip FindClip(string clipName)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(PlayerFbxPath);
        foreach (Object asset in assets)
        {
            if (asset is AnimationClip clip && clip.name == clipName)
            {
                return clip;
            }
        }

        Debug.LogWarning($"Animation clip {clipName} not found in {PlayerFbxPath}.");
        return null;
    }

    private static bool AssignMotion(AnimatorState state, Motion motion)
    {
        if (state == null || motion == null || state.motion == motion)
        {
            return false;
        }

        state.motion = motion;
        return true;
    }

    private static bool SetStatePosition(AnimatorStateMachine stateMachine, AnimatorState targetState, Vector3 position)
    {
        ChildAnimatorState[] states = stateMachine.states;
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i].state != targetState)
            {
                continue;
            }

            if (states[i].position == position)
            {
                return false;
            }

            states[i].position = position;
            stateMachine.states = states;
            return true;
        }

        return false;
    }

    private static bool EnsureTriggerTransition(AnimatorState from, AnimatorState to, string triggerName, float duration)
    {
        AnimatorStateTransition transition = FindTransition(from, to, triggerName);
        if (transition == null)
        {
            transition = from.AddTransition(to);
            transition.AddCondition(AnimatorConditionMode.If, 0f, triggerName);
            ConfigureTransition(transition, false, 0f, duration);
            return true;
        }

        return ConfigureTransition(transition, false, 0f, duration);
    }

    private static bool EnsureExitTransition(AnimatorState from, AnimatorState to, float exitTime, float duration)
    {
        AnimatorStateTransition transition = FindExitTransition(from, to);
        if (transition == null)
        {
            transition = from.AddTransition(to);
            ConfigureTransition(transition, true, exitTime, duration);
            return true;
        }

        return ConfigureTransition(transition, true, exitTime, duration);
    }

    private static bool EnsureBoolBoolTransition(
        AnimatorState from,
        AnimatorState to,
        string firstBoolName,
        bool firstExpected,
        string secondBoolName,
        bool secondExpected,
        float duration)
    {
        if (from == null || to == null)
        {
            return false;
        }

        AnimatorStateTransition transition = FindTransition(from, to, firstBoolName, secondBoolName);
        if (transition == null)
        {
            transition = from.AddTransition(to);
            transition.AddCondition(firstExpected ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, firstBoolName);
            transition.AddCondition(secondExpected ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, secondBoolName);
            ConfigureTransition(transition, false, 0f, duration);
            return true;
        }

        return ConfigureTransition(transition, false, 0f, duration);
    }

    private static bool EnsureFloatBoolBoolTransition(
        AnimatorState from,
        AnimatorState to,
        string floatName,
        AnimatorConditionMode floatMode,
        float threshold,
        string firstBoolName,
        bool firstExpected,
        string secondBoolName,
        bool secondExpected,
        float duration)
    {
        if (from == null || to == null)
        {
            return false;
        }

        AnimatorStateTransition transition = FindTransition(from, to, floatName, firstBoolName, secondBoolName);
        if (transition == null)
        {
            transition = from.AddTransition(to);
            transition.AddCondition(floatMode, threshold, floatName);
            transition.AddCondition(firstExpected ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, firstBoolName);
            transition.AddCondition(secondExpected ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, secondBoolName);
            ConfigureTransition(transition, false, 0f, duration);
            return true;
        }

        return ConfigureTransition(transition, false, 0f, duration);
    }

    private static bool EnsureFloatBoolBoolBoolTransition(
        AnimatorState from,
        AnimatorState to,
        string floatName,
        AnimatorConditionMode floatMode,
        float threshold,
        string firstBoolName,
        bool firstExpected,
        string secondBoolName,
        bool secondExpected,
        string thirdBoolName,
        bool thirdExpected,
        float duration)
    {
        if (from == null || to == null)
        {
            return false;
        }

        AnimatorStateTransition transition = FindTransition(from, to, floatName, firstBoolName, secondBoolName, thirdBoolName);
        if (transition == null)
        {
            transition = from.AddTransition(to);
            transition.AddCondition(floatMode, threshold, floatName);
            transition.AddCondition(firstExpected ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, firstBoolName);
            transition.AddCondition(secondExpected ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, secondBoolName);
            transition.AddCondition(thirdExpected ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, thirdBoolName);
            ConfigureTransition(transition, false, 0f, duration);
            return true;
        }

        return ConfigureTransition(transition, false, 0f, duration);
    }

    private static AnimatorStateTransition FindTransition(AnimatorState from, AnimatorState to, params string[] conditionParameters)
    {
        foreach (AnimatorStateTransition transition in from.transitions)
        {
            if (transition.destinationState != to)
            {
                continue;
            }

            bool hasAllConditions = true;
            for (int i = 0; i < conditionParameters.Length; i++)
            {
                bool hasCondition = false;
                foreach (AnimatorCondition condition in transition.conditions)
                {
                    if (condition.parameter == conditionParameters[i])
                    {
                        hasCondition = true;
                        break;
                    }
                }

                if (!hasCondition)
                {
                    hasAllConditions = false;
                    break;
                }
            }

            if (hasAllConditions)
            {
                return transition;
            }
        }

        return null;
    }

    private static bool RemoveTransition(AnimatorState from, AnimatorState to, params string[] conditionParameters)
    {
        if (from == null || to == null)
        {
            return false;
        }

        bool changed = false;
        AnimatorStateTransition[] transitions = from.transitions;
        for (int i = 0; i < transitions.Length; i++)
        {
            AnimatorStateTransition transition = transitions[i];
            if (transition.destinationState != to || !HasAllConditions(transition, conditionParameters))
            {
                continue;
            }

            from.RemoveTransition(transition);
            changed = true;
        }

        return changed;
    }

    private static bool HasAllConditions(AnimatorStateTransition transition, params string[] conditionParameters)
    {
        for (int i = 0; i < conditionParameters.Length; i++)
        {
            bool hasCondition = false;
            foreach (AnimatorCondition condition in transition.conditions)
            {
                if (condition.parameter == conditionParameters[i])
                {
                    hasCondition = true;
                    break;
                }
            }

            if (!hasCondition)
            {
                return false;
            }
        }

        return true;
    }

    private static AnimatorStateTransition FindExitTransition(AnimatorState from, AnimatorState to)
    {
        foreach (AnimatorStateTransition transition in from.transitions)
        {
            if (transition.destinationState == to && transition.conditions.Length == 0)
            {
                return transition;
            }
        }

        return null;
    }

    private static bool ConfigureTransition(AnimatorStateTransition transition, bool hasExitTime, float exitTime, float duration)
    {
        bool changed = false;
        float safeDuration = Mathf.Max(0f, duration);

        if (transition.hasExitTime != hasExitTime)
        {
            transition.hasExitTime = hasExitTime;
            changed = true;
        }

        if (!Mathf.Approximately(transition.exitTime, exitTime))
        {
            transition.exitTime = exitTime;
            changed = true;
        }

        if (!Mathf.Approximately(transition.duration, safeDuration))
        {
            transition.duration = safeDuration;
            changed = true;
        }

        return changed;
    }
}
