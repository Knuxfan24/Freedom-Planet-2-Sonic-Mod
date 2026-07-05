// TODO: Try and improve the edge detection if possible.
using FP2_Sonic_Mod;

public class Crabmeat : FPBaseEnemy
{
    private static int classID = -1;
    private float flashTime;
    private bool flashing;
    private FPHitBox hbAttack;
    private Animator animator;
    private AudioClip sfxKO;
    private Renderer render;

    private float genericTimer;
    private RuntimeAnimatorController projectileAnimator;

    private new void Start()
    {
        // Set this Crabmeat's health to 1.
        health = 1;

        // TODO: Properly decide on the sensor values.
        enablePhysics = true;
        terrainCollision = true;
        halfWidth = 20;
        halfHeight = 29;
        sensorOffsetX = 16;
        useScaling = true;

        // Set this Crabmeat's hitboxes.
        hbWeakpoint.left = -48;
        hbWeakpoint.top = 32;
        hbWeakpoint.right = 48;
        hbWeakpoint.bottom = -32;
        hbWeakpoint.enabled = true;
        hbWeakpoint.visible = true;
        hbAttack.left = -24;
        hbAttack.top = 16;
        hbAttack.right = 24;
        hbAttack.bottom = -16;
        hbAttack.enabled = true;
        hbAttack.visible = true;

        // Set this Crabmeat's death sound and projectile animator.
        sfxKO = Plugin.sonicAssetBundle.LoadAsset<AudioClip>("classic_pop");
        projectileAnimator = Plugin.sonicAssetBundle.LoadAsset<RuntimeAnimatorController>("crabmeat bullet animator");

        // Set this Crabmeat to the spawn state
        state = State_Spawn;

        // Get this Crabmeat's animator and renderer.
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

        // Apply gravity to this Crabmeat until its on the ground, upon which swap to the default state.
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
        // Play the walk animation.
        animator.Play("Walk");

        // If we're on the ground, remove this Crabmeat's Y velocity and apply ground velocity based on its facing direction.
        if (onGround)
        {
            velocity.y = 0f;
            if (direction == FPDirection.FACING_RIGHT) groundVel = 1;
            else groundVel = -1;
        }
        angle = groundAngle;

        // Increment this Crabmeat's generic timer.
        genericTimer += FPStage.deltaTime;

        // Swap to the wait to shoot state if this Crabmeat has hit a wall, left the ground or the timer has reached 128.
        if (colliderWall != null || !onGround || genericTimer >= 128)
        {
            groundVel = 0;
            velocity = Vector2.zero;
            genericTimer = 0;
            state = State_WaitToShoot;
        }

        InteractWithObjects();
        ShaderUpdate();
        Process360Movement();
    }

    public void State_WaitToShoot()
    {
        // Swap to the idle sprite.
        animator.Play("Idle");

        // Increment this Crabmeat's generic timer.
        genericTimer += FPStage.deltaTime;

        // Check if this Crabmeat's timer has reached 90.
        if (genericTimer >= 90)
        {
            // Swap to the shoot sprite.
            animator.Play("Shoot");

            // Reset this Crabmeat's timer.
            genericTimer = 0;

            // Spawn the two bullets.
            ProjectileBasic projectileBasic;
            projectileBasic = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, position.x + 40, position.y + 16);
            projectileBasic.velocity.x = 2f;
            projectileBasic.velocity.y = 8f;
            projectileBasic.animatorController = projectileAnimator;
            projectileBasic.animator = projectileBasic.GetComponent<Animator>();
            projectileBasic.animator.runtimeAnimatorController = projectileBasic.animatorController;
            projectileBasic.direction = direction;
            projectileBasic.explodeType = FPExplodeType.NONE;
            projectileBasic.parentObject = this;
            projectileBasic.faction = faction;
            projectileBasic = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, position.x - 40, position.y + 16);
            projectileBasic.velocity.x = -2f;
            projectileBasic.velocity.y = 8f;
            projectileBasic.animatorController = projectileAnimator;
            projectileBasic.animator = projectileBasic.GetComponent<Animator>();
            projectileBasic.animator.runtimeAnimatorController = projectileBasic.animatorController;
            projectileBasic.direction = direction;
            projectileBasic.explodeType = FPExplodeType.NONE;
            projectileBasic.parentObject = this;
            projectileBasic.faction = faction;

            // Swap to the has shot state.
            state = State_HasShot;
        }

        InteractWithObjects();
        ShaderUpdate();
        Process360Movement();
    }

    public void State_HasShot()
    {
        // Increment this Crabmeat's generic timer.
        genericTimer += FPStage.deltaTime;

        // Check if this Crabmeat's timer has reached 60.
        if (genericTimer >= 60)
        {
            // Invert this Crabmeat's direction.
            direction ^= FPDirection.FACING_RIGHT;

            // Nudge this Crabmeat to the side so its no longer stuck in the wall or floating.
            // TODO: Maybe make this a loop that checks its on the ground and not touching a wall?
            if (direction == FPDirection.FACING_RIGHT) position.x += 2;
            else position.x -= 2;

            // Force this Crabmeat to be marked as on the ground.
            onGround = true;

            // Reset this Crabmeat's timer.
            genericTimer = 0;

            // Return to the default state.
            state = State_Default;
        }

        InteractWithObjects();
        ShaderUpdate();
        Process360Movement();
    }

    // Copied from the original code for the Turretus.
    public void State_Frozen()
    {
        if (!frozen && freezeTimer > 0f)
        {
            iceBlockBack = UnityEngine.Object.Instantiate(FPResources.childSprite[1]);
            iceBlockBack.parentObject = this;
            iceBlockBack.yOffset = 6f;
            iceBlock = UnityEngine.Object.Instantiate(FPResources.childSprite[0]);
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
