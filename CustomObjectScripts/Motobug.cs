// TODO: Try and improve the edge detection if possible.
using FP2_Sonic_Mod;

public class Motobug : FPBaseEnemy
{
    private static int classID = -1;
    private float flashTime;
    private bool flashing;
    private FPHitBox hbAttack;
    private Animator animator;
    private AudioClip sfxKO;
    private Renderer render;

    private float genericTimer;

    private Sprite[] smokeSprites;
    private float smokeTimer;
    private int smokeIndex;

    private new void Start()
    {
        // Set this Motobug's health to 1.
        health = 1;

        // Set the needed values in the FPBaseObject for collision checking.
        enablePhysics = true;
        terrainCollision = true;
        halfWidth = 40;
        halfHeight = 29;
        sensorOffsetX = 32;
        useScaling = true;

        // Set this Motobug's hitboxes.
        hbWeakpoint.left = -40;
        hbWeakpoint.top = 32;
        hbWeakpoint.right = 30;
        hbWeakpoint.bottom = -32;
        hbWeakpoint.enabled = true;
        hbWeakpoint.visible = true;
        hbAttack.left = -16;
        hbAttack.top = 28;
        hbAttack.right = 16;
        hbAttack.bottom = -28;
        hbAttack.enabled = true;
        hbAttack.visible = true;

        // Set this Motobug's death sound.
        sfxKO = Plugin.sonicAssetBundle.LoadAsset<AudioClip>("classic_pop");

        // Load the smoke sprites.
        smokeSprites = Plugin.sonicAssetBundle.LoadAssetWithSubAssets<Sprite>("motobug_smoke");

        // Set this Motobug to the spawn state.
        state = State_Spawn;

        // Get this Chopper's animator and renderer.
        animator = GetComponent<Animator>();
        render = GetComponentInParent<Renderer>();

        // Handle the FPBaseEnemy stuff.
        base.Start();
        classID = FPStage.RegisterObjectType(this, GetType(), 0);
        objectID = classID;
    }

    public override void ResetStaticVars()
    {
        base.ResetStaticVars();
        classID = -1;
    }

    private void Update()
    {
        if (invincibility > 0f)
            invincibility -= FPStage.deltaTime;

        state?.Invoke();

        if (direction == FPDirection.FACING_LEFT) scale.x = -1f;
        else scale.x = 1f;
    }

    // Based on the original code for the Turretus.
    private void InteractWithObjects()
    {
        FPBaseObject objRef = null;
        while (FPStage.ForEach(FPPlayer.classID, ref objRef))
        {
            FPPlayer fPPlayer = (FPPlayer)objRef;
            if (fPPlayer.invincibilityTime <= 0f && FPCollision.CheckOOBB(this, hbAttack, objRef, fPPlayer.hbTouch))
            {
                fPPlayer.healthDamage += 1f;
                fPPlayer.damageType = 4;
                fPPlayer.hurtKnockbackX = velocity.x * 0.7f;
                fPPlayer.hurtKnockbackY = 5f;
                fPPlayer.Action_HitSpark(this);
            }
        }

        switch (DamageCheck())
        {
            case 1:
                flashTime = 2f;
                break;
            case 2:
                flashTime = 2f;
                FPAudio.PlaySfx(sfxKO);
                state = State_Death;
                activationMode = FPActivationMode.ALWAYS_ACTIVE;
                invincibility = 20f;
                break;
            case 4:
                health = 1f;
                state = State_Frozen;
                animator.SetSpeed(0f);
                break;
        }
    }

    // Copied from the original code for the Turretus.
    private void ShaderUpdate()
    {
        if (!(this != null))
        {
            return;
        }
        if (flashTime > 0f && GetComponentInParent<Renderer>() != null)
        {
            flashTime -= FPStage.deltaTime;
            if (!flashing)
            {
                render.material = FPResources.material[1];
                flashing = true;
            }
        }
        else if (flashing && GetComponentInParent<Renderer>() != null)
        {
            render.material = FPResources.material[0];
            flashing = false;
        }
    }

    private void State_Spawn()
    {
        Process360Movement();

        // Apply gravity to this Motobug until its on the ground, upon which swap to the default state.
        if (!onGround)
        {
            position.y += velocity.y * FPStage.deltaTime;
            velocity.y -= 0.375f * FPStage.deltaTime;
        }
        else
        {
            state = State_Default;
        }
    }

    private void State_Default()
    {
        // If we're on the ground, remove this Motobug's Y velocity and apply ground velocity based on its facing direction.
        if (onGround)
        {
            velocity.y = 0f;
            if (direction == FPDirection.FACING_RIGHT) groundVel = 2;
            else groundVel = -2;
        }
        angle = groundAngle;

        // Swap to the turning delay state if this Motobug has hit a wall or left the ground.
        if (colliderWall != null || !onGround)
        {
            groundVel = 0;
            velocity = Vector2.zero;
            genericTimer = 0;
            state = State_WaitToTurn;
        }

        animator.SetSpeed(1f);

        SmokeHandler();
        InteractWithObjects();
        ShaderUpdate();
        Process360Movement();
    }

    public void State_WaitToTurn()
    {
        // Increment this Motobug's generic timer.
        genericTimer += FPStage.deltaTime;

        // Check if this Motobug's timer has reached 60.
        if (genericTimer >= 60)
        {
            // Invert this Motobug's direction.
            direction ^= FPDirection.FACING_RIGHT;

            // Nudge this Motobug to the side so its no longer stuck in the wall or floating.
            // TODO: Maybe make this a loop that checks its on the ground and not touching a wall?
            if (direction == FPDirection.FACING_RIGHT) position.x += 4;
            else position.x -= 4;

            // Force this Motobug to be marked as on the ground.
            onGround = true;

            // Return to the default state.
            state = State_Default;
        }

        animator.SetSpeed(0f);

        InteractWithObjects();
        ShaderUpdate();
        Process360Movement();
    }

    // Copied from the original code for the Turretus.
    public void State_Frozen()
    {
        if (!frozen && freezeTimer > 0f)
        {
            iceBlockBack = Object.Instantiate(FPResources.childSprite[1]);
            iceBlockBack.parentObject = this;
            iceBlockBack.yOffset = 6f;
            iceBlock = Object.Instantiate(FPResources.childSprite[0]);
            iceBlock.parentObject = this;
            iceBlock.yOffset = 6f;
            iceBlock.GetComponent<SpriteRenderer>().sortingOrder = 3;
            frozen = true;
        }
        InteractWithObjects();
        ShaderUpdate();
    }

    private void State_Death()
    {
        FPStage.CreateStageObject(Explosion.classID, position.x, position.y);
        FPStage.DestroyStageObject(this);
    }

    private void SmokeHandler()
    {
        smokeTimer += FPStage.deltaTime;

        if (smokeTimer >= 4)
        {
            smokeTimer -= 4;

            if (smokeIndex < 3)
            {
                SpawnSmoke(1, 56);
                smokeIndex++;
            }
            else
            {
                SpawnSmoke(0, 40);
                SpawnSmoke(2, 72);
                smokeIndex = 0;
            }
        }

        void SpawnSmoke(int spriteIndex, float xOffset)
        {
            GameObject smoke = new("smoke");
            if (direction == FPDirection.FACING_RIGHT) smoke.transform.position = new Vector3(transform.position.x - xOffset, transform.position.y, transform.position.z);
            else smoke.transform.position = new Vector3(transform.position.x + xOffset, transform.position.y, transform.position.z);
            smoke.layer = this.gameObject.layer;
            SpriteRenderer smokeSprite = smoke.AddComponent<SpriteRenderer>();
            smokeSprite.sprite = smokeSprites[spriteIndex];
            smoke.AddComponent<MotobugSmoke>();
        }
    }
}

public class MotobugSmoke : MonoBehaviour
{
    private float genericTimer;

    void Update()
    {
        genericTimer += FPStage.deltaTime;

        if (genericTimer >= 2)
            GameObject.Destroy(this.gameObject);
    }
}