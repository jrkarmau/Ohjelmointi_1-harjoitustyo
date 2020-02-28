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



    public override void Begin()
    {
        LuoKentta();
        LisaaNappaimet();
        Camera.Follow(orava);
        Camera.ZoomFactor = 1.2;
        Camera.StayInLevel = true;
        MediaPlayer.Play("taustamusiikki");
        MediaPlayer.IsRepeating = true;
    }


    private void LuoKentta()
    {
        Gravity = new Vector(0, -1000);
        TileMap kentta = TileMap.FromLevelAsset("kentta.txt");
        kentta.SetTileMethod('#', LisaaTaso, "#");
        kentta.SetTileMethod('a', LisaaTaso, "a");
        kentta.SetTileMethod('b', LisaaTaso, "b");
        kentta.SetTileMethod('c', LisaaTaso, "c");
        kentta.SetTileMethod('@', LisaaTaso, "@");
        kentta.SetTileMethod('t', LisaaTaso, "t");
        kentta.SetTileMethod('p', LisaaPahkina);
        kentta.SetTileMethod('o', LisaaOrava);
        kentta.SetTileMethod('k', LisaaKorppi);
        kentta.SetTileMethod('s', LisaaSeuraajaKorppi);
        kentta.SetTileMethod('m', LisaaMuurahainen);
        kentta.SetTileMethod('g', lisaaPesa);
        kentta.Optimize('a','b','c', '@','t', 'g' );
        kentta.Execute(RUUDUN_KOKO, RUUDUN_KOKO);
        Level.Background.Image = LoadImage("tausta");
        DoNextUpdate(() => { Level.Background.ScaleToLevelFull(); });

        //Level.CreateBorders();
        //Level.Background.CreateGradient(Color.White, Color.SkyBlue);
    }

    private void lisaaPesa(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject pesa = PhysicsObject.CreateStaticObject(leveys, korkeus);
        pesa.Position = paikka;
        pesa.Image = LoadImage("g");
        pesa.Tag = "pesa";
        pesa.CollisionIgnoreGroup = 1;
        Add(pesa);
    }

    private void LisaaTaso(Vector paikkaKentalla, double leveys, double korkeus, string kirjain)
    {
        PhysicsObject taso = PhysicsObject.CreateStaticObject(leveys, korkeus);
        taso.Position = paikkaKentalla;
        taso.Image = LoadImage(kirjain);
        taso.CollisionIgnoreGroup = 1;
        Add(taso);
    }


    private void LisaaMuurahainen(Vector paikka, double leveys, double korkeus)
    {
        PlatformCharacter muurahainen = new PlatformCharacter(leveys, korkeus);
        muurahainen.Shape = Shape.Ellipse;
        muurahainen.CanRotate = false;
        muurahainen.Position = paikka;
        muurahainen.Mass = 4.0;
        muurahainen.Image = LoadImage("ant");
        muurahainen.Tag = "vihollinen";
        PlatformWandererBrain tasoAivot = new PlatformWandererBrain();
        tasoAivot.Speed = 150;
        muurahainen.Brain = tasoAivot;
        Add(muurahainen);
    }


    private void LisaaKorppi(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject korppi = new PhysicsObject(leveys, korkeus);
        korppi.Shape = Shape.Circle;
        korppi.Position = paikka;
        korppi.Mass = 4.0;
        korppi.Image = LoadImage("raven");
        korppi.Tag = "vihollinen";
        RandomMoverBrain satunnaisAivot = new RandomMoverBrain(200);
        satunnaisAivot.ChangeMovementSeconds = 3;
        korppi.Brain = satunnaisAivot;
        satunnaisAivot.TurnWhileMoving = true;
        satunnaisAivot.Active = true;
        satunnaisAivot.WanderRadius = 200;


        Add(korppi);
    }


    private void LisaaSeuraajaKorppi(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject seuraajaKorppi = new PhysicsObject(leveys, korkeus);
        seuraajaKorppi.Shape = Shape.Circle;
        seuraajaKorppi.Position = paikka;
        seuraajaKorppi.Mass = 4.0;
        seuraajaKorppi.Image = LoadImage("Follower_raven");
        seuraajaKorppi.Tag = "vihollinen";
        FollowerBrain seuraajanAivot = new FollowerBrain(orava);
        seuraajaKorppi.Brain = seuraajanAivot;
        seuraajanAivot.Speed = 120;
        seuraajanAivot.DistanceFar = 400;
        seuraajanAivot.TurnWhileMoving = true;
        seuraajanAivot.Active = true;
        seuraajanAivot.DistanceToTarget.AddTrigger(200, TriggerDirection.Down, LahestymisAani);
        Add(seuraajaKorppi);
    }

        
    private void LisaaPahkina(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject pahkina = PhysicsObject.CreateStaticObject(leveys, korkeus);
        pahkina.Shape = Shape.Circle;
        pahkina.Position = paikka;
        pahkina.Mass = 4.0;
        pahkina.Image = LoadImage("pahkina.png");
        pahkina.Tag = "pahkina";
        Add(pahkina);
    }

    
    private void LisaaOrava(Vector paikka, double leveys, double korkeus)
    {
        orava = new PlatformCharacter(leveys, korkeus);
        orava.Shape = Shape.Circle;
        orava.Position = paikka;
        orava.Mass = 4.0;
        orava.Image = LoadImage("squirrel.png");
        AddCollisionHandler(orava, "pahkina", TormaaPahkinaan);
        AddCollisionHandler(orava, "vihollinen", TormaaViholliseen);
        Add(orava);
    }


    private void TormaaViholliseen(IPhysicsObject collidingObject, IPhysicsObject otherObject)
    {
        MessageDisplay.Add("Törmäsit viholliseen!");
    }


    private void LisaaNappaimet()
    {
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Left, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", orava, -NOPEUS);
        Keyboard.Listen(Key.Right, ButtonState.Down, Liikuta, "Liikkuu oikealle", orava, NOPEUS);
        Keyboard.Listen(Key.Up, ButtonState.Pressed, Hyppaa, "Pelaaja hyppää", orava, HYPPYNOPEUS);
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
        SoundEffect kerays = LoadSoundEffect("collect1.wav");
        kerays.Play();
        MessageDisplay.Add("Keräsit pähkinän!");
        pahkina.Destroy();
    }

    
    private void LahestymisAani()
    {
        SoundEffect seuraajanAani = LoadSoundEffect("seuraajanMusiikki.wav");
        seuraajanAani.Play();
    }


}