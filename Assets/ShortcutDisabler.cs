using UnityEngine;
using UnityEngine.InputSystem;
 
[DefaultExecutionOrder(-30000)]
public class ShortcutDisabler : MonoBehaviour
{
    protected void Awake()
    {
        // Disable Input System 1.4.0 shortcut feature by adding this component to an enabled GameObject
        // in your main/first scene.
        // See https://docs.unity3d.com/Packages/com.unity.inputsystem@1.4/changelog/CHANGELOG.html#140---2022-04-10
        InputSystem.settings.SetInternalFeatureFlag("DISABLE_SHORTCUT_SUPPORT", true);
    }
}