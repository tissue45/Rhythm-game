using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class InspectAnimators
{
    [MenuItem("Tools/Inspect Animators")]
    public static void RunInspection()
    {
        Debug.Log("Starting Animation Inspection...");
        
        // Find Minwoo
        GameObject minwoo = GameObject.Find("Minwoo_1");
        InspectGameObject(minwoo);

        // Find minA
        GameObject mina = GameObject.Find("minA_1");
        InspectGameObject(mina);
    }

    static void InspectGameObject(GameObject go)
    {
        if (go == null)
        {
            Debug.Log($"GameObject not found in scene.");
            return;
        }

        Debug.Log($"INSPECTING: {go.name}");
        Animator anim = go.GetComponent<Animator>();
        if (anim == null)
        {
            Debug.Log("No Animator component found.");
            return;
        }

        RuntimeAnimatorController runtimeController = anim.runtimeAnimatorController;
        if (runtimeController == null)
        {
            Debug.Log("No Runtime Animator Controller assigned.");
            return;
        }

        Debug.Log($"Controller Name: {runtimeController.name}");

        AnimatorController controller = runtimeController as AnimatorController;
        if (controller == null)
        {
            // It might be an AnimatorOverrideController
            Debug.Log("Controller is not an AnimatorController asset (might be override or internal).");
            return;
        }

        foreach (var layer in controller.layers)
        {
            Debug.Log($"Layer: {layer.name}");
            foreach (var state in layer.stateMachine.states)
            {
                string motionName = state.state.motion != null ? state.state.motion.name : "NULL";
                string motionType = state.state.motion != null ? state.state.motion.GetType().Name : "None";
                Debug.Log($"  State: {state.state.name}, Motion: {motionName} ({motionType})");
            }
        }
    }
}
