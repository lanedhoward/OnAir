using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class Player : CharacterBody2D
{
    float inputDir;
    bool jumpPressed;
    bool jumpHeld;
    bool jumpReleased;
    bool dashPressed;

    float coyoteTime;

    bool flutterUsed;
    float flutterTime;

    bool dashUsed;

    AnimatedSprite2D sprite;


    public Vector2 velocity;

    public State CurrentState;



    [Export]
    float MaxSpeed;

    [Export]
    float WalkAccel;

    [Export]
    float Friction;

    [Export]
    float JumpForce;

    [Export]
    float CoyoteTimeMax = 0.1f;
    
    [Export]
    float Gravity;

    [Export]
    float DashForce;

    [Export]
    float FlutterAccel;

    [Export]
    float FlutterDurationMax;

    public enum State
    {
        Normal,
        Flutter,
        Dash,
        Hit
    }

    public override void _Ready()
    {
        base._Ready();
        sprite = GetNode<AnimatedSprite2D>("Sprite2D");
        sprite.Play("idle");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        float _delta = (float)delta;

        velocity = Velocity;

        GetInput();

        ProcessState(_delta);

        UpdateTimers(_delta);

        Velocity = velocity;

        MoveAndSlide();
    }

    private void GetInput()
    {
        inputDir = Input.GetActionStrength("Right") - Input.GetActionStrength("Left");

        jumpPressed = Input.IsActionJustPressed("Jump");
        jumpHeld = Input.IsActionPressed("Jump");
        jumpReleased = Input.IsActionJustReleased("Jump");

        dashPressed = Input.IsActionJustPressed("Dash");
    }

    public void MoveHorizontal(float delta)
    {

        float hsp = velocity.X;

        //if we only move the left stick a little bit, inari should only accelerate to a walking pace
        float _maxSpeed = MaxSpeed * Mathf.Abs(inputDir);



        if (inputDir != 0)
        {
            // we move!

            float actingHorizontalAccel = WalkAccel;


            if (
                (System.Math.Abs(hsp) <= (MaxSpeed - (actingHorizontalAccel * System.Math.Abs(inputDir) * delta))) || //if you can accelerate
                (System.Math.Sign(inputDir) != System.Math.Sign(hsp)) // or if you are trying to change directions
                )
            {
                hsp += actingHorizontalAccel * Mathf.Sign(inputDir) * delta;
            }
            else
            {
                // cap speed
                // TODO: maybe soft cap speed? like if ur going over your max speed, just slow down over time until you get back to max speed
                //hsp = maxSpeed * System.Math.Sign(hsp);
                hsp = ApplyHorizontalFriction(delta, Friction, hsp, MaxSpeed * System.Math.Sign(hsp));
            }
        }
        else
        {
            // we are not moving.
            hsp = ApplyHorizontalFriction(delta, Friction, hsp, 0);
        }

        velocity.X = hsp;

    }

    private float ApplyHorizontalFriction(float delta, float fricAmount, float hsp, float goalhsp)
    {
        if (System.Math.Abs(hsp) >= System.Math.Abs(goalhsp) + (fricAmount * delta))
        {
            hsp -= System.Math.Sign(hsp) * fricAmount * delta;
        }
        else
        {
            hsp = goalhsp;
        }

        return hsp;
    }

    private void HandleJump(float delta)
    {

        if (jumpPressed)
        {
            if (CanJump())
            {
                coyoteTime = 0f;
                velocity.Y = JumpForce;
            }
            else
            {
                if (CanFlutter())
                {
                    flutterUsed = true;
                    CurrentState = State.Flutter;
                    velocity.Y = 0;
                    flutterTime = FlutterDurationMax;
                    sprite.Play("flutter");
                }
            }
        }
    }

    private bool CanJump()
    {
        if (IsOnFloor())
        {
            return true;
        }
        else
        {
            return (coyoteTime > 0);
        }
    }

    private bool CanFlutter()
    {
        return !flutterUsed;
    }

    private void DoGravity(float delta)
    {
        if (!IsOnFloor() && CurrentState != State.Dash)
        {
            velocity.Y += delta * Gravity;
        }
    }

    private void HandleDash()
    {
        if (dashPressed)
        {
            if (CanDash())
            {
                var dashDir = Mathf.Ceil(inputDir);
                if (dashDir == 0)
                {
                    dashDir = sprite.FlipH ? -1 : 1;
                }
                velocity.X = DashForce * dashDir;
                velocity.Y = 0;
                CurrentState = State.Dash;
                dashUsed = true;
                sprite.Play("dash");
            }
        }
    }

    private bool CanDash()
    {
        return !dashUsed;
    }


    private void ProcessState(float _delta)
    {
        switch (CurrentState)
        {
            default:
            case State.Normal:
                {
                    MoveHorizontal(_delta);

                    HandleJump(_delta);

                    HandleDash();

                    DoGravity(_delta);

                    if (sprite.Animation == "idle" || sprite.Animation == "run") // not changed by dashing or fluttering
                    {
                        if (velocity.X == 0)
                        {
                            if (sprite.Animation != "idle") sprite.Play("idle");
                        }
                        else
                        {
                            if (sprite.Animation != "run") sprite.Play("run");
                        }
                    }

                    FlipSpriteInput();
                }
                break;
            case State.Dash:
                {
                    MoveHorizontal(_delta);

                    HandleJump(_delta);

                    DoGravity(_delta);

                    FlipSpriteVelocity();

                    if (Math.Abs(velocity.X) <= MaxSpeed)
                    {
                        CurrentState = State.Normal;
                    }
                }
                break;
            case State.Flutter:
                {
                    MoveHorizontal(_delta);

                    HandleDash();

                    velocity.Y += FlutterAccel * _delta;

                    flutterTime -= _delta;

                    FlipSpriteInput();

                    if (flutterTime < 0)
                    {
                        sprite.Play("run");
                        CurrentState = State.Normal;
                    }
                }
                break;
        }
    }

    private void UpdateTimers(float delta)
    {
        if (IsOnFloor())
        {
            flutterUsed = false;
            dashUsed = false;
            coyoteTime = CoyoteTimeMax;
        }
        if (coyoteTime > 0) coyoteTime -= delta;


    }

    private void FlipSpriteVelocity()
    {
        if (velocity.X != 0)
        {
            sprite.FlipH = (velocity.X < 0);
        }
    }

    private void FlipSpriteInput()
    {
        if (inputDir!= 0)
        {
            sprite.FlipH = (inputDir < 0);
        }
    }

    private void OnAnimationEnd()
    {
        GD.Print("animation ended");
        if (sprite.Animation == "dash")
        {
            GD.Print("dash ended");
            sprite.Play("run");
        }
    }
}
