﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BASeTris.AI.StoredBoardState;

namespace BASeTris.AI
{
    //interface which accepts board information and provides a score.
    public interface IGameScoringHandler
    {
        double CalculateScore(BoardScoringRuleData data, StoredBoardState state);
    }
    

    public class GameScoringHandlerAttribute : Attribute
    {
        public Type RuleDataType { get; set; }
        public Type TypeHandler { get; set; }
        public IGameScoringHandler Handler { get; set; }
        public GameScoringHandlerAttribute(Type HandlerType,Type pRuleDataType)
        {
            TypeHandler = HandlerType;
            RuleDataType = pRuleDataType;
            Handler = Activator.CreateInstance(HandlerType, new Object[] { }) as IGameScoringHandler;
        }
    }
}
