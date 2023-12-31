using Godot;
using System;

public partial class Obstacle : Area2D
{
    [Export]
    bool bouncePlayer = true;

    public void OnBodyEntered(Node2D body)
    {
        if (body is Player p)
        {
            if (p.CurrentState != Player.State.Hit)
            {
                p.GetHit(bouncePlayer);
            }
        }
    }
}
