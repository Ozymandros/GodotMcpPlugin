namespace GodotMcp.Tests.CoreTests;

public class McpProjectFilePathNormalizationTests
{
    [Fact]
    public void Constructor_NormalizesRelativePathWithMixedSeparators()
    {
        var file = new McpProjectFile(Root, @"\scenes\..\scenes/main.tscn");

        Assert.Equal(Root, file.ProjectPath);
        Assert.Equal("scenes/main.tscn", file.FileName);
    }

    [Fact]
    public void Constructor_NormalizesAbsolutePathInsideProject()
    {
        var absoluteScenePath = Path.Combine(Root, "Scenes", "Main.tscn");
        var file = new McpProjectFile(Root, absoluteScenePath);

        Assert.Equal(Root, file.ProjectPath);
        Assert.Equal("Scenes/Main.tscn", file.FileName);
    }

    [Fact]
    public void Constructor_NormalizesProjectRootFromPartialPath()
    {
        var relativeProjectRoot = @".\..\Godot-MCP-SK-Plugin";
        var file = new McpProjectFile(relativeProjectRoot, "Scenes/Main.tscn");

        Assert.Equal(Path.TrimEndingDirectorySeparator(Path.GetFullPath(relativeProjectRoot)), file.ProjectPath);
        Assert.Equal("Scenes/Main.tscn", file.FileName);
    }

    [Fact]
    public void Constructor_RejectsPathOutsideProjectRoot()
    {
        var escapingPath = Path.Combine("..", "other", "outside.tscn");

        Assert.Throws<ArgumentException>(() => new McpProjectFile(Root, escapingPath));
    }

    [Fact]
    public void ForScene_PrependsScenesFolderAndValidatesTscn()
    {
        var scene = McpProjectFile.ForScene(Root, "mouse_test.tscn");
        Assert.Equal("scenes/mouse_test.tscn", scene.FileName);
    }

    [Fact]
    public void ForScene_AcceptsAlreadyScenesRelativePath()
    {
        var scene = McpProjectFile.ForScene(Root, "scenes/sub/menu.tscn");
        Assert.Equal("scenes/sub/menu.tscn", scene.FileName);
    }

    [Fact]
    public void ForScene_RejectsNonTscnExtensions()
    {
        Assert.Throws<ArgumentException>(() => McpProjectFile.ForScene(Root, "main.scn"));
    }
}
