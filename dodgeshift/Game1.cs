using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using Microsoft.Xna.Framework.Graphics; 

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

        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);
        
        _controller = new GameController(_model);
        _view = new GameView(_model, GraphicsDevice);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        SpriteFont gameFont = Content.Load<SpriteFont>("scoreFont");
        _view.LoadContent(gameFont);
    }

    protected override void Update(GameTime gameTime)
    {
        float timePassed = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_model.IsGameOver)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.R))
            {
                _model.Reset();
            }
            base.Update(gameTime);
            return;
        }

        _controller.ProcessInput();
        _controller.Update(timePassed); 

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _view.Draw();
        base.Draw(gameTime);
    }
}