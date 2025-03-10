namespace Luxa.Interfaces
{
    public interface IAsyncRepository<T>
    {
        Task<IEnumerable<T>> GetAll();
        Task<T> GetOne(int id);
        Task<bool> Create(T model);
        Task<bool> Update(T model);
        Task<bool> Delete(T model);
    }
}

