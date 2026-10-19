using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace MiniGame.UnityHost.Editor
{
    /// <summary>Builds a lit, three-dimensional corridor outside the room's entrance.</summary>
    public static class ReferenceCorridorBuilder
    {
        public const string RootName = "Reference exterior corridor";
        public const string ThresholdName = "Corridor no-exit threshold";
        private const string Generated = "Assets/MiniGame/Generated/ReferenceRoom";
        private const string TexturePath = "Assets/MiniGame/Art/Exterior/corridor-surfaces.png";
        private const float Length = 6f, Depth = 2.2f, SkirtHeight = 1.05f;
        private static readonly Dictionary<char, string[]> Glyphs = new Dictionary<char, string[]>
        {
            ['A'] = new[] { "01110", "10001", "10001", "11111", "10001", "10001", "10001" },
            ['B'] = new[] { "11110", "10001", "10001", "11110", "10001", "10001", "11110" },
            ['E'] = new[] { "11111", "10000", "10000", "11110", "10000", "10000", "11111" },
            ['G'] = new[] { "01111", "10000", "10000", "10111", "10001", "10001", "01110" },
            ['I'] = new[] { "11111", "00100", "00100", "00100", "00100", "00100", "11111" },
            ['L'] = new[] { "10000", "10000", "10000", "10000", "10000", "10000", "11111" },
            ['N'] = new[] { "10001", "11001", "11001", "10101", "10011", "10011", "10001" },
            ['O'] = new[] { "01110", "10001", "10001", "10001", "10001", "10001", "01110" },
            ['P'] = new[] { "11110", "10001", "10001", "11110", "10000", "10000", "10000" },
            ['S'] = new[] { "01111", "10000", "10000", "01110", "00001", "00001", "11110" },
            ['T'] = new[] { "11111", "00100", "00100", "00100", "00100", "00100", "00100" },
            ['V'] = new[] { "10001", "10001", "10001", "10001", "10001", "01010", "00100" },
            ['X'] = new[] { "10001", "10001", "01010", "00100", "01010", "10001", "10001" },
            ['1'] = new[] { "00100", "01100", "00100", "00100", "00100", "00100", "01110" },
            ['6'] = new[] { "01110", "10000", "10000", "11110", "10001", "10001", "01110" },
            ['8'] = new[] { "01110", "10001", "10001", "01110", "10001", "10001", "01110" },
            ['-'] = new[] { "00000", "00000", "00000", "11111", "00000", "00000", "00000" },
            // Four 16 px SimSun glyphs stored as lit geometry, without a runtime font dependency.
            ['禁'] = new[] { "0000100000100000", "0000100000100000", "0111111011111100", "0000100000100000", "0001110001110000", "0010101010101000", "1100100000100110", "0000000000000000", "0011111111111000", "0000000000000000", "0000000000000000", "1111111111111110", "0000000100000000", "0001000100010000", "0010010100001000", "0100001000000100" },
            ['止'] = new[] { "0000000100000000", "0000000100000000", "0000000100000000", "0000000100000000", "0001000100000000", "0001000100000000", "0001000111111000", "0001000100000000", "0001000100000000", "0001000100000000", "0001000100000000", "0001000100000000", "0001000100000000", "0001000100000000", "1111111111111110", "0000000000000000" },
            ['离'] = new[] { "0000001000000000", "0000000100000000", "1111111111111110", "0000000000000000", "0001010001010000", "0001001110010000", "0001010001010000", "0001111111110000", "0000000100000000", "0111111111111100", "0100001000000100", "0100010001000100", "0100111111100100", "0100010000100100", "0100000000010100", "0100000000001000" },
            ['岗'] = new[] { "0000000100000000", "0010000100001000", "0010000100001000", "0011111111111000", "0000000000000000", "0011111111111000", "0010000000001000", "0010000001001000", "0010010001001000", "0010001010001000", "0010000100001000", "0010001010001000", "0010010001001000", "0010100000001000", "0010000000101000", "0010000000010000" }
        };

        public static void Build(Transform parent, Vector3 doorwayFloor, Vector3 outward, Vector3 lateral, float height)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            outward = Vector3.ProjectOnPlane(outward, Vector3.up).normalized;
            lateral = Vector3.ProjectOnPlane(lateral, Vector3.up).normalized;
            if (outward.sqrMagnitude < .99f || lateral.sqrMagnitude < .99f
                || Mathf.Abs(Vector3.Dot(outward, lateral)) > .02f || height < 2.5f)
                throw new ArgumentException("The corridor needs orthogonal horizontal metre-scale directions and a valid room height.");
            var previous = parent.Find(RootName);
            if (previous != null) UnityEngine.Object.DestroyImmediate(previous.gameObject);
            Directory.CreateDirectory(Generated);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var atlas = ImportAtlas();
            var plaster = Material("Corridor warm plaster", atlas, new Vector4(0, .5f, .5f, .5f), Color.white, 1.2f, .12f);
            var green = Material("Corridor sage skirt", atlas, new Vector4(.5f, .5f, .5f, .5f), Color.white, 1.2f, .17f);
            var floor = Material("Corridor terrazzo", atlas, new Vector4(0, 0, .5f, .5f), Color.white, .8f, .28f);
            var metal = Material("Corridor cream metal", atlas, new Vector4(.5f, 0, .5f, .5f), Color.white, .65f, .3f);
            var dark = Material("Corridor dark fittings", atlas, new Vector4(.5f, 0, .5f, .5f), new Color(.23f, .27f, .24f), .5f, .22f);
            var sky = Material("Corridor lightwell", atlas, new Vector4(0, .5f, .5f, .5f), new Color(.84f, .94f, 1f), 1.8f, .1f);
            var root = new GameObject(RootName).transform;
            root.SetParent(parent, false);
            root.position = doorwayFloor;
            root.rotation = Quaternion.LookRotation(outward, Vector3.up);
            var frame = new Frame(root, doorwayFloor, outward, lateral);

            frame.Box("Corridor floor", 0, 1.1f, -.07f, new Vector3(6.2f, .14f, 2.42f), floor);
            frame.Box("Corridor ceiling", 0, 1.1f, height + .06f, new Vector3(6.2f, .12f, 2.42f), plaster);
            // Near wall retains the real entrance opening: 1.5 m wide, 2.5 m high.
            foreach (float side in new[] { -1f, 1f })
            {
                frame.Box("Entrance wall upper " + side, side * 1.875f, -.055f, (height + SkirtHeight) * .5f,
                    new Vector3(2.25f, height - SkirtHeight, .12f), plaster);
                frame.Box("Entrance wall skirt " + side, side * 1.875f, -.045f, SkirtHeight * .5f,
                    new Vector3(2.25f, SkirtHeight, .14f), green);
            }
            frame.Box("Entrance lintel", 0, -.055f, (height + 2.5f) * .5f,
                new Vector3(1.5f, height - 2.5f, .12f), plaster);
            frame.Box("Outer lower wall", 0, Depth + .06f, SkirtHeight * .5f,
                new Vector3(Length, SkirtHeight, .12f), green);
            frame.Box("Outer window sill wall", 0, Depth + .06f, 1.115f,
                new Vector3(Length, .13f, .12f), plaster);
            frame.Box("Outer window lintel", 0, Depth + .06f, (height + 2.7f) * .5f,
                new Vector3(Length, height - 2.7f, .12f), plaster);
            foreach (float side in new[] { -1f, 1f })
            {
                frame.Box("Outer upper wall " + side, side * 1.875f, Depth + .06f, 1.94f,
                    new Vector3(2.25f, 1.52f, .12f), plaster);
                frame.Box("Corridor end upper " + side, side * 3.06f, 1.1f, (height + SkirtHeight) * .5f,
                    new Vector3(.12f, height - SkirtHeight, 2.42f), plaster);
                frame.Box("Corridor end skirt " + side, side * 3.045f, 1.1f, SkirtHeight * .5f,
                    new Vector3(.14f, SkirtHeight, 2.42f), green);
                frame.Box("Outer skirting trim " + side, side * 1.5f, Depth - .015f, .055f,
                    new Vector3(3, .11f, .05f), dark, false);
            }
            BuildWindow(frame, metal, sky);
            BuildNeighborDoor(frame, -1.93f, "B-16", metal, dark);
            BuildNeighborDoor(frame, 1.93f, "B-18", metal, dark);
            BuildFireDoor(frame, metal, dark);
            BuildDelivery(frame, metal, green, dark);
            frame.Sign("No leaving post sign", "禁止离岗", .99f, -.125f, 1.53f, .38f, metal, dark);
            var threshold = new GameObject(ThresholdName);
            threshold.transform.SetParent(root, true);
            threshold.transform.position = frame.Point(0, .065f, height * .5f);
            threshold.transform.rotation = root.rotation;
            threshold.layer = 2; // IgnoreRaycast: blocks the controller without covering the open door's focus ray.
            threshold.AddComponent<BoxCollider>().size = new Vector3(1.5f, height, .12f);
            frame.Box("Corridor threshold strip", 0, .12f, .012f, new Vector3(1.5f, .024f, .16f), dark, false);

            Light(frame, "Corridor daylight", frame.Point(0, 1.78f, 2.5f), new Color(.84f, .91f, 1f), .8f, 5.4f);
            frame.Box("Corridor ceiling fixture", -1.1f, 1.1f, height - .065f,
                new Vector3(1.0f, .08f, .21f), metal, false);
            Light(frame, "Corridor ceiling fill", frame.Point(-1.1f, 1.1f, height - .18f), new Color(1f, .94f, .82f), .48f, 4.5f);
            AssetDatabase.SaveAssets();
        }

        private static void BuildWindow(Frame frame, Material metal, Material sky)
        {
            frame.Box("Window lightwell rear", 0, 2.63f, 1.94f, new Vector3(1.7f, 1.65f, .08f), sky);
            foreach (float side in new[] { -1f, 1f })
                frame.Box("Window lightwell reveal " + side, side * .79f, 2.43f, 1.94f,
                    new Vector3(.08f, 1.65f, .42f), metal);
            frame.Box("Window frame top", 0, 2.18f, 2.67f, new Vector3(1.58f, .08f, .13f), metal, false);
            frame.Box("Window frame sill", 0, 2.14f, 1.20f, new Vector3(1.7f, .08f, .28f), metal, false);
            foreach (float x in new[] { -.75f, 0f, .75f })
                frame.Box("Window upright " + x, x, 2.18f, 1.94f, new Vector3(.06f, 1.5f, .13f), metal, false);
            frame.Box("Window crossbar", 0, 2.18f, 2.04f, new Vector3(1.5f, .045f, .12f), metal, false);
            var glass = PlainMaterial("Corridor window glass", new Color(.72f, .83f, .87f, .18f), true);
            frame.Box("Corridor window glass", 0, 2.2f, 1.94f, new Vector3(1.45f, 1.4f, .012f), glass, false);
        }

        private static void BuildNeighborDoor(Frame frame, float x, string number, Material metal, Material dark)
        {
            frame.Box("Neighbor door " + number, x, 2.155f, 1.09f, new Vector3(.88f, 2.18f, .065f), metal);
            foreach (float side in new[] { -1f, 1f })
                frame.Box(number + " door jamb " + side, x + side * .485f, 2.10f, 1.115f,
                    new Vector3(.07f, 2.23f, .15f), dark, false);
            frame.Box(number + " door header", x, 2.10f, 2.25f, new Vector3(1.04f, .07f, .15f), dark, false);
            frame.Box(number + " handle", x + .30f, 2.08f, 1.0f, new Vector3(.13f, .025f, .055f), dark, false);
            frame.Sign(number + " number", number, x, 2.095f, 1.72f, .26f, metal, dark);
        }

        private static void BuildFireDoor(Frame frame, Material metal, Material dark)
        {
            frame.Box("Closed fire door", 2.96f, 1.1f, 1.13f, new Vector3(.065f, 2.26f, 1.9f), metal);
            frame.Box("Fire door center seam", 2.92f, 1.1f, 1.13f, new Vector3(.012f, 2.2f, .016f), dark, false);
            frame.Box("Fire door push bar", 2.90f, 1.1f, .99f, new Vector3(.055f, .055f, 1.52f), dark, false);
            frame.Box("Fire door header", 2.91f, 1.1f, 2.30f, new Vector3(.13f, .08f, 2.03f), dark, false);
        }

        private static void BuildDelivery(Frame frame, Material metal, Material green, Material dark)
        {
            frame.Box("Ration cabinet", -2.65f, 1.2f, .57f, new Vector3(.52f, 1.14f, .62f), green);
            frame.Box("Ration cabinet counter", -2.65f, 1.2f, 1.16f, new Vector3(.59f, .055f, .68f), metal);
            frame.Box("Ration cabinet seam", -2.65f, .882f, .59f, new Vector3(.018f, .86f, .015f), dark, false);
            foreach (float side in new[] { -1f, 1f })
                frame.Box("Ration cabinet pull " + side, -2.65f + side * .07f, .856f, .72f,
                    new Vector3(.024f, .15f, .03f), dark, false);
            frame.Box("Delivery ledge", -1.04f, .27f, .91f, new Vector3(.48f, .06f, .36f), metal);
            frame.Box("Delivery ledge support", -1.04f, .12f, .72f, new Vector3(.28f, .38f, .04f), dark);
            frame.Box("Delivery tray base", -1.04f, .28f, .96f, new Vector3(.40f, .035f, .28f), metal, false);
            foreach (float side in new[] { -1f, 1f })
                frame.Box("Delivery tray rim " + side, -1.04f + side * .20f, .28f, .985f,
                    new Vector3(.018f, .045f, .28f), dark, false);
        }

        private static void Light(Frame frame, string name, Vector3 position, Color color, float intensity, float range)
        {
            var obj = new GameObject(name); obj.transform.SetParent(frame.Root, true); obj.transform.position = position;
            var light = obj.AddComponent<Light>();
            light.type = LightType.Point; light.lightmapBakeType = LightmapBakeType.Realtime;
            light.color = color; light.intensity = intensity; light.range = range;
            light.shadows = LightShadows.Soft; light.shadowBias = .03f; light.shadowNormalBias = .2f;
        }

        private static Texture2D ImportAtlas()
        {
            var source = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Art/Exterior/corridor-surfaces.png"));
            Directory.CreateDirectory(Path.GetDirectoryName(TexturePath));
            File.Copy(source, TexturePath, true);
            AssetDatabase.ImportAsset(TexturePath, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(TexturePath);
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true; importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp; importer.filterMode = FilterMode.Bilinear;
            importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
        }

        private static Material Material(string name, Texture2D atlas, Vector4 rect, Color tint, float tile, float smoothness)
        {
            var shader = Shader.Find("MiniGame/Corridor Surface");
            if (shader == null) throw new InvalidOperationException("Import CorridorSurface.shader before building the reference corridor.");
            var path = Generated + "/" + name.Replace(' ', '_') + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, path); }
            else mat.shader = shader;
            mat.name = name; mat.SetTexture("_MainTex", atlas); mat.SetVector("_AtlasRect", rect);
            mat.SetColor("_Color", tint); mat.SetFloat("_TileSize", tile); mat.SetFloat("_Glossiness", smoothness);
            mat.SetFloat("_Metallic", 0); EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material PlainMaterial(string name, Color color, bool transparent)
        {
            var path = Generated + "/" + name.Replace(' ', '_') + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) { mat = new Material(Shader.Find("Standard")); AssetDatabase.CreateAsset(mat, path); }
            mat.name = name; mat.color = color; mat.SetFloat("_Glossiness", .45f);
            mat.SetColor("_EmissionColor", Color.black); mat.DisableKeyword("_EMISSION");
            if (transparent)
            {
                mat.SetFloat("_Mode", 2); mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha); mat.SetInt("_ZWrite", 0);
                mat.EnableKeyword("_ALPHABLEND_ON"); mat.renderQueue = (int)RenderQueue.Transparent;
            }
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private sealed class Frame
        {
            public readonly Transform Root;
            private readonly Vector3 origin, outward, lateral;
            public Frame(Transform root, Vector3 origin, Vector3 outward, Vector3 lateral)
            { Root = root; this.origin = origin; this.outward = outward; this.lateral = lateral; }
            public Vector3 Point(float x, float d, float y) => origin + lateral * x + outward * d + Vector3.up * y;
            public void Box(string name, float x, float d, float y, Vector3 size, Material material, bool collide = true)
            {
                var box = GameObject.CreatePrimitive(PrimitiveType.Cube); box.name = name;
                box.transform.SetParent(Root, true); box.transform.position = Point(x, d, y);
                box.transform.rotation = Root.rotation; box.transform.localScale = size;
                box.GetComponent<MeshRenderer>().sharedMaterial = material;
                box.isStatic = true;
                if (!collide) UnityEngine.Object.DestroyImmediate(box.GetComponent<Collider>());
            }
            public void Sign(string name, string text, float x, float d, float y, float width, Material plate, Material ink)
            {
                Box(name + " plate", x, d + .012f, y, new Vector3(width + .04f, .12f, .022f), plate, false);
                var meshPath = Generated + "/Corridor_sign_" + text.Replace(' ', '_') + ".asset";
                var mesh = LetterMesh(text, width);
                var saved = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                if (saved == null) { AssetDatabase.CreateAsset(mesh, meshPath); saved = mesh; }
                else { EditorUtility.CopySerialized(mesh, saved); UnityEngine.Object.DestroyImmediate(mesh); EditorUtility.SetDirty(saved); }
                var label = new GameObject(name); label.transform.SetParent(Root, true);
                label.transform.position = Point(x, d - .001f, y); label.transform.rotation = Root.rotation;
                label.AddComponent<MeshFilter>().sharedMesh = saved;
                label.AddComponent<MeshRenderer>().sharedMaterial = ink;
            }
        }

        private static Mesh LetterMesh(string text, float width)
        {
            var vertices = new List<Vector3>(); var normals = new List<Vector3>(); var triangles = new List<int>();
            int columns = -1, maximumRows = 0;
            foreach (char character in text)
            {
                if (!Glyphs.TryGetValue(character, out var glyph)) { columns += 4; continue; }
                columns += glyph[0].Length + 1; maximumRows = Mathf.Max(maximumRows, glyph.Length);
            }
            float pixel = width / Mathf.Max(1, columns), cursor = -width * .5f;
            for (int letter = 0; letter < text.Length; letter++)
            {
                if (!Glyphs.TryGetValue(text[letter], out var rows)) { cursor += pixel * 4; continue; }
                for (int row = 0; row < rows.Length; row++)
                    for (int column = 0; column < rows[row].Length; column++)
                    {
                        if (rows[row][column] != '1') continue;
                        float x = cursor + column * pixel;
                        float y = (maximumRows * .5f - row) * pixel;
                        int start = vertices.Count;
                        vertices.Add(new Vector3(x, y, 0)); vertices.Add(new Vector3(x + pixel, y, 0));
                        vertices.Add(new Vector3(x + pixel, y - pixel, 0)); vertices.Add(new Vector3(x, y - pixel, 0));
                        for (int i = 0; i < 4; i++) normals.Add(Vector3.back);
                        triangles.AddRange(new[] { start, start + 1, start + 2, start, start + 2, start + 3 });
                    }
                cursor += (rows[0].Length + 1) * pixel;
            }
            var mesh = new Mesh { name = "Corridor sign " + text };
            mesh.SetVertices(vertices); mesh.SetNormals(normals); mesh.SetTriangles(triangles, 0); mesh.RecalculateBounds();
            return mesh;
        }
    }
}
