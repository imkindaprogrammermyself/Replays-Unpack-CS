using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replays_Unpack_CS
{
    class Constants
    {

        public static Dictionary<int, string> PropertyMapping = new()
        {
            { 0, "accountDBID" },
            { 1, "avatarId" },
            { 2, "camouflageInfo" },
            { 3, "clanColor" },
            { 4, "clanID" },
            { 5, "clanTag" },
            { 6, "crewParams" },
            { 7, "dogTag" },
            { 8, "fragsCount" },
            { 9, "friendlyFireEnabled" },
            { 10, "id" },
            { 11, "invitationsEnabled" },
            { 12, "isAbuser" },
            { 13, "isAlive" },
            { 14, "isBot" },
            { 15, "isClientLoaded" },
            { 16, "isConnected" },
            { 17, "isHidden" },
            { 18, "isLeaver" },
            { 19, "isPreBattleOwner" },
            { 20, "killedBuildingsCount" },
            { 21, "maxHealth" },
            { 22, "name" },
            { 23, "playerMode" },
            { 24, "preBattleIdOnStart" },
            { 25, "preBattleSign" },
            { 26, "prebattleId" },
            { 27, "realm" },
            { 28, "shipComponents" },
            { 29, "shipId" },
            { 30, "shipParamsId" },
            { 31, "skinId" },
            { 32, "teamId" },
            { 33, "ttkStatus" }
        };
    }
}
