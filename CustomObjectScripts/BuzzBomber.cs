using FP2_Sonic_Mod;

public class BuzzBomber : FPBaseEnemy
{
    private static int classID = -1;
    private float flashTime;
    private bool flashing;
    private FPHitBox hbAttack;
    private Animator animator;
    private AudioClip sfxKO;
    private Renderer render;

    private float genericTimer;
    private bool hasShot;
    private RuntimeAnimatorController projectileAnimator;
    private Animator wingsAnimator;
    private GameObject thruster;
    private Animator shootAnimator;

    private new void Start()
    {
        // Set this Buzz Bomber's health to 1.
        health = 1;

        // Enable scaling so this Buzz Bomber can be flipped if moving right.
        useScaling = true;

        // Set this Buzz Bomber's hitboxes.
        hbWeakpoint.left = -48;
        hbWeakpoint.top = 24;
        hbWeakpoint.right = 48;
        hbWeakpoint.bottom = -24;
        hbWeakpoint.enabled = true;
        hbWeakpoint.visible = true;
        hbAttack.left = -24;
        hbAttack.top = 12;
        hbAttack.right = 24;
        hbAttack.bottom = -12;
        hbAttack.enabled = true;
        hbAttack.visible = true;

        // Increase this Buzz Bomber's activation range.
        activationRange.x = 320;

        // Set this Buzz Bomber's death sound and projectile animator.
        sfxKO = Plugin.sonicAssetBundle.LoadAsset<AudioClip>("classic_pop");
        projectileAnimator = Plugin.sonicAssetBundle.LoadAsset<RuntimeAnimatorController>("bullet animator");

        // Get this Buzz Bomber's extra parts.
        wingsAnimator = transform.GetChild(0).GetComponent<Animator>();
        thruster = transform.GetChild(1).gameObject;
        shootAnimator = transform.GetChild(2).GetComponent<Animator>();

        // Set this Buzz Bomber to the default state.
        state = State_Default;

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

    private void State_Default()
    {
        // Move this Buzz Bomber.
        if (direction == FPDirection.FACING_LEFT) velocity.x = -8;
        if (direction == FPDirection.FACING_RIGHT) velocity.x = 8;

        // Loop through and find a player in range to shoot at.
        FPBaseObject objRef = null;
        while (FPStage.ForEach(FPPlayer.classID, ref objRef))
        {
            FPPlayer fPPlayer = (FPPlayer)objRef;
            if (fPPlayer.position.x > position.x - 192 && fPPlayer.position.x < position.x + 192 && !hasShot)
            {
                velocity.x = 0;
                thruster.SetActive(false);
                state = State_PrepareToShoot;
            }
        }

        InteractWithObjects();
        ShaderUpdate();
        Process360Movement();
    }

    private void State_PrepareToShoot()
    {
        // Increment this Buzz Bomber's generic timer.
        genericTimer += FPStage.deltaTime;

        // Check if this Buzz Bomber's timer has reached 29.
        if (genericTimer >= 29)
        {
            // Change the hitbox to match the different shape of the shooting sprite.
            hbWeakpoint.left = -32;
            hbWeakpoint.top = 24;
            hbWeakpoint.right = 48;
            hbWeakpoint.bottom = -48;
            hbWeakpoint.enabled = true;
            hbWeakpoint.visible = true;
            hbAttack.left = -16;
            hbAttack.top = 12;
            hbAttack.right = 24;
            hbAttack.bottom = -24;
            hbAttack.enabled = true;
            hbAttack.visible = true;

            // Reset this Buzz Bomber's timer.
            genericTimer = 0;

            // Change to the shooting sprites.
            shootAnimator.Play("Shoot");
            wingsAnimator.Play("Shoot");
            animator.Play("Shoot");

            // Switch to the begin shoot state.
            state = State_BeginShoot;
        }

        InteractWithObjects();
        ShaderUpdate();
        Process360Movement();
    }

    private void State_BeginShoot()
    {
        // Increment this Buzz Bomber's generic timer.
        genericTimer += FPStage.deltaTime;

        // Check if this Buzz Bomber's timer has reached 29.
        if (genericTimer >= 29)
        {
            // Create a projectile.
            ProjectileBasic projectileBasic;
            if (direction == FPDirection.FACING_LEFT)
            {
                projectileBasic = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, position.x - 40, position.y - 48);
                projectileBasic.velocity.x = -4f;
            }
            else
            {
                projectileBasic = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, position.x + 40, position.y - 48);
                projectileBasic.velocity.x = 4f;
            }
            projectileBasic.velocity.y = -4;
            projectileBasic.animatorController = projectileAnimator;
            projectileBasic.animator = projectileBasic.GetComponent<Animator>();
            projectileBasic.animator.runtimeAnimatorController = projectileBasic.animatorController;
            projectileBasic.direction = direction;
            projectileBasic.explodeType = FPExplodeType.NONE;
            projectileBasic.parentObject = this;
            projectileBasic.faction = faction;

            // Reset this Buzz Bomber's timer.
            genericTimer = 0;

            // Switch to the end shoot state.
            state = State_EndShoot;
        }

        InteractWithObjects();
        ShaderUpdate();
        Process360Movement();
    }

    private void State_EndShoot()
    {
        // Increment this Buzz Bomber's generic timer.
        genericTimer += FPStage.deltaTime;

        // Check if this Buzz Bomber's timer has reached 29.
        if (genericTimer >= 29)
        {
            // Reset this Buzz Bomber's hitboxes
            hbWeakpoint.left = -48;
            hbWeakpoint.top = 24;
            hbWeakpoint.right = 48;
            hbWeakpoint.bottom = -24;
            hbWeakpoint.enabled = true;
            hbWeakpoint.visible = true;
            hbAttack.left = -24;
            hbAttack.top = 12;
            hbAttack.right = 24;
            hbAttack.bottom = -12;
            hbAttack.enabled = true;
            hbAttack.visible = true;

            // Retun to the normal sprites and reenable the thruster.
            thruster.SetActive(true);
            wingsAnimator.Play("Default");
            animator.Play("Default");

            // Enable the flag indicating that this Buzz Bomber has already fired.
            hasShot = true;

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
