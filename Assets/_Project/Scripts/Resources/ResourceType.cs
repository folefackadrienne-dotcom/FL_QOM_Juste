namespace KingdomOfGod.Resources
{
    /// <summary>The 7 resources of the kingdom economy (see GDD section "Ressources & Économie").</summary>
    public enum ResourceType
    {
        Wheat,      // Blé — nourriture, population
        Water,      // Eau — population, agriculture, pureté
        Wood,       // Bois — construction, sièges
        Gold,       // Or — unités avancées, diplomatie, temple
        Faith,      // Foi — miracles, moral, conversion (ressource centrale)
        Wisdom,     // Sagesse — technologies, décisions, école de prophètes
        Justice     // Justice — loyauté du peuple, jugement
    }
}
