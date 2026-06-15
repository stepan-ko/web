using Microsoft.EntityFrameworkCore;

public class CameraService : ICameraService
{
    private readonly AppDbContext _db;

    public CameraService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Camera>> GetAllAsync()
    {
        return await _db.Cameras.OrderBy(e => e.Id).ToListAsync();
    }

    public async Task<Camera?> GetByIdAsync(int id)
    {
        return await _db.Cameras
            .Include(c => c.Option)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(Camera camera)
    {
        _db.Cameras.Add(camera);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Camera camera)
    {
        _db.Cameras.Update(camera);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var camera = await _db.Cameras.FindAsync(id);

        if (camera == null)
            return;

        _db.Cameras.Remove(camera);

        await _db.SaveChangesAsync();
    }
}