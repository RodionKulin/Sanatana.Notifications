namespace Sanatana.Notifications.DAL.EntityFrameworkCore.Context
{
    public interface ISenderDbContextFactory
    {
        SenderDbContext GetDbContext();
        void InitializeDatabase();
    }
}