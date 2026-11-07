using chess_website.ChessServer;

namespace chess_website.Services
{
    // Thin wrapper over the generated Connected Service proxy (Connected Services/
    // ChessServer). Every call opens a fresh proxy to the chess server, which already
    // has its address baked in from the service metadata, and closes it when done.
    public static class ChessClient
    {
        public static async Task<bool> RegisterAsync(string username, string password)
        {
            ChessServiceClient client = new ChessServiceClient();
            try
            {
                return await client.RegisterAsync(username, password);
            }
            finally
            {
                await CloseAsync(client);
            }
        }

        public static async Task<bool> LoginAsync(string username, string password)
        {
            ChessServiceClient client = new ChessServiceClient();
            try
            {
                return await client.LoginAsync(username, password);
            }
            finally
            {
                await CloseAsync(client);
            }
        }

        public static async Task<string[]> GetGamesListAsync()
        {
            ChessServiceClient client = new ChessServiceClient();
            try
            {
                return await client.GetGamesListAsync();
            }
            finally
            {
                await CloseAsync(client);
            }
        }

        private static async Task CloseAsync(ChessServiceClient client)
        {
            try
            {
                await client.CloseAsync();
            }
            catch
            {
                client.Abort();
            }
        }
    }
}
