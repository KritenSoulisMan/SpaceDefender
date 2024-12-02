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
        private List<Bonus> _bonuses = new List<Bonus>();
        public int Score { get; private set; }
        private int _level = 1;
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
        }

        private void UpdateBonuses()
        {
            foreach (var bonus in _bonuses.ToArray())
            {
                bonus.Y += 4; // Увеличиваем скорость падения бонусов (например, 4 пикселя за кадр)
                if (bonus.Tag is System.Windows.Shapes.Rectangle bonusRect)
                {
                    Canvas.SetLeft(bonusRect, bonus.X);
                    Canvas.SetTop(bonusRect, bonus.Y);
                }

                // Удаляем бонусы, которые вышли за границы канваса
                if (bonus.Y > _gameCanvas.ActualHeight)
                {
                    _bonuses.Remove(bonus);
                    if (bonus.Tag is System.Windows.Shapes.Rectangle rect)
                    {
                        _gameCanvas.Children.Remove(rect);
                    }
                }
            }
        }

        private void ApplyBonus(Bonus bonus)
        {
            switch (bonus.Type)
            {
                case 1:
                    // Увеличиваем размер игрока
                    _player.Size += 10; // Увеличиваем размер на 10
                    _player.Width = _player.Size; // Обновляем ширину модели
                    break;
                case 2:
                    Score += 50; // Увеличиваем счет на 50
                    break;
                case 3:
                    // Логика для уменьшения размера игрока
                    if (_player.Size > 10) // Минимальный размер
                    {
                        _player.Size -= 10; // Уменьшаем размер на 10
                        _player.Width = _player.Size; // Обновляем ширину модели
                    }
                    break;
                    // Добавьте другие типы бонусов по мере необходимости
            }
        }

        public void MovePlayer(System.Windows.Input.Key key)
        {
            if (key == System.Windows.Input.Key.Left && _player.X > 0) _player.X -= 10;
            if (key == System.Windows.Input.Key.Right && _player.X < _gameCanvas.ActualWidth - _player.Width) _player.X += 10;
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
                Fill = System.Windows.Media.Brushes.Red
            };
            Canvas.SetLeft(bulletRect, bullet.X);
            Canvas.SetTop(bulletRect, bullet.Y);
            _gameCanvas.Children.Add(bulletRect);
            bullet.Tag = bulletRect; // Связываем пулю с визуальным элементом
            _bullets.Add(bullet);
        }

        private void UpdateBullets()
        {
            foreach (var bullet in _bullets.ToArray())
            {
                bullet.Y -= 10; // Двигаем пулю вверх
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
            var enemy = new Enemy { X = rand.Next(0, 360), Y = 0, Width = 30, Height = 30 };
            var enemyRect = new System.Windows.Shapes.Rectangle
            {
                Width = enemy.Width,
                Height = enemy.Height,
                Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb((byte)rand.Next(0, 256), (byte)rand.Next(0, 256), (byte)rand.Next(0, 256)))
            };

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

        private bool CheckCollision(GameObject a, GameObject b)
        {
            return a.X < b.X + b.Width && a.X + a.Width > b.X && a.Y < b.Y + b.Height && a.Y + a.Height > b.Y;
        }

        private void CheckBonusCollection()
        {
            foreach (var bonus in _bonuses.ToArray())
            {
                if (CheckCollision(bonus, _player))
                {
                    ApplyBonus(bonus); // Применяем эффект бонуса
                    if (bonus.Tag is System.Windows.Shapes.Rectangle bonusRect)
                    {
                        _gameCanvas.Children.Remove(bonusRect);
                    }
                    _bonuses.Remove(bonus);
                }
            }
        }

        private void HandleCollisions()
        {
            foreach (var enemy in _enemies.ToArray())
            {
                foreach (var bullet in _bullets.ToArray())
                {
                    if (CheckCollision(bullet, enemy))
                    {
                        _bullets.Remove(bullet);
                        _enemies.Remove(enemy);

                        Score += 10; // Увеличиваем счет

                        Random rand = new Random();
                        if (rand.NextDouble() <= 0.75) // 75% шанс на выпадение бонуса
                        {
                            var bonus = new Bonus { X = enemy.X, Y = enemy.Y, Width = 20, Height = 20, Type = rand.Next(1, 4) };
                            CreateBonusVisual(bonus);
                            _bonuses.Add(bonus); // Добавляем бонус в список
                        }

                        if (bullet.Tag is System.Windows.Shapes.Rectangle bulletRect)
                        {
                            _gameCanvas.Children.Remove(bulletRect);
                        }
                        if (enemy.Tag is System.Windows.Shapes.Rectangle enemyRect)
                        {
                            _gameCanvas.Children.Remove(enemyRect);
                        }
                        break;
                    }
                }

                if (CheckCollision(enemy, _player))
                {
                    EndGame();
                    return;
                }
            }

            CheckBonusCollection();
        }

        private void CreateBonusVisual(Bonus bonus)
        {
            var bonusRect = new System.Windows.Shapes.Rectangle
            {
                Width = bonus.Width,
                Height = bonus.Height,
                Fill = System.Windows.Media.Brushes.White // Цвет для бонусов
            };

            Canvas.SetLeft(bonusRect, bonus.X);
            Canvas.SetTop(bonusRect, bonus.Y);

            _gameCanvas.Children.Add(bonusRect);
            bonus.Tag = bonusRect; // Связываем бонус с визуальным элементом
        }

        private void UpdateLevel()
        {
            if (Score >= _level * 100)
            {
                // Логика повышения уровня сложности
                foreach (var enemy in _enemies)
                {
                    enemy.Speed += 0.5; // Увеличиваем скорость врагов
                }
                _level++;
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
            UpdateBonuses(); // Обновляем позиции бонусов
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
                // Обновляем размеры модели игрока
                if (_playerShip is System.Windows.Shapes.Rectangle playerRect)
                {
                    playerRect.Width = _player.Width;
                    playerRect.Height = _player.Size; // Устанавливаем высоту в зависимости от размера
                }
            });
        }

        private void EndGame()
        {
            _gameTimer.Stop();
            // Удаляем все визуальные элементы с канваса 
            _gameCanvas.Children.Clear();
            MessageBox.Show("Попробуй снова! Ваш счёт: " + Score);
            ResetGame();
        }

        private void ResetGame()
        {
            Score = 0;
            _level = 1;
            _enemies.Clear();
            _bullets.Clear();

            // Инициализация игрока и установка начального размера
            _player.X = 180;
            _player.Size = 40; // Сброс размера до начального значения
            UpdateUI();
        }
    }
}