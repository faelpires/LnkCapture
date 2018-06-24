using System;

namespace PodProgramar.Utils.Text.Extensions
{
    public static class StringExtensions
    {
        public static string GetRandomText(this string value)
        {
            var allPhrases = value.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var rnd = new Random(DateTime.Now.Millisecond);
            int ticks = rnd.Next(0, allPhrases.Length);

            return allPhrases[ticks];
        }
    }
}