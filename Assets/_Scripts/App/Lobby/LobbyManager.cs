//using Palmmedia.ReportGenerator.Core.Parser.Filtering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour {

    [SerializeField] private PlayerDataScriptable[] playerDatas= new PlayerDataScriptable[2];
    
    public static LobbyManager Instance { get; private set; }


    public const string PLAYER_NAME_KEY = "PlayerName";
    public const string KEY_PLAYER_TYPE = "PlayerType";
    public const string KEY_SESSION_MODE = "SessionMode";
    public const string KEY_START_SESSION = "Start";


    private const string PLAYER_ID_KEY = "PlayerID";

    #region Lobby Events Setup
    public event EventHandler OnLeftLobby;

    public event EventHandler<LobbyEventArgs> OnJoinedLobby;
    public event EventHandler<LobbyEventArgs> OnJoinedLobbyUpdate;
    public event EventHandler<LobbyEventArgs> OnKickedFromLobby;
    public event EventHandler<LobbyEventArgs> OnLobbySessionModeChanged;
    public event EventHandler<EventArgs> OnSessionStarted;

    public class LobbyEventArgs : EventArgs {
        public Lobby lobby;
    }

    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
    public class OnLobbyListChangedEventArgs : EventArgs {
        public List<Lobby> lobbyList;
    }

    #endregion

    #region Session Mode Setup
    public enum SessionMode {
        Design,
        Customize,
        Visualize
    }
       
    public SessionMode GetSessionMode()
    {
        return sessionMode; // Getter
    }

    public void SetSessionMode(string value)
    {
        //Debug.Log("String value: " + value.ToString());

        if (value == SessionMode.Customize.ToString())
        {
            sessionMode = SessionMode.Customize;
        }
        else if(value == SessionMode.Design.ToString())
        {
            sessionMode = SessionMode.Design;
        }
        else
        {
            sessionMode = SessionMode.Visualize;
        }
    }

    #endregion
    public enum PlayerType {
        Expert,
        NonExpert
    }


    private System.Random random = new System.Random();
    private float heartbeatTimer;
    private float lobbyPollTimer;
    private float refreshLobbyListTimer = 5f;
    private Lobby joinedLobby;
    private int number = 5;//random.Next(1, 10000);
    private string playerName = "Player";
    private PlayerType playerType = PlayerType.NonExpert;
    public SessionMode sessionMode = SessionMode.Design;
    private int _playerID;
    private string _lobbyName;
    [SerializeField] private int currPlayer;
    public bool nameChosen=false;
    public bool colorChosen=false;

    public void SetNameChosen(bool value) { nameChosen = value; }
    public void SetPlayerType(PlayerType value) {
        playerType=value;
    }
    public PlayerType GetPlayerType()
    {
       return playerType ;
    }
    public string GetPlayerName()
    {
        return playerName;
    }
    public string GetLobbbyName()
    {
        return _lobbyName;
    }
    private async void Awake()
    {
        Instance = this;

        // Authenticate using the retrieved or newly generated Player Name
        //await Authenticate(playerName);
    }

    #region Load/Store PlayerName

    public PlayerDataScriptable GetPlayerData(int playerID)
    {
        return playerDatas[playerID];   
    }

    public void SetPlayerColor(Color colorIndex)
    {
        playerDatas[_playerID].Color=colorIndex;
    }
    public Color GetPlayerColor()
    {
        return playerDatas[_playerID].Color;
    }
    public void SetPlayerName(string name)
    {
        _playerID=currPlayer;
        string correctedName= CorrectPlayerName(name);

        playerDatas[_playerID].playerName = correctedName;

        Authenticate(correctedName);

        string playername = GetPlayerInfo();
    }
    public static string CorrectPlayerName(string playerName)
    {
        // Define a default name if no valid word is found
        const string defaultName = "Player";

        // Ensure the input is not null and trim unnecessary spaces
        if (string.IsNullOrWhiteSpace(playerName))
        {
            return defaultName;
        }

        // Use a regex to extract the first valid word (letters and numbers only)
        Match match = Regex.Match(playerName, @"[a-zA-Z0-9_\-]+");

        // Debugging output for better insight
        UnityEngine.Debug.Log($"Input Name: {playerName}");
        UnityEngine.Debug.Log($"Regex Match Found: {match.Success}");
        UnityEngine.Debug.Log($"Matched Value: {match.Value}");

        // If a valid word is found, sanitize and return it
        if (match.Success)
        {
            string validWord = match.Value;

            // Ensure the word meets length requirements (3 to 20 characters)
            if (validWord.Length < 3)
            {
                // Pad with zeros if too short
                validWord = validWord.PadRight(3, '0');
            }
            else if (validWord.Length > 20)
            {
                // Trim to the maximum allowed length
                validWord = validWord.Substring(0, 20);
            }

            return validWord;
        }

        // If no valid word is found, return the default name
        return defaultName;
    }


public string GetPlayerInfo()
    {
        /*
        // Try to load the Player Name from PlayerPrefs
        string storedPlayerName = PlayerPrefs.GetString(PLAYER_NAME_KEY, null);

        Debug.Log("Stored player name: " + storedPlayerName);

        if (string.IsNullOrEmpty(storedPlayerName))
        {
            // Generate a new Player Name if none is stored
            (playerName,playerID) = GenerateValidPlayerName();

            // Save the newly generated Player Name and number to PlayerPrefs
            PlayerPrefs.SetString(PLAYER_NAME_KEY, playerName);
            PlayerPrefs.SetString(PLAYER_ID_KEY, playerID);
            PlayerPrefs.Save();

            _playerID =playerID;

            // Log the new Player Name
            Debug.Log("New Player Name generated and saved: " + playerName);

        }
        else
        {
            // Use the stored Player Name and number
            playerName = storedPlayerName;
            Debug.Log("Using stored Player Name: " + playerName);
        }
    */
        #endregion

        playerName= playerDatas[currPlayer].playerName;

        _playerID = playerDatas[currPlayer].playerID;

        return playerName;
    }


    private (string,int) GenerateValidPlayerName()
    {
        string baseName = "Player";
        int randomSuffix = UnityEngine.Random.Range(1, 10000); // Generate random integer
        string playerName = baseName + randomSuffix.ToString();

        playerName = System.Text.RegularExpressions.Regex.Replace(playerName, "[^a-zA-Z0-9-_]", "");

        if (playerName.Length > 30)
        {
            playerName = playerName.Substring(0, 30);
        }

        Debug.Log("Generated Player Name: " + playerName);
        return (playerName,randomSuffix);
    }

    private void Update() {
       // HandleRefreshLobbyList(); // Disabled Auto Refresh for testing with multiple builds
        HandleLobbyHeartbeat();
        HandleLobbyPolling();
    }

    public async Task Authenticate(string playerName)
    {
        Debug.Log("Setting profile name: " + playerName);

        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(playerName);

        await UnityServices.InitializeAsync(initializationOptions);

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in! Player ID: " + AuthenticationService.Instance.PlayerId);
            RefreshLobbyList(sessionMode);
        };

        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (AuthenticationException e)
        {
            Debug.LogError("Authentication failed: " + e.Message);
        }
    }

    #region Lobby Refresh Functions


    //function that refreshes lobby list every 5s
    private void HandleRefreshLobbyList() {
        
        if (UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsSignedIn) {
            refreshLobbyListTimer -= Time.deltaTime;
            if (refreshLobbyListTimer < 0f) {
                float refreshLobbyListTimerMax = 5f;
                refreshLobbyListTimer = refreshLobbyListTimerMax;
                
                RefreshLobbyList(sessionMode);
            }
            
        }
    }
    //function that keeps lobby alive by sending a heartbeat every 15s
    private async void HandleLobbyHeartbeat() {
        if (IsLobbyHost()) {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f) {
                float heartbeatTimerMax = 15f;
                heartbeatTimer = heartbeatTimerMax;

                Debug.Log("Heartbeat");
                await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }
    private async void HandleLobbyPolling() {
        if (joinedLobby != null) {
            lobbyPollTimer -= Time.deltaTime;
            if (lobbyPollTimer < 0f) {
                float lobbyPollTimerMax = 1.5f;
                lobbyPollTimer = lobbyPollTimerMax;

                joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);

                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });

                if (!IsPlayerInLobby()) {
                    // Player was kicked out of this lobby
                    Debug.Log("Kicked from Lobby!");

                    OnKickedFromLobby?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });

                    joinedLobby = null;
                }

                if (joinedLobby.Data[KEY_START_SESSION].Value != "0")
                {
                    //Start Session
                    if (!IsLobbyHost())//Lobby Host already joined Relay
                    {
                        TestRelay.Instance.JoinRelay(joinedLobby.Data[KEY_START_SESSION].Value);

                        AppManager.Instance.setPhase(false);
                    }
                    else
                    {AppManager.Instance.setPhase(true);                   }

                    _lobbyName = joinedLobby.Name;

                    joinedLobby= null;

                    OnSessionStarted?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }

    #endregion

    #region Helper functions
    //returns joined lobby
    public Lobby GetJoinedLobby()
    {
        return joinedLobby;
    }

    //returns true if local player is host
    public bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    //function that returns true if a player is in a lobby
    private bool IsPlayerInLobby()
    {
        if (joinedLobby != null && joinedLobby.Players != null)
        {
            foreach (Player player in joinedLobby.Players)
            {
                if (player.Id == AuthenticationService.Instance.PlayerId)
                {
                    // This player is in this lobby
                    return true;
                }
            }
        }
        return false;
    }
    //function that returns the Player
    private Player GetPlayer() {
        return new Player(AuthenticationService.Instance.PlayerId, null, new Dictionary<string, PlayerDataObject> {
            { PLAYER_NAME_KEY, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) },
            { KEY_PLAYER_TYPE, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerType.ToString()) }
        });
    }

    public int GetPlayerID() { return _playerID; }

    #endregion


    public void ChangeSessionMode() {
        if (IsLobbyHost()) {
            SessionMode sessionMode =
                Enum.Parse<SessionMode>(joinedLobby.Data[KEY_SESSION_MODE].Value);

            switch (sessionMode) {
                default:
                case SessionMode.Design:
                    sessionMode = SessionMode.Customize;
                    break;
                case SessionMode.Customize:
                    sessionMode = SessionMode.Design;
                    break;
                case SessionMode.Visualize:
                    sessionMode = SessionMode.Visualize;
                    break;
            }

            UpdateLobbySessionMode(sessionMode);
        }
    }

    private bool lobbyCreated=false;

    public bool LobbyCreated { set { lobbyCreated = value; } get { return lobbyCreated;} }
    public async void CreateLobby(string lobbyName, int maxPlayers, bool isPrivate, SessionMode sessionMode) {
        lobbyCreated=false ;
        Player player = GetPlayer();
        //Debug.Log("Lobby created with sessionm0de:" + sessionMode);
        CreateLobbyOptions options = new CreateLobbyOptions {
            Player = player,
            IsPrivate = isPrivate,
            Data = new Dictionary<string, DataObject> {
                { KEY_SESSION_MODE, new DataObject(DataObject.VisibilityOptions.Public, sessionMode.ToString(), DataObject.IndexOptions.S1) },
                { KEY_START_SESSION, new DataObject(DataObject.VisibilityOptions.Member, "0" ) }

            }
        };

        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

        joinedLobby = lobby;

        OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });

        Debug.Log("Created Lobby " + lobby.Name);

        lobbyCreated = true;
    }

    public async void RefreshLobbyList(SessionMode? targetSessionMode = null) {

        //Debug.Log("Session mode on Refresh Lobby List"+targetSessionMode);
        try {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;

            // Filter for open lobbies only
            List<QueryFilter> filters = new List<QueryFilter>
            {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0"),
            };

            if (targetSessionMode != null)
            {
                //Debug.Log("target Session mode is: "+targetSessionMode);
                filters.Add(new QueryFilter(
                    field: QueryFilter.FieldOptions.S1,
                    op: QueryFilter.OpOptions.EQ,
                    value:targetSessionMode.Value.ToString()
                ));
            }            
        options.Filters = filters;

            // Order by newest lobbies first
            options.Order = new List<QueryOrder> {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created)
            };

            QueryResponse lobbyListQueryResponse = await Lobbies.Instance.QueryLobbiesAsync(options);


            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs { lobbyList = lobbyListQueryResponse.Results });

        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }

    }

    private bool lobbyJoined = false;

    public bool LobbyJoined { set { lobbyJoined = value; } get { return lobbyJoined; } }
    public async void JoinLobby(Lobby lobby) {
        lobbyJoined = false;
        Player player = GetPlayer();

        joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, new JoinLobbyByIdOptions {
            Player = player
        });

        OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });

        lobbyJoined = true;
    }

    public async void UpdatePlayerName(string playerName) {
        this.playerName = playerName;

        if (joinedLobby != null) {
            try {
                UpdatePlayerOptions options = new UpdatePlayerOptions();

                options.Data = new Dictionary<string, PlayerDataObject>() {
                    {
                        PLAYER_NAME_KEY, new PlayerDataObject(
                            visibility: PlayerDataObject.VisibilityOptions.Public,
                            value: playerName)
                    }
                };

                string playerId = AuthenticationService.Instance.PlayerId;

                Lobby lobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, playerId, options);
                joinedLobby = lobby;

                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }
    }

    public async void UpdatePlayerCharacter(PlayerType playerType) {
        if (joinedLobby != null) {
            try {
                UpdatePlayerOptions options = new UpdatePlayerOptions();

                options.Data = new Dictionary<string, PlayerDataObject>() {
                    {
                        KEY_PLAYER_TYPE, new PlayerDataObject(
                            visibility: PlayerDataObject.VisibilityOptions.Public,
                            value: playerType.ToString())
                    }
                };

                string playerId = AuthenticationService.Instance.PlayerId;

                Lobby lobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, playerId, options);
                joinedLobby = lobby;

                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }
    }
    public async void LeaveLobby() {
        if (joinedLobby != null) {
            try {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);

                joinedLobby = null;

                OnLeftLobby?.Invoke(this, EventArgs.Empty);
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }
    }

    public async void KickPlayer(string playerId) {
        if (IsLobbyHost()) {
            try {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }
    }

    public async void UpdateLobbySessionMode(SessionMode sessionMode) {
        try {
            //Debug.Log("UpdateLobbySessionMode " + sessionMode);
            
            Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions {
                Data = new Dictionary<string, DataObject> {
                    { KEY_SESSION_MODE, new DataObject(DataObject.VisibilityOptions.Public, sessionMode.ToString()) }
                }
            });

            joinedLobby = lobby;

            OnLobbySessionModeChanged?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }

    }

    public async void StartSession()
    {
        if (IsLobbyHost())
        {
            try
            {
                Debug.Log("Starting Session");

                string relayCode = await TestRelay.Instance.CreateRelay();

                Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { KEY_START_SESSION, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }

                    }
                });
                _lobbyName = lobby.Name;
                joinedLobby = lobby;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }


    }

    public async Task CloseLobby()
    {
        // Ensure only the lobby host can close the lobby
        if (IsLobbyHost())
        {
            try
            {
                Debug.Log("Closing the Lobby...");

                // Delete the lobby using the lobby ID
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);

                Debug.Log("Lobby closed successfully.");

                // Optionally, notify other systems that the lobby has been closed
                OnLeftLobby?.Invoke(this, EventArgs.Empty);

                // Clear the joinedLobby object after closing
                joinedLobby = null;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Error closing the lobby: {e.Message}");
            }
        }
        else
        {
            Debug.LogError("Only the lobby host can close the lobby.");
        }
    }


}
