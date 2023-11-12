using Godot;
using System;

public partial class Player : CharacterBody2D
{
    float inputDir;
    bool jumpPressed;
    bool jumpHeld;
    bool jumpReleased;

    public Vector2 velocity;

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

        MoveHorizontal(_delta);

        HandleJump();

        DoGravity(_delta);

        Velocity = velocity;

        MoveAndSlide();
    }

    private void GetInput()
    {
        inputDir = Input.GetActionStrength("Right") - Input.GetActionStrength("Left");

        jumpPressed = Input.IsActionJustPressed("Jump");
        jumpHeld = Input.IsActionPressed("Jump");
        jumpReleased = Input.IsActionJustReleased("Jump");
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
                GD.Print("trying jumping");
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
        if (!IsOnFloor())
        {
            velocity.Y += delta * Gravity;
        }
        else
        {
            //velocity.Y = 0;
        }
        /*
        if (velocity.Y < 0)
        {
        }
        else
        {
            velocity.Y = 0f;
        }
        */
    }
}
