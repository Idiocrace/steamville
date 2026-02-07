using System;
using Crucible.Types;
using SteamVille.Content;

using var game = new Game();
game.Run();

// Initialize registries
Registry<Tileable> tileableRegistry = new();
Registry<Resource> resourceRegistry = new();
BaseRegistry<SVMod> modRegistry = new();
