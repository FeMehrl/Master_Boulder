using UnityEngine;

namespace Boulderhalle
{
    /// <summary>
    /// Gelenkwinkel einer Figur (Euler-Winkel in Grad, lokal zum Elternteil).
    /// Negative x-Rotation bewegt Arme/Beine nach vorne (+z), positive z-Rotation spreizt den rechten Arm ab.
    /// </summary>
    public struct Pose
    {
        public float PelvisHeight;
        public Vector3 Torso, Head, ShoulderL, ShoulderR, ElbowL, ElbowR, HipL, HipR, KneeL, KneeR;
    }

    /// <summary>Baut einfache Figuren (Kunden, Mitarbeiter) aus Primitiven. Die Figur blickt in lokale +z-Richtung.</summary>
    public static class PersonBuilder
    {
        // ---------- Posen ----------

        public static Pose Standing() => new Pose
        {
            PelvisHeight = 0.94f,
            ShoulderL = new Vector3(0f, 0f, -5f), ShoulderR = new Vector3(0f, 0f, 5f),
            ElbowL = new Vector3(-10f, 0f, 0f), ElbowR = new Vector3(-10f, 0f, 0f)
        };

        /// <summary>Mitarbeiter: winkt mit der rechten Hand, linke Hand liegt auf dem Tresen.</summary>
        public static Pose Waving()
        {
            var p = Standing();
            p.ShoulderR = new Vector3(0f, 0f, 80f);
            p.ElbowR = new Vector3(0f, 0f, 75f);
            p.ShoulderL = new Vector3(-40f, 0f, -5f);
            p.ElbowL = new Vector3(-45f, 0f, 0f);
            return p;
        }

        /// <summary>Sichert/schaut nach oben, Arme bereit.</summary>
        public static Pose Spotting()
        {
            var p = Standing();
            p.Head = new Vector3(-30f, 0f, 0f);
            p.ShoulderL = new Vector3(-70f, 0f, -10f);
            p.ShoulderR = new Vector3(-70f, 0f, 10f);
            p.ElbowL = p.ElbowR = new Vector3(-30f, 0f, 0f);
            return p;
        }

        /// <summary>Greift mit der rechten Hand in ein Regalfach.</summary>
        public static Pose Reaching()
        {
            var p = Standing();
            p.ShoulderR = new Vector3(-85f, 0f, 5f);
            p.ElbowR = new Vector3(-10f, 0f, 0f);
            return p;
        }

        /// <summary>Sitzen auf Stuhl, Bank oder Sofa (Sitzhöhe ca. 0,45 m).</summary>
        public static Pose SittingOnSeat() => new Pose
        {
            PelvisHeight = 0.5f,
            Torso = new Vector3(-5f, 0f, 0f),
            HipL = new Vector3(-90f, 0f, -4f), HipR = new Vector3(-90f, 0f, 4f),
            KneeL = new Vector3(90f, 0f, 0f), KneeR = new Vector3(90f, 0f, 0f),
            ShoulderL = new Vector3(-10f, 0f, -6f), ShoulderR = new Vector3(-10f, 0f, 6f),
            ElbowL = new Vector3(-50f, 0f, 0f), ElbowR = new Vector3(-50f, 0f, 0f)
        };

        /// <summary>Sitzt auf der Matte, Knie angezogen, schaut zur Wand hoch.</summary>
        public static Pose SittingKneesUp() => new Pose
        {
            PelvisHeight = 0.13f,
            Torso = new Vector3(12f, 0f, 0f),
            Head = new Vector3(-25f, 0f, 0f),
            HipL = new Vector3(-120f, 0f, -10f), HipR = new Vector3(-120f, 0f, 10f),
            KneeL = new Vector3(75f, 0f, 0f), KneeR = new Vector3(75f, 0f, 0f),
            ShoulderL = new Vector3(-55f, 0f, -12f), ShoulderR = new Vector3(-55f, 0f, 12f),
            ElbowL = new Vector3(-35f, 0f, 0f), ElbowR = new Vector3(-35f, 0f, 0f)
        };

        /// <summary>Sitzt auf der Matte, Beine ausgestreckt, stützt sich nach hinten ab.</summary>
        public static Pose SittingLegsOut() => new Pose
        {
            PelvisHeight = 0.11f,
            Torso = new Vector3(-18f, 0f, 0f),
            Head = new Vector3(-12f, 0f, 0f),
            HipL = new Vector3(-85f, 0f, -8f), HipR = new Vector3(-85f, 0f, 8f),
            KneeL = new Vector3(5f, 0f, 0f), KneeR = new Vector3(5f, 0f, 0f),
            ShoulderL = new Vector3(25f, 0f, -18f), ShoulderR = new Vector3(25f, 0f, 18f)
        };

        /// <summary>Klettert: beide Arme oben, linkes Bein hoch angetreten (Blick zur Wand = +z).</summary>
        public static Pose ClimbingA() => new Pose
        {
            PelvisHeight = 0.94f,
            Torso = new Vector3(10f, 0f, 0f),
            Head = new Vector3(-25f, 0f, 0f),
            ShoulderL = new Vector3(-162f, 0f, -12f), ElbowL = new Vector3(-5f, 0f, 0f),
            ShoulderR = new Vector3(-150f, 0f, 15f), ElbowR = new Vector3(-10f, 0f, 0f),
            HipL = new Vector3(-60f, 0f, -45f), KneeL = new Vector3(50f, 0f, 0f),
            HipR = new Vector3(-25f, 0f, 18f), KneeR = new Vector3(5f, 0f, 0f)
        };

        /// <summary>Klettert: rechter Arm weit oben, rechtes Bein seitlich angetreten.</summary>
        public static Pose ClimbingB() => new Pose
        {
            PelvisHeight = 0.94f,
            Torso = new Vector3(6f, 0f, 0f),
            Head = new Vector3(-15f, 0f, 0f),
            ShoulderL = new Vector3(-140f, 0f, -25f), ElbowL = new Vector3(-25f, 0f, 0f),
            ShoulderR = new Vector3(-165f, 0f, 8f), ElbowR = new Vector3(-5f, 0f, 0f),
            HipL = new Vector3(-20f, 0f, -15f), KneeL = new Vector3(10f, 0f, 0f),
            HipR = new Vector3(-55f, 0f, 40f), KneeR = new Vector3(55f, 0f, 0f)
        };

        // ---------- Aufbau ----------

        public static Transform Create(string name, Transform parent, Vector3 position, float yaw, Pose pose, Outfit o, bool blocking = true)
            => Create(name, parent, position, Quaternion.Euler(0f, yaw, 0f), pose, o, blocking);

        public static Transform Create(string name, Transform parent, Vector3 position, Quaternion rotation, Pose pose, Outfit o, bool blocking = true)
        {
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.localPosition = position;
            root.localRotation = rotation;

            var pelvis = Joint("Becken", root, new Vector3(0f, pose.PelvisHeight, 0f), Vector3.zero);
            Geo.Box("Hueftpartie", pelvis, Vector3.zero, new Vector3(0.32f, 0.16f, 0.2f), o.Bottom, collider: false);

            var torso = Joint("Oberkoerper", pelvis, new Vector3(0f, 0.05f, 0f), pose.Torso);
            Geo.Prim(PrimitiveType.Capsule, "Brust", torso, new Vector3(0f, 0.28f, 0f), new Vector3(0.36f, 0.28f, 0.22f), o.Top, collider: false);
            Geo.Cyl("Hals", torso, new Vector3(0f, 0.6f, 0f), 0.1f, 0.1f, o.Skin, collider: false);

            var head = Joint("Kopf", torso, new Vector3(0f, 0.63f, 0f), pose.Head);
            Geo.Ball("Gesicht", head, new Vector3(0f, 0.12f, 0f), Vector3.one * 0.23f, o.Skin);
            Geo.Ball("Haare", head, new Vector3(0f, 0.15f, -0.03f), new Vector3(0.245f, 0.21f, 0.235f), o.Hair);
            Geo.Ball("Auge_L", head, new Vector3(-0.045f, 0.13f, 0.108f), Vector3.one * 0.028f, o.Eyes);
            Geo.Ball("Auge_R", head, new Vector3(0.045f, 0.13f, 0.108f), Vector3.one * 0.028f, o.Eyes);

            Arm(torso, "L", -1f, pose.ShoulderL, pose.ElbowL, o);
            Arm(torso, "R", 1f, pose.ShoulderR, pose.ElbowR, o);
            Leg(pelvis, "L", -1f, pose.HipL, pose.KneeL, o);
            Leg(pelvis, "R", 1f, pose.HipR, pose.KneeR, o);

            if (blocking)
            {
                var c = root.gameObject.AddComponent<CapsuleCollider>();
                c.center = new Vector3(0f, 0.9f, 0f);
                c.height = 1.8f;
                c.radius = 0.25f;
            }
            return root;
        }

        static void Arm(Transform torso, string side, float s, Vector3 shoulderRot, Vector3 elbowRot, Outfit o)
        {
            var shoulder = Joint($"Schulter_{side}", torso, new Vector3(0.21f * s, 0.5f, 0f), shoulderRot);
            Geo.Prim(PrimitiveType.Capsule, "Oberarm", shoulder, new Vector3(0f, -0.15f, 0f), new Vector3(0.09f, 0.16f, 0.09f), o.Top, collider: false);
            var elbow = Joint($"Ellbogen_{side}", shoulder, new Vector3(0f, -0.3f, 0f), elbowRot);
            Geo.Prim(PrimitiveType.Capsule, "Unterarm", elbow, new Vector3(0f, -0.13f, 0f), new Vector3(0.075f, 0.14f, 0.075f), o.Skin, collider: false);
            Geo.Ball($"Hand_{side}", elbow, new Vector3(0f, -0.29f, 0f), Vector3.one * 0.09f, o.Skin);
        }

        static void Leg(Transform pelvis, string side, float s, Vector3 hipRot, Vector3 kneeRot, Outfit o)
        {
            var hip = Joint($"Huefte_{side}", pelvis, new Vector3(0.1f * s, -0.05f, 0f), hipRot);
            Geo.Prim(PrimitiveType.Capsule, "Oberschenkel", hip, new Vector3(0f, -0.21f, 0f), new Vector3(0.15f, 0.23f, 0.15f), o.Bottom, collider: false);
            var knee = Joint($"Knie_{side}", hip, new Vector3(0f, -0.42f, 0f), kneeRot);
            Geo.Prim(PrimitiveType.Capsule, "Unterschenkel", knee, new Vector3(0f, -0.2f, 0f), new Vector3(0.12f, 0.22f, 0.12f), o.Bottom, collider: false);
            Geo.Box($"Fuss_{side}", knee, new Vector3(0f, -0.43f, 0.05f), new Vector3(0.1f, 0.07f, 0.24f), o.Shoes, collider: false);
        }

        static Transform Joint(string name, Transform parent, Vector3 pos, Vector3 euler)
        {
            var t = new GameObject(name).transform;
            t.SetParent(parent, false);
            t.localPosition = pos;
            t.localRotation = Quaternion.Euler(euler);
            return t;
        }

        public static Transform FindDeep(Transform root, string name)
        {
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                var found = FindDeep(child, name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
