using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Anti_Swearing_Chat_Box.Core.Moderation
{
    public class ModelSettings
    {
        public ModerationSettings Moderation { get; set; } = new ModerationSettings();

        private static ModelSettings? _instance;
        private static readonly object _lock = new object();

        public static ModelSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = LoadSettings().GetAwaiter().GetResult();
                        }
                    }
                }
                return _instance;
            }
        }

        public static async Task<ModelSettings> LoadSettings()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ModelSettings.json");
                
                // If file doesn't exist at runtime path, try development path
                if (!File.Exists(path))
                {
                    // Find the correct path relative to the executing assembly
                    string assemblyLocation = typeof(ModelSettings).Assembly.Location;
                    string assemblyDirectory = Path.GetDirectoryName(assemblyLocation) ?? string.Empty;
                    string projectPath = Path.GetFullPath(Path.Combine(assemblyDirectory, "..\\..\\..\\"));
                    path = Path.Combine(projectPath, "ModelSettings.json");
                }

                if (File.Exists(path))
                {
                    using FileStream stream = File.OpenRead(path);
                    var settings = await JsonSerializer.DeserializeAsync<ModelSettings>(stream);
                    return settings ?? new ModelSettings();
                }
            }
            catch (Exception ex)
            {
                // Log the exception if possible
                Console.WriteLine($"Error loading settings: {ex.Message}");
            }

            // Return default settings if loading fails
            return new ModelSettings();
        }

        public static async Task SaveSettings(ModelSettings settings, string? customPath = null)
        {
            try
            {
                string path = customPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ModelSettings.json");
                
                using FileStream stream = File.Create(path);
                var options = new JsonSerializerOptions { WriteIndented = true };
                await JsonSerializer.SerializeAsync(stream, settings, options);
                
                _instance = settings;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }
    }

    public class ModerationSettings
    {
        public string DefaultLanguage { get; set; } = "English";
        public string Sensitivity { get; set; } = "Medium";
        public List<string> AlwaysModerateLanguages { get; set; } = new List<string> { "English" };
        public List<FilteringRule> FilteringRules { get; set; } = new List<FilteringRule>();
        public ResponseOptions ResponseOptions { get; set; } = new ResponseOptions();
        public AIInstructions AIInstructions { get; set; } = new AIInstructions();
        public WarningThresholds WarningThresholds { get; set; } = new WarningThresholds();

        public string GetEffectivePromptPrefix()
        {
            return AIInstructions.PromptPrefix + string.Join("\n", AIInstructions.GetNumberedRules());
        }
    }

    public class FilteringRule
    {
        public string RuleType { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
        public double SensitivityLevel { get; set; } = 0.5;
        public List<string> AllowedExceptions { get; set; } = new List<string>();
        public List<string> AlwaysFilterTerms { get; set; } = new List<string>();
        public bool DetectHateSpeech { get; set; } = true;
        public bool DetectThreats { get; set; } = true;
        public bool DetectSexualContent { get; set; } = true;
        public bool ConsiderConversationHistory { get; set; } = true;
        public bool DetectSarcasm { get; set; } = true;
        public bool DetectHumor { get; set; } = true;
    }

    public class ResponseOptions
    {
        public bool IncludeExplanations { get; set; } = true;
        public bool StrictJsonFormat { get; set; } = true;
        public bool PreserveOriginalText { get; set; } = true;
        public bool ShowConfidenceScores { get; set; } = false;
        public bool AlwaysShowCulturalContext { get; set; } = true;
    }

    public class AIInstructions
    {
        public string PromptPrefix { get; set; } = "IMPORTANT INSTRUCTIONS: ";
        public List<string> Rules { get; set; } = new List<string>
        {
            "Do NOT modify or change the original text unless it contains profanity or inappropriate language.",
            "If the original text is in a non-English language, analyze it in its original language."
        };

        public List<string> GetNumberedRules()
        {
            var numberedRules = new List<string>();
            for (int i = 0; i < Rules.Count; i++)
            {
                numberedRules.Add($"{i + 1}. {Rules[i]}");
            }
            return numberedRules;
        }
    }

    public class WarningThresholds
    {
        public int LowWarningCount { get; set; } = 3;
        public int MediumWarningCount { get; set; } = 5;
        public int HighWarningCount { get; set; } = 10;
        public TimeSpan WarningExpiration { get; set; } = TimeSpan.FromDays(30);
    }
} 