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
        MediaPlayer.Play("taustamusiikki");
        MediaPlayer.IsRepeating = true;
        Camera.Follow(orava);
        Camera.ZoomFactor = 1.2;
        Camera.StayInLevel = true;
    }


    private void LuoKentta()
    {
        Gravity = new Vector(0, -1000);
        TileMap kentta = TileMap.FromLevelAsset("kentta.txt");
        char[] tasot = { '#', 'a', 'b', 'c', '@', 't' };
        for (int i = 0; i < tasot.Length; i++)
            { kentta.SetTileMethod(tasot[i], LisaaTaso, tasot[i]); }
        kentta.SetTileMethod('p', Pahkina);
        kentta.SetTileMethod('o', Orava);
        kentta.SetTileMethod('k', Korppi);
        kentta.SetTileMethod('s', Lepakko);
        kentta.SetTileMethod('m', Muurahainen);
        kentta.SetTileMethod('g', LinnunPesa);
        kentta.Optimize('a','b','c', '@','t', 'g' );
        kentta.Execute(RUUDUN_KOKO, RUUDUN_KOKO);
        Level.Background.Image = LoadImage("tausta");
        DoNextUpdate(() => { Level.Background.ScaleToLevelFull(); });
    }
       

    private void LisaaTaso(Vector paikkaKentalla, double leveys, double korkeus, char kirjain)
    {
        PhysicsObject taso = PhysicsObject.CreateStaticObject(leveys, korkeus);
        taso.Position = paikkaKentalla;
        taso.Image = LoadImage(kirjain.ToString());
        taso.CollisionIgnoreGroup = 1;
        Add(taso);
    }


    private void LinnunPesa(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject pesa = PhysicsObject.CreateStaticObject(leveys, korkeus);
        pesa.Position = paikka;
        pesa.Image = LoadImage("g");
        pesa.Tag = "pesa";
        pesa.CollisionIgnoreGroup = 1;
        Add(pesa);
    }

    
    private void Muurahainen(Vector paikka, double leveys, double korkeus)
    {
        PlatformCharacter muurahainen = new PlatformCharacter (leveys, korkeus)
        {
            Shape = Shape.Ellipse,
            CanRotate = false,
            Position = paikka,
            Mass = 4.0,
            Image = LoadImage("ant"),
            Tag = "vihollinen"
        };
        PlatformWandererBrain tasoAivot = new PlatformWandererBrain();
        tasoAivot.Speed = 170;
        muurahainen.Brain = tasoAivot;
        Add(muurahainen);
    }
    

    private void Korppi(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject korppi = new PhysicsObject(leveys, korkeus)
        {
            Shape = Shape.Circle,
            Position = paikka,
            Image = LoadImage("raven"),
            Tag = "vihollinen",
            Mass = 4.0
        };
        RandomMoverBrain satunnaisAivot = new RandomMoverBrain(200);
        satunnaisAivot.ChangeMovementSeconds = 1.5;
        korppi.Brain = satunnaisAivot;
        satunnaisAivot.TurnWhileMoving = true;
        satunnaisAivot.Active = true;
        satunnaisAivot.WanderRadius = 150;
        Add(korppi);
    }


    private void Lepakko(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject lepakko = new PhysicsObject(leveys, korkeus)
        {
            Shape = Shape.Circle,
            Position = paikka,
            Mass = 4.0,
            Image = LoadImage("lepakko"),
            Tag = "vihollinen"
        };
        FollowerBrain seuraajanAivot = new FollowerBrain(orava);
        lepakko.Brain = seuraajanAivot;
        seuraajanAivot.Speed = 120;
        seuraajanAivot.DistanceFar = 200;
        seuraajanAivot.TurnWhileMoving = true;
        seuraajanAivot.Active = true;
        seuraajanAivot.DistanceToTarget.AddTrigger(100, TriggerDirection.Down, LahestymisAani);
        RandomMoverBrain satunnaisAivot = new RandomMoverBrain(200);
        satunnaisAivot.TurnWhileMoving = true;
        satunnaisAivot.Active = true;
        satunnaisAivot.WanderRadius = 150;
        seuraajanAivot.FarBrain = satunnaisAivot;
        Add(lepakko);
    }

        
    private void Pahkina(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject pahkina = PhysicsObject.CreateStaticObject(leveys, korkeus);
        pahkina.Shape = Shape.Circle;
        pahkina.Position = paikka;
        pahkina.Image = LoadImage("pahkina.png");
        pahkina.Tag = "pahkina";
        Add(pahkina);
    }

    
    private void Orava(Vector paikka, double leveys, double korkeus)
    {
        orava = new PlatformCharacter(leveys, korkeus)
        {
            Shape = Shape.Circle,
            Position = paikka,
            Mass = 4.0,
            Image = LoadImage("squirrel.png")
        };
        AddCollisionHandler(orava, "pahkina", TormaaPahkinaan);
        AddCollisionHandler(orava, "vihollinen", TormaaViholliseen);
        Add(orava);
        orava.Weapon = new AssaultRifle(30, 10);
        orava.Weapon.Ammo.Value = 3;
        orava.Weapon.Power.Value = 100;
        orava.Weapon.Image = LoadImage("tyhja_ase");
        orava.CollisionIgnoreGroup = 2;
        orava.Weapon.ProjectileCollision = Osuma;
    }
    

    private void Osuma(PhysicsObject ammus, IPhysicsObject kohde)
    {
        ammus.Destroy();
        Explosion rajahdys = new Explosion(50);
        rajahdys.Position = kohde.Position;
        Add(rajahdys);
        kohde.Destroy();
    }


    private void AmmuAseella(PlatformCharacter orava)
    {
        PhysicsObject ammus = orava.Weapon.Shoot();
        if (ammus != null)
        {
            ammus.Size *= 1.5;
            ammus.Image = LoadImage("acorn");
            ammus.MaximumLifetime = TimeSpan.FromSeconds(5.0);
            ammus.Mass = 4;
            ammus.Tag = "ammus";
        }
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
        Keyboard.Listen(Key.Space, ButtonState.Down, AmmuAseella, "Ammu", orava);
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
        MessageDisplay.Add("Taskut täynnä tammenterhoja");
        pahkina.Destroy();
        orava.Weapon.Ammo.Value = 3;
    }

        
    private void LahestymisAani()
    {
        MediaPlayer.Pause();
        SoundEffect seuraajanAani = LoadSoundEffect("seuraajanMusiikki.wav");
        seuraajanAani.Play();
        Timer.CreateAndStart(6, MediaPlayer.Resume);
    }


}