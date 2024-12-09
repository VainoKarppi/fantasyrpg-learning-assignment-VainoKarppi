public interface ICharacter {
    int ID { get; }
    string? Name { get; }
    int Health { get; }
    int Mana { get; }

    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; }
    public int Height { get; }
    public Color Color => Color.Red;
    public Rectangle Bounds => new Rectangle(X, Y, Width, Height);

    public State CurrentState { get; set; }
    public ItemWeapon CurrentWeapon { get; set; }

    public enum State {
        Idle,
        Attacking,
        Moving,
        Waiting
    }

    public virtual bool CanAttack(ICharacter target) {
        // Check if the character is not already attacking or moving
        bool state = CurrentState != State.Attacking && CurrentState != State.Moving;
        if (!state) return false;

        // Check weapon range (assuming CalculateDistance is a method of World)
        double distance = World.CalculateDistance(this, target);
        int weaponRange = CurrentWeapon.Range;
        if (weaponRange < distance) return false;

        return true;
    }
}