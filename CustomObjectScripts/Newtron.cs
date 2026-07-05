using FP2_Sonic_Mod;
using System;

public class Newtron : FPBaseEnemy
{
    private static int classID = -1;
    private float flashTime;
    private bool flashing;
    private FPHitBox hbAttack;
    private Animator animator;
    private AudioClip sfxKO;
    private Renderer render;
    private SpriteRenderer sprite;

    private float genericTimer;
    private RuntimeAnimatorController projectileAnimator;
    public int type;
    private GameObject thruster;

    private new void Start()
    {
        // Set this Newtron's health to 1.
        health = 1;

        // Set the needed values in the FPBaseObject for collision checking.
        // TODO: Decide on the sensor values.
        enablePhysics = true;
        terrainCollision = true;
        useScaling = true;

        // Set this Newtron's death sound and projectile animator.
        sfxKO = Plugin.sonicAssetBundle.LoadAsset<AudioClip>("classic_pop");
        projectileAnimator = Plugin.sonicAssetBundle.LoadAsset<RuntimeAnimatorController>("bullet animator");

        // Get this Newtron's thruster.
        thruster = transform.GetChild(0).gameObject;

        // Set this Newtron to the spawn state.
        state = State_Spawn;

        // Get this Newtron's animator, renderer and sprite renderer. Also set its alpha to 0.
        animator = GetComponent<Animator>();
        render = GetComponent<Renderer>();
        sprite = GetComponent<SpriteRenderer>();
        sprite.color = new Color(1, 1, 1, 0);

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
        // Set the sprite of this Newtron to the correct one for its type.
        if (type == 0) animator.Play("Blue Idle");
        if (type == 1) animator.Play("Green Idle");

        // Loop through each player until we find one within 192 units of this Newtron's position, swapping to the appearing state when found.
        FPBaseObject objRef = null;
        while (FPStage.ForEach(FPPlayer.classID, ref objRef))
        {
            FPPlayer fPPlayer = (FPPlayer)objRef;
            if (fPPlayer.position.x > position.x - 192 && fPPlayer.position.x < position.x + 192)
                state = State_Appearing;
        }
    }

    private void State_Appearing()
    {
        InteractWithObjects();
        ShaderUpdate();
        Process360Movement();

        // Fade this Newtron in.
        sprite.color = new Color(1, 1, 1, sprite.color.a + (0.1f * FPStage.deltaTime));
        
        // If this Newtron has fully materalised, then set its hitboxes and swap to the wait to act state.
        if (sprite.color.a >= 1)
        {
            hbWeakpoint.left = -40;
            hbWeakpoint.top = 40;
            hbWeakpoint.right = 40;
            hbWeakpoint.bottom = -40;
            hbWeakpoint.enabled = true;
            hbWeakpoint.visible = true;
            hbAttack.left = -20;
            hbAttack.top = 20;
            hbAttack.right = 20;
            hbAttack.bottom = -20;
            hbAttack.enabled = true;
            hbAttack.visible = true;

            state = State_WaitToAct;
        }
    }

    private void State_WaitToAct()
    {
        InteractWithObjects();
        ShaderUpdate();
        Process360Movement();

        // Increment this Newtron's generic timer.
        genericTimer += FPStage.deltaTime;

        // Check if this Newtron's timer has reached 60.
        if (genericTimer >= 60)
        {
            // If this is a Blue Newtron, then go the transform state.
            if (type == 0)
            {
                // Find the player and turn to the face them.
                FPBaseObject objRef = null;
                while (FPStage.ForEach(FPPlayer.classID, ref objRef))
                {
                    FPPlayer fPPlayer = (FPPlayer)objRef;
                    if (fPPlayer.position.x > position.x) direction = FPDirection.FACING_RIGHT;
                    else direction = FPDirection.FACING_LEFT;
                }

                animator.Play("Blue Transform");
                state = State_Transform;
            }

            // If this is a Green Newtron, then shoot a bullet.
            if (type == 1)
            {
                // Switch to the shoot sprite.
                animator.Play("Green Shoot");

                // Create the bullet.
                // TODO: This position is copied from Serpentine, adjust it to be closer to the Newtron's mouth.
                ProjectileBasic projectileBasic;
                if (direction == FPDirection.FACING_LEFT)
                {
                    projectileBasic = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, position.x - Mathf.Cos((float)Math.PI / 180f * angle) * 56f + Mathf.Sin((float)Math.PI / 180f * angle) * 16, position.y + Mathf.Cos((float)Math.PI / 180f * angle) * 16 - Mathf.Sin((float)Math.PI / 180f * angle) * 56f);
                    projectileBasic.velocity.x = -4f;
                }
                else
                {
                    projectileBasic = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, position.x + Mathf.Cos((float)Math.PI / 180f * angle) * 56f + Mathf.Sin((float)Math.PI / 180f * angle) * 16, position.y + Mathf.Cos((float)Math.PI / 180f * angle) * 16 + Mathf.Sin((float)Math.PI / 180f * angle) * 56f);
                    projectileBasic.velocity.x = 4f;
                }
                projectileBasic.animatorController = projectileAnimator;
                projectileBasic.animator = projectileBasic.GetComponent<Animator>();
                projectileBasic.animator.runtimeAnimatorController = projectileBasic.animatorController;
                projectileBasic.direction = direction;
                projectileBasic.explodeType = FPExplodeType.NONE;
                projectileBasic.parentObject = this;
                projectileBasic.faction = faction;

                // Reset this Newtron's timer.
                genericTimer = 0f;

                // Move to the wait to vanish state.
                state = State_WaitToVanish;
            }
        }
    }

    private void State_Transform()
    {
        // Check that this Newtron is halfway through the transform animation.
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5f)
        {
            Process360Movement();

            // Apply gravity if this Newtron isn't on the ground yet.
            if (!onGround)
            {
                position.y += velocity.y * FPStage.deltaTime;
                velocity.y -= 0.375f * FPStage.deltaTime;
            }

            // Shrink the hitboxes, change the sprite and swap to the missile state if we've hit the ground.
            else
            {
                hbAttack.top = 32;
                hbAttack.bottom = 8;
                hbWeakpoint.bottom = 0;

                velocity.y = 0f;

                if (direction == FPDirection.FACING_RIGHT) groundVel = 4;
                else groundVel = -4;

                thruster.SetActive(true);
                animator.Play("Blue Missile");
                state = State_Missile;
            }
        }
    }

    private void State_Missile()
    {
        // If we're on the ground, remove this Newtron's Y velocity and apply ground velocity based on its facing direction.
        if (onGround)
        {
            velocity.y = 0f;
            if (direction == FPDirection.FACING_RIGHT) groundVel = 4;
            else groundVel = -4;
        }
        angle = groundAngle;

        // If this Newtron's hit a wall, disable its collision with the terrain entirely.
        if (colliderWall != null)
            terrainCollision = false;

        InteractWithObjects();
        ShaderUpdate();
        Process360Movement();
    }

    private void State_WaitToVanish()
    {
        InteractWithObjects();
        ShaderUpdate();
        Process360Movement();

        // Increment this Newtron's generic timer.
        genericTimer += FPStage.deltaTime;

        // Check if this Newtron's timer has reached 60.
        if (genericTimer >= 60)
        {
            // Remove this Newtron's hitboxes.
            hbWeakpoint.left = 0;
            hbWeakpoint.top = 0;
            hbWeakpoint.right = 0;
            hbWeakpoint.bottom = 0;
            hbWeakpoint.enabled = false;
            hbWeakpoint.visible = false;
            hbAttack.left = 0;
            hbAttack.top = 0;
            hbAttack.right = 0;
            hbAttack.bottom = 0;
            hbAttack.enabled = false;
            hbAttack.visible = false;

            // Switch to the vanish state.
            state = State_Vanish;
            animator.Play("Green Idle");
        }
    }

    private void State_Vanish()
    {
        // Fade this Newtron out.
        sprite.color = new Color(1, 1, 1, sprite.color.a - (0.1f * FPStage.deltaTime));

        // If this Newtron's alpha has reached 0, then destroy it.
        if (sprite.color.a <= 0)
            FPStage.DestroyStageObject(this);
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