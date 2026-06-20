namespace FP2_Sonic_Mod.CustomObjectScripts
{
    public class SignPost : FPBaseObject
    {
        private FPObjectState state;
        private FPHitBox hbItem;
        private Animator animator;
        private FPResultsMenu resultsMenuObj;
        private FPResultsMenu resultsMenu;
        private FPHudMaster stageHud;
        public AudioClip signSound;

        private new void Start()
        {
            // Get the animator for the sign.
            animator = GetComponent<Animator>();

            // Create a very tall, slim hitbox for the sign.
            hbItem.enabled = true;
            hbItem.visible = true;
            hbItem.left = -8;
            hbItem.top = 2440;
            hbItem.right = 8;
            hbItem.bottom = -120;

            state = State_Idle;

            base.Start();
        }

        private void Update()
        {
            // Get the Hud and Results Menu.
            if (FPStage.objectsRegistered)
            {
                if (stageHud == null) stageHud = FPStage.FindObjectOfType<FPHudMaster>();
                if (resultsMenu == null) resultsMenu = this.transform.parent.GetComponent<ItemStarCard>().resultsMenu;
            }

            // Invoke the current state if it isn't null.
            state?.Invoke();
        }

        private void State_Idle()
        {
            // Set up a value to hold an object.
            FPBaseObject objRef = null;

            // Loop through each player object in the stage.
            while (FPStage.ForEach(FPPlayer.classID, ref objRef))
            {
                // Get a reference to this player.
                FPPlayer fPPlayer = (FPPlayer)objRef;

                // Check that the sign is in the idle state and the player's hitbox has touched it.
                if (state == new FPObjectState(State_Idle) && FPCollision.CheckOOBB(this, hbItem, objRef, fPPlayer.hbTouch))
                {
                    // Swap to the Spinning state.
                    state = State_Spinning;

                    // Play the animation and sound.
                    animator.Play("Spinning");
                    FPAudio.PlaySfx(signSound);

                    // Target the camera to the sign. 
                    FPCamera.SetCameraTarget(this.gameObject);
                    //FPCamera.stageCamera.SetBoundaries(true);
                    
                    // Stop the timer.
                    FPStage.timeEnabled = false;
                }
            }
        }

        private void State_Spinning()
        {
            // Check if the sign has finished spinning.
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f)
            {
                // Loop through each player object in the stage.
                FPBaseObject objRef = null;

                // Loop through each player object in the stage.
                while (FPStage.ForEach(FPPlayer.classID, ref objRef))
                {
                    // Get a reference to this player.
                    FPPlayer fPPlayer = (FPPlayer)objRef;

                    // Hide the HUD.
                    stageHud.state = 2;

                    // Kill the player's upwards velocity and set them into the victory state.
                    fPPlayer.velocity.y = Mathf.Min(fPPlayer.velocity.y, 0f);
                    fPPlayer.state = fPPlayer.State_Victory;

                    // Create and set up the results menu.
                    resultsMenuObj = UnityEngine.Object.Instantiate(resultsMenu);
                    resultsMenuObj.stageHud = stageHud;
                    resultsMenuObj.challengeMode = false;
                    resultsMenuObj.adventureCutscene = FPStage.currentStage.adventureCutscene;

                    // Play the clear jingle and results music.
                    FPAudio.PlayMusic(fPPlayer.bgmResults);
                    FPAudio.PlayJingle(1);
                    FPAudio.currentJingle = 1;

                    // Swap to the dummy Done state.
                    state = State_Done;

                    // Stop looping through objects.
                    return;
                }
            }
        }

        private void State_Done() { }
    }
}
