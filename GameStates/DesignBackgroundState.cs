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
using System.Collections.Generic;
using System.Deployment.Application;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static SkiaSharp.SKPath;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace BASeTris.GameStates
{
    public class BackgroundDesignLayer: IXmlPersistable
    {
        private int _DesignColumns = 6;
        private int _DesignRows = 6;
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
        
        public BackgroundDesignLayer(XElement SourceNode, Object pPersistenceData)
        {
            //we can use TetrominoCollageRenderer.LoadTetrominoCollageFromXML() to help with this.
            _DesignRows = SourceNode.GetAttributeInt("Rows",6);
            _DesignColumns = SourceNode.GetAttributeInt("Columns", 6);
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
                XElement TetrominoElement = new XElement("Tetromino", new XAttribute("Type", sType), new XAttribute("Rotation", sRotation), new XAttribute("X", sX), new XAttribute("Y", sY));
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
            MenuStateTextMenuItem NextLayer = new MenuStateTextMenuItem() { Text = "Next Layer", TipText = "Edit the next layer. Creates a new layer if needed." };
            MenuStateTextMenuItem PrevLayer = new MenuStateTextMenuItem() { Text = "Previous Layer", TipText = "Edit the previous layer" };
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

            

            
            foreach (var designeritem in new MenuStateMenuItem[] { ReturnMenuItem, AddNominoItem,TestDesign, ChangeThemeItem,SaveDesign,LoadDesign, ExitMenuItem })
            {
                if (designeritem is MenuStateTextMenuItem mstmi)
                {
                    mstmi.FontFace = FontSrc.FontFamily.Name;
                    mstmi.FontSize = FontSrc.Size;
                    DesignOptionsMenuState.MenuElements.Add(mstmi);
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
                        var layercapsule = new StandardImageBackgroundDrawSkiaCapsule() { _BackgroundImage = SKImage.FromBitmap(layerbitmap), Movement = new SKPoint(5, 5) };
                        var BuildLayerbg = new StandardImageBackgroundSkia() { Data = layercapsule };
                        //if currbuild is null, this is the first layer.
                        if (CurrBuild == null) CurrBuild = BuildLayerbg;
                        else
                        {
                            //otherwise, this is a 'higher' layer, so set the current currbuild as the underlayer, then set currbuild to the current layer.
                            BuildLayerbg.Data.UnderLayer = CurrBuild;
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
                    CustomBackgroundData.SaveCustomBackground(this, 1);
                    DesignOptionsMenuState.ActivatedItem = null;
                }
                else if (e.MenuElement == LoadDesign)
                {

                    var LoadTry = CustomBackgroundData.LoadCustomBackground(1);
                    if (LoadTry != null)
                    {
                        LayerIndex = 0;
                        this.Layers = LoadTry.Layers;
                        IsDirty = true;
                    }
                    DesignOptionsMenuState.ActivatedItem = null;
                }


                else if (e.MenuElement == NextLayer)
                {
                    if (LayerIndex + 1 >= Layers.Length)
                    {
                        //need to add a layer.
                        var NewLayers = new BackgroundDesignLayer[Layers.Length + 1];
                        Array.Copy(Layers, NewLayers, Layers.Length);
                        NewLayers[NewLayers.Length] = new BackgroundDesignLayer(6, 6, new SNESTetrominoTheme());
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
        private void ApplyTheme(Nomino EditNomino)
        {
            CurrentLayer.
            DisplayedDesignerTheme.ApplyTheme(EditNomino, Collage.DummyHandler, Collage.Field, NominoTheme.ThemeApplicationReason.Normal);
        }
        private bool _defaultBG = true;
        public override void GameProc(IStateOwner pOwner)
        {
            if (!flInitialized)
            {
                TetrisGame.Soundman.PlayMusic("Design");
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
        Type[] TetrominoTypes = new Type[] { typeof(Tetrominoes.Tetromino_I), typeof(Tetrominoes.Tetromino_L), typeof(Tetrominoes.Tetromino_J), typeof(Tetrominoes.Tetromino_S), typeof(Tetrominoes.Tetromino_Z), typeof(Tetrominoes.Tetromino_O), typeof(Tetrominoes.Tetromino_T) };

        
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
                        TetrisGame.Soundman.PlaySound("block_rotate");
                        SelectedNomino.Rotate(false);
                        IsDirty = true;
                    }
                    break;
                case GameKeys.GameKey_RotateCCW:
                    if (IsSelectionValid())
                    {
                        TetrisGame.Soundman.PlaySound("block_rotate");
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
                    var FindNext = TetrisGame.Successor(TetrominoTypes, Current.GetType());
                    Nomino ConstructNext = (Nomino)Activator.CreateInstance(FindNext,new Object[] { null });
                    ApplyTheme(ConstructNext);
                    if (ConstructNext != null)
                    {
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
