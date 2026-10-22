using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MiniGame.UnityHost
{
    /// <summary>Runtime-owned observation cell. All surfaces and props are authored meshes.</summary>
    public sealed class PatientMonitorRoom : MonoBehaviour
    {
        private readonly List<Mesh> meshes = new List<Mesh>();
        private readonly List<Material> materials = new List<Material>();
        private Transform fan;
        private Light ceilingLight;
        private GameObject occupiedIndicator;
        private Material padding;
        private Material floor;
        private Material steel;
        private Material darkSteel;
        private Material cloth;
        private Material dark;
        private Material luminous;
        private Material indicator;
        private int roomLayer;
        private float phase;
        private bool occupied = true;

        public Transform ActorAnchor { get; private set; }
        public float AnimationTime { get; private set; }

        public static PatientMonitorRoom Create(Transform parent, int layer, int seed,
            Color wallTint, Color lightTint, float grime)
        {
            Shader shader = Resources.Load<Shader>("PatientMonitorRoom");
            if (shader == null)
                throw new InvalidOperationException("PatientMonitorRoom shader is missing from MiniGame/Resources.");
            var root = new GameObject("Observation cell", typeof(PatientMonitorRoom));
            root.hideFlags = HideFlags.DontSave;
            root.transform.SetParent(parent, false);
            var room = root.GetComponent<PatientMonitorRoom>();
            room.roomLayer = Mathf.Clamp(layer, 0, 31);
            root.layer = room.roomLayer;
            try
            {
                room.Build(shader, seed, wallTint, lightTint, Mathf.Clamp01(grime));
                room.Tick(0f);
                return room;
            }
            catch
            {
                Release(root);
                throw;
            }
        }

        public void Tick(float elapsedSeconds)
        {
            AnimationTime = float.IsNaN(elapsedSeconds) || float.IsInfinity(elapsedSeconds)
                ? 0f : Mathf.Max(0f, elapsedSeconds);
            float lamp = .975f + .016f * Mathf.Sin(AnimationTime * 8.7f + phase)
                + .009f * Mathf.Sin(AnimationTime * 23.3f + phase * 3.1f);
            if (fan != null) fan.localRotation = Quaternion.Euler(0f, 0f, -AnimationTime * 110f + phase * 20f);
            if (ceilingLight != null) ceilingLight.intensity = lamp * .8f;
            Matrix4x4 worldToRoom = transform.worldToLocalMatrix;
            Vector3 lampPosition = transform.TransformPoint(new Vector3(0f, 2.58f, .2f));
            foreach (Material material in materials)
            {
                material.SetMatrix("_WorldToRoom", worldToRoom);
                material.SetVector("_LampPosition", lampPosition);
                material.SetFloat("_LampStrength", lamp);
            }
        }

        public void SetOccupied(bool value)
        {
            occupied = value;
            if (occupiedIndicator != null) occupiedIndicator.SetActive(occupied);
        }

        private void Build(Shader shader, int seed, Color wallTint, Color lightTint, float grime)
        {
            var random = new System.Random(seed);
            phase = (float)random.NextDouble() * 6.28318f;
            Color wall = Color.Lerp(new Color(.75f, .76f, .71f), wallTint, .55f);
            padding = MakeMaterial(shader, "Padded ivory", wall, lightTint, grime, .05f);
            floor = MakeMaterial(shader, "Worn padded floor", wall * .81f, lightTint, grime * 1.25f, .03f);
            steel = MakeMaterial(shader, "Brushed steel", new Color(.43f, .47f, .45f), lightTint, grime * .75f, .28f);
            darkSteel = MakeMaterial(shader, "Bed and fittings", new Color(.21f, .25f, .24f), lightTint, grime * .6f, .20f);
            cloth = MakeMaterial(shader, "Unbleached linen", new Color(.69f, .70f, .63f), lightTint, grime * .6f, .015f);
            dark = MakeMaterial(shader, "Window and recess", new Color(.045f, .060f, .056f), lightTint, 0f, .1f);
            luminous = MakeMaterial(shader, "Fluorescent diffuser", new Color(.77f, .84f, .79f), lightTint, 0f, 0f, 1.1f);
            indicator = MakeMaterial(shader, "Intercom active indicator", new Color(.45f, .16f, .07f), lightTint, 0f, 0f, .45f);

            BuildShell(random);
            BuildDoor();
            BuildVent();
            BuildBed();
            BuildDesk();
            BuildBasin();
            BuildLamp(lightTint);
            ActorAnchor = Child("Patient feet / bed edge", transform).transform;
            ActorAnchor.localPosition = new Vector3(1.43f, 0f, -.65f);
        }

        private Material MakeMaterial(Shader shader, string label, Color tint, Color lightTint,
            float grime, float specular, float emission = 0f)
        {
            var material = new Material(shader) { name = label, hideFlags = HideFlags.HideAndDontSave };
            material.SetColor("_Color", tint);
            material.SetColor("_LightTint", Color.Lerp(Color.white, lightTint, .45f));
            material.SetFloat("_Grime", Mathf.Clamp01(grime));
            material.SetFloat("_Specular", specular);
            material.SetFloat("_Emission", emission);
            materials.Add(material);
            return material;
        }

        private void BuildShell(System.Random random)
        {
            var vertices = new List<Vector3>();
            var uv = new List<Vector2>();
            var colors = new List<Color>();
            var triangles = new List<int>();
            for (int row = 0; row < 4; row++)
            {
                for (int column = 0; column < 8; column++)
                    AddPad(vertices, uv, colors, triangles,
                        new Vector3(-3.3f + column * .825f, row * .7f, 2.3f),
                        Vector3.right * .825f, Vector3.up * .7f, Vector3.back,
                        .048f + (float)random.NextDouble() * .025f, 8);
                for (int column = 0; column < 6; column++)
                {
                    float z = -2.3f + column * (4.6f / 6f);
                    AddPad(vertices, uv, colors, triangles, new Vector3(-3.3f, row * .7f, z),
                        Vector3.forward * (4.6f / 6f), Vector3.up * .7f, Vector3.right, .065f, 8);
                    AddPad(vertices, uv, colors, triangles, new Vector3(3.3f, row * .7f, z),
                        Vector3.forward * (4.6f / 6f), Vector3.up * .7f, Vector3.left, .065f, 8);
                }
            }
            MeshObject("Quilted wall panels", FinishMesh("Wall padding", vertices, uv, colors, triangles), padding);
            vertices.Clear(); uv.Clear(); colors.Clear(); triangles.Clear();
            for (int row = 0; row < 6; row++)
                for (int column = 0; column < 8; column++)
                    AddPad(vertices, uv, colors, triangles,
                        new Vector3(-3.3f + column * .825f, -.015f, -2.3f + row * (4.6f / 6f)),
                        Vector3.right * .825f, Vector3.forward * (4.6f / 6f), Vector3.up, .024f, 6);
            MeshObject("Padded floor tiles", FinishMesh("Floor padding", vertices, uv, colors, triangles), floor);
            Box("Ceiling", new Vector3(0f, 2.85f, 0f), new Vector3(6.6f, .12f, 4.6f), .025f, padding);
            Box("Left skirting", new Vector3(-3.26f, .075f, 0f), new Vector3(.06f, .15f, 4.6f), .018f, steel);
            Box("Right skirting", new Vector3(3.26f, .075f, 0f), new Vector3(.06f, .15f, 4.6f), .018f, steel);
            Box("Rear skirting", new Vector3(0f, .075f, 2.25f), new Vector3(6.6f, .15f, .06f), .018f, steel);
        }

        private static void AddPad(List<Vector3> vertices, List<Vector2> uv, List<Color> colors,
            List<int> triangles, Vector3 origin, Vector3 across, Vector3 up, Vector3 normal, float depth, int divisions)
        {
            int start = vertices.Count;
            const float gap = .006f;
            for (int y = 0; y <= divisions; y++)
                for (int x = 0; x <= divisions; x++)
                {
                    float u = x / (float)divisions, v = y / (float)divisions;
                    float edge = Mathf.Min(Mathf.Min(u, 1f - u), Mathf.Min(v, 1f - v));
                    float puff = Mathf.Pow(Mathf.Max(0f, Mathf.Sin(u * Mathf.PI) * Mathf.Sin(v * Mathf.PI)), .4f);
                    vertices.Add(origin + across * Mathf.Lerp(gap, 1f - gap, u)
                        + up * Mathf.Lerp(gap, 1f - gap, v) + normal * (depth * puff));
                    uv.Add(new Vector2(u, v));
                    float tone = Mathf.Lerp(.56f, 1f, Mathf.SmoothStep(0f, .16f, edge));
                    colors.Add(new Color(tone, tone, tone, 1f));
                }
            bool reverse = Vector3.Dot(Vector3.Cross(across, up), normal) < 0f;
            GridTriangles(triangles, start, divisions, divisions, reverse);
        }

        private void BuildDoor()
        {
            const float x = -2.08f;
            Box("Door shadow reveal", new Vector3(x, 1.12f, 2.14f), new Vector3(1.22f, 2.24f, .12f), .025f, dark);
            Box("Steel door", new Vector3(x, 1.10f, 2.055f), new Vector3(1.09f, 2.17f, .08f), .022f, steel);
            Box("Door frame left", new Vector3(x - .59f, 1.13f, 1.99f), new Vector3(.055f, 2.26f, .08f), .008f, darkSteel);
            Box("Door frame right", new Vector3(x + .59f, 1.13f, 1.99f), new Vector3(.055f, 2.26f, .08f), .008f, darkSteel);
            Box("Door frame lintel", new Vector3(x, 2.24f, 1.99f), new Vector3(1.24f, .055f, .08f), .008f, darkSteel);
            Box("Observation window rim", new Vector3(x, 1.61f, 1.998f), new Vector3(.48f, .36f, .045f), .012f, darkSteel);
            Box("Observation window glass", new Vector3(x, 1.61f, 1.97f), new Vector3(.40f, .28f, .012f), .006f, dark);
            Box("Glass reflection", new Vector3(x + .13f, 1.66f, 1.96f), new Vector3(.018f, .17f, .006f), .002f, steel);
            Box("Handle plate", new Vector3(x + .38f, 1.05f, 1.99f), new Vector3(.10f, .31f, .035f), .012f, darkSteel);
            Bar("Door pull", new Vector3(x + .38f, .98f, 1.91f), new Vector3(x + .38f, 1.17f, 1.91f), .018f, steel);
            for (int i = 0; i < 3; i++)
                Bar("Door hinge", new Vector3(x - .55f, .37f + i * .73f, 2.0f),
                    new Vector3(x - .55f, .51f + i * .73f, 2.0f), .02f, darkSteel);
        }

        private void BuildVent()
        {
            var center = new Vector3(-.79f, 2.34f, 2.12f);
            Box("Vent frame", center, new Vector3(.48f, .46f, .08f), .02f, steel);
            Box("Vent recess", center + Vector3.back * .051f, new Vector3(.39f, .37f, .012f), .015f, dark);
            fan = Child("Vent fan rotor", transform).transform;
            fan.localPosition = center + Vector3.back * .066f;
            for (int i = 0; i < 4; i++)
            {
                GameObject blade = Box("Fan blade", Vector3.zero, new Vector3(.07f, .15f, .008f), .018f, darkSteel, fan);
                blade.transform.localRotation = Quaternion.Euler(0, 0, i * 90f + 22f);
                blade.transform.localPosition = blade.transform.localRotation * new Vector3(0f, .079f, 0f);
            }
            Bar("Fan axle", center + Vector3.back * .06f, center + Vector3.back * .09f, .032f, steel);
            for (int i = -2; i <= 2; i++)
                Bar("Vent grille", center + new Vector3(i * .066f, -.18f, -.092f),
                    center + new Vector3(i * .066f, .18f, -.092f), .006f, steel);
        }

        private void BuildBed()
        {
            const float x = 1.43f, z = .72f;
            Box("Bed steel base", new Vector3(x, .29f, z), new Vector3(1.65f, .24f, 2.22f), .035f, darkSteel);
            for (int i = -1; i <= 1; i += 2)
                for (int j = -1; j <= 1; j += 2)
                    Box("Bed foot", new Vector3(x + i * .69f, .15f, z + j * .91f), new Vector3(.075f, .29f, .075f), .014f, steel);
            Box("Mattress", new Vector3(x, .51f, z), new Vector3(1.58f, .25f, 2.14f), .09f, cloth);
            Box("Pillow", new Vector3(x, .736f, z + .73f), new Vector3(.86f, .18f, .46f), .083f, cloth);
            var vertices = new List<Vector3>(); var uv = new List<Vector2>();
            var colors = new List<Color>(); var triangles = new List<int>();
            const int columns = 28, rows = 32;
            for (int row = 0; row <= rows; row++)
                for (int column = 0; column <= columns; column++)
                {
                    float u = column / (float)columns, v = row / (float)rows;
                    float edgeDrop = .18f * Mathf.SmoothStep(.77f, 1f, Mathf.Abs(u * 2f - 1f));
                    edgeDrop += .13f * (1f - Mathf.SmoothStep(0f, .10f, v));
                    float wrinkles = .019f * Mathf.Sin(u * 37f + v * 18f + phase)
                        + .012f * Mathf.Sin(v * 46f - u * 14f);
                    vertices.Add(new Vector3(x + (u - .5f) * 1.71f, .656f + wrinkles - edgeDrop,
                        z - 1.11f + v * 1.68f));
                    uv.Add(new Vector2(u, v));
                    float shade = .87f + .13f * Mathf.Sin(u * 7f + v * 5f);
                    colors.Add(new Color(shade, shade, shade, 1f));
                }
            GridTriangles(triangles, 0, columns, rows, true);
            MeshObject("Folded blanket", FinishMesh("Linen folds", vertices, uv, colors, triangles), cloth);
            Box("Bed head rail", new Vector3(x, .66f, z + 1.13f), new Vector3(1.68f, .075f, .065f), .018f, steel);
            for (int i = -1; i <= 1; i += 2)
                Bar("Bed head post", new Vector3(x + i * .80f, .31f, z + 1.13f),
                    new Vector3(x + i * .80f, .92f, z + 1.13f), .022f, steel);
        }

        private void BuildDesk()
        {
            Vector3 table = new Vector3(-2.35f, .78f, -.93f);
            Box("Desk top", table, new Vector3(1.20f, .075f, .78f), .04f, steel);
            for (int i = -1; i <= 1; i += 2)
                for (int j = -1; j <= 1; j += 2)
                    Bar("Desk leg", table + new Vector3(i * .49f, -.04f, j * .28f),
                        new Vector3(table.x + i * .49f, .03f, table.z + j * .28f), .025f, darkSteel);
            Box("Meal tray lip", table + new Vector3(-.13f, .065f, -.03f), new Vector3(.38f, .06f, .27f), .035f, darkSteel);
            Box("Meal tray interior", table + new Vector3(-.13f, .092f, -.03f), new Vector3(.31f, .014f, .20f), .03f, steel);
            Vector3 seat = new Vector3(-1.92f, .46f, -1.70f);
            Box("Chair seat", seat, new Vector3(.50f, .065f, .49f), .035f, padding);
            for (int i = -1; i <= 1; i += 2)
                for (int j = -1; j <= 1; j += 2)
                    Bar("Chair leg", seat + new Vector3(i * .20f, -.03f, j * .19f),
                        new Vector3(seat.x + i * .23f, .025f, seat.z + j * .22f), .018f, darkSteel);
            Box("Chair back", seat + new Vector3(0f, .40f, -.22f), new Vector3(.49f, .40f, .055f), .045f, padding);
            for (int i = -1; i <= 1; i += 2)
                Bar("Chair back support", seat + new Vector3(i * .20f, 0f, -.21f),
                    seat + new Vector3(i * .20f, .57f, -.21f), .018f, darkSteel);
        }

        private void BuildBasin()
        {
            Vector3 position = new Vector3(2.98f, .01f, -.40f);
            Vector2[] profile = {
                new Vector2(.13f, 0f), new Vector2(.14f, .05f), new Vector2(.13f, .35f),
                new Vector2(.25f, .49f), new Vector2(.31f, .55f), new Vector2(.31f, .59f),
                new Vector2(.24f, .60f), new Vector2(.21f, .54f), new Vector2(.12f, .45f), new Vector2(0f, .44f)
            };
            MeshObject("Steel wash basin", Lathe("Basin profile", profile, 40, .82f), steel).transform.localPosition = position;
            Bar("Basin upright pipe", position + new Vector3(.09f, .47f, .17f),
                position + new Vector3(.09f, 1.06f, .17f), .027f, darkSteel);
            Bar("Basin tap", position + new Vector3(.09f, .83f, .17f),
                position + new Vector3(-.07f, .83f, .17f), .022f, steel);
            Box("Intercom housing", new Vector3(3.20f, 1.64f, -.27f), new Vector3(.07f, .32f, .21f), .023f, steel);
            for (int i = 0; i < 5; i++)
                Box("Intercom grille slot", new Vector3(3.157f, 1.66f + i * .025f, -.27f),
                    new Vector3(.008f, .008f, .13f), .002f, dark);
            occupiedIndicator = Box("Intercom occupancy lamp", new Vector3(3.151f, 1.535f, -.27f),
                new Vector3(.008f, .022f, .022f), .003f, indicator);
            SetOccupied(occupied);
        }

        private void BuildLamp(Color lightTint)
        {
            Box("Fluorescent steel housing", new Vector3(0f, 2.66f, .20f), new Vector3(2.30f, .10f, .24f), .025f, darkSteel);
            Box("Fluorescent diffuser", new Vector3(0f, 2.598f, .20f), new Vector3(2.13f, .045f, .16f), .018f, luminous);
            for (int i = -1; i <= 1; i += 2)
                Bar("Lamp suspension", new Vector3(i * .85f, 2.70f, .2f),
                    new Vector3(i * .85f, 2.81f, .2f), .008f, steel);
            var lampObject = Child("Room-local ceiling light", transform);
            lampObject.transform.localPosition = new Vector3(0f, 2.55f, .2f);
            lampObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            ceilingLight = lampObject.AddComponent<Light>();
            ceilingLight.type = LightType.Spot;
            ceilingLight.range = 6f;
            ceilingLight.spotAngle = 125f;
            ceilingLight.color = Color.Lerp(Color.white, lightTint, .35f);
            ceilingLight.shadows = LightShadows.None;
            ceilingLight.cullingMask = 1 << roomLayer;
        }

        private GameObject Child(string label, Transform parent)
        {
            var child = new GameObject(label) { layer = roomLayer, hideFlags = HideFlags.DontSave };
            child.transform.SetParent(parent, false);
            return child;
        }

        private GameObject MeshObject(string label, Mesh mesh, Material material, Transform parent = null)
        {
            GameObject child = Child(label, parent != null ? parent : transform);
            child.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = child.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            return child;
        }

        private GameObject Box(string label, Vector3 center, Vector3 size, float radius, Material material, Transform parent = null)
        {
            GameObject child = MeshObject(label, RoundedBox(label, size, radius), material, parent);
            child.transform.localPosition = center;
            return child;
        }

        private void Bar(string label, Vector3 from, Vector3 to, float radius, Material material)
        {
            Vector3 difference = to - from;
            Vector2[] profile = { new Vector2(0f, 0f), new Vector2(radius, 0f),
                new Vector2(radius, difference.magnitude), new Vector2(0f, difference.magnitude) };
            GameObject child = MeshObject(label, Lathe(label, profile, 10, 1f), material);
            child.transform.localPosition = from;
            child.transform.localRotation = Quaternion.FromToRotation(Vector3.up, difference.normalized);
        }

        private Mesh RoundedBox(string label, Vector3 size, float radius)
        {
            var vertices = new List<Vector3>(); var uv = new List<Vector2>();
            var colors = new List<Color>(); var triangles = new List<int>();
            Vector3 half = size * .5f;
            radius = Mathf.Clamp(radius, .001f, Mathf.Min(half.x, Mathf.Min(half.y, half.z)) * .98f);
            Vector3 core = half - Vector3.one * radius;
            Vector3[] normals = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
            const int divisions = 6;
            foreach (Vector3 normal in normals)
            {
                Vector3 across = Mathf.Abs(normal.y) > .5f ? Vector3.right : Vector3.Cross(Vector3.up, normal);
                Vector3 up = Vector3.Cross(normal, across);
                int start = vertices.Count;
                for (int y = 0; y <= divisions; y++)
                    for (int x = 0; x <= divisions; x++)
                    {
                        float u = x / (float)divisions, v = y / (float)divisions;
                        Vector3 cube = Vector3.Scale(normal + across * (u * 2f - 1f) + up * (v * 2f - 1f), half);
                        Vector3 closest = new Vector3(Mathf.Clamp(cube.x, -core.x, core.x),
                            Mathf.Clamp(cube.y, -core.y, core.y), Mathf.Clamp(cube.z, -core.z, core.z));
                        vertices.Add(closest + (cube - closest).normalized * radius);
                        uv.Add(new Vector2(u, v));
                        colors.Add(Color.white);
                    }
                GridTriangles(triangles, start, divisions, divisions, false);
            }
            return FinishMesh(label, vertices, uv, colors, triangles);
        }

        private Mesh Lathe(string label, Vector2[] profile, int sides, float zScale)
        {
            var vertices = new List<Vector3>(); var uv = new List<Vector2>();
            var colors = new List<Color>(); var triangles = new List<int>();
            for (int row = 0; row < profile.Length; row++)
                for (int side = 0; side <= sides; side++)
                {
                    float angle = side / (float)sides * Mathf.PI * 2f;
                    vertices.Add(new Vector3(Mathf.Cos(angle) * profile[row].x, profile[row].y,
                        Mathf.Sin(angle) * profile[row].x * zScale));
                    uv.Add(new Vector2(side / (float)sides, row / (float)(profile.Length - 1)));
                    colors.Add(Color.white);
                }
            GridTriangles(triangles, 0, sides, profile.Length - 1, true);
            return FinishMesh(label, vertices, uv, colors, triangles);
        }

        private static void GridTriangles(List<int> triangles, int start, int columns, int rows, bool reverse)
        {
            for (int row = 0; row < rows; row++)
                for (int column = 0; column < columns; column++)
                {
                    int a = start + row * (columns + 1) + column, b = a + 1;
                    int c = a + columns + 1, d = c + 1;
                    if (reverse) triangles.AddRange(new[] { a, c, b, b, c, d });
                    else triangles.AddRange(new[] { a, b, c, b, d, c });
                }
        }

        private Mesh FinishMesh(string label, List<Vector3> vertices, List<Vector2> uv, List<Color> colors, List<int> triangles)
        {
            var mesh = new Mesh { name = label, hideFlags = HideFlags.HideAndDontSave };
            if (vertices.Count > ushort.MaxValue) mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uv);
            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            meshes.Add(mesh);
            return mesh;
        }

        public void DisposePresentation(bool immediate)
        {
            foreach (Mesh mesh in meshes) Release(mesh, immediate);
            foreach (Material material in materials) Release(material, immediate);
            meshes.Clear();
            materials.Clear();
        }

        private void OnDestroy() => DisposePresentation(!Application.isPlaying);

        private static void Release(UnityEngine.Object value, bool immediate = false)
        {
            if (value == null) return;
            if (immediate || !Application.isPlaying) DestroyImmediate(value); else Destroy(value);
        }
    }
}
