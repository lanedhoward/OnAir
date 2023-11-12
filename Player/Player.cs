using Godot;
using System;

public partial class Player : CharacterBody2D
{
    float inputDir;
    bool jumpPressed;
    bool jumpHeld;
    bool jumpReleased;
    bool dashPressed;

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
    float Gravity;

    [Export]
    float DashForce;

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
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        float _delta = (float)delta;

        velocity = Velocity;

        GetInput();

        ProcessState(_delta);

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

    private void HandleJump()
    {
        if (jumpPressed)
        {
            if (CanJump())
            {
                velocity.Y = JumpForce;
            }
        }
    }

    private bool CanJump()
    {
        return IsOnFloor();
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
                velocity.X = DashForce * inputDir;
                velocity.Y = 0;
                CurrentState = State.Dash;
            }
        }
    }

    private bool CanDash()
    {
        return true;
    }


    private void ProcessState(float _delta)
    {
        switch (CurrentState)
        {
            default:
            case State.Normal:
                {
                    MoveHorizontal(_delta);

                    HandleJump();

                    HandleDash();

                    DoGravity(_delta);
                }
                break;
            case State.Dash:
                {
                    MoveHorizontal(_delta);

                    HandleJump();

                    DoGravity(_delta);

                    if (Math.Abs(velocity.X) <= MaxSpeed)
                    {
                        CurrentState = State.Normal;
                    }
                }
                break;

            
        }
    }
}
