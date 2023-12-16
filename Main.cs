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
namespace EyeAuras.Web.Repl.Component;

public partial class Main : WebUIComponent {
    
    private IInputSimulatorEx InputWindowsEx { get; init; }
    
    public Main([Dependency("WindowsInputSimulator")] IInputSimulatorEx inputWindowsEx)
    {
        InputWindowsEx = inputWindowsEx;
    }
    
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
    
   
    
    //FER COW

    private IImageSearchTrigger FermaA => AuraTree.FindAuraByPath(@".\Search\Ferma\A").Triggers.Items
        .OfType<IImageSearchTrigger>().First();
    private IImageSearchTrigger FermaD => AuraTree.FindAuraByPath(@".\Search\Ferma\D").Triggers.Items
        .OfType<IImageSearchTrigger>().First();
    private IImageSearchTrigger FermaCow => AuraTree.FindAuraByPath(@".\Search\Ferma\Cow").Triggers.Items
        .OfType<IImageSearchTrigger>().First();
    private IImageSearchTrigger FermaRage => AuraTree.FindAuraByPath(@".\Search\Ferma\Rage").Triggers.Items
        .OfType<IImageSearchTrigger>().First();
    private IImageSearchTrigger FermaE => AuraTree.FindAuraByPath(@".\Search\Ferma\E").Triggers.Items
        .OfType<IImageSearchTrigger>().First();
    
    
    //FOOD

    private IImageSearchTrigger StatusFood => AuraTree.FindAuraByPath(@".\Search\Status").Triggers.Items
        .OfType<IImageSearchTrigger>().ElementAt(0);
    private IImageSearchTrigger StatusMood => AuraTree.FindAuraByPath(@".\Search\Status").Triggers.Items
        .OfType<IImageSearchTrigger>().ElementAt(1);
    


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
        this.WhenAnyValue(x => x.Fish)
            .Where(fishValue => fishValue) 
            .Subscribe(_ => Task.Run(() => StartFishing()))
            .AddTo(Anchors);
        this.WhenAnyValue(x => x.Ferma)
            .Where(fermaValue => fermaValue)
            .Subscribe(_ => Task.Run(() => StartFerma()))
            .AddTo(Anchors);
        /*Captcha.WhenAnyValue(x => x.IsActive)
            .Where(x => x.HasValue && x.Value)
            .Subscribe(_ => Screen.Refresh())
            .AddTo(Anchors);*/
        CaptchaML.ImageSink.Subscribe(x => Task.Run(() => UploadImageAsync(x))).AddTo(Anchors);

    }

    private async Task StartFerma()
    {
        
        AuraTree.Aura["FermA"] = CalculateTargetRectangle(0.4615f, 0.8345f, 10, 10);
        AuraTree.Aura["FermD"] = CalculateTargetRectangle(0.5395f, 0.8345f, 10, 10);
        AuraTree.Aura["FermCow"] = CalculateTargetRectangle(0.6353f, 0.8345f, 50, 50);
        AuraTree.Aura["FermE"] = CalculateTargetRectangle(0.4698f, 0.8426f, 50, 50);

        var cowCondition = AuraTree.FindAuraByPath(@".\Search\Ferma\Cow").EnablingConditions.Items.OfType<IDefaultTrigger>().First();
        var eCondition = AuraTree.FindAuraByPath(@".\Search\Ferma\E").EnablingConditions.Items.OfType<IDefaultTrigger>().First();
        cowCondition.TriggerValue = true;
        eCondition.TriggerValue = true;
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
            eCondition.TriggerValue = false;
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

    
    private async Task Screenshot(Image<Bgr, byte> img)
    {
        Log.Info("In screen");
        try
        {

            string fileName = $"captcha_{DateTime.Now:yyyyMMdd_HHmmss}.png";


            string folderPath = @"E:\CapScreen";


            string filePath = Path.Combine(folderPath, fileName);


            img.Save(filePath);
        }
        catch
        {
            Log.Info("Problem with save script");
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
        while (Fish)
        {
            if(UseFood || UseMood) await CheckStatus();
            await SendBackgroundKey("6");
            await Task.Delay(2000);
            if(UseCaptcha) await CheckCaptcha();
            await MouseSearch();
            await MouseClicks();
            await Task.Delay(1000);
        }
    }


    private async Task CheckCaptcha()
    {
        var bot = await Captcha.FetchNextResult();
        if (bot.Success == true)
        {
            var ml = await CaptchaML.FetchNextResult();
            if (ml.Success == true)
            {
                var input = CalculateTargetRectangle(0.4948f, 0.5065f, 1, 1);
                await SendBackgroundKey("MouseLeft", new Point(input.X, input.Y));
                var sortedPredictions = ml.Predictions.OrderBy(p => p.Rectangle.Left).ToList();

                foreach (var prediction in sortedPredictions)
                {
                    await SendBackgroundKey($"{prediction.Label.Name}");
                    
                }
                await Task.Delay(1000);
                var ok = CalculateTargetRectangle(0.4521f, 0.5583f, 1, 1);
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
        if (result[0].Success == true)
        {
            await SendBackgroundKey("7");
            await Task.Delay(2000);
        }

        if (result[1].Success == true)
        {
            await SendBackgroundKey("8");
            await Task.Delay(2000);
        }
    }

    private async Task MouseClicks()
    {
        if(ToggleLogs) Log.Info("Mouse click");
        int count = 0;
        while (Fish)
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

        while (Fish)
        {
            if(ToggleLogs) Log.Info($"MouseLoop, Fish : {Fish}"); 
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

    private async Task CheckBuilding()
    {
        while (Building)
        {
            await ImgBuilding.FetchNextResult();
            await Task.Delay(100);
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
                    await SendKey("E");
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
}

