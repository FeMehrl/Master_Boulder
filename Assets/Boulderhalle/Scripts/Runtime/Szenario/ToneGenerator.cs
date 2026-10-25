using UnityEngine;

namespace Boulderhalle
{
    /// <summary>Erzeugt einfache Feedback-Töne, damit keine Audiodateien nötig sind.</summary>
    public static class ToneGenerator
    {
        const int SampleRate = 44100;

        /// <summary>Aufsteigender Zweiklang (richtig).</summary>
        public static AudioClip Positive() => Create("Ton_Positiv", false, (523.25f, 0.13f), (783.99f, 0.3f));

        /// <summary>Absteigender, rauer Zweiklang (falsch).</summary>
        public static AudioClip Negative() => Create("Ton_Negativ", true, (311.13f, 0.18f), (207.65f, 0.4f));

        /// <summary>Kurzer, neutraler Hinweiston.</summary>
        public static AudioClip Info() => Create("Ton_Info", false, (659.25f, 0.15f));

        static AudioClip Create(string name, bool harsh, params (float freq, float duration)[] notes)
        {
            int total = 0;
            foreach (var n in notes) total += (int)(n.duration * SampleRate);

            var data = new float[total];
            int offset = 0;
            foreach (var n in notes)
            {
                int count = (int)(n.duration * SampleRate);
                for (int i = 0; i < count; i++)
                {
                    float t = i / (float)SampleRate;
                    float phase = 2f * Mathf.PI * n.freq * t;
                    float wave = Mathf.Sin(phase);
                    if (harsh) wave = 0.6f * wave + 0.25f * Mathf.Sin(3f * phase) + 0.15f * Mathf.Sin(5f * phase);
                    else wave = 0.8f * wave + 0.2f * Mathf.Sin(2f * phase);

                    // Hüllkurve: kurzes Einblenden, weiches Ausklingen
                    float attack = Mathf.Clamp01(t / 0.01f);
                    float release = Mathf.Clamp01((n.duration - t) / (n.duration * 0.6f));
                    data[offset + i] = 0.35f * wave * attack * release;
                }
                offset += count;
            }

            var clip = AudioClip.Create(name, total, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
