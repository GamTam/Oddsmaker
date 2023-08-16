using Discord;
using UnityEngine;

public class DiscordController : MonoBehaviour
{
    private long applicationID = 1097065526334861365;
    [Space]
    public static string gameName = "Title Screen";
    public static string phaseName = "";
    [Space]
    public static string largeImage = "discord_icon";

    private long time;

    private static bool instanceExists;
    public Discord.Discord discord;

    void Awake() 
    {
        // Transition the GameObject between scenes, destroy any duplicates
        if (!instanceExists)
        {
            instanceExists = true;
            DontDestroyOnLoad(gameObject);
        }
        else if (FindObjectsOfType(GetType()).Length > 1)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Log in with the Application ID
        discord = new Discord.Discord(applicationID, (System.UInt64)Discord.CreateFlags.NoRequireDiscord);

        time = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();

        UpdateStatus();
    }

    void Update()
    {
        // Destroy the GameObject if Discord isn't running
        try
        {
            discord.RunCallbacks();
        }
        catch
        {
            Destroy(gameObject);
        }
    }

    void LateUpdate() 
    {
        UpdateStatus();
    }

    void UpdateStatus()
    {
        // Update Status every frame
        try
        {
            var activityManager = discord.GetActivityManager();
            var activity = new Discord.Activity
            {
                Details = gameName,
                State = phaseName,
                Assets = 
                {
                    LargeImage = largeImage,
                    LargeText = gameName
                },
                Timestamps =
                {
                    Start = time
                }
            };

            activityManager.UpdateActivity(activity, (res) =>
            {
                if (res != Discord.Result.Ok) Debug.LogWarning("Failed connecting to Discord!");
            });
        }
        catch
        {
            // If updating the status fails, Destroy the GameObject
            Destroy(gameObject);
        }
    }
}