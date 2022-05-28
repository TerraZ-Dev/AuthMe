using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;
using TShockAPI.Hooks;

namespace AuthMe
{
    [ApiVersion(2, 1)]
    public class AuthMePlugin : TerrariaPlugin
    {
        public override string Author => "Zoom L1";
        public override string Name => "AuthMe";

        public AuthMePlugin(Main game) : base(game) { }

        public override void Initialize()
        {
            Manager.Initailize();
            
            Commands.ChatCommands.Add(new Command("auth.me", AuthMeCommand, "authme", "auth"));

            PlayerHooks.PlayerPostLogin += OnLogin;
        }

        public void OnLogin(PlayerPostLoginEventArgs args)
        {
            string to = Manager.GetSecondAccount(args.Player.Account.Name);
            if (to != "")
                LoginPlayer(args.Player, to);
        }

        public void AuthMeCommand(CommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Invalid command. Try /authme <account> <password>");
                return;
            }

            string strAccount = args.Parameters[0];
            string strPassword = args.Parameters[1];

            var account = TShock.UserAccounts.GetUserAccountByName(strAccount);
            if (account == null)
            {
                args.Player.SendErrorMessage("Wrong account.");
                return;
            }
            if (!account.VerifyPassword(strPassword))
            {
                args.Player.SendErrorMessage("Wrong password.");
                return;
            }

            Manager.Add(args.Player.Account.Name, strAccount);
            args.Player.SendSuccessMessage("You confirmed your account, try '/logout' '/login'");
        }

        public static void LoginPlayer(TSPlayer player, string Account, bool msg = false)
        {
            UserAccount userAccountByName = TShock.UserAccounts.GetUserAccountByName(Account);
            player.DataWhenJoined = new PlayerData(player);
            player.DataWhenJoined.CopyCharacter(player);
            player.PlayerData = new PlayerData(player);
            player.PlayerData.CopyCharacter(player);
            if (userAccountByName != null)
            {
                player.PlayerData = TShock.CharacterDB.GetPlayerData(player, userAccountByName.ID);
                Group groupByName = TShock.Groups.GetGroupByName(userAccountByName.Group);
                player.Group = groupByName;
                player.tempGroup = null;
                player.Account = userAccountByName;
                player.IsLoggedIn = true;
                player.IsDisabledForSSC = false;
                if (Main.ServerSideCharacter)
                {
                    if (player.HasPermission(Permissions.bypassssc))
                    {
                        player.PlayerData.CopyCharacter(player);
                        TShock.CharacterDB.InsertPlayerData(player, false);
                    }
                    player.PlayerData.RestoreCharacter(player);
                }
                player.LoginFailsBySsi = false;
                if (player.HasPermission(Permissions.ignorestackhackdetection))
                {
                    player.IsDisabledForStackDetection = false;
                }
                if (player.HasPermission(Permissions.usebanneditem))
                {
                    player.IsDisabledForBannedWearable = false;
                }

                if (msg)
                    player.SendSuccessMessage("Authenticated as " + userAccountByName.Name + " successfully.");

                TShock.Log.ConsoleInfo(player.Name + " authenticated successfully as user " + player.Name + ".");
                PlayerHooks.OnPlayerPostLogin(player);
            }
        }
    }
}
