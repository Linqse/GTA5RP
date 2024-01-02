using System.IO;
using System.Reactive.Disposables;
using System.Runtime.InteropServices;
using AntDesign.JsInterop;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using EyeAuras.OpenCVAuras.Scaffolding;
using EyeAuras.Roxy.Shared;

using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using PoeShared.Modularity;

namespace EyeAuras.Web.Repl.Component;

public partial class Main : WebUIComponent {
    
    private IInputSimulatorEx InputWindowsEx { get; init; }
    private Config _config = new Config();
    
    public Main([Dependency("WindowsInputSimulator")] IInputSimulatorEx inputWindowsEx,
        IAppArguments appArguments,
        IAuraTreeScriptingApi treeApi)
    {
        ConfigPath = Path.Combine(appArguments.AppDataDirectory, "EyeSquad", $"GTA5RP-{treeApi.Aura.Id}.cfg");
        InputWindowsEx = inputWindowsEx;
    }
    private static string ConfigPath { get; set; } 
    
    private IWinExistsTrigger Win =>
        AuraTree.FindAuraByPath(@".\WinExists").Triggers.Items.OfType<IWinExistsTrigger>().First();

    private IImageSearchTrigger ImgPort =>
        AuraTree.FindAuraByPath(@".\Search\Port").Triggers.Items.OfType<IImageSearchTrigger>().First();

    private IImageSearchTrigger ImgBuilding => AuraTree.FindAuraByPath(@".\Search\Building").Triggers.Items
        .OfType<IImageSearchTrigger>().First();
    private IDefaultTrigger ImgBuildingConditions => AuraTree.FindAuraByPath(@".\Search\Building").EnablingConditions.Items
        .OfType<IDefaultTrigger>().First();

    private IImageSearchTrigger LatterE => AuraTree.FindAuraByPath(@".\Search\BuildingLatters\Latters").Triggers.Items
        .OfType<IImageSearchTrigger>().ElementAt(0);
    private IImageSearchTrigger LatterF => AuraTree.FindAuraByPath(@".\Search\BuildingLatters\Latters").Triggers.Items
        .OfType<IImageSearchTrigger>().ElementAt(1);
    private IImageSearchTrigger LatterY => AuraTree.FindAuraByPath(@".\Search\BuildingLatters\Latters").Triggers.Items
        .OfType<IImageSearchTrigger>().ElementAt(2);
    private IImageSearchTrigger FishMouseLeft => AuraTree.FindAuraByPath(@".\Search\Fish\Main").Triggers.Items
        .OfType<IImageSearchTrigger>().ElementAt(0);

    private IImageSearchTrigger FishRed => AuraTree.FindAuraByPath(@".\Search\Fish\Main").Triggers.Items
        .OfType<IImageSearchTrigger>().ElementAt(1);
    
    private IImageSearchTrigger Captcha => AuraTree.FindAuraByPath(@".\Search\Fish\Main").Triggers.Items
        .OfType<IImageSearchTrigger>().ElementAt(2);
    private IMLSearchTrigger CaptchaML => AuraTree.FindAuraByPath(@".\Search\Fish\ML").Triggers.Items
        .OfType<IMLSearchTrigger>().First();
    private ITextSearchTrigger TextSearch => AuraTree.FindAuraByPath(@".\Search\Fish\Main").Triggers.Items
        .OfType<ITextSearchTrigger>().First();
   
    
    //FER COW

    private IImageSearchTrigger FermaA => AuraTree.FindAuraByPath(@".\Search\Ferma\Main").Triggers.Items
        .OfType<IImageSearchTrigger>().ElementAt(0);
    private IImageSearchTrigger FermaD => AuraTree.FindAuraByPath(@".\Search\Ferma\Main").Triggers.Items
        .OfType<IImageSearchTrigger>().ElementAt(1);
    private IImageSearchTrigger FermaCow => AuraTree.FindAuraByPath(@".\Search\Ferma\Condition").Triggers.Items
        .OfType<IImageSearchTrigger>().ElementAt(0);
    private IImageSearchTrigger FermaRage => AuraTree.FindAuraByPath(@".\Search\Ferma\Main").Triggers.Items
        .OfType<IImageSearchTrigger>().ElementAt(2);
    private IImageSearchTrigger FermaE => AuraTree.FindAuraByPath(@".\Search\Ferma\Condition").Triggers.Items
        .OfType<IImageSearchTrigger>().ElementAt(1);
    
     
    //FOOD

    private IImageSearchTrigger StatusFood => AuraTree.FindAuraByPath(@".\Search\Status").Triggers.Items
        .OfType<IImageSearchTrigger>().ElementAt(0);
    private IImageSearchTrigger StatusMood => AuraTree.FindAuraByPath(@".\Search\Status").Triggers.Items
        .OfType<IImageSearchTrigger>().ElementAt(1);
    
    
    //BINDS
    private IHotkeyIsActiveTrigger FishHotkey => AuraTree.FindAuraByPath(@".\WinExists").Triggers.Items
        .OfType<IHotkeyIsActiveTrigger>().First();
    


    protected override async Task HandleAfterFirstRender()
    {
        Disposable.Create(() => Log.Info("Created")).AddTo(Anchors);

        this.WhenAnyValue(x => x.Port)
            .Where(portValue => portValue) 
            .Subscribe(_ => Task.Run(() => StartPort()))
            .AddTo(Anchors);

        this.WhenAnyValue(x => x.Building)
            .Where(buildingValue => buildingValue) 
            .Subscribe(_ => Task.Run(() => StartBuilding()))
            .AddTo(Anchors);
        this.WhenAnyValue(x => x.Mines)
            .Where(minesValue => minesValue) 
            .Subscribe(_ => Task.Run(() => StartMines()))
            .AddTo(Anchors);
        this.WhenAnyValue(x => x.FishHotkey.IsActive)
            .Where(x => x.HasValue && x.Value) 
            .Subscribe(_ => Task.Run(() => StartFishing()))
            .AddTo(Anchors);
        this.WhenAnyValue(x => x.Ferma)
            .Where(fermaValue => fermaValue)
            .Subscribe(_ => Task.Run(() => StartFerma()))
            .AddTo(Anchors);
        
        LoadConfig();
        CaptchaML.ImageSink.Subscribe(x => Task.Run(() => UploadImageAsync(x))).AddTo(Anchors);

    }

    private async Task StartFerma()
    {
        
        AuraTree.Aura["FermA"] = CalculateTargetRectangle(0.4615f, 0.8345f, 10, 10);
        AuraTree.Aura["FermD"] = CalculateTargetRectangle(0.5395f, 0.8345f, 10, 10);
        AuraTree.Aura["FermCow"] = CalculateTargetRectangle(0.6353f, 0.8345f, 50, 50);
        AuraTree.Aura["FermE"] = CalculateTargetRectangle(0.4698f, 0.8426f, 50, 50);

        var cowCondition = AuraTree.FindAuraByPath(@".\Search\Ferma\Condition").EnablingConditions.Items.OfType<IDefaultTrigger>().First();
        
        cowCondition.TriggerValue = true;
        
        try
        {
            while (Ferma)
            {
                if (FermaCow.IsActive == true)
                {
                    await Task.Run(() => MilkCow());
                }

                if (FermaE.IsActive == true)
                {
                    await SendKey("E");
                }

                await Task.Delay(100);
            }
        }
        finally
        {
            cowCondition.TriggerValue = false;
            
        }
        
        
    }

    private async Task MilkCow()
    {
        if (ToggleLogs) Log.Info("Start Milking");

        while (FermaCow.IsActive == true && Ferma)
        {
            var results = await Task.WhenAll(
                Task.Run(() => FermaA.FetchNextResult()),
                Task.Run(() => FermaD.FetchNextResult()),
                Task.Run(() => FermaRage.FetchNextResult())
            );

            if (results[2].Success == true)
            {
                await Task.Delay(1000);
                continue;
            }

            if (results[0].Success == true) await SendKey("A");
            if (results[1].Success == true) await SendKey("D");

            await Task.Delay(100);
        }
    }
    
    
    private async Task StartFishing()
    {
        Log.Info("Start fish");
        
        
        AuraTree.Aura["FishMouse"] = CalculateTargetRectangle(0.5541f, 0.8489f, 70, 70);
        AuraTree.Aura["Status"] = CalculateTargetRectangle(0.2050f, 0.8975f, 200, 50);
        AuraTree.Aura["Captcha"] = CalculateTargetRectangle(0.5000f, 0.4444f, 224, 100);
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        Task.Run(() => FishLogic(token));
    }

    private async Task FishLogic(CancellationToken token)
    {
        while (FishHotkey.IsActive == true)
        {
            if(UseFood || UseMood) await CheckStatus();
            
            await SendBackgroundKey(_config.roadkey);
            await Task.Delay(2000);
            if(UseCaptcha) await CheckCaptcha();
            await Task.Delay(2000);
            var checkbag = await CheckBag();
            if (!checkbag) break;
            
            await MouseSearch();
            await MouseClicks();
            await Task.Delay(1000);
        }
    }

    private async Task<bool> CheckBag()
    {
        await SendBackgroundKey("I");
        await Task.Delay(1000);
        var checkbag = await CheckFreeSpace();
        if (!checkbag)
        {
            FishHotkey.TriggerValue = false;
            await SendBackgroundKey("I");
            if (_config.TelegramAlert && _config.TelegramUsername != null)
            {
                TelegramMessage("Был достигнут лимит веса. GTA5RP бот выключен.");
            }
            return false;
            
        }
        await SendBackgroundKey("I");
        return true;
    }
    
    

    private async Task<bool> CheckFreeSpace()
    {

        AuraTree.Aura["TextSearch"] = CalculateTargetRectangle(0.8714f, 0.2944f, 200, 100);
        var result = await TextSearch.FetchNextResult();
        if (result.Text != null)
        {
            var regex = new Regex(@"(\d\D?\d\D?\d\D?\d\D?\d)\s*kr");
            var match = regex.Match(result.Text);

            if (match.Success)
            {
                var fiveDigits = match.Groups[1].Value;
                fiveDigits = Regex.Replace(fiveDigits, @"\D", "");

                if (fiveDigits.Length == 5)
                {
                    var firstThreeDigits = int.Parse(fiveDigits.Substring(0, 3)) / 100.0f;
                    var lastTwoDigits = int.Parse(fiveDigits.Substring(3, 2)) / 100.0f;

                    var multipliedLastTwo = lastTwoDigits * 100;
                    var freeSpace = (multipliedLastTwo - firstThreeDigits);
                    Log.Info($"Free space {freeSpace:F2}kg");

                    return freeSpace > 0.5f;
                }
            }
        }
        return true;
    }






    private async Task CheckCaptcha()
    {
        var bot = await Captcha.FetchNextResult();
        if (bot.Success == true)
        {
            var ml = await CaptchaML.FetchNextResult();
            if (ml.Success == true)
            {
                if (ml.Predictions.Length < 7)
                {
                    if (_config.TelegramAlert)
                    {
                        TelegramMessage("Хоязин, я не смог разобрать капчу, я выключаюсь...");
                    }

                    FishHotkey.TriggerValue = false;
                    return;
                }

                
                var input = CalculateTargetRectangleMessageAPI(0.5000f, 0.5093f, 1, 1);
                await Task.Delay(500); // IDK CAPTCHA TRY FIX
                await SendBackgroundKey("MouseLeft", new Point(input.X, input.Y));
                var sortedPredictions = ml.Predictions.OrderBy(p => p.Rectangle.Left).ToList();
                await Task.Delay(1500); // IDK CAPTCHA TRY FIX
                string captcha = "";
                foreach (var prediction in sortedPredictions)
                {
                    await SendBackgroundKey($"{prediction.Label.Name}");
                    await Task.Delay(100);
                    captcha += prediction.Label.Name;
                }

                ///TelegramMessage($"Captcha : {captcha}");
                await Task.Delay(1000);
                var ok = CalculateTargetRectangleMessageAPI(0.4583f, 0.5602f, 1, 1);
                await SendBackgroundKey("MouseLeft", new Point(ok.X, ok.Y));
                
                
            }
        }
    }
    
    private async Task CheckStatus()
    {
        var food = StatusFood.FetchNextResult();
        var mood = StatusMood.FetchNextResult();
        
        var result = await Task.WhenAll(food, mood);
        if(ToggleLogs) Log.Info($"Food : {result[0].Success} , Mood : {result[1].Success}");
        if (result[0].Success == true && UseFood)
        {
            await SendBackgroundKey(_config.foodkey);
            await Task.Delay(2000);
        }

        if (result[1].Success == true && UseMood)
        {
            await SendBackgroundKey(_config.moodkey);
            await Task.Delay(2000);
        }
    }

    private async Task MouseClicks()
    {
        if(ToggleLogs) Log.Info("Mouse click");
        int count = 0;
        while (FishHotkey.IsActive == true)
        {
            if (count % 5 == 0 && count != 0)
            {
                var res = await FishMouseLeft.FetchNextResult();
                if (res.Success == false)
                {
                    break;
                }
            }
            var result = await FishRed.FetchNextResult();
            if (result.Success == true)
            {
                //await SendKey("MouseLeft");
                await MouseMove();
                
            }
            count++;
            await Task.Delay(100);
        }
    }
    private async Task MouseSearch()
    {
        if(ToggleLogs) Log.Info("Mouse search");
        var startTime = DateTime.Now; // Start time of the method

        while (FishHotkey.IsActive == true)
        {
            if(ToggleLogs) Log.Info($"MouseLoop, Fish : {FishHotkey.IsActive}"); 
            var result = await FishMouseLeft.FetchNextResult();
            if (result.Success == true) break;

            
            if (DateTime.Now - startTime > TimeSpan.FromMinutes(1))
            {
                await SendBackgroundKey("6");
                startTime = DateTime.Now; 
            }

            await Task.Delay(200);
        }
    }
    
    private async Task StartMines()
    {
        Log.Info("Start mines");
        
        if (!Mines) return;
        
        ImgBuildingConditions.TriggerValue = true;
        
        try
        {
            float relativeX = 0.4995f;
            float relativeY = 0.4748f;
            
            AuraTree.Aura["Building"] = CalculateTargetRectangle(relativeX, relativeY, 1, 1);
            
            
            while (Mines)
            {
                if (ImgBuilding.IsActive == true)
                {
                    await SendKey("E");
                }

                await Task.Delay(100);
            }
            
        }
        finally
        {
            ImgBuildingConditions.TriggerValue = false;
        }
    }
    

    private async Task StartBuilding()
    {
        Log.Info("Start building");
        
        if (!Building) return;
        
        ImgBuildingConditions.TriggerValue = true;
        
        try
        {
            float relativeX = 0.4058f;
            float relativeY = 0.4694f;
            float relativeXL = 0.4995f;
            float relativeYL = 0.5396f;
            AuraTree.Aura["Building"] = CalculateTargetRectangle(relativeX, relativeY, 1, 1);
            AuraTree.Aura["BuildingLatters"] = CalculateTargetRectangle(relativeXL, relativeYL, 70, 70);

            
            string key = "";
            
            while (Building)
            {
                if (ImgBuilding.IsActive == true)
                {
                    if(ToggleLogs) Log.Info("FetchLatters"); 
                    var fetchTasks = new[]
                    {
                        Task.Run(() => LatterE.FetchNextResult()),
                        Task.Run(() => LatterF.FetchNextResult()),
                        Task.Run(() => LatterY.FetchNextResult())
                    };

                    
                    await Task.WhenAll(fetchTasks);

                    
                    var e = fetchTasks[0].Result;
                    var f = fetchTasks[1].Result;
                    var y = fetchTasks[2].Result;

                    
                    if (e?.Success == true) key = "E";
                    else if (f?.Success == true) key = "F";
                    else if (y?.Success == true) key = "Y";

                    while (ImgBuilding.IsActive == true)
                    {
                        await SendKey(key);
                        await Task.Delay(100);
                    }
                    
                }

                await Task.Delay(100);
            }
            
        }
        finally
        {
            ImgBuildingConditions.TriggerValue = false;
        }
    }
    
    
    private async Task StartPort()
    {
        Log.Info("Start port");
        if (Port)
        {
            float relativeX = 0.4995f;
            float relativeY = 0.4523f;
            AuraTree.Aura["Port"] = CalculateTargetRectangle(relativeX, relativeY, 20, 20);
            while (Port)
            {
                var result = await ImgPort.FetchNextResult();
                if (result.Success == true)
                {
                    await SendBackgroundKey("E");
                    await Task.Delay(100);
                }

                await Task.Delay(100);
            }
        }
    }
    
    public async void UploadImageAsync(Image<Bgr, byte> image)
    {
        try
        {
            byte[] imageBytes;
            using (var stream = new System.IO.MemoryStream())
            {
                image.ToBitmap().Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                imageBytes = stream.ToArray();
            }

            
            string base64Image = Convert.ToBase64String(imageBytes);
            string jsonPayload = $"{{\"image\": \"data:image/bmp;base64,{base64Image}\"}}";
            
            using (var client = new HttpClient())
            {
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                await client.PostAsync("https://api.eyesquad.net/captcha/index.php", content);
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Произошла ошибка: {ex.Message}");
        }
    }
    
    
    
    private void SaveConfig() 
    {
        
        string json = JsonConvert.SerializeObject(_config);
        string directoryPath = Path.GetDirectoryName(ConfigPath);
        
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        using (StreamWriter streamWriter = new StreamWriter(ConfigPath))
        using (JsonWriter jsonWriter = new JsonTextWriter(streamWriter))
        {
            jsonWriter.Formatting = Formatting.Indented; // Устанавливаем форматирование для отступов

            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(jsonWriter, _config);
        }
        
    }
    
    
    private void LoadConfig()
    {
        if (File.Exists(ConfigPath))
        {
            string json = File.ReadAllText(ConfigPath);
            _config = JsonConvert.DeserializeObject<Config>(json);
        }
        else
        {
            _config = new Config();
        }

        
    }
}

