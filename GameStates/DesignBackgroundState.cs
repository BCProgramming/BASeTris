using BASeCamp.Elementizer;
using BASeTris.AssetManager;
using BASeTris.BackgroundDrawers;
using BASeTris.GameStates.GameHandlers;
using BASeTris.GameStates.Menu;
using BASeTris.Rendering;
using BASeTris.Settings;
using BASeTris.Tetrominoes;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
// TODO Although ClickOnce is supported on .NET 5+, apps do not have access to the System.Deployment.Application namespace. For more details see https://github.com/dotnet/deployment-tools/issues/27 and https://github.com/dotnet/deployment-tools/issues/53.
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static SkiaSharp.SKPath;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace BASeTris.GameStates
{
    public class BackgroundDesignLayer: IXmlPersistable
    {
        public struct LayerRandomizationParameters
        {
            public int MinSize = 6;
            public int MaxSize = 24;
            

            public LayerRandomizationParameters()
            {
            }


        }
        private int _DesignColumns = 6;
        private int _DesignRows = 6;
        public double Scale { get; set; } = 1.0d;
        public SKColor GridColor { get; set; } = SKColors.Yellow;
        public int SelectedIndex { get; set; }
        public List<Nomino> DesignNominoes { get; set; } = new List<Nomino>();
        public NominoTheme DisplayedDesignerTheme = new SNESTetrominoTheme();
        public int DesignColumns { get { return _DesignColumns; } set { _DesignColumns = value; RecreateFakeHandler(); } }
        public int DesignRows { get { return _DesignRows; } set { _DesignRows = value; RecreateFakeHandler(); } }
        public TetrominoCollageRenderer Collage { get; set; } = null;
        private void RecreateFakeHandler()
        {

            Collage = new TetrominoCollageRenderer(DesignRows, DesignColumns, 500, 500, 0, DisplayedDesignerTheme, SKColors.Transparent);
        }

        public Thread RandomizerThread = null;

        public void RandomizeLayer(int MinimumSize, int MaximumSize,Action CompletionAction = null)
        {


            if (RandomizerThread != null)
            {
                RandomizerThread = null;
                return;
            }
            RandomizerThread = new Thread(() =>
            {
                //NOTE to self: issue found where it seems to not place anything in the top row. Investigate
                IRandomizer rgen = RandomHelpers.Construct();
                int chosensize = TetrisGame.StatelessRandomizer.Next((MaximumSize - MinimumSize)) + MinimumSize;
                chosensize = 8;
                _DesignColumns = 11;  _DesignRows = 20;
                DesignNominoes = new List<Nomino>();
                RecreateFakeHandler();

                bool[][] PositionData = new bool[_DesignRows][];
                for (int r = 0; r < _DesignRows; r++)
                {
                    PositionData[r] = new bool[_DesignColumns];
                }
                MinoTileGenerator mtg = new MinoTileGenerator();
                try
                {
                    var BuildMinos = mtg.Generate(PositionData, null, DesignBackgroundState.TetrominoTypes.Select<Type, Func<Nomino>>((t) => new Func<Nomino>(() => (Nomino)Activator.CreateInstance(t, new object[] { null }))).ToArray(), null, null, (newlist) =>
                    {

                        try
                        {

                            foreach (var iterate in newlist)
                            {
                                DisplayedDesignerTheme.ApplyTheme(iterate, Collage.DummyHandler, Collage.Field, NominoTheme.ThemeApplicationReason.Normal);
                            }
                            DesignNominoes = newlist;
                            Thread.Sleep(1);

                        }
                        catch (Exception exr)
                        {
                            ;
                        }
                        return RandomizerThread == null;
                    }); ;
                    if (BuildMinos != null)
                    {
                        DesignNominoes = BuildMinos.ToList();
                        foreach (var iterate in DesignNominoes)
                        {
                            DisplayedDesignerTheme.ApplyTheme(iterate, Collage.DummyHandler, Collage.Field, NominoTheme.ThemeApplicationReason.Normal);
                        }
                    }
                }
                catch (Exception exr)
                {
                }
                if (CompletionAction != null) CompletionAction();
                RandomizerThread = null;
                //DesignNominoes = BuildMinos.ToList();
            });
            RandomizerThread.Start();
            return;
            /*
            HashSet<Point> UsedPositions = new HashSet<Point>();


            bool AddingBlocks = true;
            int NoneAddedSequence = 0;
            while (AddingBlocks)
            {
                //choose a random Tetromino.

                Type choosetype = TetrisGame.Choose(DesignBackgroundState.TetrominoTypes);
                Nomino ConstructNext = (Nomino)Activator.CreateInstance(choosetype, new Object[] { null });
                ConstructNext.SetRotation(TetrisGame.StatelessRandomizer.Next(4));
                ConstructNext.X = TetrisGame.StatelessRandomizer.Next(_DesignColumns);
                ConstructNext.Y = TetrisGame.StatelessRandomizer.Next(_DesignRows);
                if (ConstructNext.Any((ne) => UsedPositions.Contains(new Point(ne.X+ConstructNext.X, ne.Y+ConstructNext.Y))))
                {
                    NoneAddedSequence++;
                    if (NoneAddedSequence > 64) AddingBlocks = false;
                    continue;
                }


                foreach (var elem in ConstructNext)
                {
                    UsedPositions.Add(new Point(elem.X+ConstructNext.X, elem.Y + ConstructNext.Y));    
                }
                DesignNominoes.Add(ConstructNext);




            }*/

            foreach (var iterate in DesignNominoes)
            {
                DisplayedDesignerTheme.ApplyTheme(iterate, Collage.DummyHandler, Collage.Field, NominoTheme.ThemeApplicationReason.Normal);
            }









        }

        public BackgroundDesignLayer(XElement SourceNode, Object pPersistenceData)
        {
            //we can use TetrominoCollageRenderer.LoadTetrominoCollageFromXML() to help with this.
            _DesignRows = SourceNode.GetAttributeInt("Rows",6);
            _DesignColumns = SourceNode.GetAttributeInt("Columns", 6);
            Scale = SourceNode.GetAttributeDouble("Scale", 1d);
            String sThemeName = SourceNode.GetAttributeString("Theme", "");
            if (!String.IsNullOrEmpty(sThemeName))
            {
                DisplayedDesignerTheme = NominoTheme.GetNewThemeInstanceByName(sThemeName) ?? DisplayedDesignerTheme;

            }
            DesignNominoes = TetrominoCollageRenderer.LoadTetrominoCollageFromXML(SourceNode).ToList();
            
            RecreateFakeHandler();
            foreach (var iterate in DesignNominoes)
            {
                DisplayedDesignerTheme.ApplyTheme(iterate, Collage.DummyHandler, Collage.Field, NominoTheme.ThemeApplicationReason.Normal);
            }

            /*
             <TetrominoCollage Rows=""6"" Columns=""6"">
<Tetromino Type=""I"" Rotation=""1"" X=""-1"" Y=""-3"" />
<Tetromino Type=""I"" Rotation=""1"" X=""-1"" Y=""3"" />
<Tetromino Type=""T"" Rotation=""3"" X=""2"" Y=""1"" />
<Tetromino Type=""T"" Rotation=""1"" X=""3"" Y=""-1"" />
<Tetromino Type=""T"" Rotation=""1"" X=""3"" Y=""5"" />
<Tetromino Type=""L"" Rotation=""1"" X=""-1"" Y=""0"" />
<Tetromino Type=""L"" Rotation=""1"" X=""1"" Y=""3"" />
<Tetromino Type=""S"" Rotation=""0"" X=""1"" Y=""0"" />
<Tetromino Type=""J"" Rotation=""3"" X=""-1"" Y=""3"" />
<Tetromino Type=""J"" Rotation=""3"" X=""5"" Y=""3"" />
<Tetromino Type=""Z"" Rotation=""1"" X=""3"" Y=""1"" />
<Tetromino Type=""L"" Rotation=""0"" X=""3"" Y=""3"" />
</TetrominoCollage>
             */


        }
        public SKBitmap GetLayerBitmap(int BlockSize = 100)
        {
            TetrominoCollageRenderer tcr = new TetrominoCollageRenderer(DesignColumns, DesignRows, BlockSize, BlockSize, 0, DisplayedDesignerTheme, SKColors.Transparent);

            foreach (var iterate in DesignNominoes)
                tcr.AddNomino(iterate);

            return tcr.Render();

        }

        public XElement GetXmlData(string pNodeName, object PersistenceData)
        {
            XElement CreateNode = new XElement(pNodeName);
            CreateNode.Add(new XAttribute("Rows", _DesignRows), new XAttribute("Columns", _DesignColumns),new XAttribute("Theme",this.DisplayedDesignerTheme.GetType().Name));
            
            foreach (var checkMino in DesignNominoes)
            {
                String sType = checkMino.GetType().Name.Replace("Tetromino_","");
                String sRotation = checkMino.GetBlockData().First().RotationModulo.ToString();
                String sX = checkMino.X.ToString();
                String sY = checkMino.Y.ToString();
                XElement TetrominoElement = new XElement("Tetromino", new XAttribute("Scale",Scale),  new XAttribute("Type", sType), new XAttribute("Rotation", sRotation), new XAttribute("X", sX), new XAttribute("Y", sY));
                CreateNode.Add(TetrominoElement);
            }
            

            return CreateNode;

        }

        public BackgroundDesignLayer(int pColumns, int pRows, NominoTheme pTheme)
        {
            DisplayedDesignerTheme = pTheme;
            _DesignColumns = pColumns;
            _DesignRows = pRows;
            RecreateFakeHandler();
        }
        public bool IsDirty { get; set; }

        //An interesting "knapsack problem" idea: a way to effectively fill the layer with tetrominoes such that they do not overlap. Furthermore an interesting caveat would be tiling: eg we can fill squares with partial tetrominoes but the remainder of that tetromino has to 'fit' on the other side.

        //This might frankly not be worth trying to implement if we're honest! just make a bunch of different manual arrangements and choose them randomly or something to that effect.

    }
    public class DesignBackgroundState : GameState,IXmlPersistable
    {
        //Gamestate for designing a tetromino collage. Mostly intended to be used to build up the collages that are used in the game, though may serve other purposes.

        public GameState RevertState { get; set; }

        public BackgroundDesignLayer[] Layers = new BackgroundDesignLayer[] {new BackgroundDesignLayer(6,6,new SNESTetrominoTheme()) };
        public int LayerIndex = 0;

        public BackgroundDesignLayer CurrentLayer { get { return Layers[LayerIndex]; } }
        public bool IsDirty { get { return Layers.Any((l) => l.IsDirty); } set { CurrentLayer.IsDirty = true; } }

        

        public List<Nomino> DesignNominoes { get { return CurrentLayer.DesignNominoes; } set { CurrentLayer.DesignNominoes = value; } }
        

        public int DesignColumns { get { return CurrentLayer.DesignColumns; } set { CurrentLayer.DesignColumns = value; } }
        public int DesignRows { get { return CurrentLayer.DesignRows; } set { CurrentLayer.DesignRows = value; } }
        public override DisplayMode SupportedDisplayMode => DisplayMode.Full;
        public int SelectedIndex { get { return CurrentLayer.SelectedIndex; } set { CurrentLayer.SelectedIndex = value; } }
        bool flInitialized = false;

        //the drawing code is responsible for filling this information in when it paints.

        public SKRect? BoxBound { get; set; }
        public ConcurrentDictionary<(int, int), Nomino> PositionalMapping = new ConcurrentDictionary<(int, int), Nomino>();



        public XElement GetXmlData(string pNodeName, object PersistenceData)
        {
            XElement CreateNode = new XElement(pNodeName);

            CreateNode.Add(from x in Layers select x.GetXmlData("Layer", PersistenceData));

            return CreateNode;

        }
        public DesignBackgroundState(XElement SourceNode, Object pPersistenceData)
        {
            Layers = (from l in SourceNode.Elements("Layer") select new BackgroundDesignLayer(l, pPersistenceData)).ToArray();
        }
        private bool IsSelectionValid()
        {
            return DesignNominoes != null && SelectedIndex >= 0 && SelectedIndex < DesignNominoes.Count;
        }
       
        private Nomino SelectedNomino { get {
                if (!IsSelectionValid()) return null;
                else { return DesignNominoes[SelectedIndex]; }
            } }
        private MenuState DesignOptionsMenuState = null;
        private TetrominoCollageRenderer Collage { get { return CurrentLayer.Collage; } set { CurrentLayer.Collage = value; } }
        //private NominoTheme DisplayedDesignerTheme = new SNESTetrominoTheme();

      

        public DesignBackgroundState(IStateOwner pOwner,GameState pRevert,IBackground usebg = null)
        {
            DesignOptionsMenuState  = new MenuState(_BG);
            var FontSrc = TetrisGame.GetRetroFont(14, pOwner.ScaleFactor);




            
            //ControlsOption.FontFace = FontSrc.FontFamily.Name;
            //ControlsOption.FontSize = FontSrc.Size;




            MenuStateTextMenuItem ReturnMenuItem = new MenuStateTextMenuItem() { Text = "Return", TipText = "Return to Designer" };
            MenuStateTextMenuItem AddNominoItem = new MenuStateTextMenuItem() { Text = "Add Nomino", TipText = "Add a new Nomino" };
            MenuStateTextMenuItem SaveDesign = new MenuStateTextMenuItem() { Text = "Save Design", TipText = "Save Current Design" };
            MenuStateTextMenuItem TestDesign = new MenuStateTextMenuItem() { Text = "Test Design", TipText = "Test Design" };
            MenuStateTextMenuItem LoadDesign = new MenuStateTextMenuItem() { Text = "Load Design", TipText = "Load Design" };
            MenuStateTextMenuItem RandomizeItem = new MenuStateTextMenuItem() { Text = CurrentLayer.RandomizerThread==null?"Randomize":"Cancel Randomize", TipText = "Randomize this Layer's arrangement" };
            MenuStateTextMenuItem NextLayer = new MenuStateTextMenuItem() { Text = "Next Layer", TipText = "Edit the next layer. Creates a new layer if needed." };
            MenuStateTextMenuItem PrevLayer = new MenuStateTextMenuItem() { Text = "Previous Layer", TipText = "Edit the previous layer" };
            MenuStateTextMenuItem LayerOptions = new MenuStateTextMenuItem() { Text = "Layer Options", TipText = "Edit Layer Properties" };
            MenuStateSliderOption MusicVolume = new MenuStateSliderOption(0,3,1) { Label = "Music Volume", TipText = "Change the volume of the incredible design music." ,ChangeSize = 0.1};

            MusicVolume.ValueChanged += (oa, ob) =>
            {
                if (DesignMusic != null)
                    DesignMusic.SetVolume((float)ob.Value);
            };

            MenuStateDisplayThemeMenuItem ChangeThemeItem = new MenuStateDisplayThemeMenuItem(pOwner, typeof(StandardTetrisHandler),CurrentLayer.DisplayedDesignerTheme.GetType());
            ChangeThemeItem.SimpleSelectionFunction = (nt) =>
            {
                CurrentLayer.DisplayedDesignerTheme = nt.GenerateThemeFunc();
                
                foreach (var iterate in CurrentLayer.DesignNominoes)
                {
                    CurrentLayer.DisplayedDesignerTheme.ApplyTheme(iterate, Collage.DummyHandler, Collage.Field, NominoTheme.ThemeApplicationReason.Normal);
                }
                
            };
            ConfirmedTextMenuItem ExitMenuItem = new ConfirmedTextMenuItem() {Text="Exit",TipText= "Exit Designer" };
            ExitMenuItem.OnOptionConfirmed += (o, e) =>
            {
                if (RevertState != null)
                {
                    TetrisGame.Soundman.StopMusic();
                    pOwner.CurrentState = RevertState;
                    DesignOptionsMenuState.ActivatedItem = null;
                }
            };

            

            
            foreach (var designeritem in new MenuStateMenuItem[] { ReturnMenuItem, AddNominoItem,TestDesign, ChangeThemeItem,SaveDesign,LoadDesign,NextLayer,PrevLayer, LayerOptions,RandomizeItem, ExitMenuItem })
            {
                if (designeritem is MenuStateTextMenuItem mstmi)
                {
                    mstmi.FontFace = FontSrc.FontFamily.Name;
                    mstmi.FontSize = FontSrc.Size;
                    DesignOptionsMenuState.MenuElements.Add(mstmi);
                }
                else
                {
                    DesignOptionsMenuState.MenuElements.Add(designeritem);
                }
            }
            DesignOptionsMenuState.FadedBGFadeState = new MenuState.MenuStateFadedParentStateInformation(this,false);
            DesignOptionsMenuState.MenuItemActivated += (o, e) =>
            {
                if (e.MenuElement == ReturnMenuItem)
                {
                    pOwner.CurrentState = this;
                    DesignOptionsMenuState.ActivatedItem = null;

                }
                else if (e.MenuElement == AddNominoItem)
                {


                    Nomino ConstructNext = new Tetromino_I();
                    ApplyTheme(ConstructNext);
                    if (ConstructNext != null)
                    {
                        DesignNominoes.Add(ConstructNext);
                        SelectedIndex = DesignNominoes.Count - 1;
                    }

                    pOwner.CurrentState = this;
                    DesignOptionsMenuState.ActivatedItem = null;

                }
                else if (e.MenuElement == TestDesign)
                {
                    //go through all of our layers in REVERSE order.

                    StandardImageBackgroundSkia CurrBuild = null;

                    foreach (var layer in this.Layers.Reverse())
                    {
                        //create the Render background for this layer.
                        var layerbitmap = layer.GetLayerBitmap();
                        var layercapsule = new StandardImageBackgroundDrawSkiaCapsule() { _BackgroundImage = SKImage.FromBitmap(layerbitmap), Movement = new SKPoint(5, 5), Scale = layer.Scale };
                        var BuildLayerbg = new StandardImageBackgroundSkia() { Data = layercapsule };
                        //if currbuild is null, this is the first layer.
                        if (CurrBuild == null) CurrBuild = BuildLayerbg;
                        else
                        {
                            //otherwise, this is a 'higher' layer, so set the current currbuild as the underlayer, then set currbuild to the current layer.
                            BuildLayerbg.Underlayer = CurrBuild;
                            
                            CurrBuild = BuildLayerbg;
                        }

                    }
                    this.BG = CurrBuild;

                    //var layerbitmap = CurrentLayer.GetLayerBitmap();
                    //var newcapsule = new StandardImageBackgroundDrawSkiaCapsule() { _BackgroundImage = SKImage.FromBitmap(layerbitmap), Movement = new SKPoint(5, 5) };
                    //newcapsule.theFilter = SKColorMatrices.GetFader(0.5f);
                    //newcapsule.UnderLayer = (StandardImageBackgroundSkia)this.BG;
                    //skiacap.ResetState(SKRect.Empty);
                    //this.BG = new StandardImageBackgroundSkia() { Data = newcapsule };

                    //this.BG = new StandardImageBackgroundSkia() { Data = new StandardImageBackgroundDrawSkiaCapsule() { BackgroundImage = CollageImage };
                    /*if (this.BG is StandardImageBackgroundSkia sibs)
                    {
                        if (sibs.Data is StandardImageBackgroundDrawSkiaCapsule skiacap)
                        {
                            
                            skiacap._BackgroundImage = SKImage.FromBitmap(CurrentLayer.GetLayerBitmap());
                            
                        }
                    }*/
                    DesignOptionsMenuState.ActivatedItem = null;
                }
                else if (e.MenuElement == SaveDesign)
                {
                    //present a menu of the 10 "save slots"
                    //we could use names but... the text input state is sorta bad right now.
                    //we'll maybe revisit this later.
                    var submenu = GetSaveSlotSelectionState(pOwner, DesignOptionsMenuState, (y) => { CustomBackgroundData.SaveCustomBackground(this, y); });
                    pOwner.CurrentState = submenu;

                    DesignOptionsMenuState.ActivatedItem = null;
                }
                else if (e.MenuElement == LoadDesign)
                {
                    var submenu = GetSaveSlotSelectionState(pOwner, DesignOptionsMenuState, (y) =>
                    {
                        var LoadTry = CustomBackgroundData.LoadCustomBackground(y);
                        if (LoadTry != null)
                        {
                            LayerIndex = 0;
                            this.Layers = LoadTry.Layers;
                            IsDirty = true;
                        }

                    });
                    pOwner.CurrentState = MenuState.CreateOutroState(pOwner,submenu);
                    DesignOptionsMenuState.ActivatedItem = null;

                }
                else if (e.MenuElement == LayerOptions)
                {
                    var submenu = GetSizeEditState(pOwner, DesignOptionsMenuState);
                    pOwner.CurrentState = MenuState.CreateOutroState(pOwner,submenu);
                    DesignOptionsMenuState.ActivatedItem = null;

                }
                else if (e.MenuElement == RandomizeItem)
                {
                    RandomizeItem.Text = "Cancel Randomization";
                    RandomizeItem.TipText = "Cancel Randomization";
                    CurrentLayer.RandomizeLayer(4, 16);
                    IsDirty = true;
                    DesignOptionsMenuState.ActivatedItem = null;
                }

                else if (e.MenuElement == NextLayer)
                {
                    if (LayerIndex + 1 >= Layers.Length)
                    {
                        //need to add a layer.
                        var NewLayers = new BackgroundDesignLayer[Layers.Length + 1];
                        Array.Copy(Layers, NewLayers, Layers.Length);
                        NewLayers[NewLayers.Length - 1] = new BackgroundDesignLayer(6, 6, new SNESTetrominoTheme());
                        Layers = NewLayers;

                    }
                    LayerIndex++;
                    DesignOptionsMenuState.ActivatedItem = null;
                }
                else if (e.MenuElement == PrevLayer)
                {
                    if (LayerIndex == 0) LayerIndex = Layers.Length - 1; else LayerIndex--;
                    DesignOptionsMenuState.ActivatedItem = null;
                }

            };


            RevertState = pRevert;
            _BG = usebg;
            DesignOptionsMenuState.BG = _BG;
            DesignOptionsMenuState.StateHeader = "BG Design Menu";
            DesignOptionsMenuState.HeaderTypeface = FontSrc.FontFamily.Name;
            DesignNominoes.Add(new Tetromino_T());
            DesignNominoes.Add(new Tetromino_Z());
            DesignNominoes.Add(new Tetromino_S());
            foreach (var iterate in DesignNominoes)
            {
                CurrentLayer.
                DisplayedDesignerTheme.ApplyTheme(iterate,Collage.DummyHandler, Collage.Field, NominoTheme.ThemeApplicationReason.Normal);
            }
            IsDirty = true;
        }
        private MenuState PrepareSubstate(IStateOwner pOwner, GameState Parent,String pTitle, params MenuStateMenuItem[] Items)
        {
            var ResultState = new MenuState(_BG);

            var FontSrc = TetrisGame.GetRetroFont(14, pOwner.ScaleFactor);

            MenuStateTextMenuItem ReturnMenuItem = new MenuStateTextMenuItem() { Text = "Cancel", TipText = "Cancel and return to previous menu" };

            ResultState.MenuElements.Add(ReturnMenuItem);
            ResultState.MenuElements.AddRange(Items);
            ResultState.BG = _BG;
            ResultState.StateHeader = pTitle;
            ResultState.HeaderTypeface = FontSrc.FontFamily.Name;
            ResultState.HeaderTypeSize *= 1.5f;

            ResultState.MenuItemActivated += (o, e) =>
            {
                if (e.MenuElement == ReturnMenuItem)
                {
                    pOwner.CurrentState = MenuState.CreateOutroState(pOwner, Parent);
                    ResultState.ActivatedItem = null;
                }
            };
            foreach (var styleitem in ResultState.MenuElements)
            {
                if (styleitem is MenuStateTextMenuItem mstmi)
                {
                    mstmi.FontFace = FontSrc.FontFamily.Name;
                    mstmi.FontSize = FontSrc.Size;
                    mstmi.BackColor = new SKColor(128, 128, 128, 128);
                    //DesignOptionsMenuState.MenuElements.Add(mstmi);
                }
            }
            ResultState.FadedBGFadeState = new MenuState.MenuStateFadedParentStateInformation(Parent, true);
            return ResultState;
        }

        private MenuState GetSizeEditState(IStateOwner pOwner, GameState Parent)
        {
            MenuStateSliderOption ColumnsOption = new MenuStateSliderOption(1, 64, CurrentLayer.DesignColumns) { SmallDetent = 1, LargeDetentCount = 4,Label = "Columns" };
            MenuStateSliderOption RowsOption = new MenuStateSliderOption(1, 64, CurrentLayer.DesignRows) { SmallDetent = 1, LargeDetentCount = 4,Label = "Rows" };
            MenuStateSliderOption ScaleOption = new MenuStateSliderOption(0, 10, CurrentLayer.Scale) { ChangeSize = 0.25f, Label = "Scale", LargeDetentCount = 5, SmallDetent = 0.1 };
            ScaleOption.ValueChanged += (ob, ea) =>
            {
                CurrentLayer.Scale = ea.Value;
            };
            ColumnsOption.ValueChanged += (csender, cargs) => CurrentLayer.DesignColumns = (int)cargs.Value;
            RowsOption.ValueChanged += (rsender, rargs) => CurrentLayer.DesignRows = (int)rargs.Value;


            return PrepareSubstate(pOwner, Parent, "Layer Options",ColumnsOption, RowsOption,ScaleOption);
        }

        private void ColumnsOption_ValueChanged(object sender, MenuStateSliderOption.SliderValueChangeEventArgs e)
        {
            CurrentLayer.DesignColumns = (int)e.Value;
        }

        private MenuState GetSaveSlotSelectionState(IStateOwner pOwner,GameState Parent,Action<int> LoadSlotAction)
        {
            var ResultState = new MenuState(_BG);
            
            var FontSrc = TetrisGame.GetRetroFont(14, pOwner.ScaleFactor);
            MenuStateTextMenuItem BitmapFileItem = new MenuStateTextMenuItem() { Text = "Bitmap File", TipText = "Export design to a bitmap file." };
            MenuStateTextMenuItem ReturnMenuItem = new MenuStateTextMenuItem() { Text = "Cancel", TipText = "Cancel Save and return to previous menu" };
            ResultState.MenuElements.Add(ReturnMenuItem);
            for (int i = 1; i < 11; i++)
            {
                DateTime? Touched = CustomBackgroundData.GetCustomBackgroundTouched(i);
                String sDescription = "Empty Save Slot.";
                String sText = "Slot " + i;
                if (Touched != null)
                {
                    DesignBackgroundState Loaded = CustomBackgroundData.LoadCustomBackground(i);
                    sDescription = String.Join(",", from l in Loaded.Layers select "(" + l.DesignNominoes.Count.ToString() + ")");
                    sText = Touched.Value.ToShortDateString() + " - " + Touched.Value.ToShortTimeString();
                }
                MenuStateTextMenuItem SlotItem  = new MenuStateTextMenuItem() { Text = sText, TipText = sDescription,Tag = i };
                ResultState.MenuElements.Add(SlotItem);
            }
            ResultState.MenuElements.Add(BitmapFileItem);
            ResultState.MenuItemActivated += (a, b) =>
            {
                if (b.MenuElement == ReturnMenuItem)
                {
                    pOwner.CurrentState = MenuState.CreateOutroState(pOwner, DesignOptionsMenuState);
                }
                else if (b.MenuElement == BitmapFileItem)
                {
                    using (var getbitmap = CurrentLayer.GetLayerBitmap())
                    {
                        String sTargetFile = Path.Combine(TetrisGame.AppDataFolder, "LayerExport", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ffffff") + ".png");
                        String sDirectory = Path.GetDirectoryName(sTargetFile);
                        if (!Directory.Exists(sDirectory)) Directory.CreateDirectory(sDirectory);
                        using (var data = getbitmap.Encode(SKEncodedImageFormat.Png, 80))
                        {
                            using (var writer = new FileStream(sTargetFile, FileMode.Create))
                            {
                                data.SaveTo(writer);
                            }
                        }
                        ResultState.ActivatedItem = null;
                    }

                    pOwner.CurrentState = MenuState.CreateOutroState(pOwner, DesignOptionsMenuState);
                }
                else
                {
                    LoadSlotAction((int)(b.MenuElement.Tag));
                    pOwner.CurrentState = MenuState.CreateOutroState(pOwner, DesignOptionsMenuState); //fix: save slots act weird because the menu wasn't closing and the items were getting activated.
                    ResultState.ActivatedItem = null;

                }

                DesignOptionsMenuState.ActivatedItem = null;
            };

            ResultState.BG = _BG;
            ResultState.StateHeader = "Choose Slot";
            ResultState.HeaderTypeface = FontSrc.FontFamily.Name;
            ResultState.HeaderTypeSize *= 1.5f;
            foreach (var styleitem in ResultState.MenuElements)
            {
                if (styleitem is MenuStateTextMenuItem mstmi)
                {
                    mstmi.FontFace = FontSrc.FontFamily.Name;
                    mstmi.FontSize = FontSrc.Size;
                    mstmi.BackColor = new SKColor(128, 128, 128, 128);
                    //DesignOptionsMenuState.MenuElements.Add(mstmi);
                }
            }
            ResultState.FadedBGFadeState = new MenuState.MenuStateFadedParentStateInformation(Parent, true);
            return ResultState;
            
        }
        private void ApplyTheme(Nomino EditNomino)
        {
            CurrentLayer.
            DisplayedDesignerTheme.ApplyTheme(EditNomino, Collage.DummyHandler, Collage.Field, NominoTheme.ThemeApplicationReason.Normal);
        }
        private bool _defaultBG = true;
        IActiveSound DesignMusic = null;
        public override void GameProc(IStateOwner pOwner)
        {
            if (!flInitialized)
            {
                DesignMusic = TetrisGame.Soundman.PlayMusic("Design");
                flInitialized = true;
            }
            if (_BG == null)
            {
                if (pOwner is BASeTris bt)
                {
                    _BG = StandardImageBackgroundGDI.GetStandardBackgroundDrawer();
                    
                }
                else if (pOwner is BASeTrisTK)
                {
                    _BG = StandardImageBackgroundSkia.GetMenuBackgroundDrawer();
                    
                }
            }
            
        }
        public static Type[] TetrominoTypes = new Type[] { typeof(Tetrominoes.Tetromino_I), typeof(Tetrominoes.Tetromino_L), typeof(Tetrominoes.Tetromino_J), typeof(Tetrominoes.Tetromino_S), typeof(Tetrominoes.Tetromino_Z), typeof(Tetrominoes.Tetromino_O), typeof(Tetrominoes.Tetromino_T) };

        
        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            //The designer will have a separate menu option.
            
            //up,down (drop), left and right will move the current nomino.
            //RotateCW will rotate clockwise.
            //Rotate CCW will rotate counter clockwise
            //Pause will open a designer menu "substate". here is where we would have options such as saving, loading, or exiting back to the title.
            //Hold will change the current nomino to something else.
            //hmm, still need way to add a nomino, delete the current nomino (if it is not the only one) and switch between present nominoes, though...
            switch (g)
            {
                case GameKeys.GameKey_Null:
                    break;
                case GameKeys.GameKey_RotateCW:
                    if (IsSelectionValid())
                    {
                        TetrisGame.Soundman.PlaySound("block_rotate", new AudioHandlerPlayDetails() { Pitch = 5 });
                        SelectedNomino.Rotate(false);
                        IsDirty = true;
                    }
                    break;
                case GameKeys.GameKey_RotateCCW:
                    if (IsSelectionValid())
                    {
                        TetrisGame.Soundman.PlaySound("block_rotate",new AudioHandlerPlayDetails() { Pitch = 750 });
                        SelectedNomino.Rotate(true);
                        IsDirty = true;
                    }
                    break;
                case GameKeys.GameKey_Drop:
                    if (IsSelectionValid())
                    {
                        SelectedNomino.Y--;
                        IsDirty = true;
                    }
                    //move up
                    break;
                case GameKeys.GameKey_Left:
                    if (IsSelectionValid())
                    {
                        SelectedNomino.X--;
                        IsDirty = true;
                    }
                    //move left
                    break;
                case GameKeys.GameKey_Right:

                    if (IsSelectionValid())
                    {
                        SelectedNomino.X++;
                        IsDirty = true;
                    }
                    //move right
                    break;
                case GameKeys.GameKey_Down:
                    if (IsSelectionValid())
                    {
                        SelectedNomino.Y++;
                        IsDirty = true;
                    }
                    //move down
                    break;
                case GameKeys.GameKey_Pause:
                    //launch into the designer substate.
                    //Add Tetromino adds a new tetromino to the end of the list and makes it the selected entry.
                    //Save allows the bg set to be saved.
                    //Load should allow loading an existing set.
                    //Quit will go back to the reversion state we were provided in the constructor.
                    pOwner.CurrentState = DesignOptionsMenuState;
                    break;

                case GameKeys.GameKey_MenuActivate:
                    break;
                case GameKeys.GameKey_Debug1:
                    break;
                case GameKeys.GameKey_Debug2:
                    break;
                case GameKeys.GameKey_Debug3:
                    break;
                case GameKeys.GameKey_Debug4:
                    break;
                case GameKeys.GameKey_Debug5:
                    break;
                case GameKeys.GameKey_Debug6:
                    break;
                case GameKeys.GameKey_PopHold:
                    break;
                case GameKeys.GameKey_DesignerNextNomino:
                    if(DesignNominoes!=null && DesignNominoes.Count > 0) SelectedIndex = (SelectedIndex + 1) % DesignNominoes.Count;
                    break;
                case GameKeys.GameKey_DesignerPrevNomino:
                    if (DesignNominoes != null && DesignNominoes.Count > 0)
                    {
                        if (SelectedIndex == 0) SelectedIndex = DesignNominoes.Count; else SelectedIndex = SelectedIndex - 1;
                    }
                    break;
                case GameKeys.GameKey_Hold:
                case GameKeys.GameKey_DesignerChangeNomino:
                    //swap out the current nomino with a new one of the successor Tetromino type.
                    var Current = SelectedNomino;

                    var FindNext = Current==null?typeof(Tetromino_I): TetrisGame.Successor(TetrominoTypes, Current.GetType());
                    Nomino ConstructNext = (Nomino)Activator.CreateInstance(FindNext,new Object[] { null });
                    ConstructNext.X = Current==null?0:Current.X;
                    ConstructNext.Y = Current == null ? 0 : Current.Y;
                    ApplyTheme(ConstructNext);
                    if (ConstructNext != null)
                    {
                        if (DesignNominoes.Count == 0) DesignNominoes.Add(ConstructNext);
                        else
                           DesignNominoes[SelectedIndex] = ConstructNext;
                    }

                    break;
                case GameKeys.Gamekey_DesignerDeleteNomino:
                    //remove the current entry from the list.
                    if (DesignNominoes != null && DesignNominoes.Count > 1)
                    {
                        DesignNominoes.RemoveAt(SelectedIndex);
                        SelectedIndex = SelectedIndex % DesignNominoes.Count;
                    }
                    break;
            }


        }
    }
}
