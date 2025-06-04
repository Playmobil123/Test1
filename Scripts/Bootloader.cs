public static class Bootloader {
    public static void RunMainMenuBoot() {
        MatrixManager.InitGlobalFrameCounter(0);
        MatrixManager.InitMatrixStack();

        PathManager.InitializePaths();

        PathManager.GameDataPath = "GameData";
        PathManager.LevelsPath = "Levels";
        PathManager.TexturesPath = "Textures";
        PathManager.CharactersPath = "Characters";
        PathManager.AnimationsPath = "Animations";
        PathManager.FixTexturesPath = "FixTextures";
        PathManager.SoundPath = "Sound";
        PathManager.SavesPath = "Saves";

        GameDescriptorLoader.Load("gamedsc.bin");
    }
}
