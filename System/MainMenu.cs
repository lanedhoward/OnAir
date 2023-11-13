using Godot;
using System;

public partial class MainMenu : Control
{
    //[Export]
    //public PackedScene gameScene;
    [Export]
    public string ScenePath;
    public void StartGame()
    {
        //GetTree().ChangeSceneToPacked(gameScene);
        GetTree().ChangeSceneToFile(ScenePath);
    }

    public void ExitGame()
    {
        GetTree().Quit();
    }
}
