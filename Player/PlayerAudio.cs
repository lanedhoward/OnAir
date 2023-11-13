using Godot;
using System;

public partial class PlayerAudio : AudioStreamPlayer2D
{

    [Export]
    public AudioStream jumpSound, dashSound, landSound, flutterSound, hitSound;

    public void PlaySound(AudioStream sound)
    {
        Stop();
        Stream = sound;
        Play();
    }
}
