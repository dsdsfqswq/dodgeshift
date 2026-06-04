using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace dodgeshift;

public enum BlockType { Regular, ShieldBonus }

public struct Block
{
    public Rectangle Rect;
    public BlockType Type;
}

public class GameModel
{
    public Rectangle Player;
    public List<Block> FallingBlocks { get; } = new();
    public List<Block> StaticBlocks { get; } = new();
    
    public int WindowWidth { get; } = 480;
    public int WindowHeight { get; } = 700;
    public int BlockSize { get; } = 40;
    public int GroundLevelY { get; } = 700; 
    
    private const float GravityForce = 900f;       
    private const float JumpImpulse = -450f;       
    private const int MovementSpeed = 350;        
    
    private const float SpawnIntervalSeconds = 0.5f; 
    private const float FallingSpeed = 300f;      
    
    private const int CollisionTolerance = 14;
    private const float CameraSmooth = 0.1f;
    private const string HighScoreFile = "highscore.txt";
    
    private float _playerExactX;
    private float _playerExactY;
    private List<float> _fallingBlocksExactY = new(); 

    private float _verticalVelocity;
    private bool _isOnTheFloor;
    private float _spawnTimer;
    private readonly Random _randomGenerator = new();
    private readonly int _columns;

    private int _blocksScore;
    private int _heightScore;

    private float _shieldTimer = 0f;
    public bool IsShielded => _shieldTimer > 0f;

    public bool IsGameOver { get; private set; }
    public float CameraMovement { get; private set; }
    public int Score => _blocksScore + _heightScore;
    public int HighScore { get; private set; }

    public GameModel()
    {
        _columns = WindowWidth / BlockSize;
        LoadHighScore();
        Reset();
    }

    private void LoadHighScore()
    {
        if (File.Exists(HighScoreFile))
        {
            if (int.TryParse(File.ReadAllText(HighScoreFile), out int savedScore))
            {
                HighScore = savedScore;
            }
        }
        else
        {
            HighScore = 0;
        }
    }

    private void SaveHighScore()
    {
        if (Score > HighScore)
        {
            HighScore = Score;
            File.WriteAllText(HighScoreFile, HighScore.ToString());
        }
    }

    public void Reset()
    {
        CameraMovement = 0;
        _blocksScore = 0;
        _heightScore = 0;
        _playerExactX = WindowWidth / 2f;
        _playerExactY = GroundLevelY - BlockSize;
        Player = new Rectangle((int)MathF.Round(_playerExactX), (int)MathF.Round(_playerExactY), BlockSize, BlockSize);
        
        FallingBlocks.Clear();
        _fallingBlocksExactY.Clear();
        StaticBlocks.Clear();
        
        _verticalVelocity = 0;
        _isOnTheFloor = true;
        _spawnTimer = 0;
        _shieldTimer = 0f;
        IsGameOver = false;
        LoadHighScore();
    }
    
    public void MovePlayer(int direction, float timePassed)
    {
        if (direction == 0) return;

        float oldX = _playerExactX;
        _playerExactX += direction * MovementSpeed * timePassed;
        Player.X = (int)MathF.Round(_playerExactX);

        foreach (var staticBlock in StaticBlocks)
        {
            if (Player.Intersects(staticBlock.Rect))
            {
                _playerExactX = oldX;
                Player.X = (int)MathF.Round(_playerExactX);
                break;
            }
        }
        _playerExactX = Math.Clamp(_playerExactX, 0, WindowWidth - Player.Width);
        Player.X = (int)MathF.Round(_playerExactX);
    }

    public void TryJump()
    {
        if (_isOnTheFloor)
        {
            _verticalVelocity = JumpImpulse;
            _isOnTheFloor = false;
        }
    }

    public void UpdatePhysics(float timePassed)
    {
        if (_shieldTimer > 0f)
        {
            _shieldTimer -= timePassed;
        }

        _verticalVelocity += GravityForce * timePassed;
        _playerExactY += _verticalVelocity * timePassed;
        Player.Y = (int)MathF.Round(_playerExactY);

        _isOnTheFloor = false;
        if (Player.Bottom >= GroundLevelY)
        {
            _playerExactY = GroundLevelY - Player.Height;
            Player.Y = (int)MathF.Round(_playerExactY);
            _verticalVelocity = 0;
            _isOnTheFloor = true;
        }
        
        foreach (var staticBlock in StaticBlocks)
        {
            if (Player.Intersects(staticBlock.Rect))
            {
                if (_verticalVelocity >= 0 && Player.Bottom <= staticBlock.Rect.Top + CollisionTolerance + 5)
                {
                    _playerExactY = staticBlock.Rect.Y - Player.Height;
                    Player.Y = (int)MathF.Round(_playerExactY);
                    _verticalVelocity = 0;
                    _isOnTheFloor = true;
                }
            }
        }

        _spawnTimer += timePassed;
        if (_spawnTimer > SpawnIntervalSeconds)
        {
            int gridPositionX = _randomGenerator.Next(0, _columns) * BlockSize;
            int spawnY = ((int)CameraMovement / BlockSize) * BlockSize - BlockSize;
            
            BlockType spawnedType = BlockType.Regular;
            int chance = _randomGenerator.Next(0, 100);
            
            if (chance < 5)
            {
                spawnedType = BlockType.ShieldBonus;
            }

            Block newBlock = new Block 
            { 
                Rect = new Rectangle(gridPositionX, spawnY, BlockSize, BlockSize), 
                Type = spawnedType 
            };

            FallingBlocks.Add(newBlock);
            _fallingBlocksExactY.Add(spawnY); 
            _spawnTimer = 0;
        }
        
        for (int i = 0; i < FallingBlocks.Count; i++)
        {
            _fallingBlocksExactY[i] += FallingSpeed * timePassed;
            
            var fallingBlock = FallingBlocks[i];
            fallingBlock.Rect.Y = (int)MathF.Round(_fallingBlocksExactY[i]);
            FallingBlocks[i] = fallingBlock;
            
            if (fallingBlock.Rect.Intersects(Player))
            {
                if (fallingBlock.Rect.Bottom < Player.Bottom + CollisionTolerance)
                {
                    if (fallingBlock.Type == BlockType.ShieldBonus)
                    {
                        _shieldTimer = 5f;
                        FallingBlocks.RemoveAt(i);
                        _fallingBlocksExactY.RemoveAt(i);
                        i--;
                        continue;
                    }
                    else if (!IsShielded)
                    {
                        IsGameOver = true;
                        SaveHighScore();
                    }
                }
            }
            
            bool touchesStaticBlock = StaticBlocks.Any(sb => fallingBlock.Rect.Intersects(sb.Rect));
            bool touchesGround = fallingBlock.Rect.Bottom >= GroundLevelY;

            if (touchesGround || touchesStaticBlock)
            {
                fallingBlock.Rect.X = (fallingBlock.Rect.X / BlockSize) * BlockSize;

                if (touchesStaticBlock)
                {
                    var hitBlock = StaticBlocks.First(sb => fallingBlock.Rect.Intersects(sb.Rect));
                    fallingBlock.Rect.Y = hitBlock.Rect.Y - BlockSize;
                }
                else
                {
                    fallingBlock.Rect.Y = GroundLevelY - BlockSize;
                }

                _fallingBlocksExactY[i] = fallingBlock.Rect.Y;

                StaticBlocks.Add(fallingBlock);
                FallingBlocks.RemoveAt(i);
                _fallingBlocksExactY.RemoveAt(i);
                i--;
                
                _blocksScore += 10;
            }
        }

        for (int i = StaticBlocks.Count - 1; i >= 0; i--)
        {
            int screenY = StaticBlocks[i].Rect.Y - (int)CameraMovement;
            if (screenY > WindowHeight + BlockSize)
            {
                StaticBlocks.RemoveAt(i);
            }
        }
        
        UpdateCamera(timePassed);
    }

    private void UpdateCamera(float timePassed)
    {
        float screenCenterY = (WindowHeight / 2f) + CameraMovement;
        if (Player.Y < screenCenterY)
        {
            CameraMovement += (Player.Y - screenCenterY) * CameraSmooth;
        }
        
        int currentHeightScore = (int)Math.Max(0, -CameraMovement);
        if (currentHeightScore > _heightScore)
        {
            _heightScore = currentHeightScore;
        }
        
        if (Player.Y > WindowHeight + CameraMovement)
        {
            IsGameOver = true;
            SaveHighScore();
        }
    }
}
