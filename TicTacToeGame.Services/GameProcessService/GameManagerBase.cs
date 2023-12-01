using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacToeGame.Domain.Models;

namespace TicTacToeGame.Services.GameProcessService;
public abstract class GameManagerBase
{
    protected int Counter;
    protected Player CurrentPlayerHost;
    protected Player CurrentPlayerGuest;
    protected readonly AuthenticationStateProvider _authenticationStateProvider;

    protected GameManagerBase(AuthenticationStateProvider authenticationStateProvider)
    {
        _authenticationStateProvider = authenticationStateProvider;
    }
}
