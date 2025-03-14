using System;
using HarmonyLib;
using UnityEngine;

namespace ArchipelagoClusterTruck.Patches;

public class Player : ClassPatch
{
    private static bool _noclip;
    #if DEBUG
    private static bool UpdatePrefix(player __instance, float ___airSpeed)
    {
        if (Input.GetKeyDown(Configuration.Instance.ToggleNoclipKey.Value))
        {
            _noclip = !_noclip;
            if (_noclip)
            {
                __instance.rig.isKinematic = true;
                __instance.rig.detectCollisions = false;
            }
            else
            {
                __instance.rig.isKinematic = false;
                __instance.rig.detectCollisions = true;
            }
        }
        if(!_noclip)
            return true;

        __instance.framesSinceStart++;
        
        float x = Input.GetAxis("Mouse X") * (options.invertedX ? -1f : 1f);
        float y = -Input.GetAxis("Mouse Y") * (options.inverted ? -1f : 1f);
        
        __instance.transform.Rotate(Vector3.up * x * player.sensitivity);
        __instance.camHolder.transform.Rotate(Vector3.right * y * player.sensitivity);
        
        
        Vector3 movement = __instance.camHolder.forward * ((Input.GetButton("Forward") ? 1 : 0) - (Input.GetButton("Back") ? 1 : 0)) +
                           __instance.camHolder.right * ((Input.GetButton("Right") ? 1 : 0) - (Input.GetButton("Left") ? 1 : 0)) +
                           Vector3.up * ((Input.GetButton("Jump") ? 1 : 0) - (Input.GetKey(KeyCode.LeftControl) ? 1 : 0));
        
        __instance.transform.position += movement * Time.deltaTime * ___airSpeed * 2f * Configuration.Instance.NoclipSpeed.Value * (Input.GetButton("Sprint") ? 2.0f : 1.0f);
        
        
        
        return false;
    }
    #endif
    public override Exception Patch(Harmony harmony)
    {
        #if DEBUG
        return MakePatch(harmony, typeof(player), "Update", nameof(UpdatePrefix));
        #else
        return null;
        #endif
    }
}