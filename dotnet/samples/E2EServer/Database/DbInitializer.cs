namespace E2EServer.Database
{
    public static class DbInitializer
    {
        public static void Initialize(E2EDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }
    }
}
