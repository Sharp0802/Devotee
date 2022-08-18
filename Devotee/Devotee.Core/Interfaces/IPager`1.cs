namespace Devotee.Core.Interfaces;

public interface IPager<T>
{
    public Task<IEnumerable<T>> GetPageAt(int page);
}