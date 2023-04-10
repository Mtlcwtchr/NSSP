namespace Model
{
    public interface ISupplies<T>
    {
        SuppliesType Type { get; }
        T Value { get; }

        void Add(ISupplies<T> supplies);
        void Consume(T value);
    }

    public abstract class Supplies<T> : ISupplies<T>
    {
        public abstract SuppliesType Type { get; }
        public T Value { get; set; }
        public abstract void Add(ISupplies<T> supplies);
        public abstract void Consume(T value);
    }

    public class Ammo : Supplies<float>
    {
        public override SuppliesType Type => SuppliesType.Ammo;

        public Ammo(float value)
        {
            Value = value;
        }

        public override void Add(ISupplies<float> supplies)
        {
            Value += supplies.Value;
        }

        public override void Consume(float value)
        {
            Value -= value;
        }
    }

    public class Provision : Supplies<float>
    {
        public override SuppliesType Type => SuppliesType.Provision;

        public Provision(float value)
        {
            Value = value;
        }

        public override void Add(ISupplies<float> supplies)
        {
            Value += supplies.Value;
        }

        public override void Consume(float value)
        {
            Value -= value;
        }
    }

    public enum SuppliesType
    {
        Ammo,
        Provision
    }
}