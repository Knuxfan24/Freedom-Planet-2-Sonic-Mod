using FP2_Sonic_Mod.Patchers;

namespace FP2_Sonic_Mod.CustomObjectScripts
{
    internal class ZoomTube : FPBaseObject
    {
        public static int classID = -1;

        public FPObjectState state;

        private bool isValidatedInObjectList;

        public FPHitBox hbTouch;

        private new void Start()
        {
            state = State_Default;

            // Set up the tube's hitbox.
            hbTouch.left = -256;
            hbTouch.top = 512;
            hbTouch.right = 256;
            hbTouch.bottom = -512;
            hbTouch.enabled = true;
            hbTouch.visible = true;

            base.Start();
            classID = FPStage.RegisterObjectType(this, GetType(), 0);
            objectID = classID;
        }

        private void Update()
        {
            if (!isValidatedInObjectList && FPStage.objectsRegistered)
                isValidatedInObjectList = FPStage.ValidateStageListPos(this);

            // Invoke the current state if it isn't null.
            state?.Invoke();
        }

        private void State_Default()
        {
            // Set up a value to hold an object.
            FPBaseObject objRef = null;

            // Loop through each player object in the stage.
            while (FPStage.ForEach(FPPlayer.classID, ref objRef))
            {
                // Get a reference to this player.
                FPPlayer fPPlayer = (FPPlayer)objRef;

                // Check if the player's collider has overlapped this tube's.
                if (FPCollision.CheckOOBB(this, hbTouch, objRef, fPPlayer.hbTouch))
                {
                    // Check if the player isn't already in the physics ball state.
                    if (fPPlayer.state != fPPlayer.State_Ball_Physics)
                    {
                        // Set the player to the physics ball state.
                        fPPlayer.state = fPPlayer.State_Ball_Physics;

                        // Check that the player isn't already in the rolling animation.
                        if (fPPlayer.currentAnimation != "Rolling")
                        {
                            // Set the player's animation to the rolling one.
                            fPPlayer.SetPlayerAnimation("Rolling");

                            // Play the roll sound.
                            fPPlayer.Action_PlaySoundUninterruptable(fPPlayer.sfxRolling);
                        }
                    }

                }

                // If the player isn't touching this tube but are in the ball physics state, then set them to the normal rolling state.
                else if (fPPlayer.state == fPPlayer.State_Ball_Physics)
                    fPPlayer.state = FPPlayerPatcher.State_Sonic_Roll;
            }
        }
    }
}