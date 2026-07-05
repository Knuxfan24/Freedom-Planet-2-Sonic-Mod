using FP2_Sonic_Mod;

public class Chopper : FPBaseEnemy
{
    private static int classID = -1;
    private float flashTime;
    private bool flashing;
    private FPHitBox hbAttack;
    private Animator animator;
    private AudioClip sfxKO;
    private Renderer render;

    private float startY;

    private new void Start()
    {
        // Set this Chopper's health to 1.
        health = 1;

        // Set this Chopper's hitboxes.
        hbWeakpoint.left = -32;
        hbWeakpoint.top = 32;
        hbWeakpoint.right = 32;
        hbWeakpoint.bottom = -32;
        hbWeakpoint.enabled = true;
        hbWeakpoint.visible = true;
        hbAttack.left = -16;
        hbAttack.top = 16;
        hbAttack.right = 16;
        hbAttack.bottom = -16;
        hbAttack.enabled = true;
        hbAttack.visible = true;

        // Make this Chopper's activation only depend on its X range.
        activationMode = FPActivationMode.X_RANGE;

        // Set this Chopper's death sound.
        sfxKO = Plugin.sonicAssetBundle.LoadAsset<AudioClip>("classic_pop");

        // Set this Chopper to the default state.
        state = State_Default;

        // Get this Chopper's animator and renderer.
        animator = GetComponent<Animator>();
        render = GetComponentInParent<Renderer>();

        // Handle the FPBaseEnemy stuff.
        base.Start();
        classID = FPStage.RegisterObjectType(this, GetType(), 0);
        objectID = classID;

        // Save this Chopper's starting Y Position.
        startY = position.y;
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

    private void State_Default()
    {
        InteractWithObjects();

        // If this Chopper's gone below its starting Y Position, then give it some upwards velocity.
        if (position.y <= startY) velocity.y = 17f;

        // Apply gravity to this Chopper.
        position.y += velocity.y * FPStage.deltaTime;
        velocity.y -= 0.25f * FPStage.deltaTime;
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
}