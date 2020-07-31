using System.Threading.Tasks;

namespace Todo.NetStandard.Common
{
    public class TodoManager
    {
        #region Singleton
        private static TodoManager instance;

        /// <summary>
        /// Obtain a reference to the TodoRepository
        /// </summary>
        public static ITodoRepository Repository
        {
            get
            {
                if (instance == null)
                {
                    instance = new TodoManager();
                }
                return instance.repository;
            }
        }

        /// <summary>
        /// Synchronize the data in all the managed repositories
        /// </summary>
        /// <returns></returns>
        public static Task SynchronizeAsync()
        {
            if (instance == null)
            {
                instance = new TodoManager();
            }
            return instance.SynchronizeRepositoriesAsync();
        }
        #endregion

        private ITodoRepository repository;

        private TodoManager()
        {
            repository = new InMemoryTodoRepository();
        }

        private async Task SynchronizeRepositoriesAsync()
        {
            // Do nothing
        }
    }
}
