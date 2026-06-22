using Crystal.Infrastructure.Context;

namespace Crystal.Infrastructure.Repositories;

public abstract class RepositoryBase
{
    protected readonly CrystalDbContext m_context;

    protected RepositoryBase(CrystalDbContext p_context)
    {
        m_context = p_context;
    }

    public async Task SaveChangesAsync()
    {
        await m_context.SaveChangesAsync();
    }
}
