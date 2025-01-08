namespace Watermelon
{
    // DO NOT CHANGE ORDER OF THE CURRENCIES AFTER RELEASE. IT CAN BRAKE SAVES OF THE GAME!
    public enum CurrencyType
    {
        Coins = 0,

        // raw resources
        Wood = 1,
        Stone = 2,
        Fiber = 3,

        // produced resources
        Planks = 50,
        Bricks = 51,
        Rope = 52,

        // raw food
        Berries = 100,
        Coconut = 101,
        Pumpkin = 102,
        Fish = 103,

        // cooked food
        Jam = 150,
        BigJam = 151,
        CookedFish = 152,
        Soup = 153,

        // special
        Scissors = 500,
    }
}