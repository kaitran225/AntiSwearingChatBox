using System.Text.Json;

namespace AntiSwearingChatBox.AI.Moderation
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
                
                if (!File.Exists(path))
                {
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
                Console.WriteLine($"Error loading settings: {ex.Message}");
            }

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
        public string Sensitivity { get; set; } = "High";
        public List<string> AlwaysModerateLanguages { get; set; } = new List<string> { "English" };
        public List<FilteringRule> FilteringRules { get; set; } = new List<FilteringRule>();
        public ResponseOptions ResponseOptions { get; set; } = new ResponseOptions();
        public AIInstructions AIInstructions { get; set; } = new AIInstructions();
        public WarningThresholds WarningThresholds { get; set; } = new WarningThresholds();
        
        // List of common evasion patterns to detect
        public List<string> EvasionPatterns { get; set; } = new List<string>
        {
            // F-word variants
            "fuk", "fvck", "fck", "fuq", "phuck", "phuk", "fxck", "f**k", "f-ck", "fug", "fcuk", "f.ck", "f_ck", 
            "f0ck", "fook", "foock", "phuck", "phuq", "phvck", "fk", "fuggin", "fuggin'", "effin", "effin'", "fudge",
            "freak", "freaking", "fvcking", "facking", "fokkin", "fokking", "fugging", "flippin", "frigging", "fack", 
            "fackin", "facking", "feking", "fakk", "fak", "fakk", "faq", "facq", "fak you", "fack you",
            
            // S-word variants
            "shi", "sht", "sh1t", "sh!t", "s**t", "shyt", "sh1t", "schit", "chit", "sheet", "shizz", "sh.t", "sh_t",
            "5h1t", "5hit", "5h!t", "$hit", "$h!t", "shitt", "shiet", "sheeeit", "sheeit", "shiz", "schitt",
            
            // A-word variants
            "a$$", "a**", "@ss", "azz", "as$", "@$$", "@$s", "a$$h0le", "a**hole", "@sshole", "ash0le", "a5s", 
            "a5sh0le", "arse", "arsehole", "a-hole", "a.ss", "a_ss", "ahole", "ayss", "ay$",
            
            // B-word variants
            "b!tch", "b1tch", "biatch", "bytch", "btch", "b*tch", "bicth", "bish", "b1sh", "beetch", "bich", "biach",
            "beyotch", "biotch", "b!sh", "bi+ch", "b.tch", "b_tch", "beach", "bxtch", "biach", "bizitch", "bi@tch", 
            "bi@ch", "biatc", "biatchh", "biatches", "biatchez", "biyatch", "beeatch", "beeotch", "beotch",
            
            // D-word variants
            "d1ck", "dik", "d!ck", "dikk", "dck", "d*ck", "d**k", "d!k", "d1k", "d.ck", "d_ck", "deek", "dyck",
            "dic", "dixk", "diq", "dck", "doock", "dig", "dork", "dong", "weiner", "wiener", 
            
            // C-word variants
            "c*nt", "kunt", "cnt", "c-nt", "c.nt", "cvnt", "knt", "cnut", "khunt", "kant", "kunnt", "quunt",
            "knut", "coont", "cuhnt", "kuunt", "c__t", "c**t", "cvnt", "kount", "qunt", "quunt",
            
            // P-word variants
            "pusy", "pu$$y", "pussi", "pvssy", "psy", "p*ssy", "p**sy", "p.ssy", "p_ssy", "pusC", "pus$y",
            "pu$$i", "pusss", "p*s", "puss", "pssy", "pssi", "pvssi", "poossy", "pwssy", "pu55y", "kitty", 
            "pvssy", "pvssi", "poosy", "poosi", "punani",
            
            // Other offensive terms
            "wh0re", "h0e", "sl*t", "slvt", "bit¢h", "pron", "p0rn", "sp3rm", "cum", "cuming", "cumming", 
            "n1gga", "nigg", "n1gg", "negro", "nigga", "nigger", "n1gg3r", "n!gg3r", "niqqer", "n!qq3r", 
            "f4g", "f4gg0t", "fagg0t", "f@g", "f@ggot",
            
            // Various numeric and symbol substitutions
            "5ex", "$ex", "5ex", "s3x", "s3xy", "pr0n", "p0rn", "pen!s", "p3n!s", "vag!na", "v@g!na", "v@g!n@"
        };

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