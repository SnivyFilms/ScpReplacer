﻿using Exiled.API.Features;
using Exiled.CustomRoles.API;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCPReplacer
{
    public class Plugin : Plugin<Config, Translations>
    {
        public override string Name => "SCP Replacer";
        public override string Author => "Jon M & Vicious Vikki";
        public override Version Version => new Version(1, 0, 2);

        // Singleton pattern allows easy access to the central state from other classes
        // (e.g. commands)
        public static Plugin Singleton { get; private set; }

        // A list of Roles (SCPs) that have quit early in the round and have not yet been replaced
        public List<ScpToReplace> ScpsAwaitingReplacement { get; } = new List<ScpToReplace>();

        public override void OnEnabled()
        {
            // Set up the Singleton, so we can easily get the instance with all the state
            // from another class.
            Singleton = this;

            // Register event handlers
            //Exiled.Events.Handlers.Server.RoundStarted += OnRoundStart;
            Exiled.Events.Handlers.Player.Left += OnPlayerLeave;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            // Deregister event handlers
            //Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStart;
            Exiled.Events.Handlers.Player.Left -= OnPlayerLeave;

            // This will prevent commands and other classes from being able to access
            // any state while the plugin is disabled
            Singleton = null;

            base.OnDisabled();
        }

        // These event handlers can be pulled out to their own class if needed.
        // However, due to the small size of the plugin, I kept them in this class
        // to cut back on coupling. (Partial classes would be another alternative)
        /*public void OnRoundStart()
        {
            foreach (var scp in Player.List.Where(p => p.IsScp))
            {
                scp.Broadcast(8, "Use <color=#ff4eac>.human</color> in the <color=#b8bd00>~</color> console if you want to be a human class instead");
            }

            ScpsAwaitingReplacement.Clear();
        }*/

        public void OnPlayerLeave(LeftEventArgs ev)
        {
            ScpLeft(ev.Player);
        }
        
        public void ScpLeft(Player scpPlayer)
        {
            if (scpPlayer.IsScp && scpPlayer.Role != RoleTypeId.Scp0492)
            {
                var elapsedSeconds = Round.ElapsedTime.TotalSeconds;
                // Minimum required health (configurable percentage) of the SCP
                // when they quit to be eligible for replacement
                var requiredHealth = (int)(Config.RequiredHealthPercentage / 100.0 * scpPlayer.MaxHealth);
                var customRole = scpPlayer.GetCustomRoles().FirstOrDefault();
                var scpNumber = customRole?.Name.ScpNumber() ?? scpPlayer.Role.ScpNumber();
                Log.Debug($"{scpPlayer.Nickname} left {elapsedSeconds} seconds into the round, was SCP-{scpNumber} with {scpPlayer.Health}/{scpPlayer.MaxHealth} HP ({requiredHealth} required for replacement)");
                if (elapsedSeconds > Config.QuitCutoff)
                {
                    Log.Debug("This SCP will not be replaced because the quit cutoff has already passsed");
                    return;
                }

                if (scpPlayer.Health < requiredHealth)
                {
                    Log.Debug("This SCP will not be replaced because they have lost too much health");
                    return;
                }

                // Let all non-SCPs (including spectators) know of the opportunity to become SCP
                // SCPs are not told of this because then they would also have to be replaced after swapping 
                foreach (var p in Player.List.Where(x => !x.IsScp))
                {
                    var message = Translation.ReplaceBroadcast.Replace("%NUMBER%", scpNumber);
                    // Longer broadcast time since beta test revealed users were having trouble reading it all in time
                    p.Broadcast(16, Translation.BroadcastHeader + message, Broadcast.BroadcastFlags.Normal, true);
                    // Also send console message in case they miss the broadcast
                    p.SendConsoleMessage(message, "yellow");
                }

                // Add the SCP to our list so that a user can claim it with the .volunteer command

                ScpsAwaitingReplacement.Add(new ScpToReplace
                {
                    Name = scpNumber,
                    Role = scpPlayer.Role,
                    CustomRole = customRole
                });
            }
        }

        /// <summary>
        ///     Whether the replacement cutoff (i.e. the max time in seconds after the round
        ///     that a player can still use .volunteer) has passed
        ///     We put this function here so that we can conveniently access the Config without
        ///     needing to implement the Singleton pattern in Config
        /// </summary>
        /// <returns>whether the replacement period cutoff has passed (true if passed)</returns>
        public bool HasReplacementCutoffPassed()
        {
            return Round.ElapsedTime.TotalSeconds > Config.ReplaceCutoff;
        }
    }
}