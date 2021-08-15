//Config
 string Tag = "IPanel";

 string SpeedUnit = "kmph"; //"kmph", "miph", "mps" or "knots"
 string AltUnit = "m"; //"m" or "ft"

 int SpecialMode = 0; //0 Normal, 1 Fighter Cockpit Main Screen

 float MaxAcceleration = 0; // Max acceleration for the Acceleration Meter in m/s

// constants

// Amber
Color ColorA = new Color(231, 194, 81);
Color ColorB = new Color(231, 174, 0);
// Blue
// Color ColorA = new Color(135, 206, 235);
// Color ColorB = new Color(20, 144, 255);

 //Script

 float lastAcc = 0;

 int Timer = 0;

 bool Electric = false;

 float a = 0;
 float b = 0;
 float c = 0;

 float desiredSpeed = 0;

 float LastSpeed = 0;

 IMyShipController ShipController = null;
 IMyTextSurfaceProvider SpeedLCD = null;
 int LCDID = -1;
 int setupLCDID = 0;
 string image;
 double MaxLCDSpeed = 100;

 int settings = -1;

 bool trip = false;

 float ODO = 0;
 float Trip = 0;

 //----
 string SpeedSuffix;
 float speedConversion;

 string AltSuffix;
 float AltConversion;

 //---
 string tx = null;
 ContentType ct = ContentType.TEXT_AND_IMAGE;
 Color bg = Color.White;
 string fi = null;
 float fs = 0;
 List<string> im = new List<string>();
 Color fc = Color.White;
 TextAlignment ta = TextAlignment.CENTER;

 public Program()
 {
     if (Me.CustomData != "")
     {
         if (!float.TryParse(Me.CustomData.Split('\n')[1], out ODO)) ODO = 0;
     }

     Runtime.UpdateFrequency = UpdateFrequency.Update1;
     MaxLCDSpeed *= 2;

     switch (SpeedUnit)
     {
         case "kmph":
             speedConversion = 3.6f;
             SpeedSuffix = "km/h";
             break;
         case "miph":
             speedConversion = 2.23694f;
             SpeedSuffix = "mi/h";
             break;
         case "mps":
             speedConversion = 1f;
             SpeedSuffix = "m/s/h";
             break;
         case "knots":
             speedConversion = 1.94384f;
             SpeedSuffix = "kt";
             break;
         default:
             speedConversion = 0f;
             SpeedSuffix = "fucks";
             break;
     }

     switch (AltUnit)
     {
         case "m":
             AltConversion = 1f;
             AltSuffix = "m";
             break;
         case "ft":
             AltConversion = 3f;
             AltSuffix = "ft";
             break;
         default:
             AltConversion = 0f;
             AltSuffix = "fucks";
             break;
     }
 }

 void Main(string argument)
 {
   if (argument.ToLower().StartsWith("lcd"))
   {
       int _LCDid = -1;
       string nid = argument.Substring(4);
       if (int.TryParse(nid, out _LCDid))
       {
           LCDID = _LCDid;
       }
   }

     if (ShipController == null || SpeedLCD == null)
     {

         if (ShipController == null)
         {

             Echo("Setup Assistant : Ship Controller\n");
             Echo("The script is now searching for a ship controller (cockpit, flight seat, etc)");
             Echo("Please take a seat in any ship controller (or press control on a remote control)");
             ShipController = getController();

         }
         if (SpeedLCD == null)
         {

             Echo("Setup Assistant : LCD Block\n");
             Echo("The script is now searching for a Block with LCD's that has the tag " + Tag +
                 "either in its name or custom data.");
             Echo("Please put that tag into the name or custom data of the Block you want this to be shown on (cockpit / LCD / Text panel) you want to use");
             SpeedLCD = getLCD(Tag);

         }

     }
     else
     {

         if (LCDID == -1)
         {
             if (Me.CustomData != "")
             {
                 int _LCDid = -1;
                 string LCDIDs = Me.CustomData.Split('\n')[0];
                 if (!int.TryParse(LCDIDs, out _LCDid)) _LCDid = -1;
                 if (_LCDid != -1)
                 {
                     LCDID = _LCDid;
                     return;
                 }

             }
             IMyTextSurface surface = SpeedLCD.GetSurface(setupLCDID);
             if (surface == null) { setupLCDID = 0; return; }
             if (tx == null)
             {

                 ct = surface.ContentType;
                 bg = surface.BackgroundColor;
                 fi = surface.Font;
                 fs = surface.FontSize;
                 tx = surface.GetText();
                 im = new List<string>();
                 surface.GetSelectedImages(im);
                 fc = surface.FontColor;
                 ta = surface.Alignment;
                 surface.ContentType = ContentType.TEXT_AND_IMAGE;
                 surface.FontSize = 2;
                 surface.BackgroundColor = Color.DarkGreen;
                 surface.FontColor = Color.LightGreen;
                 surface.WriteText("\n\n\nHere?");
                 surface.ClearImagesFromSelection();
                 surface.Alignment = TextAlignment.CENTER;
                 surface.Font = "Debug";
             }
             Echo("Setup Assistant : LCD Index\n");
             Echo("You can now select the LCD");
             Echo("An image is now displayed on the LCD");
             Echo("Write into Argument and click run:");
             Echo("\"n\" for next");
             Echo("\"p\" for previous");
             Echo("\"ok\" for ... well... ok");
             if (argument.ToLower().Contains("n"))
             {
                 surface.ContentType = ct;
                 surface.FontSize = fs;
                 surface.BackgroundColor = bg;
                 surface.FontColor = fc;
                 surface.WriteText(tx);
                 surface.AddImagesToSelection(im);
                 surface.Alignment = ta;
                 surface.Font = fi;

                 tx = null;

                 setupLCDID++;

             }

             if (argument.ToLower().Contains("p"))
             {
                 surface.ContentType = ct;
                 surface.FontSize = fs;
                 surface.BackgroundColor = bg;
                 surface.FontColor = fc;
                 surface.WriteText(tx);
                 surface.AddImagesToSelection(im);
                 surface.Alignment = ta;
                 surface.Font = fi;

                 tx = null;

                 setupLCDID--;

             }

             if (argument.ToLower().Contains("ok"))
             {
                 LCDID = setupLCDID;
                 return;

             }

         }
         else
         {
             Me.CustomData = LCDID.ToString() + "\n" + ODO.ToString();
             Echo("Setup Assistant : Done\n");
             Echo("Setup is done, you are now set to go");

             if (argument.ToLower().Contains("cc"))
             {
                 if (desiredSpeed == 0) desiredSpeed = (float)ShipController.GetShipSpeed();
                 else desiredSpeed = 0;
             }

             if (argument.ToLower().Contains("rtrip")) Trip = ODO;
             else if (argument.ToLower().Contains("trip")) trip = !trip;

             if (!ShipController.IsUnderControl)
             {
                 Timer = 121;
             }
             else if (Timer > 0) { Timer--; }

             if (2 * ShipController.GetShipSpeed() >= MaxLCDSpeed)
             {
                 MaxLCDSpeed = 2 * ShipController.GetShipSpeed();
             }


             float minA = (float)Math.PI / 2;
             float maxA = -(float)Math.PI / 2;
             if (SpecialMode == 1)
             {
                 minA = 1.53f;
                 maxA = -1.53f;
             }
             float minB = (float)Math.PI;
             float maxB = 0;
             float minC = (float)Math.PI;
             float maxC = 0;

             if (Timer == 0)
             {
                 float fuelLevel = FuelLevel(out Electric);
                 a = lerp(minA, maxA, (float)(ShipController.GetShipSpeed() / MaxLCDSpeed * 2));
                 b = lerp(minB, maxB, (float)(Acc() / MaxAcceleration * 2));
                 c = 2 * (float)Math.PI - lerp(minC, maxC, fuelLevel);
             }
             else if (Timer <= 60)
             {
                 a = lerp(minA, maxA, (float)Timer / 60f);
                 b = a + (float)Math.PI / 2;
                 c = 2 * (float)Math.PI - b;
             }
             else
             {
                 a = lerp(minA, maxA, (60f - ((float)Timer - 60f)) / 60f);
                 b = a + (float)Math.PI / 2;
                 c = 2 * (float)Math.PI - b;
             }


             SpeedLCD.GetSurface(LCDID).ContentType = ContentType.SCRIPT;
             SpeedLCD.GetSurface(LCDID).Script = "empty";
             SpeedLCD.GetSurface(LCDID).ScriptBackgroundColor = Color.Black;
             SpeedLCD.GetSurface(LCDID).PreserveAspectRatio = false;
             using (MySpriteDrawFrame spriteFrame = SpeedLCD.GetSurface(LCDID).DrawFrame())
             {
                 IMyTextSurface Surface = SpeedLCD.GetSurface(LCDID);
                 Vector2 Size = new Vector2(Surface.TextureSize.X, Surface.TextureSize.X);
                 float rel = Size.X / 256f;
                 float relY = Surface.TextureSize.Y / 182.8571f;
                 Vector2 Center = new Vector2(Surface.TextureSize.X / 2, Surface.TextureSize.Y - Surface.TextureSize.X * 0.25f);



                 Vector2 SIPos = Center + new Vector2((float)Math.Sin(a + Math.PI) * Size.X * 0.355f, (float)Math.Cos(a + Math.PI) * Size.Y * 0.355f);


                 Vector2[] STicksPos = new Vector2[] {
                 Center + new Vector2((float)Math.Sin(-Math.PI / 2 - Math.PI / 10) * Size.X * 0.425f, (float)Math.Cos(-Math.PI / 2 - Math.PI / 10) * Size.Y * 0.425f),
                 Center + new Vector2((float)Math.Sin(-Math.PI / 2 - 2 * Math.PI / 10) * Size.X * 0.425f, (float)Math.Cos(-Math.PI / 2 - 2 * Math.PI / 10) * Size.Y * 0.425f),
                 Center + new Vector2((float)Math.Sin(-Math.PI / 2 - 3 * Math.PI / 10) * Size.X * 0.425f, (float)Math.Cos(-Math.PI / 2 - 3 * Math.PI / 10) * Size.Y * 0.425f),
                 Center + new Vector2((float)Math.Sin(-Math.PI / 2 - 4 * Math.PI / 10) * Size.X * 0.425f, (float)Math.Cos(-Math.PI / 2 - 4 * Math.PI / 10) * Size.Y * 0.425f),
                 Center + new Vector2((float)Math.Sin(-Math.PI / 2 - 5 * Math.PI / 10) * Size.X * 0.425f, (float)Math.Cos(-Math.PI / 2 - 5 * Math.PI / 10) * Size.Y * 0.425f),
                 Center + new Vector2((float)Math.Sin(-Math.PI / 2 - 6 * Math.PI / 10) * Size.X * 0.425f, (float)Math.Cos(-Math.PI / 2 - 6 * Math.PI / 10) * Size.Y * 0.425f),
                 Center + new Vector2((float)Math.Sin(-Math.PI / 2 - 7 * Math.PI / 10) * Size.X * 0.425f, (float)Math.Cos(-Math.PI / 2 - 7 * Math.PI / 10) * Size.Y * 0.425f),
                 Center + new Vector2((float)Math.Sin(-Math.PI / 2 - 8 * Math.PI / 10) * Size.X * 0.425f, (float)Math.Cos(-Math.PI / 2 - 8 * Math.PI / 10) * Size.Y * 0.425f),
                 Center + new Vector2((float)Math.Sin(-Math.PI / 2 - 9 * Math.PI / 10) * Size.X * 0.425f, (float)Math.Cos(-Math.PI / 2 - 9 * Math.PI / 10) * Size.Y * 0.425f)
             };


                 //Acceleration Meter


                 Vector2 AccPos = Surface.TextureSize / 2 + new Vector2(115 * rel, -32 * relY);

                 MySprite ACCcircle1 = MySprite.CreateSprite("SemiCircle", AccPos, new Vector2(Size.X, Size.Y) * 0.9f / 3f);
                 ACCcircle1.Color = new Color(40, 40, 40);
                 ACCcircle1.RotationOrScale = -(float)Math.PI / 2;
                 spriteFrame.Add(ACCcircle1);
                 MySprite ACCcircle2 = MySprite.CreateSprite("SemiCircle", AccPos, new Vector2(Size.X, Size.Y) * 0.8f / 3f);
                 ACCcircle2.Color = Color.Black;
                 ACCcircle2.RotationOrScale = -(float)Math.PI / 2;
                 spriteFrame.Add(ACCcircle2);
                 MySprite ACCcircle3 = MySprite.CreateSprite("SemiCircle", AccPos, new Vector2(Size.X, Size.Y) * 0.75f / 3f);
                 ACCcircle3.Color = new Color(40, 40, 40);
                 ACCcircle3.RotationOrScale = -(float)Math.PI / 2;
                 spriteFrame.Add(ACCcircle3);
                 MySprite ACCcircle4 = MySprite.CreateSprite("Circle", AccPos, new Vector2(Size.X, Size.Y) * 0.73f / 3f);
                 ACCcircle4.Color = Color.Black;
                 ACCcircle4.RotationOrScale = -(float)Math.PI / 2;
                 spriteFrame.Add(ACCcircle4);

                 Vector2 AIPos = AccPos + new Vector2((float)Math.Sin(b + Math.PI) * Size.X * 0.355f / 3f, (float)Math.Cos(b + Math.PI) * Size.Y * 0.355f / 3f);

                 MySprite AI = MySprite.CreateSprite("SquareSimple", AIPos, new Vector2(1f, 20f / 3 * rel));
                 AI.RotationOrScale = -b;
                 AI.Color = Color.Red;
                 spriteFrame.Add(AI);

                 //Fuel Meter
                 Vector2 FuelPos = Surface.TextureSize / 2 + new Vector2(-115 * rel, -32 * relY);

                 MySprite Fuelcircle1 = MySprite.CreateSprite("SemiCircle", FuelPos, new Vector2(Size.X, Size.Y) * 0.9f / 3f);
                 Fuelcircle1.Color = new Color(40, 40, 40);
                 Fuelcircle1.RotationOrScale = (float)Math.PI / 2;
                 spriteFrame.Add(Fuelcircle1);
                 MySprite Fuelcircle2 = MySprite.CreateSprite("SemiCircle", FuelPos, new Vector2(Size.X, Size.Y) * 0.8f / 3f);
                 Fuelcircle2.Color = Color.Black;
                 Fuelcircle2.RotationOrScale = (float)Math.PI / 2;
                 spriteFrame.Add(Fuelcircle2);
                 MySprite Fuelcircle3 = MySprite.CreateSprite("SemiCircle", FuelPos, new Vector2(Size.X, Size.Y) * 0.75f / 3f);
                 Fuelcircle3.Color = new Color(40, 40, 40);
                 Fuelcircle3.RotationOrScale = (float)Math.PI / 2;
                 spriteFrame.Add(Fuelcircle3);
                 MySprite Fuelcircle4 = MySprite.CreateSprite("Circle", FuelPos, new Vector2(Size.X, Size.Y) * 0.73f / 3f);
                 Fuelcircle4.Color = Color.Black;
                 Fuelcircle4.RotationOrScale = (float)Math.PI / 2;
                 spriteFrame.Add(Fuelcircle4);

                 Vector2 FIPos = FuelPos + new Vector2((float)Math.Sin(c + Math.PI) * Size.X * 0.355f / 3f, (float)Math.Cos(c + Math.PI) * Size.Y * 0.355f / 3f);

                 MySprite FI = MySprite.CreateSprite("SquareSimple", FIPos, new Vector2(1f, 20f / 3 * rel));
                 FI.RotationOrScale = -c;
                 FI.Color = Color.Yellow;
                 if (Electric) FI.Color = Color.Blue;
                 spriteFrame.Add(FI);

                 //Speedometer Background
                 MySprite circle1 = MySprite.CreateSprite("SemiCircle", Center, new Vector2(Size.X, Size.Y) * 0.9f);
                 circle1.Color = new Color(40, 40, 40);
                 spriteFrame.Add(circle1);
                 MySprite circle2 = MySprite.CreateSprite("SemiCircle", Center, new Vector2(Size.X, Size.Y) * 0.8f);
                 circle2.Color = Color.Black;
                 spriteFrame.Add(circle2);
                 MySprite circle3 = MySprite.CreateSprite("SemiCircle", Center, new Vector2(Size.X, Size.Y) * 0.75f);
                 circle3.Color = new Color(40, 40, 40);
                 spriteFrame.Add(circle3);
                 MySprite circle4 = MySprite.CreateSprite("Circle", Center, new Vector2(Size.X, Size.Y) * 0.73f);
                 circle4.Color = Color.Black;
                 spriteFrame.Add(circle4);


                 MySprite SI = MySprite.CreateSprite("SquareSimple", SIPos, new Vector2(2, 20f * rel));
                 SI.RotationOrScale = -a;
                 SI.Color = Color.Red;
                 spriteFrame.Add(SI);

                 //Speed Ticks (every 18 degrees (10 m/s))
                 MySprite S10I = MySprite.CreateSprite("SquareSimple", STicksPos[0], new Vector2(1, 12f * rel));
                 S10I.Color = new Color(60, 60, 60);
                 S10I.RotationOrScale = -(float)(-Math.PI / 2 - Math.PI / 10);
                 spriteFrame.Add(S10I);
                 MySprite S20I = MySprite.CreateSprite("SquareSimple", STicksPos[1], new Vector2(1, 12f * rel));
                 S20I.Color = new Color(60, 60, 60);
                 S20I.RotationOrScale = -(float)(-Math.PI / 2 - 2 * Math.PI / 10);
                 spriteFrame.Add(S20I);
                 MySprite S30I = MySprite.CreateSprite("SquareSimple", STicksPos[2], new Vector2(1, 12f * rel));
                 S30I.Color = new Color(60, 60, 60);
                 S30I.RotationOrScale = -(float)(-Math.PI / 2 - 3 * Math.PI / 10);
                 spriteFrame.Add(S30I);
                 MySprite S40I = MySprite.CreateSprite("SquareSimple", STicksPos[3], new Vector2(1, 12f * rel));
                 S40I.Color = new Color(60, 60, 60);
                 S40I.RotationOrScale = -(float)(-Math.PI / 2 - 4 * Math.PI / 10);
                 spriteFrame.Add(S40I);
                 MySprite S50I = MySprite.CreateSprite("SquareSimple", STicksPos[4], new Vector2(1, 12f * rel));
                 S50I.Color = new Color(60, 60, 60);
                 S50I.RotationOrScale = -(float)(-Math.PI / 2 - 5 * Math.PI / 10);
                 spriteFrame.Add(S50I);
                 MySprite S60I = MySprite.CreateSprite("SquareSimple", STicksPos[5], new Vector2(1, 12f * rel));
                 S60I.Color = new Color(60, 60, 60);
                 S60I.RotationOrScale = -(float)(-Math.PI / 2 - 6 * Math.PI / 10);
                 spriteFrame.Add(S60I);
                 MySprite S70I = MySprite.CreateSprite("SquareSimple", STicksPos[6], new Vector2(1, 12f * rel));
                 S70I.Color = new Color(60, 60, 60);
                 S70I.RotationOrScale = -(float)(-Math.PI / 2 - 7 * Math.PI / 10);
                 spriteFrame.Add(S70I);
                 MySprite S80I = MySprite.CreateSprite("SquareSimple", STicksPos[7], new Vector2(1, 12f * rel));
                 S80I.Color = new Color(60, 60, 60);
                 S80I.RotationOrScale = -(float)(-Math.PI / 2 - 8 * Math.PI / 10);
                 spriteFrame.Add(S80I);
                 MySprite S90I = MySprite.CreateSprite("SquareSimple", STicksPos[8], new Vector2(1, 12f * rel));
                 S90I.Color = new Color(60, 60, 60);
                 S90I.RotationOrScale = -(float)(-Math.PI / 2 - 9 * Math.PI / 10);
                 spriteFrame.Add(S90I);





                 MySprite SpeedTexts = MySprite.CreateText(this.SpeedText(), "Debug", Color.Orange, Size.X / 250, TextAlignment.CENTER);
                 MySprite SpeedSuffixs = MySprite.CreateText(this.SpeedSuffix, "Debug", Color.Orange, Size.X / 700, TextAlignment.CENTER);
                 SpeedTexts.Position = Surface.TextureSize / 2 + new Vector2(0, Surface.TextureSize.Y / 4 - 40f * relY);
                 SpeedSuffixs.Position = Surface.TextureSize / 2 + new Vector2(35f * rel, Surface.TextureSize.Y / 4 + 12f * relY - 40f * relY);
                 SpeedTexts.Color = ColorA;
                 SpeedSuffixs.Color = ColorB;
                 ODO += (float)ShipController.GetShipSpeed();
                 MySprite ODOSprite;
                 if (trip)
                 {
                     ODOSprite = MySprite.CreateText("TRIP " + ((ODO - Trip) * 3.6 / 216000).ToString("000000.0") + "km", "Debug", ColorB, Size.X / 600, TextAlignment.CENTER);
                 }
                 else
                 {
                     ODOSprite = MySprite.CreateText("ODO " + ((ODO) * 3.6 / 210000).ToString("000000.0") + "km", "Debug", ColorB, Size.X / 600, TextAlignment.CENTER);
                 }
                 ODOSprite.Color = new Color(60, 60, 60);
                 ODOSprite.Position = Size / 2 + new Vector2(0, 40 * relY);
                 spriteFrame.Add(ODOSprite);

                 Vector2 HLPos = Size / 2 + new Vector2(-31 * rel, 0);

                 if (Timer == 0)
                 {
                     MySprite HL1 = MySprite.CreateSprite("SemiCircle", HLPos - new Vector2(2 * rel, 0), new Vector2(10 * rel, 10 * rel));
                     MySprite HL2 = MySprite.CreateSprite("SquareSimple", HLPos + new Vector2(3f * rel, 0), new Vector2(5f * rel, 1 * rel));
                     MySprite HL3 = MySprite.CreateSprite("SquareSimple", HLPos + new Vector2(3f * rel, -4 * rel), new Vector2(5f * rel, 1 * rel));
                     MySprite HL4 = MySprite.CreateSprite("SquareSimple", HLPos + new Vector2(3f * rel, 4 * rel), new Vector2(5f * rel, 1 * rel));
                     HL1.Color = new Color(60, 60, 60);
                     HL2.Color = new Color(60, 60, 60);
                     HL3.Color = new Color(60, 60, 60);
                     HL4.Color = new Color(60, 60, 60);
                     if (HL())
                     {
                         HL1.Color = Color.Green;
                         HL2.Color = Color.Green;
                         HL3.Color = Color.Green;
                         HL4.Color = Color.Green;
                     }
                     HL1.RotationOrScale = -(float)Math.PI / 2;
                     spriteFrame.Add(HL1);
                     spriteFrame.Add(HL2);
                     spriteFrame.Add(HL3);
                     spriteFrame.Add(HL4);

                     Vector2 PBPos = Size / 2 + new Vector2(-10 * rel, 0);

                     MySprite PB1 = MySprite.CreateSprite("CircleHollow", PBPos, new Vector2(12 * rel, 12 * rel));
                     MySprite PB2 = MySprite.CreateText("P", "Debug", new Color(60, 60, 60), Size.X / 750, TextAlignment.CENTER);
                     PB2.Position = PBPos + new Vector2(0, -4f * rel);
                     PB1.Color = new Color(60, 60, 60);
                     if (ShipController.HandBrake)
                     {
                         PB1.Color = Color.Red;
                         PB2.Color = Color.Red;
                     }
                     spriteFrame.Add(PB1);
                     spriteFrame.Add(PB2);
                 }

                 if (desiredSpeed != 0)
                 {
                     SpeedTexts.Color = new Color(102, 255, 102);
                 }
                 if (Timer == 0 & ShipController.GetNaturalGravity().LengthSquared() != 0)
                 {
                     MySprite TimeBorder = MySprite.CreateSprite("AH_TextBox", Surface.TextureSize / 2 + new Vector2(65f * rel, 62f * rel), new Vector2(32f * rel, 14f * rel));
                     TimeBorder.Color = new Color(60, 60, 60);
                     spriteFrame.Add(TimeBorder);

                     spriteFrame.Add(SpeedTexts);
                     spriteFrame.Add(SpeedSuffixs);

                     MySprite TimeText = MySprite.CreateText(this.HourText(), "Debug", Color.Orange, Size.X / 580, TextAlignment.CENTER);
                     TimeText.Position = Surface.TextureSize / 2 + new Vector2(65f * rel, 55f * rel);
                     TimeText.Color = ColorB;
                     spriteFrame.Add(TimeText);

                     MySprite PowerOverloadInd = MySprite.CreateSprite("IconEnergy", Surface.TextureSize / 2 + new Vector2(-35f * rel, Surface.TextureSize.Y / 4 - 28f * relY), new Vector2(20f * rel, 20f * rel));
                     PowerOverloadInd.Color = new Color(60, 60, 60);
                     if (GetPowerPercentage() > .8f) PowerOverloadInd.Color = new Color(225, 225, 102);
                     if (GetPowerPercentage() > .9f) PowerOverloadInd.Color = new Color(120, 60, 60);
                     spriteFrame.Add(PowerOverloadInd);

                     MySprite CargoCapacityBG = MySprite.CreateSprite("SquareSimple", Surface.TextureSize / 2 + new Vector2(0 * rel, 30 * relY), new Vector2(30 * rel, 7 * rel));
                     CargoCapacityBG.Color = new Color(60, 60, 60);
                     spriteFrame.Add(CargoCapacityBG);
                     MySprite CargoCapacityBar = MySprite.CreateSprite("SquareSimple", Surface.TextureSize / 2 + new Vector2(0 * rel, 30 * relY), new Vector2(30f * rel * MassPerc(), 7 * rel));
                     CargoCapacityBar.Color = ColorB;
                     spriteFrame.Add(CargoCapacityBar);


                 }
                 else if (Timer == 0)
                 {
                     SpeedTexts = MySprite.CreateText("Ship outside of\nGravity\nThis script is solely\nintended for use on Planets", "Debug", Color.Orange, Size.X / 500, TextAlignment.CENTER);
                     spriteFrame.Add(SpeedTexts);
                 }
                 else if (Timer < 121)
                 {
                     MySprite EntryTitle = MySprite.CreateText(EntryText(), "Debug", Color.Orange, Size.X / 600, TextAlignment.CENTER);
                     EntryTitle.Position = Surface.TextureSize / 2 + new Vector2(0, Surface.TextureSize.Y / 4 - 45f * relY);
                     EntryTitle.Color = new Color(60, 60, 60);
                     spriteFrame.Add(EntryTitle);
                     //MySprite VehicleWheight = MySprite.CreateText(MassText() + " Cargo", "Debug", Color.Orange, Size.X / 600, TextAlignment.CENTER);
                     //VehicleWheight.Position = Surface.TextureSize / 2 + new Vector2(5f * rel, Surface.TextureSize.Y / 4 - 20f * relY);
                     //VehicleWheight.Color = new Color(100, 142, 209);
                     //spriteFrame.Add(VehicleWheight); Needs to be moved
                 }

                 List<IMyMotorSuspension> wheels = new List<IMyMotorSuspension>();
                 GridTerminalSystem.GetBlocksOfType(wheels);
                 if (desiredSpeed != 0)
                 {
                     if (ShipController.MoveIndicator.Z != 0) desiredSpeed = 0;



                     float error = desiredSpeed - (float)ShipController.GetShipSpeed();
                     float p = error * 0.3f;
                     foreach (IMyMotorSuspension w in wheels)
                     {
                         if (w.WorldMatrix.Left == ShipController.WorldMatrix.Forward)
                             (w).SetValue("Propulsion override", p);
                         else if (w.WorldMatrix.Right == ShipController.WorldMatrix.Forward)
                             (w).SetValue("Propulsion override", -p);
                     }
                 }
                 else
                 {
                     foreach (IMyMotorSuspension w in wheels)
                     {
                         (w).SetValue("Propulsion override", (float)0);
                     }
                 }


             }
         }
     }

 }

 float GetPowerPercentage()
 {
     List<IMyPowerProducer> powerProducers = new List<IMyPowerProducer>();
     GridTerminalSystem.GetBlocksOfType(powerProducers);

     if (powerProducers.Count == 0) return 0;
     float Max = 0;
     float Current = 0;
     foreach (IMyPowerProducer pp in powerProducers)
     {
         if (pp.Enabled & pp.IsFunctional)
         {
             Max += pp.MaxOutput;
             Current += pp.CurrentOutput;
         }
     }
     float percentage = Current / Max;
     return percentage;
 }

 IMyShipController getController()
 {

     List<IMyShipController> controllers = new List<IMyShipController>();
     GridTerminalSystem.GetBlocksOfType(controllers);

     foreach (IMyShipController c in controllers)
     {
         if (c.CanControlShip & c.IsUnderControl)
             return c;
     }

     return null;

 }

 IMyTextSurfaceProvider getLCD(string Tag)
 {

     List<IMyTextSurfaceProvider> lcds = new List<IMyTextSurfaceProvider>();
     GridTerminalSystem.GetBlocksOfType(lcds);

     foreach (IMyTextSurfaceProvider tp in lcds)
     {
         if ((tp as IMyTerminalBlock).CustomData.Contains(Tag) | (tp as IMyTerminalBlock).CustomName.Contains(Tag))
             return tp;
     }

     return null;

 }

 bool IdleStateFull(IMyShipController sc, float FontSize, UpdateFrequency uf, IMyTextPanel tp, Color c)
 {
     return false; // will contain the mikrsoft logo at some point
 }


 float lerp(float x, float y, float p)
 {
     return x + (y - x) * p;
 }

 string SpeedText()
 {
     float speed = (float)ShipController.GetShipSpeed();
     return (speed * speedConversion).ToString("000");
 }

 string HourText()
 {

     StringBuilder s = new StringBuilder();

     s.Append(DateTime.Now.Hour.ToString("00") + ":");
     s.Append(DateTime.Now.Minute.ToString("00"));

     return s.ToString();
 }


 bool between(double var, double min, double max)
 {
     if (min < var & var < max) return true;
     return false;
 }

 float MassPerc()
 {

     float maxCap = 0;
     float curCap = 0;

     List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
     GridTerminalSystem.GetBlocks(blocks);

     foreach (IMyTerminalBlock t in blocks)
     {
         if (t.HasInventory)
         {
             maxCap += t.GetInventory().MaxVolume.RawValue;
             curCap += t.GetInventory().CurrentVolume.RawValue;

         }
     }

     float VolumePerc = curCap / maxCap;

     return VolumePerc;


 }

 string EntryText()
 {
     return "Welcome, it is " + DateTime.Now.ToShortTimeString();

 }

 bool HL()
 {
     List<IMyReflectorLight> Headlights = new List<IMyReflectorLight>();
     GridTerminalSystem.GetBlocksOfType(Headlights);
     for (int i = Headlights.Count - 1; i > 0; i--)
     {
         if (Headlights[i].WorldMatrix.Forward == ShipController.WorldMatrix.Forward)
         {
             if (Headlights[i].Enabled)
                 return true;
             else
                 continue;
         }
     }
     return false;
 }

 float Acc()
 {
     float SpeedDifferential = (float)ShipController.GetShipSpeed() - LastSpeed;
     float Acceleration = Math.Abs(SpeedDifferential) / (float)Runtime.TimeSinceLastRun.TotalSeconds;
     Acceleration = (Acceleration - lastAcc) * .9f + lastAcc;
     LastSpeed = (float)ShipController.GetShipSpeed();
     lastAcc = (float)Acceleration;
     if (Acceleration > MaxAcceleration) MaxAcceleration = Acceleration;
     return Acceleration;
 }

 float FuelLevel(out bool Electric)
 {
     List<IMyGasTank> hTanks = new List<IMyGasTank>();
     GridTerminalSystem.GetBlocksOfType(hTanks);
     List<IMyReactor> reactors = new List<IMyReactor>();
     GridTerminalSystem.GetBlocksOfType(reactors);
     List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
     GridTerminalSystem.GetBlocksOfType(batteries);

     for (int i = hTanks.Count - 1; i > 0; i--)
     {
         if (!hTanks[i].Name.ToLower().Contains("hydro")) hTanks.RemoveAt(i);
     }

     float UraniumConversionRate = 0.001272f;
     float HydrogenConversionRate = 0.001111f;

     float FuelCapacity = 0;
     float FuelLevel = 0;
     foreach (IMyReactor r in reactors)
     {
         if (r.GetInventory().CurrentVolume.RawValue != 0)
         {
             FuelCapacity += r.GetInventory().CurrentMass.RawValue /
                             r.GetInventory().CurrentVolume.RawValue *
                             r.GetInventory().MaxVolume.RawValue * UraniumConversionRate;
         }
         FuelLevel += r.GetInventory().CurrentMass.RawValue * UraniumConversionRate;
     }
     foreach (IMyGasTank t in hTanks)
     {
         FuelCapacity += t.Capacity * HydrogenConversionRate;
         FuelLevel += (float)t.FilledRatio * t.Capacity * HydrogenConversionRate;
     }
     Electric = FuelLevel == 0;
     foreach (IMyBatteryBlock b in batteries)
     {
         FuelCapacity += b.MaxStoredPower;
         FuelLevel += b.CurrentStoredPower;
     }
     return FuelLevel / FuelCapacity;

 }
