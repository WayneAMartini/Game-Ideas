#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using Ascendant.Combat;

namespace Ascendant.Editor
{
    public static class SpriteGenerator
    {
        const int HeroSize = 128;
        const int EnemySize = 96;
        const int BossSize = 160;
        const string OutputPath = "Assets/Art/Sprites/Generated";

        [MenuItem("Ascendant/Generate Placeholder Art")]
        public static void GenerateAll()
        {
            EnsureDirectory(OutputPath + "/Heroes");
            EnsureDirectory(OutputPath + "/Enemies");
            EnsureDirectory(OutputPath + "/Bosses");
            EnsureDirectory(OutputPath + "/UI");

            GenerateHeroSprites();
            GenerateEnemySprites();
            GenerateBossSprites();
            GenerateUISprites();

            AssetDatabase.Refresh();
            Debug.Log("[SpriteGenerator] All placeholder art generated.");
        }

        static void GenerateHeroSprites()
        {
            var heroes = new (string name, string abbr, Affinity affinity)[]
            {
                ("Warrior", "WR", Affinity.Flame),
                ("Mage", "MG", Affinity.Frost),
                ("Priest", "PR", Affinity.Radiance),
                ("Rogue", "RG", Affinity.Shadow),
                ("Paladin", "PL", Affinity.Radiance),
                ("Ranger", "RA", Affinity.Nature),
                ("Berserker", "BK", Affinity.Flame),
                ("Necromancer", "NC", Affinity.Shadow),
                ("Druid", "DR", Affinity.Nature),
                ("Bard", "BD", Affinity.Storm),
                ("Monk", "MK", Affinity.Storm),
                ("Gunslinger", "GS", Affinity.Flame),
                ("Summoner", "SM", Affinity.Nature),
                ("Defender", "DF", Affinity.Frost),
                ("Alchemist", "AL", Affinity.Nature),
                ("Warlock", "WK", Affinity.Shadow),
                ("SpellBlade", "SB", Affinity.Frost),
                ("Shaman", "SH", Affinity.Storm),
                ("Thief", "TH", Affinity.Shadow),
                ("Warden", "WD", Affinity.Nature),
                ("Marksman", "MR", Affinity.Storm),
                ("Reaper", "RP", Affinity.Shadow),
                ("Chronomancer", "CR", Affinity.Frost),
                ("DragonHunter", "DH", Affinity.Flame)
            };

            foreach (var (name, abbr, affinity) in heroes)
            {
                var tex = CreateHeroTexture(abbr, GetAffinityColor(affinity));
                SaveTexture(tex, $"{OutputPath}/Heroes/{name}.png");
                Object.DestroyImmediate(tex);
            }
        }

        static void GenerateEnemySprites()
        {
            var enemies = new (string name, EnemyCategory category, Affinity affinity)[]
            {
                ("Slime", EnemyCategory.Beast, Affinity.Nature),
                ("Wolf", EnemyCategory.Beast, Affinity.Nature),
                ("Goblin", EnemyCategory.Humanoid, Affinity.None),
                ("Skeleton", EnemyCategory.Construct, Affinity.Shadow),
                ("Ghost", EnemyCategory.Ethereal, Affinity.Shadow),
                ("Golem", EnemyCategory.Construct, Affinity.Frost),
                ("FireImp", EnemyCategory.Ethereal, Affinity.Flame),
                ("IceWraith", EnemyCategory.Ethereal, Affinity.Frost),
                ("StormHarpy", EnemyCategory.Beast, Affinity.Storm),
                ("VineCreeper", EnemyCategory.Beast, Affinity.Nature),
                ("DarkKnight", EnemyCategory.Armored, Affinity.Shadow),
                ("CrystalGuard", EnemyCategory.Construct, Affinity.Radiance),
                ("LavaWorm", EnemyCategory.Beast, Affinity.Flame),
                ("FrostGiant", EnemyCategory.Humanoid, Affinity.Frost),
                ("ThunderBird", EnemyCategory.Beast, Affinity.Storm),
                ("ShadowAssassin", EnemyCategory.Humanoid, Affinity.Shadow)
            };

            foreach (var (name, category, affinity) in enemies)
            {
                var tex = CreateEnemyTexture(category, GetAffinityColor(affinity));
                SaveTexture(tex, $"{OutputPath}/Enemies/{name}.png");
                Object.DestroyImmediate(tex);
            }
        }

        static void GenerateBossSprites()
        {
            var bosses = new (string name, Affinity affinity)[]
            {
                ("MeadowGuardian", Affinity.Nature),
                ("CrystalSentinel", Affinity.Frost),
                ("VolcanicLord", Affinity.Flame),
                ("StormKing", Affinity.Storm),
                ("ShadowOverlord", Affinity.Shadow),
                ("CelestialArchon", Affinity.Radiance),
                ("RealmBoss1", Affinity.None)
            };

            foreach (var (name, affinity) in bosses)
            {
                var tex = CreateBossTexture(name[0].ToString(), GetAffinityColor(affinity));
                SaveTexture(tex, $"{OutputPath}/Bosses/{name}.png");
                Object.DestroyImmediate(tex);
            }
        }

        static void GenerateUISprites()
        {
            // Tab bar icons
            var tabs = new[] { "Combat", "Islands", "Heroes", "Social", "More" };
            foreach (var tab in tabs)
            {
                var tex = CreateUIIconTexture(tab[0].ToString(), Color.white);
                SaveTexture(tex, $"{OutputPath}/UI/Tab_{tab}.png");
                Object.DestroyImmediate(tex);
            }

            // App icon placeholder
            var iconTex = CreateAppIconTexture();
            SaveTexture(iconTex, $"{OutputPath}/UI/AppIcon.png");
            Object.DestroyImmediate(iconTex);
        }

        static Texture2D CreateHeroTexture(string abbreviation, Color affinityColor)
        {
            var tex = new Texture2D(HeroSize, HeroSize, TextureFormat.RGBA32, false);
            int cx = HeroSize / 2, cy = HeroSize / 2;
            int radius = HeroSize / 2 - 4;
            int borderWidth = 4;

            for (int y = 0; y < HeroSize; y++)
            {
                for (int x = 0; x < HeroSize; x++)
                {
                    float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    if (dist <= radius - borderWidth)
                    {
                        // Inner fill: lighter version of affinity color
                        Color fill = Color.Lerp(affinityColor, Color.white, 0.4f);
                        tex.SetPixel(x, y, fill);
                    }
                    else if (dist <= radius)
                    {
                        // Border: affinity color
                        tex.SetPixel(x, y, affinityColor);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }

            // Draw abbreviation letters (simple pixel font approximation)
            DrawText(tex, abbreviation, cx, cy, Color.white, 3);

            tex.Apply();
            return tex;
        }

        static Texture2D CreateEnemyTexture(EnemyCategory category, Color affinityColor)
        {
            var tex = new Texture2D(EnemySize, EnemySize, TextureFormat.RGBA32, false);
            ClearTexture(tex, Color.clear);

            int cx = EnemySize / 2, cy = EnemySize / 2;
            int half = EnemySize / 2 - 4;

            switch (category)
            {
                case EnemyCategory.Beast:
                    // Triangle
                    DrawTriangle(tex, cx, cy, half, affinityColor);
                    break;
                case EnemyCategory.Humanoid:
                    // Pentagon
                    DrawPolygon(tex, cx, cy, half, 5, affinityColor);
                    break;
                case EnemyCategory.Construct:
                    // Square
                    DrawRect(tex, cx - half, cy - half, half * 2, half * 2, affinityColor);
                    break;
                case EnemyCategory.Ethereal:
                    // Diamond
                    DrawPolygon(tex, cx, cy, half, 4, affinityColor);
                    break;
                case EnemyCategory.Armored:
                    // Hexagon
                    DrawPolygon(tex, cx, cy, half, 6, affinityColor);
                    break;
                case EnemyCategory.Resistant:
                    // Octagon
                    DrawPolygon(tex, cx, cy, half, 8, affinityColor);
                    break;
            }

            tex.Apply();
            return tex;
        }

        static Texture2D CreateBossTexture(string initial, Color affinityColor)
        {
            var tex = new Texture2D(BossSize, BossSize, TextureFormat.RGBA32, false);
            ClearTexture(tex, Color.clear);

            int cx = BossSize / 2, cy = BossSize / 2;
            int radius = BossSize / 2 - 6;

            // Outer glow
            for (int y = 0; y < BossSize; y++)
            {
                for (int x = 0; x < BossSize; x++)
                {
                    float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    if (dist <= radius + 6 && dist > radius)
                    {
                        float alpha = 1f - (dist - radius) / 6f;
                        tex.SetPixel(x, y, new Color(affinityColor.r, affinityColor.g, affinityColor.b, alpha * 0.6f));
                    }
                    else if (dist <= radius && dist > radius - 5)
                    {
                        tex.SetPixel(x, y, affinityColor);
                    }
                    else if (dist <= radius - 5)
                    {
                        Color fill = Color.Lerp(affinityColor, Color.black, 0.3f);
                        tex.SetPixel(x, y, fill);
                    }
                }
            }

            DrawText(tex, initial, cx, cy, Color.white, 5);
            tex.Apply();
            return tex;
        }

        static Texture2D CreateUIIconTexture(string letter, Color color)
        {
            int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            ClearTexture(tex, Color.clear);

            int cx = size / 2, cy = size / 2;
            int radius = size / 2 - 4;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    if (dist <= radius)
                        tex.SetPixel(x, y, new Color(0.3f, 0.3f, 0.4f, 1f));
                }
            }

            DrawText(tex, letter, cx, cy, color, 2);
            tex.Apply();
            return tex;
        }

        static Texture2D CreateAppIconTexture()
        {
            int size = 1024;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

            // Gradient background
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float t = (float)y / size;
                    Color c = Color.Lerp(new Color(0.1f, 0.05f, 0.3f), new Color(0.4f, 0.2f, 0.6f), t);
                    tex.SetPixel(x, y, c);
                }
            }

            DrawText(tex, "A", size / 2, size / 2, new Color(1f, 0.85f, 0.3f), 40);
            tex.Apply();
            return tex;
        }

        // --- Drawing helpers ---

        static void ClearTexture(Texture2D tex, Color color)
        {
            var pixels = new Color[tex.width * tex.height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels(pixels);
        }

        static void DrawTriangle(Texture2D tex, int cx, int cy, int half, Color color)
        {
            Vector2 a = new(cx, cy + half);
            Vector2 b = new(cx - half, cy - half);
            Vector2 c = new(cx + half, cy - half);

            for (int y = 0; y < tex.height; y++)
            {
                for (int x = 0; x < tex.width; x++)
                {
                    if (PointInTriangle(new Vector2(x, y), a, b, c))
                        tex.SetPixel(x, y, color);
                }
            }
        }

        static void DrawPolygon(Texture2D tex, int cx, int cy, int radius, int sides, Color color)
        {
            Vector2[] verts = new Vector2[sides];
            for (int i = 0; i < sides; i++)
            {
                float angle = 2f * Mathf.PI * i / sides - Mathf.PI / 2f;
                verts[i] = new Vector2(cx + radius * Mathf.Cos(angle), cy + radius * Mathf.Sin(angle));
            }

            for (int y = 0; y < tex.height; y++)
            {
                for (int x = 0; x < tex.width; x++)
                {
                    if (PointInPolygon(new Vector2(x, y), verts))
                        tex.SetPixel(x, y, color);
                }
            }
        }

        static void DrawRect(Texture2D tex, int x0, int y0, int w, int h, Color color)
        {
            for (int y = y0; y < y0 + h && y < tex.height; y++)
            {
                for (int x = x0; x < x0 + w && x < tex.width; x++)
                {
                    if (x >= 0 && y >= 0)
                        tex.SetPixel(x, y, color);
                }
            }
        }

        static void DrawText(Texture2D tex, string text, int cx, int cy, Color color, int scale)
        {
            // Simple 5x7 bitmap font for basic letters
            int charWidth = 5 * scale;
            int totalWidth = text.Length * charWidth + (text.Length - 1) * scale;
            int startX = cx - totalWidth / 2;
            int startY = cy - 7 * scale / 2;

            for (int i = 0; i < text.Length; i++)
            {
                byte[] glyph = GetGlyph(text[i]);
                int ox = startX + i * (charWidth + scale);

                for (int row = 0; row < 7; row++)
                {
                    for (int col = 0; col < 5; col++)
                    {
                        if ((glyph[row] & (1 << (4 - col))) != 0)
                        {
                            for (int sy = 0; sy < scale; sy++)
                            {
                                for (int sx = 0; sx < scale; sx++)
                                {
                                    int px = ox + col * scale + sx;
                                    int py = startY + (6 - row) * scale + sy;
                                    if (px >= 0 && px < tex.width && py >= 0 && py < tex.height)
                                        tex.SetPixel(px, py, color);
                                }
                            }
                        }
                    }
                }
            }
        }

        static byte[] GetGlyph(char c)
        {
            return char.ToUpper(c) switch
            {
                'A' => new byte[] { 0x0E, 0x11, 0x11, 0x1F, 0x11, 0x11, 0x11 },
                'B' => new byte[] { 0x1E, 0x11, 0x11, 0x1E, 0x11, 0x11, 0x1E },
                'C' => new byte[] { 0x0E, 0x11, 0x10, 0x10, 0x10, 0x11, 0x0E },
                'D' => new byte[] { 0x1E, 0x11, 0x11, 0x11, 0x11, 0x11, 0x1E },
                'E' => new byte[] { 0x1F, 0x10, 0x10, 0x1E, 0x10, 0x10, 0x1F },
                'F' => new byte[] { 0x1F, 0x10, 0x10, 0x1E, 0x10, 0x10, 0x10 },
                'G' => new byte[] { 0x0E, 0x11, 0x10, 0x17, 0x11, 0x11, 0x0E },
                'H' => new byte[] { 0x11, 0x11, 0x11, 0x1F, 0x11, 0x11, 0x11 },
                'I' => new byte[] { 0x0E, 0x04, 0x04, 0x04, 0x04, 0x04, 0x0E },
                'K' => new byte[] { 0x11, 0x12, 0x14, 0x18, 0x14, 0x12, 0x11 },
                'L' => new byte[] { 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x1F },
                'M' => new byte[] { 0x11, 0x1B, 0x15, 0x11, 0x11, 0x11, 0x11 },
                'N' => new byte[] { 0x11, 0x19, 0x15, 0x13, 0x11, 0x11, 0x11 },
                'O' => new byte[] { 0x0E, 0x11, 0x11, 0x11, 0x11, 0x11, 0x0E },
                'P' => new byte[] { 0x1E, 0x11, 0x11, 0x1E, 0x10, 0x10, 0x10 },
                'R' => new byte[] { 0x1E, 0x11, 0x11, 0x1E, 0x14, 0x12, 0x11 },
                'S' => new byte[] { 0x0E, 0x11, 0x10, 0x0E, 0x01, 0x11, 0x0E },
                'T' => new byte[] { 0x1F, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04 },
                'W' => new byte[] { 0x11, 0x11, 0x11, 0x11, 0x15, 0x1B, 0x11 },
                _ => new byte[] { 0x1F, 0x11, 0x11, 0x11, 0x11, 0x11, 0x1F }
            };
        }

        static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = Sign(p, a, b);
            float d2 = Sign(p, b, c);
            float d3 = Sign(p, c, a);
            bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
            return !(hasNeg && hasPos);
        }

        static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }

        static bool PointInPolygon(Vector2 p, Vector2[] verts)
        {
            bool inside = false;
            for (int i = 0, j = verts.Length - 1; i < verts.Length; j = i++)
            {
                if (((verts[i].y > p.y) != (verts[j].y > p.y)) &&
                    (p.x < (verts[j].x - verts[i].x) * (p.y - verts[i].y) / (verts[j].y - verts[i].y) + verts[i].x))
                    inside = !inside;
            }
            return inside;
        }

        static Color GetAffinityColor(Affinity affinity)
        {
            return affinity switch
            {
                Affinity.Flame => new Color(1f, 0.4f, 0.1f),
                Affinity.Frost => new Color(0.3f, 0.7f, 1f),
                Affinity.Storm => new Color(0.6f, 0.4f, 1f),
                Affinity.Nature => new Color(0.2f, 0.8f, 0.3f),
                Affinity.Shadow => new Color(0.5f, 0.2f, 0.7f),
                Affinity.Radiance => new Color(1f, 0.9f, 0.4f),
                _ => new Color(0.6f, 0.6f, 0.6f)
            };
        }

        static void EnsureDirectory(string path)
        {
            string fullPath = Path.Combine(Application.dataPath, "..", path);
            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);
        }

        static void SaveTexture(Texture2D tex, string assetPath)
        {
            string fullPath = Path.Combine(Application.dataPath, "..", assetPath);
            File.WriteAllBytes(fullPath, tex.EncodeToPNG());
        }
    }
}
#endif
