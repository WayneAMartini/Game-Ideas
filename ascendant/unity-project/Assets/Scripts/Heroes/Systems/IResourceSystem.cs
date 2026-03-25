namespace Ascendant.Heroes.Systems
{
    public interface IResourceSystem
    {
        string ResourceName { get; }
        float Current { get; }
        float Max { get; }
        float Percent { get; }
        void Add(float amount);
        bool TrySpend(float amount);
        void Reset();
    }
}
