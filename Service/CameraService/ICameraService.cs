public interface ICameraService
{
    Task<List<Camera>> GetAllAsync();

    Task<Camera?> GetByIdAsync(int id);

    Task AddAsync(Camera camera);

    Task UpdateAsync(Camera camera);

    Task DeleteAsync(int id);
}