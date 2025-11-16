using Fleece;

namespace RunnerUtils.Components.UI
{
    internal class Text
    {
        private static int currentOffset = 0;

        // not sure exactly what is needed here - this seems to work
        // see https://kaiclavier.com/docs/Fleece.html#what-is-a-parser
        public static Jumper MakeJumper(string text)
        {
            Passage customPassage = new();
            // trying to protect from collisions. if I'm honest with you I don't even know what would happen if we did collide
            customPassage.id = Story.active.passages.Count + 100000 + currentOffset++;
            customPassage.text = text;
            Story.active.passages.Add(customPassage);

            Jumper j = new();
            j.passage = customPassage;
            return j;
        }
    }
}
