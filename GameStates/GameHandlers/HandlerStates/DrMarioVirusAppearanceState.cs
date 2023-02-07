using BASeTris.Blocks;
using BASeTris.Particles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates.GameHandlers.HandlerStates
{

    public class DrMarioVirusAppearanceState : BlockAppearanceState //mostly useful for code semantics...
    {
        public DrMarioVirusAppearanceState(GameplayGameState startupState) : base(startupState)
        {
        }
    }

    public class BlockAppearanceState : GameState,ICompositeState<GameplayGameState>
    {
        private GameplayGameState StandardState = null;
        Dictionary<NominoBlock, Point> BLockLocationCache = new Dictionary<NominoBlock, Point>();
        Queue<Blocks.NominoBlock> AppearanceBlocks = null;
        uint LastAppearanceTick = 0;
        uint AppearanceTimeDifference = 200; //aiming for 50ms here

       
        public BlockAppearanceState(GameplayGameState startupState)
        {
            SortedList<Guid, Blocks.NominoBlock> appearanceshuffler = new SortedList<Guid, Blocks.NominoBlock>();
            StandardState = startupState;
            //set all Primary Blocks to invisible and force a redraw.

            var field = StandardState.PlayField.Contents;
            for(int x=0;x<StandardState.PlayField.ColCount;x++)
            { 
                for(int y=0;y<StandardState.PlayField.RowCount;y++)
                {
                    var block = field[y][x];
                    if (block is Blocks.LineSeriesPrimaryBlock lsmb)
                    {
                        lsmb.Visible = false;
                        appearanceshuffler.Add(Guid.NewGuid(), lsmb);
                    }
                }

            }
            AppearanceBlocks = new Queue<Blocks.NominoBlock>(appearanceshuffler.Values);
            BLockLocationCache = StandardState.PlayField.FindBlockLocations(appearanceshuffler.Values);
            StandardState.PlayField.HasChanged = true; //since we made them all invisible I'd say that counts as a change!
            LastAppearanceTick = TetrisGame.GetTickCount();

        }
        public override void DrawForegroundEffect(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
           // throw new NotImplementedException();
        }
        public const int virusappearanceparticlecount = 100;
        public const double startradius = 0.25f;
        public override void GameProc(IStateOwner pOwner)
        {
            //if the timeout has elapsed, make another one visible.
            var currtick = TetrisGame.GetTickCount();
            var tickdiff = currtick - LastAppearanceTick;
            var currhandler = (pOwner.GetHandler() as DrMarioHandler);
            var usediff = currhandler==null?50:Math.Min(50, AppearanceTimeDifference - currhandler.Level);
            if(tickdiff > AppearanceTimeDifference)
            {
                LastAppearanceTick = currtick;
                if(AppearanceBlocks.Count == 0)
                {
                    if (StandardState.NoTetrominoSpawn) StandardState.NoTetrominoSpawn = false;
                    //StandardState.PlayField.ClearActiveBlockGroups();
                    pOwner.CurrentState = StandardState;
                    
                    return;
                }
                var nextAppear = AppearanceBlocks.Dequeue();
                nextAppear.Visible = true;
                StandardState.Sounds.PlaySound("block_appear", false);
                //doublefun: add particles!
                var handleritem = pOwner.GetHandler();
                var AngleDelta = (2 * Math.PI / virusappearanceparticlecount);
                if (handleritem is DrMarioHandler dmh && nextAppear is Blocks.LineSeriesBlock lsb)
                {

                    if (BLockLocationCache.ContainsKey(nextAppear))
                    {
                        Point AppearLocation = BLockLocationCache[nextAppear];
                        
                        
                        var usecolors = dmh.GetCombiningColor(lsb.CombiningIndex);
                        for (int i = 0; i < virusappearanceparticlecount; i++)
                        {
                            double Angle = (2 * Math.PI / virusappearanceparticlecount) * i;



                            PointF FirstPoint = new PointF((float)(Math.Cos(Angle) * startradius),
                                (float)(Math.Sin(Angle) * startradius));

                            float VelocityFactor = (float)TetrisGame.rgen.NextDouble() * 0.3f;

                            PointF PointSpeed = new PointF((float)(Math.Cos(Angle) * VelocityFactor),
                                (float)(Math.Sin(Angle) * VelocityFactor));

                            PointF ParticleLocation = new PointF(FirstPoint.X + AppearLocation.X + 0.5f, FirstPoint.Y + AppearLocation.Y + 0.5f-2) ;

                            BaseParticle bp = new BaseParticle(ParticleLocation,PointSpeed, TetrisGame.Choose(usecolors)) { TTL = 1000 };
                            
                            StandardState.TopParticles.Add(bp);
    


                        } 
                    }



                }
                StandardState.PlayField.HasChanged = true;
                
                

            }
            
        }

        public GameplayGameState GetComposite()
        {
            return StandardState;
        }

        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            //do nothing- we don't allow control here.
        }
    }
}
