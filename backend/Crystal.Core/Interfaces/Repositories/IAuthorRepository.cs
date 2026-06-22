using Crystal.Core.Entities;

namespace Crystal.Core.Interfaces.Repositories;

public interface IAuthorRepository
{
    Task<IEnumerable<Author>> GetAllAsync();
    Task<Author?> GetByIdAsync(int p_id);
    Task AddAsync(Author p_author);
    void Update(Author p_author);
    void Delete(Author p_author);
    Task SaveChangesAsync();
}
