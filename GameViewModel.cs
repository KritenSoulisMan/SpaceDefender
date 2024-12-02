using System;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SpaceDefender
{
    public class GameViewModel
    {
        private DispatcherTimer _gameTimer;
        private Player _player;
        private readonly Canvas _gameCanvas;
        private readonly UIElement _playerShip;
        private List<Enemy> _enemies = new List<Enemy>();
        private List<Bullet> _bullets = new List<Bullet>();
        public int Score { get; private set; }
        private int _level = 1;
        private Ball _ball;
        private int _enemySpawnTimer = 0;

        public GameViewModel(Canvas gameCanvas, UIElement playerShip)
        {
            _player = new Player { X = 180, Y = 550, Width = 40, Height = 20 };
            Score = 0;

            _gameTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _gameTimer.Tick += GameLoop;
            _gameTimer.Start();
            _gameCanvas = gameCanvas;
            _playerShip = playerShip;
            InitializeBall();
            _gameTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            }; _gameTimer.Tick += GameLoop;
            _gameTimer.Start();
        }


        private void InitializeBall()
        {
            _ball = new Ball
            {
                X = 200,
                Y = 300,
                Width = 20,
                Height = 20,
                VelocityX = 5,
                VelocityY = -5
            };
            var ballRect = new System.Windows.Shapes.Rectangle
            {
                Width = _ball.Width,
                Height = _ball.Height,
                Fill = System.Windows.Media.Brushes.Yellow
            };
            Canvas.SetLeft(ballRect, _ball.X);
            Canvas.SetTop(ballRect, _ball.Y);
            _gameCanvas.Children.Add(ballRect);
            _ball.Tag = ballRect;
        }

        private void UpdateBall()
        {
            _ball.X += _ball.VelocityX; 
            _ball.Y += _ball.VelocityY; 
            if (_ball.Tag is System.Windows.Shapes.Rectangle ballRect) 
            { 
                Canvas.SetLeft(ballRect, _ball.X); 
                Canvas.SetTop(ballRect, _ball.Y); 
            } 

            // Проверяем столкновение с границами
            if (_ball.X <= 0 || _ball.X + _ball.Width >= _gameCanvas.ActualWidth) 
            { 
                _ball.VelocityX *= -1; // Меняем направление по X
            } 

            if (_ball.Y <= 0) 
            { 
                _ball.VelocityY *= -1; // Меняем направление по Y
            } 

            // Проверяем столкновение с платформой игрока
            if (CheckCollision(_ball, _player)) 
            { 
                _ball.VelocityY *= -1; // Меняем направление по Y
            } 

            // Если шарик покидает нижнюю границу
            if (_ball.Y > _gameCanvas.ActualHeight) 
            { 
                // Можно добавить логику завершения игры или сброса шарика
                InitializeBall(); 
            } 
        }


        public void MovePlayer(System.Windows.Input.Key key)
        {
            if (key == System.Windows.Input.Key.Left && _player.X > 0)
                _player.X -= 10;
            if (key == System.Windows.Input.Key.Right && _player.X < _gameCanvas.ActualWidth - _player.Width)
                _player.X += 10;
        }

        public void Shoot()
        {
            var bullet = new Bullet
            {
                X = _player.X + _player.Width / 2 - 5,
                Y = _player.Y - 10,
                Width = 10,
                Height = 20
            };
            var bulletRect = new System.Windows.Shapes.Rectangle
            {
                Width = bullet.Width,
                Height = bullet.Height,
                Fill = System.Windows.Media.Brushes.Red // Color for bullets
            };
            Canvas.SetLeft(bulletRect, bullet.X);
            Canvas.SetTop(bulletRect, bullet.Y);
            _gameCanvas.Children.Add(bulletRect);
            bullet.Tag = bulletRect; // Link bullet with its visual representation
            _bullets.Add(bullet);
        }

        private void UpdateBullets()
        {
            foreach (var bullet in _bullets.ToArray())
            {
                bullet.Y -= 10; // Двигаем пулю вверх

                // Обновляем визуальное представление пули
                if (bullet.Tag is System.Windows.Shapes.Rectangle bulletRect)
                {
                    Canvas.SetLeft(bulletRect, bullet.X);
                    Canvas.SetTop(bulletRect, bullet.Y);
                }
            }

            // Удаляем пули, которые вышли за границы канваса
            _bullets.RemoveAll(b => b.Y < 0);
        }

        private void GenerateEnemy()
        {
            Random rand = new Random(); 
            var enemy = new Enemy 
            { 
                X = rand.Next(0, 360),
                Y = 0, 
                Width = 30, 
                Height = 30 
            }; // Создаем прямоугольник-врага
            var enemyRect = new System.Windows.Shapes.Rectangle
            {
                Width = enemy.Width,
                Height = enemy.Height,
                Fill = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(
                        (byte)rand.Next(0, 256),
                        (byte)rand.Next(0, 256),
                        (byte)rand.Next(0, 256)
            ))};
            Canvas.SetLeft(enemyRect, enemy.X);
            Canvas.SetTop(enemyRect, enemy.Y);
            _gameCanvas.Children.Add(enemyRect);
            enemy.Tag = enemyRect; // Связываем врага с визуальным элементом
            _enemies.Add(enemy);
        }

        private void UpdateEnemies()
        {
            foreach (var enemy in _enemies.ToArray())
            {
                enemy.Y += enemy.Speed;
                if (enemy.Tag is System.Windows.Shapes.Rectangle enemyRect)
                {
                    Canvas.SetTop(enemyRect, enemy.Y);
                }
                if (enemy.Y > 600) // Удаляем врагов, покинувших экран 
                {
                    _enemies.Remove(enemy);
                    if (enemy.Tag is System.Windows.Shapes.Rectangle rect)
                    {
                        _gameCanvas.Children.Remove(rect);
                    }
                }
            }
        }


        private bool CheckCollision( GameObject a, GameObject b)
        {
            return a.X < b.X + b.Width &&
               a.X + a.Width > b.X &&
               a.Y < b.Y + b.Height &&
               a.Y + a.Height > b.Y;
        }

        private void HandleCollisions()
        {
            foreach (var enemy in _enemies.ToArray())
            {
                foreach (var bullet in _bullets.ToArray())
                {
                    if (CheckCollision(bullet, enemy))
                    {
                        // Удаляем пулю и врага
                        _bullets.Remove(bullet);
                        _enemies.Remove(enemy);

                        // Увеличиваем счет
                        Score += 10;

                        // Удаляем визуальные элементы из канваса
                        if (bullet.Tag is System.Windows.Shapes.Rectangle bulletRect)
                        {
                            _gameCanvas.Children.Remove(bulletRect);
                        }

                        if (enemy.Tag is System.Windows.Shapes.Rectangle enemyRect)
                        {
                            _gameCanvas.Children.Remove(enemyRect);
                        }

                        break; // Выходим из внутреннего цикла, чтобы не изменять коллекцию во время итерации
                    }
                }

                // Проверяем столкновение врага с игроком
                if (CheckCollision(enemy, _player))
                {
                    EndGame();
                    return;
                }
            }
        }

        private void UpdateLevel()
        {
            if (Score >= _level * 100)
            {
                _level++;
                foreach (var enemy in _enemies)
                {
                    enemy.Speed += 0.5;
                }
            }
        }


        private void GameLoop(object sender, EventArgs e)
        {
            _enemySpawnTimer++;
            if (_enemySpawnTimer > 60)
            {
                GenerateEnemy();
                _enemySpawnTimer = 0;
            }
            UpdateBullets();
            UpdateEnemies();
            UpdateBall();
            HandleCollisions();
            UpdateLevel();
            UpdateUI();
        }

        private void UpdateUI()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Canvas.SetLeft(_playerShip, _player.X);
                Canvas.SetTop(_playerShip, _player.Y);
            });
        }

        private void EndGame()
        {
            _gameTimer.Stop();

            // Удаляем все визуальные элементы с канваса
            _gameCanvas.Children.Clear();

            // Можно добавить сообщение о завершении игры
            MessageBox.Show("Попробуй снова! Ваш счёт: " + Score);

            // Сброс состояния игры
            ResetGame();
        }

        private void ResetGame()
        {
            Score = 0;
            _level = 1;
            _enemies.Clear();
            _bullets.Clear();

            // Инициализация игрока и шара
            _player.X = 180;
            InitializeBall();

            // Обновляем интерфейс пользователя
            UpdateUI();
        }
    }
}