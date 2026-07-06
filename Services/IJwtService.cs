namespace NeighborHelp.Services
{
    public interface IJwtService
    {
        string GenerateToken(string userId, string email, IList<string> roles);
    }
}
