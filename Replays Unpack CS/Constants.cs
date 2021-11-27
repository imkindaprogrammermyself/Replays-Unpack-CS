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
            {
                1,
                "antiAbuseEnabled"
            },
            { 2, "avatarId" },
            { 3, "camouflageInfo" },
            { 4, "clanColor" },
            {
                5,
                "clanID"
            },
            { 6, "clanTag" },
            { 7, "crewParams" },
            { 8, "dogTag" },
            { 9, "fragsCount" },
            { 10, "friendlyFireEnabled" },
            {
                11,
                "id"
            },
            { 12, "invitationsEnabled" },
            { 13, "isAbuser" },
            { 14, "isAlive" },
            { 15, "isBot" },
            { 16, "isClientLoaded" },
            {
                17,
                "isConnected"
            },
            { 18, "isHidden" },
            { 19, "isLeaver" },
            { 20, "isPreBattleOwner" },
            { 21, "isTShooter" },
            {
                22,
                "killedBuildingsCount"
            },
            { 23, "maxHealth" },
            { 24, "name" },
            { 25, "playerMode" },
            { 26, "preBattleIdOnStart" },
            {
                27,
                "preBattleSign"
            },
            { 28, "prebattleId" },
            { 29, "realm" },
            { 30, "shipComponents" },
            { 31, "shipConfigDump" },
            {
                32,
                "shipId"
            },
            { 33, "shipParamsId" },
            { 34, "skinId" },
            { 35, "teamId" },
            { 36, "ttkStatus" }
        };
    }
}
