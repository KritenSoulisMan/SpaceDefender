public class GameObject
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
}

public class Player : GameObject { }
public class Enemy : GameObject
{
    public double Speed { get; set; } = 2.0;
    public object Tag { get; set; }
}
public class Bullet : GameObject 
{
    public object Tag { get; set; }
}
public class Bonus : GameObject
{
    public string Type { get; set; } // Например, "SpeedBoost", "Shield"
}
public class Ball : GameObject 
{ 
    public double VelocityX { get; set; } 
    public double VelocityY { get; set; }
    public object Tag { get; set; }
}