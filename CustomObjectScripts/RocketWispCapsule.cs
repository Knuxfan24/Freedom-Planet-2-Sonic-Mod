using FP2_Sonic_Mod.Patchers;

namespace FP2_Sonic_Mod.CustomObjectScripts
{
    internal class RocketWispCapsule : FPBaseObject
    {
        public static int classID = -1;

        public FPObjectState state;

        private bool isValidatedInObjectList;

        private FPHitBox hitbox;

        private float respawnTimer;

        public AFKeyfishLock[] activators;

        private new void Start()
        {
            state = State_Idle;

            // Set up the Wisp Capsule's hitbox.
            hitbox.left = -14f;
            hitbox.top = 26f;
            hitbox.right = 14f;
            hitbox.bottom = -26f;
            hitbox.enabled = true;

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

        private void State_Idle()
        {
            // If the scale of the Wisp Capsule isn't 1, then increase it. If it is, then set it to 1 just to be certain.
            if (transform.localScale.x < 1)
                transform.localScale = new(transform.localScale.x + 0.1f, transform.localScale.y + 0.1f, transform.localScale.z + 0.1f);
            else
                transform.localScale = new(1, 1, 1);

            // Check if this Wisp Capsule has any activators.
            if (activators != null)
            {
                // Loop through each activator.
                foreach (AFKeyfishLock activator in activators)
                {
                    // Check if this activator doesn't have a key in it.
                    if (activator.storedKey == null)
                    {
                        // Scale the Capsule down to 0 to hide it.
                        transform.localScale = new(0, 0, 0);

                        // Set our state to inactive.
                        state = State_Inactive;
                    }
                }
            }

            // Run the collision check.
            CollisionCheck();
        }

        private void State_Inactive()
        {
            // Loop through each activator, if any of them don't have a key, then return.
            for (int activatorIndex = 0; activatorIndex < activators.Length; activatorIndex++)
                if (activators[activatorIndex].storedKey == null)
                    return;

            // If we've reached here, then set the state back to idle.
            state = State_Idle;
        }

        private void State_Collected()
        {
            // If the scale of the Wisp Capsule isn't 0, then shrink it. If it is, then set it to 0 just to be certain.
            if (transform.localScale.x > 0)
                transform.localScale = new(transform.localScale.x - 0.1f, transform.localScale.y - 0.1f, transform.localScale.z - 0.1f);
            else
                transform.localScale = new(0, 0, 0);

            // Increment our respawn timer.
            respawnTimer += FPStage.deltaTime;

            // Check if our respawn timer has reached 180.
            if (respawnTimer >= 180f)
            {
                // Switch to the idle state.
                state = State_Idle;

                // Play the respawn sound.
                FPAudio.PlaySfx(Plugin.sonicAssetBundle.LoadAsset<AudioClip>("wisp_capsule_respawn"));
            }
        }

        private void CollisionCheck()
        {
            // Set up a value to hold an object.
            FPBaseObject objRef = null;

            // Loop through each player object in the stage.
            while (FPStage.ForEach(FPPlayer.classID, ref objRef))
            {
                // Get a reference to this player.
                FPPlayer fPPlayer = (FPPlayer)objRef;

                // Check if our bounding boxes overlap.
                if (FPCollision.CheckOOBB(this, hitbox, objRef, fPPlayer.hbTouch))
                {
                    // Play the release sound.
                    FPAudio.PlaySfx(Plugin.sonicAssetBundle.LoadAsset<AudioClip>("wisp_capsule_release"));

                    // Set our state to the collected one.
                    state = State_Collected;

                    // Reset our respawn timer.
                    respawnTimer = 0;

                    // Set the player patcher's Wisp flag.
                    FPPlayerPatcher.HasWisp = true;

                    // Refill the player's energy gauge.
                    fPPlayer.energy = 100;
                }
            }
        }
    }
}
