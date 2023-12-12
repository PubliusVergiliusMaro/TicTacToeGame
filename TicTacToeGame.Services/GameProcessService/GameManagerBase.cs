using Microsoft.AspNetCore.Components.Authorization;
using TicTacToeGame.Domain.Models;

namespace TicTacToeGame.Services.GameProcessService;
public abstract class GameManagerBase
{
    protected Player CurrentPlayerHost;
    protected Player CurrentPlayerGuest;
    protected readonly AuthenticationStateProvider _authenticationStateProvider;

    protected GameManagerBase(AuthenticationStateProvider authenticationStateProvider)
    {
        _authenticationStateProvider = authenticationStateProvider;
    }
}
