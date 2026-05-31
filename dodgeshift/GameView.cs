using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace dodgeshift;

public class GameView
{
    private readonly GameModel _model;
    private readonly GraphicsDevice _graphicsDevice;
    private SpriteBatch _spriteBatch;
    private Texture2D _blankTexture;
    private SpriteFont _font;

    public GameView(GameModel model, GraphicsDevice graphicsDevice)
    {
        _model = model;
        _graphicsDevice = graphicsDevice;
    }

    public void LoadContent(SpriteFont font)
    {
        _spriteBatch = new SpriteBatch(_graphicsDevice);
        _blankTexture = new Texture2D(_graphicsDevice, 1, 1);
        _blankTexture.SetData(new[] { Color.White });
        _font = font;
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

        _spriteBatch.DrawString(_font, $"Score: {_model.Score}", new Vector2(20, 20), Color.White);

        if (_model.IsGameOver)
        {
            Rectangle overlay = new Rectangle(0, 0, _model.WindowWidth, _model.WindowHeight);
            _spriteBatch.Draw(_blankTexture, overlay, new Color(Color.Black, 0.6f));
            
            string gameOverText = $"GAME OVER\nFinal Score: {_model.Score}\nPress R to Restart";
            Vector2 textSize = _font.MeasureString(gameOverText);
            Vector2 textPosition = new Vector2(
                (_model.WindowWidth - textSize.X) / 2,
                (_model.WindowHeight - textSize.Y) / 2
            );
            
            _spriteBatch.DrawString(_font, gameOverText, textPosition, Color.Red);
        }

        _spriteBatch.End();
    }
}
