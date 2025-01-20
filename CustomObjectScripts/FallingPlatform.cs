namespace FP2_Sonic_Mod
{
    internal class FallingPlatform : FPBaseObject
    {
        public static int classID = -1;

        public FPObjectState state;

        private bool isValidatedInObjectList;

        private Collider2D colliderPlatform;

        private SpriteRenderer spriteRenderer;

        private GameObject falling;

        private float genericTimer;

        private new void Start()
        {
            // Get the collider and sprite renderer for this platform.
            colliderPlatform = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            state = State_Default;

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

                // Check if the player's ground collider is this platform's.
                if (fPPlayer.colliderGround == colliderPlatform)
                {
                    // Make this platform always active.
                    activationMode = FPActivationMode.ALWAYS_ACTIVE;

                    // Reset this platform's timer.
                    genericTimer = 0;

                    // Set this platform's state to the collapse waiting one.
                    state = State_CollapseWait;
                }
            }
        }

        private void State_CollapseWait()
        {
            // Increment this platform's timer.
            genericTimer += FPStage.deltaTime;

            // Check if this platform's timer has hit 30.
            if (genericTimer >= 30)
            {
                // Disable the two elements of the game object.
                colliderPlatform.enabled = false;
                spriteRenderer.enabled = false;

                // Reset this platform's timer.
                genericTimer = 0;

                // Create and set up the falling object.
                falling = new GameObject("platform fall");
                falling.transform.position = transform.position;
                falling.transform.position = new(transform.position.x, transform.position.y, transform.position.z);
                falling.layer = LayerMask.NameToLayer("FG Plane A");
                var fallingSprite = falling.AddComponent<SpriteRenderer>();
                fallingSprite.sprite = Plugin.sonicAssetBundle.LoadAssetWithSubAssets<Sprite>("ghz_objects")[2];

                // Set this platform to the falling state.
                state = State_Fall;
            }
        }

        private void State_Fall()
        {
            // Increment this platform's timer by half the usual value.
            genericTimer += (FPStage.deltaTime / 2);

            // Subtract this platform's timer from the falling's Y position.
            falling.transform.position = new(falling.transform.position.x, falling.transform.position.y - genericTimer, falling.transform.position.z);
        }
    }
}
