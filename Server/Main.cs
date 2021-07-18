using CitizenFX.Core;
using System;

namespace Server
{
    public class Main : BaseScript
    {
        dynamic ESX;

        public Main()
        {
            TriggerEvent("esx:getSharedObject", new object[] { new Action<dynamic>(esx =>
            {
                ESX = esx;
            })});
            Debug.WriteLine("Meth car got loaded, made by Edward#2000");

            EventHandlers["esx_methcar:start"] += new Action<Player>(Start);
            EventHandlers["esx_methcar:stopf"] += new Action<Player, int>(StopFreeze);
            EventHandlers["esx_methcar:make"] += new Action<Player, float, float, float>(Make);
            EventHandlers["esx_methcar:finish"] += new Action<Player, int>(Finish);
            EventHandlers["esx_methcar:blow"] += new Action<Player, float, float, float>(Blow);
        }

        private void Start([FromSource] Player source)
        {
            var xPlayer = ESX.GetPlayerFromId(source);

            if (xPlayer.getInventoryItem("acetone").count >= 5 && xPlayer.getInventoryItem("lithium").count >= 2 && xPlayer.getInventoryItem("methlab").count >= 1)
            {
                if (xPlayer.getInventoryItem("meth").count >= 30)
                {
                    TriggerClientEvent("esx_methcar:notify", source, "~r~~h~You can't hold more meth");
                }
                else
                {
                    TriggerClientEvent("esx_methcar:startprod", source);
                    xPlayer.removeInventoryItem("acetone", 5);
                    xPlayer.removeInventoryItem("lithium", 2);
                }
            }
            else
            {
                TriggerClientEvent("esx_methcar:notify", source, "~r~~h~Not enough supplies to start producing Meth");
            }
        }

        private void StopFreeze([FromSource] Player source, int id)
        {
            var xPlayers = ESX.GetExtendedPlayers();
            for (int i = 1; i <= xPlayers.Count; i++)
            {
                TriggerClientEvent("esx_methcar:stopfreeze", xPlayers[i], id);
            }
        }

        private void Make([FromSource] Player source, float posx, float posy, float posz)
        {
            var xPlayer = ESX.GetPlayerFromId(source);

            if (xPlayer.getInventoryItem("methlab").count >= 1)
            {
                var xPlayers = ESX.GetExtendedPlayers();
                for (int i = 1; i <= xPlayers.Count; i++)
                {
                    TriggerClientEvent("esx_methcar:smoke", xPlayers[i], posx, posy, posz);
                }
            }
            else
            {
                TriggerClientEvent("esx_methcar:stop", source);
            }
        }

        private void Finish([FromSource] Player source, int quality)
        {
            var xPlayer = ESX.GetPlayerFromId(source);
            Debug.WriteLine(quality.ToString());
            Random rnd = new Random();
            xPlayer.addInventoryItem("meth", (int)Math.Floor((double)quality / 2) + rnd.Next(-5, 5));
        }

        private void Blow([FromSource] Player source, float posx, float posy, float posz)
        {
            var xPlayers = ESX.GetExtendedPlayers();
            var xPlayer = ESX.GetPlayerFromId(source);

            for (int i = 1; i <= xPlayers.Count; i++)
            {
                TriggerClientEvent("esx_methcar:blowup", xPlayers[i], posx, posy, posz);
            }
            xPlayer.removeInventoryItem("methlab", 1);
        }
    }
}
