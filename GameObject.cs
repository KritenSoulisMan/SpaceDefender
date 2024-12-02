public class GameObject
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
}
public class Bonus : GameObject
{
    public int Type { get; set; } // Тип бонуса (например, 1 - здоровье, 2 - очки)
    public object Tag { get; set; }
}
public class Player : GameObject
{
    public double Size { get; set; } = 40; // Начальный размер игрока
}
public class Enemy : GameObject
{
    public double Speed { get; set; } = 2.0;
    public object Tag { get; set; }
}
public class Bullet : GameObject 
{
    public object Tag { get; set; }
}