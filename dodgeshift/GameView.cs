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
        
        foreach (var staticBlock in _model.StaticBlocks)
        {
            int screenY = staticBlock.Y - (int)_model.CameraMovement;
            if (screenY >= -_model.BlockSize && screenY <= _model.WindowHeight)
            {
                Rectangle screenRect = new Rectangle(
                    staticBlock.X, 
                    screenY, 
                    staticBlock.Width + 1, 
                    staticBlock.Height + 1
                );
                _spriteBatch.Draw(_blankTexture, screenRect, Color.Yellow);
            }
        }
        
        foreach (var fallingBlock in _model.FallingBlocks)
        {
            int screenY = fallingBlock.Y - (int)_model.CameraMovement;
            
            Rectangle screenRect = new Rectangle(
                fallingBlock.X, 
                screenY, 
                fallingBlock.Width + 1, 
                fallingBlock.Height + 1
            );
            _spriteBatch.Draw(_blankTexture, screenRect, Color.Red);
        }
        
        int playerScreenY = _model.Player.Y - (int)_model.CameraMovement;
        Rectangle playerScreenRect = new Rectangle(_model.Player.X, playerScreenY, _model.Player.Width, _model.Player.Height);
        _spriteBatch.Draw(_blankTexture, playerScreenRect, Color.Green);

        _spriteBatch.End();
    }
}
