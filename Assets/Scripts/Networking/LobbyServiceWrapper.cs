using System;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace MyGame.Networking
{
    public class LobbyServiceWrapper
    {
        public Lobby CurrentLobby { get; private set; }

        public async Task<Lobby> CreateLobbyAsync(string lobbyName, int maxPlayers)
        {
            try
            {
                // Вызов Unity API — класс Lobbies (обёртка с Instance)
                CurrentLobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName, maxPlayers);
                Debug.Log("Lobby created: " + CurrentLobby.Id);
                return CurrentLobby;
            }
            catch (Exception e)
            {
                Debug.LogError("CreateLobby failed: " + e);
                throw;
            }
        }

      
        public async Task UpdateLobbyDataAsync(string lobbyId, string key, string value)
        {
            var updateOptions = new UpdateLobbyOptions
            {
                Data = new System.Collections.Generic.Dictionary<string, DataObject>
                {
                    { key, new DataObject(DataObject.VisibilityOptions.Public, value) }
                }
            };
            await Lobbies.Instance.UpdateLobbyAsync(lobbyId, updateOptions);
        }
    }
}
