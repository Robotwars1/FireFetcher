
namespace FireFetcher
{
    internal class CmMapParser
    {
        private readonly Dictionary<string, int> MapDictionary = new()
        {
            { "Container Ride",   62761 },
            { "Portal Carousel",  62758 },
            { "Portal Gun",       47458 },
            { "Smooth Jazz",      47455 },
            { "Cube Momentum",    47452 },
            { "Future Starter",   47106 },
            { "Secret Panel",     62763 },
            { "Wakeup",           62759 },
            { "Incinerator",      47735 },

            { "Laser Intro",     62765 },
            { "Laser Stairs",    47736 },
            { "Dual Lasers",     47738 },
            { "Laser Over Goo",  47742 },
            { "Catapult Intro",  62767 },
            { "Trust Fling",     47744 },
            { "Pit Flings",      47465 },
            { "Fizzler Intro",   47746 },

            { "Ceiling Catapult",      47748 },
            { "Ricochet",              47751 },
            { "Bridge Intro",          47752 },
            { "Bridge the Gap",        47755 },
            { "Turret Intro",          47756 },
            { "Laser Relays",          47759 },
            { "Turret Blocker",        47760 },
            { "Laser vs Turret",       47763 },
            { "Pull the Rug",          47764 },

            { "Column Blocker",    47766 },
            { "Laser Chaining",    47768 },
            { "Triple Laser",      47770 },
            { "Jail Break",        47773 },
            { "Escape",            47774 },

            { "Turret Factory",       47776 },
            { "Turret Sabotage",      47779 },
            { "Neurotoxin Sabotage",  47780 },
            { "Core",                 62771 },

            { "Underground",      47783 },
            { "Cave Johnson",     47784 },
            { "Repulsion Intro",  47787 },
            { "Bomb Flings",      47468 },
            { "Crazy Box",        47469 },
            { "PotatOS",          47472 },

            { "Propulsion Intro",   47791 },
            { "Propulsion Flings",  47793 },
            { "Conversion Intro",   47795 },
            { "Three Gels",         47798 },

            { "Test",                88350 },
            { "Funnel Intro",        47800 },
            { "Ceiling Button",      47802 },
            { "Wall Button",         47804 },
            { "Polarity",            47806 },
            { "Funnel Catch",        47808 },
            { "Stop the Box",        47811 },
            { "Laser Catapult",      47813 },
            { "Laser Platform",      47815 },
            { "Propulsion Catch",    47817 },
            { "Repulsion Polarity",  47819 },

            { "Finale 1",  62776 },
            { "Finale 2",  47821 },
            { "Finale 3",  47824 },
            { "Finale 4",  47456 }
        };

        public string ParseMap(int MapValue)
        {
            return MapDictionary.ElementAt(MapDictionary.Values.ToList().IndexOf(MapValue)).Key;
        }
    }
}
