# SCPReplacer

A plugin that allows players to volunteer to replace SCPs that have quit early in the round.


## Configuration

* `quit_cutoff` - The maximum time after the round start, in seconds, that a quitting SCP can cause the volunteer opportunity announcement (default 60 seconds)

* `replace_cutoff` - The maximum time after the round start, in seconds, that a player can use the .volunteer command (defaults to 85)

* `required_health_percentage` - The required percentage of health (0-100) the SCP must have had to be eligible for replacement. Defaults to 95 (no percent sign)

* `lottery_period_seconds` - The amount of time that the server takes before replacing players to an SCP, this is to ensure multiple people, if they want to, can volunteer to be replaced as an SCP.

