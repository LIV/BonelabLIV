namespace BonelabLIV
{
  public enum GameLayer
  {
    // Base game layers.
    Default = 0,
    TransparentFX = 1,
    IgnoreRaycast = 2,
    Water = 4,
    UI = 5,
    Player = 8,
    NoCollide = 9,
    Dynamic = 10,
    StereoRenderIgnore = 11,
    EnemyColliders = 12,
    Static = 13,
    SpawnGunUI = 14,
    Interactable = 15,
    Hand = 16,
    HandOnly = 17,
    Socket = 18,
    Plug = 19,
    InteractableOnly = 20,
    PlayerAndNpc = 21,
    NoSelfCollide = 22,
    FeetOnly = 23,
    Feet = 24,
    NoFootBall = 25,
    Trigger = 27,

    // Custom layers to use in the mod.
    LivOnly = 31
  }
}
