using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

public class Oravan_pelastus : PhysicsGame
{
    private const double NOPEUS = 200;
    private const double HYPPYNOPEUS = 750;
    private const int RUUDUN_KOKO = 30;
    private PlatformCharacter orava;
    private Image oravanKuva = LoadImage("squirrel.png");
    private Image pahkinanKuva = LoadImage("pahkina.png");
    private Image korpinKuva = LoadImage("raven");
    private Image muurahaisenKuva = LoadImage("ant");
    private SoundEffect maaliAani = LoadSoundEffect("maali.wav");


    public override void Begin()
    {
        LuoKentta();
        LisaaNappaimet();
        Camera.Follow(orava);
        Camera.ZoomFactor = 1.2;
        Camera.StayInLevel = true;
    }


    private void LuoKentta()
    {
        Gravity = new Vector(0, -1000);
        TileMap kentta = TileMap.FromLevelAsset("kentta.txt");
        kentta.SetTileMethod('#', LisaaTaso);
        kentta.SetTileMethod('a', LisaaTaso);
        kentta.SetTileMethod('b', LisaaTaso);
        kentta.SetTileMethod('c', LisaaTaso);
        kentta.SetTileMethod('@', LisaaTaso);
        kentta.SetTileMethod('d', LisaaTaso);
        kentta.SetTileMethod('t', LisaaTaso);
        kentta.SetTileMethod('*', LisaaPahkina);
        kentta.SetTileMethod('o', LisaaOrava);
        kentta.SetTileMethod('k', LisaaKorppi);
        kentta.SetTileMethod('m', LisaaMuurahainen);
        kentta.Execute(RUUDUN_KOKO, RUUDUN_KOKO);
        Level.CreateBorders();
        Level.Background.CreateGradient(Color.White, Color.SkyBlue);
    }

    private void LisaaMuurahainen(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject muurahainen = new PhysicsObject(leveys, korkeus);
        muurahainen.Position = paikka;
        muurahainen.Mass = 4.0;
        muurahainen.Image = muurahaisenKuva;
        muurahainen.Tag = "muurahainen";
        Add(muurahainen);
    }

    private void LisaaKorppi(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject korppi = new PhysicsObject (leveys, korkeus);
        korppi.Position = paikka;
        korppi.Mass = 4.0;
        korppi.Image = korpinKuva;
        korppi.Tag = "korppi";
        Add(korppi);
    }

    private void LisaaTaso(Vector paikkaKentalla, double leveys, double korkeus)
    {
        PhysicsObject taso = PhysicsObject.CreateStaticObject(leveys, korkeus);
        taso.Position = paikkaKentalla;
        taso.Image = LoadImage("a");
        Add(taso);
    }

    private void LisaaPahkina(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject pahkina = PhysicsObject.CreateStaticObject(leveys, korkeus);
        pahkina.IgnoresCollisionResponse = true;
        pahkina.Position = paikka;
        pahkina.Image = pahkinanKuva;
        pahkina.Tag = "pahkina";
        Add(pahkina);
    }

    private void LisaaOrava(Vector paikka, double leveys, double korkeus)
    {
        orava = new PlatformCharacter(leveys, korkeus);
        orava.Position = paikka;
        orava.Mass = 4.0;
        orava.Image = oravanKuva;
        AddCollisionHandler(orava, "pahkina", TormaaPahkinaan);
        AddCollisionHandler(orava, "korppi", TormaaKorppiin);
        AddCollisionHandler(orava, "muurahainen", TormaaMuurahaiseen);
        Add(orava);
    }



    private void LisaaNappaimet()
    {
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");

        Keyboard.Listen(Key.Left, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", orava, -NOPEUS);
        Keyboard.Listen(Key.Right, ButtonState.Down, Liikuta, "Liikkuu oikealle", orava, NOPEUS);
        Keyboard.Listen(Key.Up, ButtonState.Pressed, Hyppaa, "Pelaaja hyppää", orava, HYPPYNOPEUS);

        //ControllerOne.Listen(Button.Back, ButtonState.Pressed, Exit, "Poistu pelistä");
        //ControllerOne.Listen(Button.DPadLeft, ButtonState.Down, Liikuta, "Pelaaja liikkuu vasemmalle", orava, -NOPEUS);
        //ControllerOne.Listen(Button.DPadRight, ButtonState.Down, Liikuta, "Pelaaja liikkuu oikealle", orava, NOPEUS);
        //ControllerOne.Listen(Button.A, ButtonState.Pressed, Hyppaa, "Pelaaja hyppää", orava, HYPPYNOPEUS);
        //PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
    }

    private void Liikuta(PlatformCharacter orava, double nopeus)
    {
        orava.Walk(nopeus);
    }

    private void Hyppaa(PlatformCharacter orava, double nopeus)
    {
        orava.Jump(nopeus);
    }

    private void TormaaPahkinaan(PhysicsObject hahmo, PhysicsObject pahkina)
    {
        maaliAani.Play();
        MessageDisplay.Add("Keräsit pähkinän!");
        pahkina.Destroy();
    }

    private void TormaaMuurahaiseen(IPhysicsObject collidingObject, IPhysicsObject otherObject)
    {
        throw new NotImplementedException();
    }

    private void TormaaKorppiin(IPhysicsObject collidingObject, IPhysicsObject otherObject)
    {
        throw new NotImplementedException();
    }
}