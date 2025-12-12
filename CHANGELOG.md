# v1.1.1

- Added a GUI to allow or disallow late joiners in game.
- Previous profile pictures and names are removed in spectator mode (only applies to players with the mod)
- Trying a new method to fix the issue of invisible players when the previous owner dies and leaves mid-game.

# v1.1.0

- Rewrote the function to sync names

# v1.0.9

- Added a sync function to PlayerNameFixPatch so clients can update their playernames locally. 

# v1.0.8

- Added a LobbyManager which should reliably list and delist the lobby from the list.

# v1.0.7

- Late joiners should be able to join now when the lobby is not full.
- Should correctly show the right amount of players on the lobby list.

# v1.0.6

- Implementing a fix for "Unknown" name for late joiners.

# v1.0.5
- Removed a broken logic. It was causing issues with some other mods.

# v1.0.4

- Attempt fix late-joining players appearing invisible or desynced by enabling renderers, animator, and resetting player state automatically.

# v1.0.3

- Hotfix what i broke.

# v1.0.2

- After the game has started and the lobby was reopened it wasnt properly showing the amount of players present in the lobby. It should be fixed and should show up properly. (faulty logic, reverted the changes)
- Also modified some other stuff. 

# v1.0.1

- Added a logic to try to fix players having the name "Unknown" when joining in late.


# v1.0.0

- The lobby should not show up on the lobby list after the game has started or the lobby is full, and it should be visible once in orbit and the lobby is not full.