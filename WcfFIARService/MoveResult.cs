﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WcfFIARService
{
    /// <summary>
    /// simple enum for game to handle move result for each player
    /// </summary>
    [DataContract]
    public enum MoveResult
    {
        [EnumMember]
        YouWon,
        [EnumMember]
        YouLost,
        [EnumMember]
        Draw,
        [EnumMember]
        NotYourTurn,
        [EnumMember]
        GameOn,
        [EnumMember]
        PlayerLeft,
        [EnumMember]
        IlligelMove
    }
}
