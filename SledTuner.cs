using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MelonLoader;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

[assembly: MelonInfo(typeof(S.P.S), "SledTuner", "1.0.0", "YourName")]
[assembly: MelonGame("Hanki Games", "Sledders")]

namespace S
{
    public class P : MelonMod
    {
        private readonly Dictionary<string, string[]> a = new Dictionary<string, string[]>
        {
            ["SnowmobileController"] = new[]
            {
                "leanSteerFactorSoft",
                "leanSteerFactorTrail",
                "throttleExponent",
                "drowningDepth",
                "drowningTime",
                "isEngineOn",
                "isStuck",
                "canRespawn",
                "hasDrowned",
                "rpmSensitivity",
                "rpmSensitivityDown",
                "minThrottleOnClutchEngagement",
                "clutchRpmMin",
                "clutchRpmMax",
                "isHeadlightOn",
                "wheelieThreshold"
            },
            ["SnowmobileControllerBase"] = new[]
            {
                "skisMaxAngle",
                "driverZCenter",
                "enableVerticalWeightTransfer",
                "trailLeanDistance",
                "switchbackTransitionTime"
            },
            ["MeshInterpretter"] = new[]
            {
                "power",
                "powerEfficiency",
                "breakForce",
                "frictionForce",
                "trackMass",
                "coefficientOfFriction",
                "snowPushForceFactor",
                "snowPushForceNormalizedFactor",
                "snowSupportForceFactor",
                "maxSupportPressure",
                "lugHeight",
                "snowOutTrackWidth",
                "pitchFactor"
            },
            ["SnowParameters"] = new[]
            {
                "snowNormalConstantFactor",
                "snowNormalDepthFactor",
                "snowFrictionFactor"
            },
            ["SuspensionController"] = new[]
            {
                "suspensionSubSteps",
                "antiRollBarFactor",
                "skiAutoTurn",
                "trackRigidityFront",
                "trackRigidityRear"
            },
            ["Stabilizer"] = new[]
            {
                "trackSpeedGyroMultiplier",
                "idleGyro"
            },
            ["Rigidbody"] = new[]
            {
                "mass",
                "drag",
                "angularDrag",
                "useGravity",
                "maxAngularVelocity"
            }
        };

        private string b;

        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("[SledTuner] OnInitializeMelon => Started.");
            b = Path.Combine(Directory.GetCurrentDirectory(), "Mods", "SledTuner");
            if (!Directory.Exists(b))
                Directory.CreateDirectory(b);
            MelonLogger.Msg($"[SledTuner] JSON folder path: {b}");
            SceneManager.sceneLoaded += c;
        }

        private void c(Scene d, LoadSceneMode e)
        {
            MelonLogger.Msg($"[SledTuner] Scene Loaded: {d.name}");
        }

        public override void OnUpdate()
        {
            bool f = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            if (f && Input.GetKeyDown(KeyCode.R))
            {
                MelonLogger.Msg("[SledTuner] CTRL+R => Saving config for current sled (VehicleScriptableObject)...");
                g();
            }
            else if (f && Input.GetKeyDown(KeyCode.S))
            {
                MelonLogger.Msg("[SledTuner] CTRL+S => Loading config for current sled (VehicleScriptableObject)...");
                h();
            }
        }

        private string i()
        {
            GameObject j = GameObject.Find("Snowmobile(Clone)");
            if (j == null)
            {
                MelonLogger.Warning("[SledTuner] Snowmobile(Clone) not found => can't read property GKMNAIKNNMJ.");
                return null;
            }
            var k = j.transform.Find("Body")?.gameObject;
            if (k == null)
            {
                MelonLogger.Warning("[SledTuner] Body not found => can't read property GKMNAIKNNMJ.");
                return null;
            }
            var l = k.GetComponent("SnowmobileController");
            if (l == null)
            {
                MelonLogger.Warning("[SledTuner] SnowmobileController not found => can't read property GKMNAIKNNMJ.");
                return null;
            }
            Type m = l.GetType();
            var n = m.GetProperty("GKMNAIKNNMJ", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (n == null || !n.CanRead)
            {
                MelonLogger.Warning("[SledTuner] GKMNAIKNNMJ property not found/readable on SnowmobileController.");
                return null;
            }
            object o;
            try
            {
                o = n.GetValue(l, null);
            }
            catch (Exception p)
            {
                MelonLogger.Warning($"[SledTuner] Error reading GKMNAIKNNMJ: {p.Message}");
                return null;
            }
            if (o == null)
            {
                MelonLogger.Warning("[SledTuner] GKMNAIKNNMJ returned null => no sled name.");
                return null;
            }
            string q = o.ToString();
            const string r = " (VehicleScriptableObject)";
            if (q.EndsWith(r))
            {
                return q.Substring(0, q.Length - r.Length);
            }
            else
            {
                return q;
            }
        }

        private string s(string t)
        {
            if (string.IsNullOrEmpty(t))
                return "UnknownSled";
            string u = t.Replace("(", "").Replace(")", "").Replace(" ", "_").Replace(":", "_").Replace("/", "_").Replace("\\", "_");
            return u;
        }

        private void g()
        {
            string v = i();
            if (string.IsNullOrEmpty(v))
            {
                MelonLogger.Warning("[SledTuner] Could not get a name from GKMNAIKNNMJ => skipping save.");
                return;
            }
            string w = s(v);
            string x = w + ".json";
            string y = Path.Combine(b, x);
            var z = A();
            if (z == null)
            {
                MelonLogger.Warning("[SledTuner] Reflection returned no data => skipping save.");
                return;
            }
            var aa = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Error = (sender, args) => { args.ErrorContext.Handled = true; }
            };
            try
            {
                string ab = JsonConvert.SerializeObject(z, aa);
                File.WriteAllText(y, ab);
                MelonLogger.Msg($"[SledTuner] Saved config => {x} for sled '{v}'");
            }
            catch (Exception ac)
            {
                MelonLogger.Error($"[SledTuner] Error saving JSON: {ac}");
            }
        }

        private void h()
        {
            string ad = i();
            if (string.IsNullOrEmpty(ad))
            {
                MelonLogger.Warning("[SledTuner] Could not get a name from GKMNAIKNNMJ => skipping load.");
                return;
            }
            string ae = s(ad);
            string af = ae + ".json";
            string ag = Path.Combine(b, af);
            if (!File.Exists(ag))
            {
                MelonLogger.Warning($"[SledTuner] No JSON found for '{ad}' => {af}");
                return;
            }
            Dictionary<string, Dictionary<string, object>> ah = null;
            try
            {
                string ai = File.ReadAllText(ag);
                var aj = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Error = (sender, args) => { args.ErrorContext.Handled = true; }
                };
                ah = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(ai, aj);
            }
            catch (Exception ak)
            {
                MelonLogger.Error($"[SledTuner] Error reading JSON: {ak}");
                return;
            }
            if (ah != null)
            {
                B(ah);
                MelonLogger.Msg($"[SledTuner] Loaded config => {af} for sled '{ad}'");
            }
        }

        private Dictionary<string, Dictionary<string, object>> A()
        {
            var al = GameObject.Find("Snowmobile(Clone)");
            if (al == null)
            {
                MelonLogger.Warning("[SledTuner] Snowmobile(Clone) not found in scene.");
                return null;
            }
            var am = al.transform.Find("Body")?.gameObject;
            if (am == null)
            {
                MelonLogger.Warning("[SledTuner] Body not found on Snowmobile(Clone).");
                return null;
            }
            var an = new Dictionary<string, Dictionary<string, object>>();
            foreach (var ao in a)
            {
                string ap = ao.Key;
                string[] aq = ao.Value;
                Component ar = (ap == "Rigidbody") ? am.GetComponent<Rigidbody>() : am.GetComponent(ap);
                if (ar == null)
                    continue;
                var asType = ar.GetType();
                var at = new Dictionary<string, object>();
                foreach (string au in aq)
                {
                    object av = aw(ar, asType, au);
                    at[au] = av;
                }
                an[ap] = at;
            }
            return an;
        }

        private object aw(object ax, Type ay, string az)
        {
            var a0 = ay.GetField(az, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (a0 != null)
            {
                try
                {
                    object a1 = a0.GetValue(ax);
                    return a2(a1, a0.FieldType);
                }
                catch (Exception a3)
                {
                    return $"Error reading field '{az}': {a3.Message}";
                }
            }
            var a4 = ay.GetProperty(az, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (a4 != null && a4.CanRead)
            {
                try
                {
                    object a5 = a4.GetValue(ax, null);
                    return a2(a5, a4.PropertyType);
                }
                catch (Exception a6)
                {
                    return $"Error reading property '{az}': {a6.Message}";
                }
            }
            return $"(Not found: {az})";
        }

        private object a2(object a7, Type a8)
        {
            if (a7 == null)
                return null;
            if (a8.IsSubclassOf(typeof(UnityEngine.Object)))
                return "(Skipped UnityEngine.Object)";
            if (!a8.IsPrimitive && a8 != typeof(string) && a8 != typeof(decimal))
                return "(Skipped complex type)";
            return a7;
        }

        private void B(Dictionary<string, Dictionary<string, object>> a9)
        {
            var aa0 = GameObject.Find("Snowmobile(Clone)");
            if (aa0 == null)
            {
                MelonLogger.Warning("[SledTuner] Cannot apply => Snowmobile(Clone) not found.");
                return;
            }
            var aa1 = aa0.transform.Find("Body")?.gameObject;
            if (aa1 == null)
            {
                MelonLogger.Warning("[SledTuner] Cannot apply => Body not found.");
                return;
            }
            foreach (var aa2 in a9)
            {
                string aa3 = aa2.Key;
                var aa4 = aa2.Value;
                Component aa5 = (aa3 == "Rigidbody") ? aa1.GetComponent<Rigidbody>() : aa1.GetComponent(aa3);
                if (aa5 == null)
                {
                    MelonLogger.Msg($"[SledTuner] {aa3} not found on Body; skipping...");
                    continue;
                }
                var aa6 = aa5.GetType();
                foreach (var aa7 in aa4)
                {
                    string aa8 = aa7.Key;
                    object aa9 = aa7.Value;
                    ab(aa5, aa6, aa8, aa9);
                }
            }
        }

        private void ab(object ab0, Type ab1, string ab2, object ab3)
        {
            var ab4 = ab1.GetField(ab2, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (ab4 != null)
            {
                try
                {
                    object ab5 = aB(ab3, ab4.FieldType);
                    ab4.SetValue(ab0, ab5);
                    MelonLogger.Msg($"[SledTuner] Set Field {ab1.Name}.{ab2} = {ab5}");
                }
                catch (Exception ab6)
                {
                    MelonLogger.Error($"[SledTuner] Error setting field {ab2}: {ab6.Message}");
                }
                return;
            }
            var ab7 = ab1.GetProperty(ab2, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (ab7 != null && ab7.CanWrite)
            {
                try
                {
                    object ab8 = aB(ab3, ab7.PropertyType);
                    ab7.SetValue(ab0, ab8, null);
                    MelonLogger.Msg($"[SledTuner] Set Property {ab1.Name}.{ab2} = {ab8}");
                }
                catch (Exception ab9)
                {
                    MelonLogger.Error($"[SledTuner] Error setting property {ab2}: {ab9.Message}");
                }
                return;
            }
            MelonLogger.Warning($"[SledTuner] '{ab2}' not found on {ab1.Name}.");
        }

        private object aB(object aC, Type aD)
        {
            if (aC == null)
                return null;
            try
            {
                if (aD == typeof(float) && aC is double aE)
                    return (float)aE;
                if (aD == typeof(int) && aC is long aF)
                    return (int)aF;
                if (aD.IsInstanceOfType(aC))
                    return aC;
                return Convert.ChangeType(aC, aD);
            }
            catch
            {
                return aC;
            }
        }
    }
}
