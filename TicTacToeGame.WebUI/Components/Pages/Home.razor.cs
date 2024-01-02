using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using TicTacToeGame.Domain.Models;
using TicTacToeGame.WebUI.BackgroundServices;


namespace TicTacToeGame.WebUI.Components.Pages
{
    public partial class Home
    {
        private Player CurrentPlayer;

        string userName = "";

        private bool isLoading = true;

        protected override void OnInitialized()
        {

            _ = InvokeAsync(async () =>
            {
                AuthenticationState authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
                ClaimsPrincipal? user = authState.User;

                string userId = GetCurrentUserIdentifier(user);

                CurrentPlayer = PlayerRepository.GetById(userId);
                if (CurrentPlayer != null)
                    userName = CurrentPlayer.UserName;

                bool needsToReconect = GameReconnectingService.CheckIfRecconectPlayer(userId);

                if (needsToReconect)
                {
                    if (GameReconnectingService.IsPlayerLeaveGameByButton)
                    {
                        NavigationManager.NavigateTo("/game");
                    }
                }

                isLoading = false;

                GameReconnectingService.CheckIfPlayerIsPlayingAndHasGameById(userId);

                StateHasChanged();
            });
        }

        protected string GetCurrentUserIdentifier(ClaimsPrincipal? claimsPrincipal)
        {
            return claimsPrincipal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }
    }
}
