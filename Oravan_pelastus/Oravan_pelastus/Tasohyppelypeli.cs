using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using Jypeli.Effects;

public class Oravan_pelastus : PhysicsGame
{
    private const double NOPEUS = 200;
    private const double HYPPYNOPEUS = 750; 
    private const int RUUDUN_KOKO = 30; 
    private PlatformCharacter orava;
    private IntMeter pisteLaskuri;


    public override void Begin()
    {
        Alkuvalikko();
    }

    private void Aloita()
    {
        LuoKentta();
        LisaaNappaimet();
        LuoKamera();
        PähkinäLaskuri();
    }
    /// <summary>
    /// alkuvalikko, josta voidaan aloittaa tai lopettaa peli.
    /// </summary>
    private void Alkuvalikko()
    {
        Level.Background.Image = LoadImage("tausta");
        Level.Background.ScaleToLevelFull();
        MultiSelectWindow valikko = new MultiSelectWindow("",
            "Aloita uusi peli", "Parhaat pisteet", "Lopeta");
        valikko.Color = Color.JungleGreen;
        Add(valikko);
        valikko.AddItemHandler(0, Aloita);
        valikko.AddItemHandler(1, null);
        valikko.AddItemHandler(2, Exit);

    }

    
    
    /// <summary>
    /// Luo Kameran ja alkaa siirtämään kameraa ylöspäin.
    /// </summary>
    private void LuoKamera()
    {
        //IsFullScreen = true;
        Camera.Position = orava.Position;
        Camera.FollowX(orava);
        Camera.ZoomFactor = 1;
        Camera.StayInLevel = true;
        Timer.SingleShot(5.0, delegate { Camera.Velocity = new Vector(0, 50); });
        Timer onkoRuudulla = new Timer();
        onkoRuudulla.Interval = 0.1;
        onkoRuudulla.Start();
        onkoRuudulla.Timeout += delegate
            { if ((orava.Y - Camera.Y) < -450) Alusta(); };        
    }


    /// <summary>
    /// Luo pelimaailman ja viholliset sekä aloittaa taustamusiikin.
    /// </summary>
    private void LuoKentta()
    {        

        MediaPlayer.Play("taustamusiikki");
        MediaPlayer.IsRepeating = true;
        Gravity = new Vector(0, -1000);
        TileMap kentta = TileMap.FromLevelAsset("kentta.txt");
        char[] tasot = { '#', 'a', 'b', 'c', '@', 't' };
        for (int i = 0; i < tasot.Length; i++)
            { kentta.SetTileMethod(tasot[i], LisaaTaso, tasot[i]); }
        kentta.SetTileMethod('k', LuoKorppi);
        kentta.SetTileMethod('p', LuoPahkina);
        kentta.SetTileMethod('o', LuoOrava);
        kentta.SetTileMethod('s', LuoLepakko);
        kentta.SetTileMethod('m', LuoMuurahainen);
        kentta.SetTileMethod('g', LuoLinnunPesa);
        kentta.Optimize('a','b','c', '@','t', 'g' );
        kentta.Execute(RUUDUN_KOKO, RUUDUN_KOKO);
        Level.Background.Image = LoadImage("tausta");
        DoNextUpdate(() => { Level.Background.ScaleToLevelFull(); });
    }

       
    /// <summary>
    /// Luo tasot pelikentälle.
    /// </summary>
    /// <param name="paikkaKentalla">tason paikka pelimaailmaassa</param>
    /// <param name="leveys">tason leveys</param>
    /// <param name="korkeus">tason korkeus</param>
    /// <param name="kirjain">tason ulkoasu</param>
    private void LisaaTaso(Vector paikkaKentalla, double leveys, double korkeus, char kirjain)
    {
        PhysicsObject taso = PhysicsObject.CreateStaticObject(leveys, korkeus);
        taso.Position = paikkaKentalla;
        taso.Image = LoadImage(kirjain.ToString());
        taso.CollisionIgnoreGroup = 1;
        Add(taso);
    }


    /// <summary>
    /// Luo tuhottavan linnunpesän pelimaailmaan
    /// </summary>
    /// <param name="paikka">pesän paikka</param>
    /// <param name="leveys">pesän leveys</param>
    /// <param name="korkeus">pesän korkeus</param>
    private void LuoLinnunPesa(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject pesa = PhysicsObject.CreateStaticObject(leveys, korkeus);
        pesa.Position = paikka;
        pesa.Image = LoadImage("g");
        pesa.Tag = "pesa";
        Add(pesa);
    }


    /// <summary>
    /// Luo tasolla kulkevan muurahaisen.
    /// </summary>
    /// <param name="paikka">olion paikka pelimaailmassa</param>
    /// <param name="leveys">muurahaisen leveys</param>
    /// <param name="korkeus">muurahaisen korkeus</param>
    private void LuoMuurahainen(Vector paikka, double leveys, double korkeus)
    {
        PlatformCharacter muurahainen = new PlatformCharacter (leveys, korkeus)
        {
            Shape = Shape.Ellipse,
            CanRotate = false,
            Position = paikka,
            Image = LoadImage("ant"),
            Tag = "vihollinen"
        };
        PlatformWandererBrain tasoAivot = new PlatformWandererBrain();
        tasoAivot.Speed = 170;
        muurahainen.Brain = tasoAivot;
        Add(muurahainen);
    }

    
    /// <summary>
    /// Luo satunnaisesti liikkuvan korpin.
    /// </summary>
    /// <param name="paikka">korpin paikka pelimaailmassa</param>
    /// <param name="leveys">korpin leveys</param>
    /// <param name="korkeus">korpin korkeus</param>
    private void LuoKorppi(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject korppi = new PhysicsObject(leveys, korkeus)
        {
            Shape = Shape.Circle,
            Position = paikka,
            Image = LoadImage("raven"),
            Tag = "vihollinen",
        };
        RandomMoverBrain satunnaisAivot = new RandomMoverBrain(200);
        satunnaisAivot.ChangeMovementSeconds = 1.5;
        korppi.Brain = satunnaisAivot;
        satunnaisAivot.TurnWhileMoving = true;
        satunnaisAivot.WanderRadius = 250;
        Add(korppi);
    }


    /// <summary>
    /// Luo lepakon, joka liikkuu satunnaisesti, mutta kun pelaaja tulee lähelle alkaa seurata.
    /// </summary>
    /// <param name="paikka">Lepakon paikka pelimaailmassa</param>
    /// <param name="leveys">lepakon leveys</param>
    /// <param name="korkeus">lepakon korkeus</param>
    private void LuoLepakko(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject lepakko = new PhysicsObject(leveys, korkeus)
        {
            Shape = Shape.Circle,
            Position = paikka,
            Image = LoadImage("lepakko"),
            Tag = "vihollinen"
        };
        FollowerBrain seuraajanAivot = new FollowerBrain(orava);
        RandomMoverBrain satunnaisAivot = new RandomMoverBrain(200);
        satunnaisAivot.TurnWhileMoving = true;
        satunnaisAivot.Active = true;
        satunnaisAivot.WanderRadius = 150;
        seuraajanAivot.FarBrain = satunnaisAivot;
        seuraajanAivot.Speed = 120;
        seuraajanAivot.DistanceFar = 200;
        seuraajanAivot.TurnWhileMoving = true;
        seuraajanAivot.Active = true;
        lepakko.Brain = seuraajanAivot;
        Add(lepakko);   
    }


    /// <summary>
    /// Luo kerättävän pähkinän
    /// </summary>
    /// <param name="paikka">Pähkinän paikka pelimaailmassa</param>
    /// <param name="leveys">pähkinän leveys</param>
    /// <param name="korkeus">pähkinän korkeus</param>
    private void LuoPahkina(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject pahkina =  new PhysicsObject(leveys, korkeus);
        pahkina.Shape = Shape.Circle;
        pahkina.Position = paikka;
        pahkina.Image = LoadImage("pahkina.png");
        pahkina.Tag = "pahkina";
        Add(pahkina);
    }


    /// <summary>
    /// Luo pelihahmon ja asettaa sille aseen.
    /// </summary>
    /// <param name="paikka">pelaajan paikka pelimaailmassa</param>
    /// <param name="leveys">pelaajan leveys</param>
    /// <param name="korkeus">pelaajan korkeus</param>
    private void LuoOrava(Vector paikka, double leveys, double korkeus)
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
        orava.Weapon.Ammo.Value = 2;
        orava.Weapon.Power.Value = 100;
        orava.Weapon.Image = LoadImage("tyhja_ase");
        orava.CollisionIgnoreGroup = 2;
        orava.Weapon.ProjectileCollision = Osuma;
    }

    

    /// <summary>
    /// käsittelee pelaajan ampuman ammuksen ja vihollisen osuman.
    /// </summary>
    /// <param name="ammus">ammuttu panos</param>
    /// <param name="kohde">osuman saanut vihollinen</param>
    private void Osuma(IPhysicsObject ammus, IPhysicsObject kohde)
    {
        if (kohde.Tag.ToString() == "vihollinen")
        {
            kohde.Destroy();
            ammus.Destroy();
            Explosion rajahdys = new Explosion(50);
            rajahdys.Position = kohde.Position;
            Add(rajahdys);
        }
        else if (kohde.Tag.ToString() == "pesa")
        {
            kohde.Destroy();
            ammus.Destroy();
            int pMaxMaara = 200;
            ExplosionSystem rajahdys =
              new ExplosionSystem(LoadImage("rajahdys"), pMaxMaara);
            Add(rajahdys);
            int pMaara = 150;
            rajahdys.AddEffect(kohde.X, kohde.Y, pMaara);
            MessageDisplay.Add("Voitit pelin!");
            Timer.SingleShot(3, Begin);
        }
        else return;
    }


    /// <summary>
    /// käsittelee ampumistapahtuman
    /// </summary>
    /// <param name="orava">ampuja</param>
    private void AmmuAseella(PlatformCharacter orava)
    {
        PhysicsObject ammus = orava.Weapon.Shoot();
        if (ammus != null)
        {
            ammus.Size *= 1.5;
            ammus.Image = LoadImage("acorn");
            ammus.MaximumLifetime = TimeSpan.FromSeconds(5.0);
            ammus.Tag = "ammus";
            pisteLaskuri.Value -= 1;
        }
    }


    /// <summary>
    /// Käsittelee pelaajan törmäyksen viholliseen.
    /// </summary>
    /// <param name="collidingObject">kuka törmää</param>
    /// <param name="otherObject">mihin törmää</param>
    private void TormaaViholliseen(IPhysicsObject collidingObject, IPhysicsObject otherObject)
    {
        if (orava.Weapon.Ammo.Value == 0)
        {
            Alusta();
        }
        else
        {
            MessageDisplay.Add("menetit kaikki pähkinät!");
            pisteLaskuri.Value = 0;
            orava.Weapon.Ammo.Value = 0;
            SoundEffect osuma = LoadSoundEffect("hit.wav");
            osuma.Play();
        }
    }

        
    /// <summary>
    /// Lisää peliin ohjaimet.
    /// </summary>
    private void LisaaNappaimet()
    {
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Left, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", orava, -NOPEUS);
        Keyboard.Listen(Key.Right, ButtonState.Down, Liikuta, "Liikkuu oikealle", orava, NOPEUS);
        Keyboard.Listen(Key.Up, ButtonState.Pressed, Hyppaa, "Pelaaja hyppää", orava, HYPPYNOPEUS);
        Keyboard.Listen(Key.Space, ButtonState.Down, AmmuAseella, "Ammu", orava);
    }


    /// <summary>
    /// liikuttaa pelaajaa.
    /// </summary>
    /// <param name="orava">ketä liikutetaan</param>
    /// <param name="nopeus">millä nopeudella liikutetaan</param>
    private void Liikuta(PlatformCharacter orava, double nopeus)
    {
        orava.Walk(nopeus);
    }


    /// <summary>
    /// Käsittelee hyppäystapahtuman.
    /// </summary>
    /// <param name="orava">kuka hyppää</param>
    /// <param name="nopeus">millä nopeudella hyppää</param>
    private void Hyppaa(PlatformCharacter orava, double nopeus)
    {
        orava.Jump(nopeus);
    }


    /// <summary>
    /// Käsittelee pelaajan ja pähkinän törmäyksen.
    /// </summary>
    /// <param name="hahmo">kuka törmää</param>
    /// <param name="pahkina">mihin törmää</param>
    private void TormaaPahkinaan(PhysicsObject hahmo, PhysicsObject pahkina)
    {
        SoundEffect kerays = LoadSoundEffect("collect1.wav");
        kerays.Play();
        MessageDisplay.Add("Sait kaksi uutta pähkinää");
        pahkina.Destroy();
        orava.Weapon.Ammo.Value += 2;
        pisteLaskuri.Value += 2;
    }       


    /// <summary>
    /// Luo uuden laskurin joka näyttää pelaajalle pähkinöiden määrän ruudulla.
    /// </summary>
    private void PähkinäLaskuri()
    {
        pisteLaskuri = new IntMeter(2);
        Label pisteNaytto = new Label();
        pisteNaytto.X = Screen.Right - 100;
        pisteNaytto.Y = Screen.Top - 50;
        pisteNaytto.TextColor = Color.Black;
        pisteNaytto.Color = Color.JungleGreen;
        pisteNaytto.BindTo(pisteLaskuri);
        pisteNaytto.Title = "Pähkinöitä";
        Add(pisteNaytto);
    }

    private void Alusta()
    {
        orava.Destroy();
        ClearGameObjects();
        ClearTimers();
        Camera.Reset();
        Alkuvalikko();
    }
}