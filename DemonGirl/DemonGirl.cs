using Godot;
using System;

public partial class DemonGirl : Area2D
{
    [Signal]
    public delegate void PlayerTouchedDevilEventHandler();

    AnimatedSprite2D sprite;

    Timer timer;

    [Export]
    public string ScenePath;

    public override void _Ready()
    {
        base._Ready();
        sprite = GetNode<AnimatedSprite2D>("Sprite2D");
        sprite.Play("idle");
        timer = GetNode<Timer>("Timer");
    }

    public void OnBodyEntered(Node2D body)
    {
        if (body is Player p)
        {
            //yippeee !!!!!
            sprite.Play("yippee");
            EmitSignal(SignalName.PlayerTouchedDevil);
            p.StartYippee();
            timer.Start();
        }
    }

    public void ChangeScene()
    {
        GetTree().ChangeSceneToFile(ScenePath);
    }
}
