using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Crystal.Infrastructure.Repositories;

public class AuthorRepository : RepositoryBase, IAuthorRepository
{
    public AuthorRepository(CrystalDbContext p_context)
        : base(p_context)
    {
    }

    public async Task<IEnumerable<Author>> GetAllAsync()
    {
        return await m_context.Authors
            .AsNoTracking()
            .OrderBy(p_author => p_author.Name)
            .ToListAsync();
    }

    public async Task<Author?> GetByIdAsync(int p_id)
    {
        return await m_context.Authors
            .FirstOrDefaultAsync(p_author => p_author.Id == p_id);
    }

    public async Task AddAsync(Author p_author)
    {
        await m_context.Authors.AddAsync(p_author);
    }

    public void Update(Author p_author)
    {
        m_context.Authors.Update(p_author);
    }

    public void Delete(Author p_author)
    {
        m_context.Authors.Remove(p_author);
    }

}
