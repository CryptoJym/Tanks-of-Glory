using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages game state, levels, and other global functionality
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }
    
    [Header("Game Settings")]
    [SerializeField] private GameMode currentGameMode = GameMode.Arcade;
    [SerializeField] private int maxPlayers = 4;
    [SerializeField] private float matchTimeLimit = 300f; // 5 minutes
    [SerializeField] private int scoreToWin = 10;
    
    [Header("Player Management")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Material[] playerMaterials;
    [SerializeField] private string[] playerNames = { "Commander Alpha", "Commander Beta", "Commander Gamma", "Commander Delta" };
    
    [Header("UI References")]
    [SerializeField] private GameObject gameHUD;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameOverScreen;
    
    [Header("Audio")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameMusic;
    [SerializeField] private AudioClip victoryMusic;
    [SerializeField] private AudioClip defeatMusic;
    [SerializeField] private AudioSource musicSource;
    
    // Game state
    public bool IsGamePaused { get; private set; }
    public bool IsGameOver { get; private set; }
    public float RemainingTime { get; private set; }
    
    // Player tracking
    private List<GameObject> players = new List<GameObject>();
    private Dictionary<int, int> playerScores = new Dictionary<int, int>();
    
    // Events
    public delegate void GameStateChangeHandler(GameState newState);
    public event GameStateChangeHandler OnGameStateChanged;
    
    public delegate void PlayerSpawnedHandler(GameObject player, int playerIndex);
    public event PlayerSpawnedHandler OnPlayerSpawned;
    
    public delegate void ScoreChangedHandler(int playerIndex, int newScore);
    public event ScoreChangedHandler OnScoreChanged;
    
    public delegate void TimeChangedHandler(float remainingTime);
    public event TimeChangedHandler OnTimeChanged;
    
    // Enums
    public enum GameMode
    {
        Campaign,
        Arcade,
        Multiplayer_Deathmatch,
        Multiplayer_TeamBattle,
        Multiplayer_CaptureTheFlag,
        Multiplayer_KingOfTheHill
    }
    
    public enum GameState
    {
        MainMenu,
        Loading,
        Playing,
        Paused,
        GameOver
    }
    
    private GameState currentState;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Initialize
        RemainingTime = matchTimeLimit;
        IsGamePaused = false;
        IsGameOver = false;
        
        // Set initial state
        SetGameState(GameState.MainMenu);
        
        // Configure audio source if not set
        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.volume = 0.5f;
            }
        }
    }
    
    private void Start()
    {
        PlayMusic(menuMusic);
    }
    
    private void Update()
    {
        if (currentState == GameState.Playing)
        {
            // Update match timer
            if (RemainingTime > 0)
            {
                RemainingTime -= Time.deltaTime;
                if (OnTimeChanged != null)
                {
                    OnTimeChanged.Invoke(RemainingTime);
                }
                
                // Check for time over
                if (RemainingTime <= 0)
                {
                    EndGame(GetWinningPlayer());
                }
            }
            
            // Handle pause input
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }
    }
    
    /// <summary>
    /// Set the current game state and trigger associated events
    /// </summary>
    private void SetGameState(GameState newState)
    {
        // Skip if same state
        if (currentState == newState)
            return;
            
        // Update state
        currentState = newState;
        
        // Handle state-specific logic
        switch (newState)
        {
            case GameState.MainMenu:
                Time.timeScale = 1f;
                IsGamePaused = false;
                IsGameOver = false;
                PlayMusic(menuMusic);
                break;
                
            case GameState.Loading:
                Time.timeScale = 1f;
                IsGamePaused = false;
                IsGameOver = false;
                break;
                
            case GameState.Playing:
                Time.timeScale = 1f;
                IsGamePaused = false;
                IsGameOver = false;
                PlayMusic(gameMusic);
                break;
                
            case GameState.Paused:
                Time.timeScale = 0f;
                IsGamePaused = true;
                if (pauseMenu != null)
                {
                    pauseMenu.SetActive(true);
                }
                break;
                
            case GameState.GameOver:
                Time.timeScale = 1f; // Keep time running for victory/defeat animations
                IsGameOver = true;
                if (gameOverScreen != null)
                {
                    gameOverScreen.SetActive(true);
                }
                break;
        }
        
        // Notify listeners
        if (OnGameStateChanged != null)
        {
            OnGameStateChanged.Invoke(newState);
        }
    }
    
    /// <summary>
    /// Start a new game with the specified mode
    /// </summary>
    public void StartGame(GameMode mode)
    {
        // Save game mode
        currentGameMode = mode;
        
        // Reset game state
        players.Clear();
        playerScores.Clear();
        RemainingTime = matchTimeLimit;
        IsGameOver = false;
        
        // Set loading state
        SetGameState(GameState.Loading);
        
        // Determine scene to load based on mode
        string sceneToLoad = "";
        switch (mode)
        {
            case GameMode.Campaign:
                sceneToLoad = "Campaign_01"; // First campaign level
                break;
                
            case GameMode.Arcade:
                sceneToLoad = "Arcade_Arena_01";
                break;
                
            case GameMode.Multiplayer_Deathmatch:
                sceneToLoad = "MP_Deathmatch_01";
                break;
                
            case GameMode.Multiplayer_TeamBattle:
                sceneToLoad = "MP_TeamBattle_01";
                break;
                
            case GameMode.Multiplayer_CaptureTheFlag:
                sceneToLoad = "MP_CTF_01";
                break;
                
            case GameMode.Multiplayer_KingOfTheHill:
                sceneToLoad = "MP_KotH_01";
                break;
        }
        
        // Load scene
        LoadLevel(sceneToLoad);
    }
    
    /// <summary>
    /// Load a level by name
    /// </summary>
    public void LoadLevel(string levelName)
    {
        StartCoroutine(LoadLevelAsync(levelName));
    }
    
    /// <summary>
    /// Asynchronously load a level
    /// </summary>
    private IEnumerator LoadLevelAsync(string levelName)
    {
        // Start loading the scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelName);
        
        // Wait until the scene is fully loaded
        while (!asyncLoad.isDone)
        {
            // Update loading progress if needed
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            // Could update a loading bar here
            
            yield return null;
        }
        
        // Scene loaded, now set up the game
        SetupLevel();
    }
    
    /// <summary>
    /// Set up the level after loading
    /// </summary>
    private void SetupLevel()
    {
        // Find spawn points if not already set
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag("SpawnPoint");
            spawnPoints = new Transform[spawnPointObjects.Length];
            
            for (int i = 0; i < spawnPointObjects.Length; i++)
            {
                spawnPoints[i] = spawnPointObjects[i].transform;
            }
        }
        
        // Spawn players based on game mode
        if (currentGameMode == GameMode.Campaign || currentGameMode == GameMode.Arcade)
        {
            // Single player mode
            SpawnPlayer(0);
            
            // Spawn AI opponents
            SpawnAIOpponents();
        }
        else
        {
            // Multiplayer mode - spawn all players
            for (int i = 0; i < maxPlayers; i++)
            {
                SpawnPlayer(i);
            }
        }
        
        // Initialize UI
        if (gameHUD != null)
        {
            gameHUD.SetActive(true);
        }
        
        // Start the game
        SetGameState(GameState.Playing);
    }
    
    /// <summary>
    /// Spawn a player tank at the specified spawn point
    /// </summary>
    private void SpawnPlayer(int playerIndex)
    {
        if (playerPrefab == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Missing player prefab or spawn points!");
            return;
        }
        
        // Determine spawn point
        Transform spawnPoint = GetSpawnPoint(playerIndex);
        
        // Instantiate player
        GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        
        // Set player properties
        player.name = "Player_" + playerIndex;
        
        // Customize player based on index
        CustomizePlayer(player, playerIndex);
        
        // Initialize player score
        playerScores[playerIndex] = 0;
        
        // Add to list of players
        players.Add(player);
        
        // Notify listeners
        if (OnPlayerSpawned != null)
        {
            OnPlayerSpawned.Invoke(player, playerIndex);
        }
    }
    
    /// <summary>
    /// Get a spawn point for the specified player index
    /// </summary>
    private Transform GetSpawnPoint(int playerIndex)
    {
        // Use player index if within range
        if (playerIndex < spawnPoints.Length)
        {
            return spawnPoints[playerIndex];
        }
        
        // Otherwise use a random spawn point
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }
    
    /// <summary>
    /// Customize a player tank based on player index
    /// </summary>
    private void CustomizePlayer(GameObject player, int playerIndex)
    {
        // Set player material if available
        if (playerMaterials != null && playerIndex < playerMaterials.Length)
        {
            Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                // Skip renderers with specific tags or components if needed
                renderer.material = playerMaterials[playerIndex];
            }
        }
        
        // Set player name
        if (playerIndex < playerNames.Length)
        {
            player.GetComponent<PlayerInfo>()?.SetPlayerName(playerNames[playerIndex]);
        }
        
        // Configure input for multiplayer (e.g., different control schemes)
        // Implement based on input system being used
    }
    
    /// <summary>
    /// Spawn AI opponents for single-player modes
    /// </summary>
    private void SpawnAIOpponents()
    {
        // Implementation depends on game mode and difficulty
        // For now, just spawn some basic AI tanks
        
        // Example: Find all AI spawn points
        GameObject[] aiSpawnPoints = GameObject.FindGameObjectsWithTag("AISpawnPoint");
        
        foreach (GameObject spawnPoint in aiSpawnPoints)
        {
            // Spawn AI tank
            // Use appropriate AI prefab based on difficulty, level, etc.
        }
    }
    
    /// <summary>
    /// Toggle the game pause state
    /// </summary>
    public void TogglePause()
    {
        if (IsGamePaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }
    
    /// <summary>
    /// Pause the game
    /// </summary>
    public void PauseGame()
    {
        if (currentState == GameState.Playing)
        {
            SetGameState(GameState.Paused);
        }
    }
    
    /// <summary>
    /// Resume the game
    /// </summary>
    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            SetGameState(GameState.Playing);
            
            // Hide pause menu
            if (pauseMenu != null)
            {
                pauseMenu.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// End the current game
    /// </summary>
    public void EndGame(int winningPlayerIndex)
    {
        IsGameOver = true;
        SetGameState(GameState.GameOver);
        
        // Play appropriate music
        if (winningPlayerIndex == 0)
        {
            PlayMusic(victoryMusic);
        }
        else
        {
            PlayMusic(defeatMusic);
        }
        
        // Show results
        // Implementation would depend on UI system
    }
    
    /// <summary>
    /// Return to the main menu
    /// </summary>
    public void ReturnToMainMenu()
    {
        SetGameState(GameState.MainMenu);
        SceneManager.LoadScene("MainMenu");
    }
    
    /// <summary>
    /// Restart the current game
    /// </summary>
    public void RestartGame()
    {
        StartGame(currentGameMode);
    }
    
    /// <summary>
    /// Update a player's score
    /// </summary>
    public void UpdateScore(int playerIndex, int scoreIncrement)
    {
        // Ensure player exists in dictionary
        if (!playerScores.ContainsKey(playerIndex))
        {
            playerScores[playerIndex] = 0;
        }
        
        // Update score
        playerScores[playerIndex] += scoreIncrement;
        
        // Notify listeners
        if (OnScoreChanged != null)
        {
            OnScoreChanged.Invoke(playerIndex, playerScores[playerIndex]);
        }
        
        // Check for win condition
        if (playerScores[playerIndex] >= scoreToWin)
        {
            EndGame(playerIndex);
        }
    }
    
    /// <summary>
    /// Get the current score for a player
    /// </summary>
    public int GetScore(int playerIndex)
    {
        if (playerScores.ContainsKey(playerIndex))
        {
            return playerScores[playerIndex];
        }
        
        return 0;
    }
    
    /// <summary>
    /// Get the player with the highest score
    /// </summary>
    private int GetWinningPlayer()
    {
        int winningPlayer = -1;
        int highestScore = -1;
        
        foreach (KeyValuePair<int, int> score in playerScores)
        {
            if (score.Value > highestScore)
            {
                highestScore = score.Value;
                winningPlayer = score.Key;
            }
        }
        
        return winningPlayer;
    }
    
    /// <summary>
    /// Play background music
    /// </summary>
    private void PlayMusic(AudioClip music)
    {
        if (musicSource != null && music != null)
        {
            if (musicSource.clip != music)
            {
                musicSource.clip = music;
                musicSource.Play();
            }
        }
    }
} 