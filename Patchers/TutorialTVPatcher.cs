using System;
using System.Reflection;
using UnityEngine.SceneManagement;

namespace FP2_Sonic_Mod.Patchers
{
    internal class TutorialTVPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(TutorialTV), "State_Neera_Ray")]
        private static bool State_SpinDash(TutorialTV __instance)
        {
            // If we're not in Sonic's tutorial, then leave this alone.
            if (SceneManager.GetActiveScene().name != "Tutorial1Sonic")
                return true;

            // Get the values of the TV's Generic Timer and Generic State, as they're private.
            float genericTimer = (float)typeof(TutorialTV).GetField("genericTimer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(__instance);
            int genericState = (int)typeof(TutorialTV).GetField("genericState", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(__instance);

            // Increment the generic timer.
            typeof(TutorialTV).GetField("genericTimer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(__instance, genericTimer += FPStage.deltaTime);

            // Handle animation, timer and state settings.
            if (genericState == 0 && genericTimer >= 60)
                SetAnimation(__instance, "Idle", genericTimer, genericState, 0, 1);

            else if (genericState == 1 && genericTimer >= 65)
                SetAnimation(__instance, "Crouching", genericTimer, genericState, 60, 2);

            else if (genericState >= 2 && genericState <= 10 && genericTimer >= 90)
                SetAnimation(__instance, "SpindashCharge", genericTimer, genericState, 75, genericState + 1);

            else if (genericState == 11 && genericTimer >= 105)
                SetAnimation(__instance, "Rolling", genericTimer, genericState, -60, 0);

            // Stop the original function from running.
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TutorialTV), "State_Neera_Doublejump")]
        private static bool State_HomingAttack(TutorialTV __instance)
        {
            // If we're not in Sonic's tutorial, then leave this alone.
            if (SceneManager.GetActiveScene().name != "Tutorial1Sonic")
                return true;

            // Get the values of the TV's Generic Timer and Generic State, as they're private.
            float genericTimer = (float)typeof(TutorialTV).GetField("genericTimer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(__instance);
            int genericState = (int)typeof(TutorialTV).GetField("genericState", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(__instance);

            // Increment the generic timer.
            typeof(TutorialTV).GetField("genericTimer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(__instance, genericTimer += FPStage.deltaTime);

            // Handle animation, timer and state settings.
            if (genericState == 0 && genericTimer >= 60)
                SetAnimation(__instance, "Rolling", genericTimer, genericState, 0, 1);

            else if (genericState == 1 && genericTimer >= 30)
                SetAnimation(__instance, "GuardAir", genericTimer, genericState, -10, 2);

            else if (genericState == 2 && genericTimer >= 10)
                SetAnimation(__instance, "Jumping_Loop", genericTimer, genericState, 0, 0);

            // Stop the original function from running.
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TutorialTV), "State_Neera_Sniper")]
        private static bool State_Stomp(TutorialTV __instance)
        {
            // If we're not in Sonic's tutorial, then leave this alone.
            if (SceneManager.GetActiveScene().name != "Tutorial1Sonic")
                return true;
            
            // Set the display to the Stomp animation.
            SetAnimation(__instance, "Stomp");

            // Stop the original function from running.
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TutorialTV), "State_Neera_Baton")]
        private static bool State_RocketWisp(TutorialTV __instance)
        {
            // If we're not in Sonic's tutorial, then leave this alone.
            if (SceneManager.GetActiveScene().name != "Tutorial1Sonic")
                return true;

            // Get the values of the TV's Generic Timer and Generic State, as they're private.
            float genericTimer = (float)typeof(TutorialTV).GetField("genericTimer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(__instance);
            int genericState = (int)typeof(TutorialTV).GetField("genericState", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(__instance);

            // Increment the generic timer.
            typeof(TutorialTV).GetField("genericTimer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(__instance, genericTimer += FPStage.deltaTime);

            // Handle animation, timer and state settings.
            if (genericState == 0 && genericTimer >= 60)
                SetAnimation(__instance, "UseWisp", genericTimer, genericState, 0, 1);

            else if (genericState == 1 && genericTimer >= 65)
                SetAnimation(__instance, "RocketWisp", genericTimer, genericState, 0, 2);

            else if (genericState == 2 && genericTimer >= 90)
                SetAnimation(__instance, "GuardAir", genericTimer, genericState, -10, 3);

            else if (genericState == 3 && genericTimer >= 10)
                SetAnimation(__instance, "Jumping_Loop", genericTimer, genericState, 0, 0);

            // Stop the original function from running.
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TutorialTV), "State_Neera_Lasso")]
        private static bool State_HopJump(TutorialTV __instance)
        {
            // If we're not in Sonic's tutorial, then leave this alone.
            if (SceneManager.GetActiveScene().name != "Tutorial1Sonic")
                return true;

            // Get the values of the TV's Generic Timer and Generic State, as they're private.
            float genericTimer = (float)typeof(TutorialTV).GetField("genericTimer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(__instance);
            int genericState = (int)typeof(TutorialTV).GetField("genericState", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(__instance);

            // Increment the generic timer.
            typeof(TutorialTV).GetField("genericTimer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(__instance, genericTimer += FPStage.deltaTime);

            // Handle animation, timer and state settings.
            if (genericState == 0 && genericTimer >= 0)
                SetAnimation(__instance, "Spring", genericTimer, genericState, 0, 1);

            else if (genericState == 1 && genericTimer >= 65)
                SetAnimation(__instance, "HopStart", genericTimer, genericState, -120, 0);

            // Stop the original function from running.
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TutorialTV), "State_Neera_Focus")]
        private static bool State_HummingTop(TutorialTV __instance)
        {
            // If we're not in Sonic's tutorial, then leave this alone.
            if (SceneManager.GetActiveScene().name != "Tutorial1Sonic")
                return true;

            // Get the values of the TV's Generic Timer and Generic State, as they're private.
            float genericTimer = (float)typeof(TutorialTV).GetField("genericTimer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(__instance);
            int genericState = (int)typeof(TutorialTV).GetField("genericState", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(__instance);

            // Increment the generic timer.
            typeof(TutorialTV).GetField("genericTimer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(__instance, genericTimer += FPStage.deltaTime);

            // Handle animation, timer and state settings.
            if (genericState == 0 && genericTimer >= 0)
                SetAnimation(__instance, "Spring", genericTimer, genericState, 0, 1);

            else if (genericState == 1 && genericTimer >= 65)
                SetAnimation(__instance, "Cyclone", genericTimer, genericState, -120, 0);

            // Stop the original function from running.
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TutorialTV), "State_Milla_Blaster")]
        private static bool State_DoubleJump(TutorialTV __instance)
        {
            // If we're not in Sonic's tutorial, then leave this alone.
            if (SceneManager.GetActiveScene().name != "Tutorial1Sonic")
                return true;

            // Get the values of the TV's Generic Timer and Generic State, as they're private.
            float genericTimer = (float)typeof(TutorialTV).GetField("genericTimer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(__instance);
            int genericState = (int)typeof(TutorialTV).GetField("genericState", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(__instance);

            // Increment the generic timer.
            typeof(TutorialTV).GetField("genericTimer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(__instance, genericTimer += FPStage.deltaTime);

            // Handle animation, timer and state settings.
            if (genericState == 0 && genericTimer >= 0)
                SetAnimation(__instance, "Rolling", genericTimer, genericState, 0, 1);

            else if (genericState == 1 && genericTimer >= 45)
                SetAnimation(__instance, "Jumping", genericTimer, genericState, -90, 0);

            // Stop the original function from running.
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TutorialTV), "State_Milla_Burst")]
        private static bool State_AirDash(TutorialTV __instance)
        {
            // If we're not in Sonic's tutorial, then leave this alone.
            if (SceneManager.GetActiveScene().name != "Tutorial1Sonic")
                return true;

            // Get the values of the TV's Generic Timer and Generic State, as they're private.
            float genericTimer = (float)typeof(TutorialTV).GetField("genericTimer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(__instance);
            int genericState = (int)typeof(TutorialTV).GetField("genericState", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(__instance);

            // Increment the generic timer.
            typeof(TutorialTV).GetField("genericTimer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(__instance, genericTimer += FPStage.deltaTime);

            // Handle animation, timer and state settings.
            if (genericState == 0 && genericTimer >= 0)
                SetAnimation(__instance, "Rolling", genericTimer, genericState, 0, 1);

            else if (genericState == 1 && genericTimer >= 45)
                SetAnimation(__instance, "Airdash", genericTimer, genericState, -90, 0);

            // Stop the original function from running.
            return false;
        }

#pragma warning disable IDE0060 // Remove unused parameter. Because it IS used VS...
        private static void SetAnimation(TutorialTV __instance, string animationName, float genericTimer = 0, int genericState = 0, float timerValue = 0, int stateValue = 0)
        {
            typeof(TutorialTV).GetMethod("SetTutorialAnimation", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(string) }, null).Invoke(__instance, new object[] { animationName });
            genericState = stateValue;
            typeof(TutorialTV).GetField("genericState", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(__instance, genericState);
            genericTimer = timerValue;
            typeof(TutorialTV).GetField("genericTimer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(__instance, genericTimer);
        }
#pragma warning restore IDE0060
    }
}
