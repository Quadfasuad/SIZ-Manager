using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace SizManager.Helpers;

public static class OpenXmlHelper
{
    /// <summary>
    /// Merges split placeholder runs in all paragraphs.
    /// Word sometimes splits {Placeholder} across multiple runs like {, Placeholder, }.
    /// This method detects and merges them.
    /// </summary>
    public static void MergePlaceholderRuns(OpenXmlElement root)
    {
        foreach (var paragraph in root.Descendants<Paragraph>())
        {
            MergePlaceholderRunsInParagraph(paragraph);
        }
    }

    private static void MergePlaceholderRunsInParagraph(Paragraph paragraph)
    {
        var runs = paragraph.Elements<Run>().ToList();
        if (runs.Count < 2) return;

        bool changed = true;
        while (changed)
        {
            changed = false;
            runs = paragraph.Elements<Run>().ToList();

            // Build concatenated text and find placeholder boundaries
            var texts = runs.Select(r => r.InnerText).ToList();
            var fullText = string.Join("", texts);

            // Find all {placeholder} patterns in the full text
            var matches = Regex.Matches(fullText, @"\{[A-Za-z_]+\}");

            foreach (Match match in matches)
            {
                int matchStart = match.Index;
                int matchEnd = matchStart + match.Length;

                // Find which runs this placeholder spans
                int charPos = 0;
                int startRunIdx = -1, endRunIdx = -1;

                for (int i = 0; i < runs.Count; i++)
                {
                    int runStart = charPos;
                    int runEnd = charPos + texts[i].Length;

                    if (startRunIdx == -1 && runEnd > matchStart)
                        startRunIdx = i;
                    if (runEnd >= matchEnd)
                    {
                        endRunIdx = i;
                        break;
                    }

                    charPos = runEnd;
                }

                // If placeholder spans multiple runs, merge them
                if (startRunIdx >= 0 && endRunIdx > startRunIdx)
                {
                    MergeRuns(paragraph, runs, startRunIdx, endRunIdx);
                    changed = true;
                    break; // Restart since run list changed
                }
            }
        }
    }

    private static void MergeRuns(Paragraph paragraph, List<Run> runs, int startIdx, int endIdx)
    {
        var firstRun = runs[startIdx];
        var mergedText = string.Join("", runs.Skip(startIdx).Take(endIdx - startIdx + 1).Select(r => r.InnerText));

        // Clear and set text on first run
        foreach (var text in firstRun.Elements<Text>().ToList())
            text.Remove();

        firstRun.AppendChild(new Text(mergedText) { Space = SpaceProcessingModeValues.Preserve });

        // Remove the other runs
        for (int i = startIdx + 1; i <= endIdx; i++)
        {
            // Also remove any proofErr elements between runs
            var prev = runs[i].PreviousSibling();
            while (prev != null && prev is ProofError)
            {
                var toRemove = prev;
                prev = prev.PreviousSibling();
                toRemove.Remove();
            }
            runs[i].Remove();
        }
    }

    /// <summary>
    /// Replace all occurrences of placeholder text in all paragraphs.
    /// </summary>
    public static void ReplacePlaceholders(OpenXmlElement root, Dictionary<string, string> replacements)
    {
        foreach (var text in root.Descendants<Text>())
        {
            foreach (var (placeholder, value) in replacements)
            {
                if (text.Text.Contains(placeholder))
                {
                    text.Text = text.Text.Replace(placeholder, value);
                }
            }
        }
    }

    /// <summary>
    /// Set the text content of a table cell.
    /// </summary>
    public static void SetCellText(TableCell cell, string text)
    {
        var paragraph = cell.Elements<Paragraph>().FirstOrDefault();
        if (paragraph == null)
        {
            paragraph = new Paragraph();
            cell.AppendChild(paragraph);
        }

        // Preserve existing paragraph properties (alignment, etc.)
        var existingRun = paragraph.Elements<Run>().FirstOrDefault();
        RunProperties? runProps = null;
        if (existingRun != null)
        {
            runProps = existingRun.RunProperties?.CloneNode(true) as RunProperties;
        }

        // Clear paragraph content (runs only)
        foreach (var run in paragraph.Elements<Run>().ToList())
            run.Remove();

        // Add new run with text
        var newRun = new Run();
        if (runProps != null)
            newRun.AppendChild(runProps);
        newRun.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        paragraph.AppendChild(newRun);
    }
}
