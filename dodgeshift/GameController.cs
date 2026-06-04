using Microsoft.Xna.Framework.Input;

namespace dodgeshift;

public class GameController
{
    private readonly GameModel _model;
    private KeyboardState _previousKeyboardState;

    public GameController(GameModel model)
    {
        _model = model;
    }

    public void ProcessInput()
    {
        var currentKeyboardState = Keyboard.GetState();
        
        if (currentKeyboardState.IsKeyDown(Keys.Space) && !_previousKeyboardState.IsKeyDown(Keys.Space))
        {
            _model.TryJump();
        }

        _previousKeyboardState = currentKeyboardState;
    }
    
    public void Update(float timePassed)
    {
        if (_model.IsGameOver)
        {
            return;
        }

        int horizontalMove = 0;

        if (_previousKeyboardState.IsKeyDown(Keys.A))
        {
            horizontalMove -= 1;
        }

        if (_previousKeyboardState.IsKeyDown(Keys.D))
        {
            horizontalMove += 1;
        }
        
        _model.MovePlayer(horizontalMove, timePassed);
        _model.UpdatePhysics(timePassed);
    }
}