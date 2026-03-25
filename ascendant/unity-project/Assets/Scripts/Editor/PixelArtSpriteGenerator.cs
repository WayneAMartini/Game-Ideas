#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Ascendant.Editor
{
    /// <summary>
    /// Generates detailed pixel art sprites for heroes and enemies,
    /// replacing placeholder colored shapes with recognizable characters.
    /// </summary>
    public static class PixelArtSpriteGenerator
    {
        [MenuItem("Ascendant/Generate Pixel Art")]
        public static void GenerateAll()
        {
            Gen("Assets/Art/Sprites/Heroes/Warrior.png", 128, DrawWarrior);
            Gen("Assets/Art/Sprites/Heroes/Mage.png", 128, DrawMage);
            Gen("Assets/Art/Sprites/Heroes/Priest.png", 128, DrawPriest);
            Gen("Assets/Art/Sprites/Heroes/Rogue.png", 128, DrawRogue);
            Gen("Assets/Art/Sprites/Enemies/Slime.png", 96, DrawSlime);
            Gen("Assets/Art/Sprites/Enemies/Goblin.png", 96, DrawGoblin);
            Gen("Assets/Art/Sprites/Enemies/Wolf.png", 96, DrawWolf);
            AssetDatabase.Refresh();
            Debug.Log("[PixelArt] All pixel art sprites generated!");
        }

        [MenuItem("Ascendant/Generate Pixel Art And Wire")]
        public static void GenerateAndWire()
        {
            GenerateAll();
            EditorTools.SceneFixup.FixEverything();
            Debug.Log("[PixelArt] Sprites generated and scene wired!");
        }

        static void Gen(string path, int size, System.Action<Texture2D> draw)
        {
            var t = new Texture2D(size, size, TextureFormat.RGBA32, false);
            t.filterMode = FilterMode.Point;
            Clear(t);
            draw(t);
            AddOutline(t, new Color(0.12f, 0.08f, 0.12f, 1f), 2);
            t.Apply();
            string full = Path.Combine(Application.dataPath, "..", path);
            string dir = Path.GetDirectoryName(full);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllBytes(full, t.EncodeToPNG());
            Object.DestroyImmediate(t);
            Debug.Log($"[PixelArt] {path}");
        }

        // ================================================================
        // WARRIOR - Armored knight with sword and shield
        // ================================================================
        static void DrawWarrior(Texture2D t)
        {
            Color aD = C(80, 80, 100);      // armor dark
            Color aM = C(130, 130, 155);     // armor medium
            Color aL = C(180, 180, 200);     // armor light
            Color gd = C(220, 180, 50);      // gold
            Color gL = C(240, 200, 70);      // gold light
            Color fl = C(235, 195, 155);     // flesh
            Color fD = C(195, 155, 115);     // flesh shadow
            Color br = C(110, 75, 40);       // brown
            Color bD = C(75, 50, 25);        // brown dark
            Color rd = C(175, 35, 35);       // red cape
            Color rD = C(130, 25, 25);       // red dark
            Color sw = C(200, 210, 225);     // sword silver
            Color sH = C(230, 240, 250);     // sword highlight
            Color bl = C(45, 75, 155);       // shield blue
            Color bL = C(65, 100, 180);      // shield blue light
            Color ey = C(45, 65, 85);        // eye color

            // Cape behind body
            FillRect(t, 30, 14, 14, 56, rD);
            FillRect(t, 84, 14, 14, 56, rD);
            FillRect(t, 32, 16, 10, 52, rd);
            FillRect(t, 86, 16, 10, 52, rd);
            FillTri(t, 30, 14, 44, 14, 30, 4, rD);
            FillTri(t, 84, 14, 98, 14, 98, 4, rD);

            // Boots
            FillRect(t, 42, 4, 18, 12, bD);
            FillRect(t, 68, 4, 18, 12, bD);
            FillRect(t, 44, 6, 14, 8, br);
            FillRect(t, 70, 6, 14, 8, br);
            FillRect(t, 46, 8, 4, 4, C(140, 100, 55));
            FillRect(t, 72, 8, 4, 4, C(140, 100, 55));

            // Legs (greaves)
            FillRect(t, 44, 14, 16, 24, aD);
            FillRect(t, 68, 14, 16, 24, aD);
            FillRect(t, 46, 16, 12, 20, aM);
            FillRect(t, 70, 16, 12, 20, aM);
            FillRect(t, 46, 30, 12, 6, aL);
            FillRect(t, 70, 30, 12, 6, aL);

            // Belt
            FillRect(t, 38, 36, 52, 6, br);
            FillRect(t, 56, 35, 16, 8, gd);
            FillRect(t, 58, 36, 12, 6, gL);

            // Torso chest armor
            FillRect(t, 38, 42, 52, 36, aD);
            FillRect(t, 40, 44, 48, 32, aM);
            FillRect(t, 44, 48, 40, 24, aL);
            // Golden cross emblem
            FillRect(t, 60, 50, 8, 20, gd);
            FillRect(t, 50, 58, 28, 4, gd);
            // Shoulder pauldrons
            FillEllipse(t, 38, 72, 10, 8, aD);
            FillEllipse(t, 38, 72, 8, 6, aM);
            FillEllipse(t, 90, 72, 10, 8, aD);
            FillEllipse(t, 90, 72, 8, 6, aM);

            // Arms
            FillRect(t, 24, 42, 14, 30, aD);
            FillRect(t, 26, 44, 10, 26, aM);
            FillRect(t, 90, 42, 14, 30, aD);
            FillRect(t, 92, 44, 10, 26, aM);
            // Gauntlets
            FillRect(t, 26, 40, 10, 6, fl);
            FillRect(t, 92, 40, 10, 6, fl);

            // Shield (left arm)
            FillEllipse(t, 20, 54, 16, 20, aD);
            FillEllipse(t, 20, 54, 13, 17, bl);
            FillEllipse(t, 20, 54, 10, 14, bL);
            FillRect(t, 17, 51, 6, 6, gd);
            FillRect(t, 18, 52, 4, 4, gL);

            // Sword (right hand)
            FillRect(t, 100, 16, 6, 52, sw);
            FillRect(t, 101, 20, 4, 44, sH);
            FillTri(t, 100, 68, 106, 68, 103, 74, sw);
            FillRect(t, 94, 42, 18, 4, gd);
            FillRect(t, 95, 43, 16, 2, gL);
            FillRect(t, 100, 8, 6, 10, bD);
            FillRect(t, 101, 9, 4, 8, br);
            FillCircle(t, 103, 6, 4, gd);

            // Neck + gorget
            FillRect(t, 54, 78, 20, 6, fl);
            FillRect(t, 48, 78, 32, 4, aD);

            // Face
            FillRect(t, 48, 82, 32, 18, fl);
            FillRect(t, 50, 84, 28, 14, fl);
            // Eyes
            FillRect(t, 54, 92, 8, 5, Color.white);
            FillRect(t, 66, 92, 8, 5, Color.white);
            FillRect(t, 57, 93, 4, 4, ey);
            FillRect(t, 69, 93, 4, 4, ey);
            Px(t, 58, 94, Color.black); Px(t, 59, 94, Color.black);
            Px(t, 70, 94, Color.black); Px(t, 71, 94, Color.black);
            Px(t, 57, 95, Color.white); Px(t, 69, 95, Color.white);
            // Mouth + nose
            FillRect(t, 58, 84, 12, 2, C(190, 130, 110));
            FillRect(t, 62, 88, 4, 4, fD);

            // Helmet
            FillRect(t, 44, 96, 40, 8, aD);
            FillRect(t, 46, 98, 36, 6, aM);
            FillEllipse(t, 64, 106, 24, 14, aD);
            FillEllipse(t, 64, 106, 22, 12, aM);
            FillEllipse(t, 64, 110, 16, 8, aL);
            FillRect(t, 48, 96, 32, 3, C(25, 20, 30)); // visor slit
            FillRect(t, 62, 94, 4, 6, aM); // nose guard
            FillRect(t, 60, 116, 8, 10, gd); // crest
            FillRect(t, 62, 124, 4, 3, gL);
        }

        // ================================================================
        // MAGE - Robed wizard with pointed hat and staff
        // ================================================================
        static void DrawMage(Texture2D t)
        {
            Color rD = C(50, 30, 100);       // robe dark
            Color rM = C(80, 50, 150);        // robe medium
            Color rL = C(110, 75, 185);       // robe light
            Color gd = C(220, 180, 50);       // gold
            Color gL = C(240, 200, 70);       // gold light
            Color fl = C(235, 195, 155);      // flesh
            Color fD = C(195, 155, 115);      // flesh shadow
            Color br = C(100, 70, 40);        // staff brown
            Color bD = C(70, 45, 25);         // dark brown
            Color ob = C(100, 200, 255);      // orb cyan
            Color oG = C(150, 230, 255);      // orb glow
            Color bd = C(200, 200, 210);      // beard light

            // Staff (behind body, right side)
            FillRect(t, 94, 8, 5, 100, bD);
            FillRect(t, 95, 10, 3, 96, br);
            // Staff orb
            FillCircle(t, 96, 112, 12, CA(60, 150, 220, 80));
            FillCircle(t, 96, 112, 10, C(60, 150, 220));
            FillCircle(t, 96, 112, 8, ob);
            FillCircle(t, 96, 114, 5, oG);

            // Robe bottom (wide, flared)
            FillTri(t, 20, 4, 108, 4, 64, 40, rD);
            FillRect(t, 30, 4, 68, 34, rD);
            FillRect(t, 34, 8, 60, 28, rM);
            FillRect(t, 24, 4, 80, 4, gd); // gold trim

            // Shoes peeking out
            FillRect(t, 48, 4, 10, 6, bD);
            FillRect(t, 70, 4, 10, 6, bD);

            // Robe middle
            FillRect(t, 38, 32, 52, 30, rD);
            FillRect(t, 40, 34, 48, 26, rM);

            // Sash
            FillRect(t, 36, 56, 56, 6, gd);
            FillRect(t, 38, 57, 52, 4, gL);

            // Robe chest
            FillRect(t, 40, 62, 48, 20, rD);
            FillRect(t, 42, 64, 44, 16, rM);
            FillRect(t, 46, 68, 36, 8, rL);

            // Sleeves (wide, flowing)
            FillTri(t, 24, 50, 42, 50, 40, 74, rD);
            FillTri(t, 26, 52, 40, 52, 39, 72, rM);
            FillTri(t, 86, 50, 104, 50, 88, 74, rD);
            FillTri(t, 88, 52, 102, 52, 89, 72, rM);
            // Hands
            FillRect(t, 26, 46, 10, 6, fl);
            FillRect(t, 90, 46, 10, 6, fl);

            // Collar
            FillRect(t, 48, 80, 32, 4, rD);
            FillRect(t, 46, 82, 36, 3, gd);
            // Neck
            FillRect(t, 54, 80, 20, 4, fl);

            // Face
            FillEllipse(t, 64, 92, 16, 12, fl);
            FillRect(t, 50, 84, 28, 16, fl);
            // Eyes
            FillRect(t, 54, 94, 6, 4, Color.white);
            FillRect(t, 68, 94, 6, 4, Color.white);
            FillRect(t, 56, 95, 3, 3, C(60, 40, 120));
            FillRect(t, 70, 95, 3, 3, C(60, 40, 120));
            Px(t, 57, 96, Color.black); Px(t, 71, 96, Color.black);
            Px(t, 56, 97, Color.white); Px(t, 70, 97, Color.white);
            // Bushy eyebrows
            FillRect(t, 53, 98, 8, 2, bd);
            FillRect(t, 67, 98, 8, 2, bd);
            // Nose
            FillRect(t, 62, 90, 4, 4, fD);
            // Beard
            FillRect(t, 52, 82, 24, 6, bd);
            FillTri(t, 52, 82, 76, 82, 64, 72, bd);
            FillRect(t, 56, 76, 16, 6, C(180, 180, 195));

            // Wizard hat
            FillRect(t, 42, 98, 44, 8, rD);   // brim
            FillRect(t, 44, 100, 40, 6, rM);
            FillTri(t, 44, 106, 84, 106, 64, 126, rD);  // cone
            FillTri(t, 48, 106, 80, 106, 64, 124, rM);
            FillRect(t, 44, 106, 40, 3, gd);  // hat band
            // Star emblem on hat
            FillRect(t, 62, 114, 4, 6, C(255, 255, 100));
            FillRect(t, 60, 116, 8, 2, C(255, 255, 100));
        }

        // ================================================================
        // PRIEST - White-robed healer with hood and halo
        // ================================================================
        static void DrawPriest(Texture2D t)
        {
            Color wh = C(245, 240, 225);     // robe white
            Color wL = C(255, 250, 240);     // robe light
            Color wD = C(210, 205, 190);     // robe shadow
            Color gd = C(220, 180, 50);      // gold
            Color gL = C(245, 210, 80);      // gold light
            Color fl = C(235, 195, 155);     // flesh
            Color fD = C(195, 155, 115);     // flesh shadow
            Color br = C(110, 75, 40);       // staff brown
            Color bD = C(70, 45, 25);        // dark brown
            Color hg = CA(120, 255, 160, 100); // healing glow

            // Staff (left side, behind body)
            FillRect(t, 14, 8, 4, 100, bD);
            FillRect(t, 15, 10, 2, 96, br);
            // Staff holy cross top
            FillRect(t, 12, 108, 10, 4, gd);
            FillRect(t, 14, 104, 6, 12, gd);
            FillCircle(t, 17, 110, 8, CA(255, 240, 130, 50));

            // Robe bottom
            FillRect(t, 30, 4, 68, 28, wD);
            FillRect(t, 34, 8, 60, 22, wh);
            FillRect(t, 30, 4, 68, 4, gd);
            FillRect(t, 32, 5, 64, 2, gL);

            // Sandals
            FillRect(t, 48, 4, 12, 6, br);
            FillRect(t, 68, 4, 12, 6, br);

            // Robe middle
            FillRect(t, 36, 30, 56, 30, wD);
            FillRect(t, 38, 32, 52, 26, wh);

            // Golden sash
            FillRect(t, 34, 58, 60, 6, gd);
            FillRect(t, 36, 59, 56, 4, gL);

            // Robe chest
            FillRect(t, 38, 64, 52, 18, wD);
            FillRect(t, 40, 66, 48, 14, wh);
            FillRect(t, 44, 68, 40, 10, wL);
            // Holy cross on chest
            FillRect(t, 60, 66, 8, 14, gd);
            FillRect(t, 54, 72, 20, 4, gd);

            // Sleeves
            FillRect(t, 22, 52, 18, 24, wD);
            FillRect(t, 24, 54, 14, 20, wh);
            FillRect(t, 88, 52, 18, 24, wD);
            FillRect(t, 90, 54, 14, 20, wh);
            FillRect(t, 22, 52, 18, 3, gd);
            FillRect(t, 88, 52, 18, 3, gd);

            // Hands with healing glow
            FillRect(t, 24, 48, 12, 6, fl);
            FillRect(t, 92, 48, 12, 6, fl);
            FillCircle(t, 30, 50, 8, hg);
            FillCircle(t, 98, 50, 8, hg);

            // Collar
            FillRect(t, 46, 80, 36, 4, wD);
            FillRect(t, 48, 82, 32, 3, gd);
            FillRect(t, 54, 80, 20, 4, fl);

            // Face
            FillEllipse(t, 64, 94, 16, 14, fl);
            // Eyes (kind, blue)
            FillRect(t, 54, 94, 6, 4, Color.white);
            FillRect(t, 68, 94, 6, 4, Color.white);
            FillRect(t, 56, 95, 3, 3, C(80, 140, 200));
            FillRect(t, 70, 95, 3, 3, C(80, 140, 200));
            Px(t, 57, 96, C(30, 60, 100)); Px(t, 71, 96, C(30, 60, 100));
            Px(t, 56, 97, Color.white); Px(t, 70, 97, Color.white);
            // Gentle smile
            FillRect(t, 58, 86, 12, 2, C(200, 140, 120));
            Px(t, 57, 87, C(200, 140, 120)); Px(t, 70, 87, C(200, 140, 120));
            FillRect(t, 62, 90, 4, 3, fD);

            // Hood
            FillEllipse(t, 64, 102, 24, 16, wD);
            FillEllipse(t, 64, 102, 22, 14, wh);
            FillEllipse(t, 64, 106, 18, 10, wL);
            FillRect(t, 44, 84, 4, 20, wD);
            FillRect(t, 80, 84, 4, 20, wD);

            // Halo (golden ring above head)
            FillRing(t, 64, 118, 24, 6, 4, gL);
            FillCircle(t, 64, 122, 4, CA(255, 240, 130, 80));
        }

        // ================================================================
        // ROGUE - Hooded figure with twin daggers
        // ================================================================
        static void DrawRogue(Texture2D t)
        {
            Color lD = C(50, 42, 42);        // leather dark
            Color lM = C(75, 65, 60);         // leather medium
            Color lL = C(100, 90, 82);        // leather light
            Color hD = C(35, 30, 35);         // hood dark
            Color hM = C(55, 48, 52);         // hood medium
            Color fl = C(235, 195, 155);      // flesh
            Color br = C(100, 75, 45);        // brown belt
            Color bD = C(70, 50, 30);         // dark brown
            Color dg = C(190, 200, 215);      // dagger silver
            Color dH = C(220, 230, 240);      // dagger highlight
            Color ey = C(120, 255, 100);      // glowing green eyes

            // Dark cloak
            FillRect(t, 28, 8, 16, 62, hD);
            FillRect(t, 84, 8, 16, 62, hD);
            FillTri(t, 28, 8, 44, 8, 28, 0, hD);
            FillTri(t, 84, 8, 100, 8, 100, 0, hD);

            // Boots (sleek, pointed)
            FillRect(t, 46, 4, 14, 12, bD);
            FillRect(t, 68, 4, 14, 12, bD);
            FillRect(t, 48, 6, 10, 8, lD);
            FillRect(t, 70, 6, 10, 8, lD);
            Px(t, 45, 4, bD); Px(t, 44, 4, bD);
            Px(t, 83, 4, bD); Px(t, 84, 4, bD);

            // Legs (slim)
            FillRect(t, 48, 14, 12, 22, lD);
            FillRect(t, 68, 14, 12, 22, lD);
            FillRect(t, 50, 16, 8, 18, lM);
            FillRect(t, 70, 16, 8, 18, lM);

            // Belt with pouches
            FillRect(t, 38, 34, 52, 6, br);
            FillRect(t, 56, 33, 16, 8, bD);
            FillRect(t, 40, 32, 8, 6, bD);
            FillRect(t, 80, 32, 8, 6, bD);

            // Torso (leather armor, slim)
            FillRect(t, 40, 40, 48, 34, lD);
            FillRect(t, 42, 42, 44, 30, lM);
            FillRect(t, 46, 46, 36, 22, lL);
            // Cross-straps
            for (int i = 0; i < 20; i++)
            {
                FillRect(t, 46 + i, 46 + i, 3, 2, bD);
                FillRect(t, 78 - i, 46 + i, 3, 2, bD);
            }

            // Arms
            FillRect(t, 28, 44, 12, 26, lD);
            FillRect(t, 30, 46, 8, 22, lM);
            FillRect(t, 88, 44, 12, 26, lD);
            FillRect(t, 90, 46, 8, 22, lM);
            FillRect(t, 28, 40, 10, 6, fl);
            FillRect(t, 90, 40, 10, 6, fl);

            // Left dagger
            FillRect(t, 20, 22, 3, 30, dg);
            FillRect(t, 21, 26, 1, 22, dH);
            FillTri(t, 20, 52, 23, 52, 21, 56, dg);
            FillRect(t, 18, 20, 7, 3, br);
            FillRect(t, 19, 14, 5, 8, bD);

            // Right dagger
            FillRect(t, 105, 22, 3, 30, dg);
            FillRect(t, 106, 26, 1, 22, dH);
            FillTri(t, 105, 52, 108, 52, 106, 56, dg);
            FillRect(t, 103, 20, 7, 3, br);
            FillRect(t, 104, 14, 5, 8, bD);

            // Neck
            FillRect(t, 50, 74, 28, 4, lD);

            // Hood / cowl
            FillEllipse(t, 64, 96, 26, 22, hD);
            FillEllipse(t, 64, 96, 24, 20, hM);
            FillEllipse(t, 64, 90, 18, 14, C(25, 22, 28)); // inner shadow

            // Face (only glowing eyes through mask)
            FillRect(t, 48, 84, 32, 14, C(30, 25, 32));
            FillCircle(t, 56, 92, 4, CA(100, 220, 80, 80));
            FillCircle(t, 72, 92, 4, CA(100, 220, 80, 80));
            FillRect(t, 54, 91, 5, 3, ey);
            FillRect(t, 70, 91, 5, 3, ey);
            Px(t, 55, 92, C(200, 255, 180));
            Px(t, 71, 92, C(200, 255, 180));

            // Hood peak
            FillTri(t, 44, 106, 84, 106, 64, 122, hD);
            FillTri(t, 48, 106, 80, 106, 64, 120, hM);
        }

        // ================================================================
        // SLIME - Cute green blob with eyes
        // ================================================================
        static void DrawSlime(Texture2D t)
        {
            Color gD = C(40, 120, 50);      // dark green
            Color gM = C(70, 180, 80);       // medium green
            Color gL = C(110, 220, 120);     // light green
            Color gH = C(160, 240, 170);     // highlight
            Color mo = C(40, 100, 50);       // mouth dark

            // Drips at base
            FillEllipse(t, 24, 6, 6, 5, gD);
            FillEllipse(t, 72, 7, 5, 6, gD);

            // Main blob body
            FillEllipse(t, 48, 34, 40, 28, gD);
            FillEllipse(t, 48, 44, 36, 34, gM);
            FillEllipse(t, 48, 48, 30, 28, gL);

            // Top bulge
            FillEllipse(t, 48, 62, 26, 20, gM);
            FillEllipse(t, 48, 64, 22, 16, gL);

            // Highlight / shine
            FillEllipse(t, 60, 68, 8, 6, gH);
            FillEllipse(t, 62, 70, 4, 3, C(200, 250, 210));

            // Big cute eyes
            FillEllipse(t, 36, 54, 10, 12, Color.white);
            FillEllipse(t, 60, 54, 10, 12, Color.white);
            FillCircle(t, 38, 52, 5, Color.black);
            FillCircle(t, 62, 52, 5, Color.black);
            FillCircle(t, 36, 56, 3, Color.white);
            FillCircle(t, 60, 56, 3, Color.white);

            // Happy mouth
            FillRect(t, 42, 40, 14, 3, mo);
            Px(t, 41, 41, mo); Px(t, 56, 41, mo);
            Px(t, 40, 42, mo); Px(t, 57, 42, mo);

            // Goo drips
            FillRect(t, 20, 4, 6, 12, gD);
            FillRect(t, 21, 6, 4, 8, gM);
            FillRect(t, 68, 4, 5, 10, gD);
            FillRect(t, 69, 5, 3, 7, gM);
            FillRect(t, 80, 4, 4, 6, gD);
            FillRect(t, 81, 5, 2, 3, gM);
        }

        // ================================================================
        // GOBLIN - Small green humanoid with club and pointy ears
        // ================================================================
        static void DrawGoblin(Texture2D t)
        {
            Color sD = C(50, 110, 40);       // skin dark
            Color sM = C(80, 160, 60);        // skin medium
            Color sL = C(110, 190, 80);       // skin light
            Color cl = C(130, 95, 55);        // cloth brown
            Color cD = C(95, 70, 40);         // cloth dark
            Color cb = C(90, 60, 35);         // club brown
            Color cB = C(65, 40, 20);         // club dark brown
            Color ey = C(255, 230, 50);       // yellow eyes
            Color eD = C(180, 40, 40);        // red pupils
            Color wh = Color.white;

            // Feet
            FillRect(t, 30, 4, 12, 8, sD);
            FillRect(t, 54, 4, 12, 8, sD);
            FillRect(t, 32, 5, 8, 6, sM);
            FillRect(t, 56, 5, 8, 6, sM);
            Px(t, 29, 4, sD); Px(t, 28, 4, sD);
            Px(t, 67, 4, sD); Px(t, 68, 4, sD);

            // Short legs
            FillRect(t, 32, 10, 10, 16, sD);
            FillRect(t, 56, 10, 10, 16, sD);
            FillRect(t, 33, 12, 8, 12, sM);
            FillRect(t, 57, 12, 8, 12, sM);

            // Ragged tunic
            FillRect(t, 26, 24, 44, 26, cD);
            FillRect(t, 28, 26, 40, 22, cl);
            FillTri(t, 26, 24, 34, 24, 30, 18, cD);
            FillTri(t, 40, 24, 48, 24, 44, 20, cD);
            FillTri(t, 52, 24, 60, 24, 56, 18, cD);
            FillTri(t, 62, 24, 70, 24, 66, 20, cD);
            FillRect(t, 26, 42, 44, 4, cB);

            // Arms
            FillRect(t, 16, 30, 12, 18, sD);
            FillRect(t, 18, 32, 8, 14, sM);
            FillRect(t, 68, 30, 12, 18, sD);
            FillRect(t, 70, 32, 8, 14, sM);
            // Hands
            FillRect(t, 16, 26, 10, 6, sD);
            FillRect(t, 17, 27, 8, 4, sM);
            FillRect(t, 70, 26, 10, 6, sD);
            FillRect(t, 71, 27, 8, 4, sM);

            // Club (right hand)
            FillRect(t, 74, 28, 5, 40, cB);
            FillRect(t, 75, 30, 3, 36, cb);
            FillRect(t, 70, 64, 14, 12, cB);
            FillRect(t, 72, 66, 10, 8, cb);
            Px(t, 73, 70, C(180, 180, 180)); // nails
            Px(t, 79, 68, C(180, 180, 180));

            // Big head
            FillEllipse(t, 48, 62, 22, 18, sD);
            FillEllipse(t, 48, 62, 20, 16, sM);
            FillEllipse(t, 48, 64, 16, 12, sL);

            // Pointy ears
            FillTri(t, 24, 64, 30, 56, 30, 72, sD);
            FillTri(t, 25, 64, 30, 58, 30, 70, sM);
            FillTri(t, 66, 64, 72, 72, 72, 56, sD);
            FillTri(t, 67, 64, 72, 70, 72, 58, sM);

            // Big yellow eyes
            FillEllipse(t, 40, 66, 8, 6, wh);
            FillEllipse(t, 58, 66, 8, 6, wh);
            FillCircle(t, 40, 66, 4, ey);
            FillCircle(t, 58, 66, 4, ey);
            Px(t, 41, 66, eD); Px(t, 42, 66, eD);
            Px(t, 59, 66, eD); Px(t, 60, 66, eD);

            // Big nose
            FillEllipse(t, 48, 58, 6, 5, sD);
            FillEllipse(t, 48, 58, 4, 3, sM);

            // Mouth with fangs
            FillRect(t, 38, 52, 20, 4, C(100, 30, 30));
            FillTri(t, 40, 52, 44, 52, 42, 48, wh);
            FillTri(t, 52, 52, 56, 52, 54, 48, wh);

            // Angry eyebrows
            FillRect(t, 34, 72, 12, 2, sD);
            FillRect(t, 52, 72, 12, 2, sD);
        }

        // ================================================================
        // WOLF - Gray quadruped with fangs (side view, facing left)
        // ================================================================
        static void DrawWolf(Texture2D t)
        {
            Color fD = C(80, 80, 95);        // fur dark
            Color fM = C(120, 118, 128);      // fur medium
            Color fL = C(165, 160, 155);      // fur light
            Color ey = C(255, 50, 30);        // red eyes
            Color no = C(30, 22, 22);         // nose
            Color fn = Color.white;           // fangs
            Color tg = C(200, 80, 80);        // tongue

            // Tail (raised, right side, bushy)
            FillTri(t, 72, 44, 92, 44, 90, 78, fD);
            FillTri(t, 74, 46, 90, 46, 88, 74, fM);
            FillTri(t, 84, 66, 92, 66, 90, 76, fL);

            // Back legs
            FillRect(t, 60, 4, 12, 28, fD);
            FillRect(t, 62, 6, 8, 24, fM);
            FillRect(t, 72, 4, 12, 24, fD);
            FillRect(t, 74, 6, 8, 20, fM);
            FillRect(t, 58, 4, 16, 6, fD);
            FillRect(t, 60, 5, 12, 4, fM);
            FillRect(t, 70, 4, 16, 6, fD);
            FillRect(t, 72, 5, 12, 4, fM);

            // Front legs
            FillRect(t, 16, 4, 12, 32, fD);
            FillRect(t, 18, 6, 8, 28, fM);
            FillRect(t, 30, 4, 12, 28, fD);
            FillRect(t, 32, 6, 8, 24, fM);
            FillRect(t, 14, 4, 16, 6, fD);
            FillRect(t, 16, 5, 12, 4, fM);
            FillRect(t, 28, 4, 16, 6, fD);
            FillRect(t, 30, 5, 12, 4, fM);

            // Body (long horizontal oval)
            FillEllipse(t, 50, 40, 36, 18, fD);
            FillEllipse(t, 50, 40, 34, 16, fM);
            FillEllipse(t, 50, 34, 28, 10, fL);
            FillEllipse(t, 50, 50, 30, 6, fD); // back ridge

            // Chest
            FillEllipse(t, 24, 40, 18, 18, fD);
            FillEllipse(t, 24, 40, 16, 16, fM);
            FillEllipse(t, 24, 36, 14, 12, fL);

            // Neck
            FillEllipse(t, 20, 52, 14, 12, fD);
            FillEllipse(t, 20, 52, 12, 10, fM);

            // Head
            FillEllipse(t, 16, 62, 16, 12, fD);
            FillEllipse(t, 16, 62, 14, 10, fM);

            // Snout
            FillEllipse(t, 6, 58, 10, 7, fD);
            FillEllipse(t, 6, 58, 8, 5, fM);
            FillEllipse(t, 6, 58, 6, 3, fL);

            // Nose
            FillCircle(t, 2, 60, 3, no);

            // Pointed ears
            FillTri(t, 14, 68, 22, 68, 16, 82, fD);
            FillTri(t, 15, 69, 21, 69, 17, 80, fM);
            FillTri(t, 16, 70, 20, 70, 17, 78, C(180, 120, 120));
            FillTri(t, 24, 68, 32, 68, 26, 80, fD);
            FillTri(t, 25, 69, 31, 69, 27, 78, fM);
            FillTri(t, 26, 70, 30, 70, 27, 76, C(180, 120, 120));

            // Red menacing eyes
            FillCircle(t, 14, 66, 4, CA(255, 50, 30, 80));
            FillCircle(t, 14, 66, 3, ey);
            Px(t, 13, 67, C(255, 200, 100));
            FillCircle(t, 24, 66, 4, CA(255, 50, 30, 80));
            FillCircle(t, 24, 66, 3, ey);
            Px(t, 23, 67, C(255, 200, 100));

            // Open mouth with fangs
            FillRect(t, 2, 52, 16, 4, C(100, 30, 30));
            FillRect(t, 2, 52, 16, 2, fD);
            FillRect(t, 2, 56, 16, 2, fD);
            FillTri(t, 4, 56, 7, 56, 5, 50, fn);
            FillTri(t, 12, 56, 15, 56, 13, 50, fn);
            FillTri(t, 6, 52, 9, 52, 7, 48, fn);
            FillRect(t, 6, 50, 6, 2, tg);
        }

        // ================================================================
        // DRAWING PRIMITIVES
        // ================================================================

        static Color C(int r, int g, int b) =>
            new Color(r / 255f, g / 255f, b / 255f, 1f);

        static Color CA(int r, int g, int b, int a) =>
            new Color(r / 255f, g / 255f, b / 255f, a / 255f);

        static void Clear(Texture2D t)
        {
            var px = new Color[t.width * t.height];
            for (int i = 0; i < px.Length; i++) px[i] = Color.clear;
            t.SetPixels(px);
        }

        static void Px(Texture2D t, int x, int y, Color c)
        {
            if (x < 0 || x >= t.width || y < 0 || y >= t.height) return;
            if (c.a >= 0.99f)
            {
                t.SetPixel(x, y, c);
            }
            else if (c.a > 0.01f)
            {
                Color e = t.GetPixel(x, y);
                float a = c.a;
                t.SetPixel(x, y, new Color(
                    e.r * (1 - a) + c.r * a,
                    e.g * (1 - a) + c.g * a,
                    e.b * (1 - a) + c.b * a,
                    Mathf.Max(e.a, c.a)));
            }
        }

        static void FillRect(Texture2D t, int x0, int y0, int w, int h, Color c)
        {
            for (int y = y0; y < y0 + h; y++)
                for (int x = x0; x < x0 + w; x++)
                    Px(t, x, y, c);
        }

        static void FillCircle(Texture2D t, int cx, int cy, int r, Color c)
        {
            for (int y = cy - r; y <= cy + r; y++)
                for (int x = cx - r; x <= cx + r; x++)
                    if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r)
                        Px(t, x, y, c);
        }

        static void FillEllipse(Texture2D t, int cx, int cy, int rx, int ry, Color c)
        {
            if (rx <= 0 || ry <= 0) return;
            for (int y = cy - ry; y <= cy + ry; y++)
                for (int x = cx - rx; x <= cx + rx; x++)
                {
                    float dx = (float)(x - cx) / rx, dy = (float)(y - cy) / ry;
                    if (dx * dx + dy * dy <= 1f)
                        Px(t, x, y, c);
                }
        }

        static void FillRing(Texture2D t, int cx, int cy, int rx, int ry, int thick, Color c)
        {
            if (rx <= 0 || ry <= 0) return;
            int irx = rx - thick, iry = ry - thick;
            for (int y = cy - ry; y <= cy + ry; y++)
                for (int x = cx - rx; x <= cx + rx; x++)
                {
                    float dx = (float)(x - cx) / rx, dy = (float)(y - cy) / ry;
                    if (dx * dx + dy * dy > 1f) continue;
                    if (irx > 0 && iry > 0)
                    {
                        float idx = (float)(x - cx) / irx, idy = (float)(y - cy) / iry;
                        if (idx * idx + idy * idy < 1f) continue;
                    }
                    Px(t, x, y, c);
                }
        }

        static void FillTri(Texture2D t, int x0, int y0, int x1, int y1, int x2, int y2, Color c)
        {
            int minX = Mathf.Min(x0, Mathf.Min(x1, x2));
            int maxX = Mathf.Max(x0, Mathf.Max(x1, x2));
            int minY = Mathf.Min(y0, Mathf.Min(y1, y2));
            int maxY = Mathf.Max(y0, Mathf.Max(y1, y2));
            for (int y = minY; y <= maxY; y++)
                for (int x = minX; x <= maxX; x++)
                    if (InTri(x, y, x0, y0, x1, y1, x2, y2))
                        Px(t, x, y, c);
        }

        static bool InTri(int px, int py, int x0, int y0, int x1, int y1, int x2, int y2)
        {
            float d1 = (px - x1) * (y0 - y1) - (x0 - x1) * (py - y1);
            float d2 = (px - x2) * (y1 - y2) - (x1 - x2) * (py - y2);
            float d3 = (px - x0) * (y2 - y0) - (x2 - x0) * (py - y0);
            bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
            return !(hasNeg && hasPos);
        }

        static void AddOutline(Texture2D t, Color oc, int thick)
        {
            int w = t.width, h = t.height;
            var src = t.GetPixels();
            var dst = new Color[w * h];
            System.Array.Copy(src, dst, src.Length);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    if (src[y * w + x].a > 0.1f) continue;
                    bool adj = false;
                    for (int dy = -thick; dy <= thick && !adj; dy++)
                        for (int dx = -thick; dx <= thick && !adj; dx++)
                        {
                            int nx = x + dx, ny = y + dy;
                            if (nx >= 0 && nx < w && ny >= 0 && ny < h && src[ny * w + nx].a > 0.1f)
                                adj = true;
                        }
                    if (adj) dst[y * w + x] = oc;
                }
            t.SetPixels(dst);
        }
    }
}
#endif
