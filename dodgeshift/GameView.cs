using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace dodgeshift;

public class GameView
{
    private readonly GameModel _model;
    private readonly GraphicsDevice _graphicsDevice;
    private SpriteBatch _spriteBatch;
    private Texture2D _blankTexture;

    public GameView(GameModel model, GraphicsDevice graphicsDevice)
    {
        _model = model;
        _graphicsDevice = graphicsDevice;
    }

    public void LoadContent()
    {
        _spriteBatch = new SpriteBatch(_graphicsDevice);
        
        _blankTexture = new Texture2D(_graphicsDevice, 1, 1);
        _blankTexture.SetData(new[] { Color.White });
    }

    public void Draw()
    {
        _graphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin();
        
        _spriteBatch.Draw(_blankTexture, _model.Player, Color.Green);
        
        foreach (var fallingBlock in _model.FallingBlocks)
        {
            _spriteBatch.Draw(_blankTexture, fallingBlock, Color.Red);
        }
        
        foreach (var staticBlock in _model.StaticBlocks)
        {
            _spriteBatch.Draw(_blankTexture, staticBlock, Color.Yellow);
        }

        _spriteBatch.End();
    }
}