using WKFTournamentIS.Core.Models;

namespace WKFTournamentIS.Views.Windows.Interfaces
{
    public interface IInitializableWithUser
    {
        void InitializeUser(User authenticatedUser);
    }
}