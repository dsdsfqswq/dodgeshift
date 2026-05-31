using Microsoft.Xna.Framework;

namespace dodgeshift;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private GameModel _model;
    private GameView _view;
    private GameController _controller;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _model = new GameModel();
        _graphics.PreferredBackBufferWidth = _model.WindowWidth;
        _graphics.PreferredBackBufferHeight = _model.WindowHeight;
        _graphics.ApplyChanges();
        
        _controller = new GameController(_model);
        _view = new GameView(_model, GraphicsDevice);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _view.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        float timePassed = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _controller.ProcessInput();
        _controller.Update(timePassed); 
    
        if (_model.IsGameOver)
        {
            Exit(); 
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _view.Draw();
        base.Draw(gameTime);
    }
}